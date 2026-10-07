#Requires -Version 5.1
<#
.SYNOPSIS
  Checks on the whole specification (docs/spec-kit/spec-kit-brief.md, checks 1 to 7) for
  specs/001-robot-rules, specs/002-console-session and specs/003-delivery.

.DESCRIPTION
  1 Coverage        FR-01..FR-18 and NFR-01..NFR-09 from page 10 are each owned by exactly one spec, as
                    the brief's ownership table says (FR-18, --help, was added after the run, so the brief
                    does not list it); every requirement-like ID defined in a spec.md
                    cites an FR or NFR on its defining bullet.
  2 Exact strings   every warning, the how-to-run message, the Output: label, the prompt and the echo written in
                    the spec folders match design-decisions.md (DEC-04, DEC-21, DEC-22, DEC-24, DEC-25);
                    a warning that quotes a concrete line must give the reason DEC-08/DEC-22 give for it.
  3 Exit codes and  every acceptance scenario: stdout holds only Output:/position lines, stderr holds no
    channels        Output: result, exit codes are 0, 1 or 2 (1 only for an argument, 2 only for the
                    unexpected problem of DEC-32; script errors in 003),
                    and page 2's stdout and stderr for each FR appear in the owning spec's scenario.
  4 Names           type and member names in the plans, data models and contracts are page 6's, the code
                    names the specs decided, .NET/xUnit names or test classes/helpers; decided names are
                    used the same way in all three specs.
  5 DEC-31 coverage every case of DEC-31's "Must be covered" list is matched inside a test task.
  6 Analyze         not scriptable; run /speckit-analyze separately.
  7 No code         the specification phase (git diff f9e10c0..a374aaa: from the commit that adds the
                    Spec Kit brief to the commit that adds this script) has nothing under src/ or tests/.

  Exit code 0 only when checks 1 to 5 and 7 all pass.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$root = & git rev-parse --show-toplevel 2>$null
if (-not $root) { Write-Host 'FAIL: cannot find the git repository root'; exit 1 }
$root = [System.IO.Path]::GetFullPath(($root | Select-Object -First 1))
$sep = [System.IO.Path]::DirectorySeparatorChar

function Join-Root([string]$rel) { return [System.IO.Path]::Combine($root, ($rel -replace '/', $sep)) }
function Get-Rel([string]$path) { return ($path.Substring($root.Length).TrimStart('\', '/') -replace '\\', '/') }
function Read-Lines([string]$path) {
    $c = Get-Content -LiteralPath $path -Encoding UTF8
    if ($null -eq $c) { return , @() }
    return , @($c)
}
function Show([string]$s) {
    if ($null -eq $s) { return '""' }
    $v = $s.Replace("`t", '[TAB]').Replace([string][char]0xA0, '[NBSP]').Replace([string][char]0xFEFF, '[BOM]')
    return '"' + $v + '"'
}

$specIds = @('001-robot-rules', '002-console-session', '003-delivery')
$designFile = Join-Root 'docs/design-analysis/design-decisions.md'
$page2File = Join-Root 'docs/solution-design/02-functional-requirements.md'
$page10File = Join-Root 'docs/solution-design/10-traceability.md'
$briefFile = Join-Root 'docs/spec-kit/spec-kit-brief.md'
$design = Read-Lines $designFile

# ---------------------------------------------------------------------------------------------
# Result bookkeeping
# ---------------------------------------------------------------------------------------------
$script:issues = New-Object System.Collections.Generic.List[string]
$script:passed = 0
$script:failed = 0
function Add-Issue([string]$where, [string]$text) { $script:issues.Add(('{0}: {1}' -f $where, $text)) }
function Start-Check([string]$title) {
    $script:issues = New-Object System.Collections.Generic.List[string]
    Write-Host ''
    Write-Host $title
}
function End-Check([string]$passText) {
    if ($script:issues.Count -eq 0) {
        Write-Host ('  PASS: ' + $passText)
        $script:passed++
    } else {
        foreach ($i in $script:issues) { Write-Host ('  - ' + $i) }
        Write-Host ('  FAIL: {0} issue(s)' -f $script:issues.Count)
        $script:failed++
    }
}

# ---------------------------------------------------------------------------------------------
# Markdown model: every .md file under the three spec folders, line by line
# ---------------------------------------------------------------------------------------------
function Get-MdModel([string]$path) {
    $lines = Read-Lines $path
    $res = New-Object System.Collections.Generic.List[object]
    $inFence = $false; $indent = 0; $lang = ''
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $l = $lines[$i]
        $m = [regex]::Match($l, '^(\s*)```(\S*)')
        if ($m.Success) {
            if (-not $inFence) { $inFence = $true; $indent = $m.Groups[1].Length; $lang = $m.Groups[2].Value; $kind = 'fence-start' }
            else { $inFence = $false; $kind = 'fence-end' }
            $res.Add([pscustomobject]@{ N = $i + 1; Kind = $kind; Text = $l; Lang = $lang })
            continue
        }
        if ($inFence) {
            $t = $l
            if ($t.Length -ge $indent -and $t.Substring(0, $indent).Trim().Length -eq 0) { $t = $t.Substring($indent) }
            $res.Add([pscustomobject]@{ N = $i + 1; Kind = 'code'; Text = $t; Lang = $lang })
        } else {
            $res.Add([pscustomobject]@{ N = $i + 1; Kind = 'text'; Text = $l; Lang = '' })
        }
    }
    return , $res
}

# Inline code spans of one text line. "`a` + X + `b`" (a span, an invisible character or a value
# named in prose, then a span) is joined into one span "a{X}b" first.
function Get-Spans([string]$t) {
    $prev = $null
    while ($prev -ne $t) {
        $prev = $t
        $t = [regex]::Replace($t, '`([^`]*)` \+ ([^`]{1,40}?) \+ `([^`]*)`', '`$1{$2}$3`')
    }
    $out = New-Object System.Collections.Generic.List[string]
    foreach ($m in [regex]::Matches($t, '`([^`]+)`')) { $out.Add($m.Groups[1].Value) }
    return , $out
}

# C# string expressions inside a span or code line, rebuilt as text: literals keep their content,
# interpolations and concatenated names become {name}.
$litRe = '\$?"(?:[^"\\]|\\.)*"'
$termRe = '(?:' + $litRe + '|[A-Za-z_][\w\.]*(?:\(\))?)'
$exprRe = $termRe + '(?:\s*\+\s*' + $termRe + ')*'
function Get-Literals([string]$s) {
    $out = New-Object System.Collections.Generic.List[string]
    foreach ($m in [regex]::Matches($s, $exprRe)) {
        if ($m.Value -notmatch '"') { continue }
        $sb = New-Object System.Text.StringBuilder
        foreach ($t in [regex]::Matches($m.Value, $termRe)) {
            $v = $t.Value
            if ($v -match '^\$?"') {
                $inner = $v.Substring($v.IndexOf('"') + 1)
                $inner = $inner.Substring(0, $inner.Length - 1)
                try { $inner = [regex]::Unescape($inner) } catch { }
                [void]$sb.Append($inner)
            } else {
                [void]$sb.Append('{' + $v + '}')
            }
        }
        $out.Add($sb.ToString())
    }
    return , $out
}

$md = @{}
$mdFiles = New-Object System.Collections.Generic.List[string]
foreach ($s in $specIds) {
    foreach ($f in (Get-ChildItem -LiteralPath (Join-Root "specs/$s") -Recurse -Filter '*.md' | Sort-Object FullName)) {
        $md[$f.FullName] = Get-MdModel $f.FullName
        $mdFiles.Add($f.FullName)
    }
}
function Spec-File([string]$specId, [string]$name) { return (Join-Root "specs/$specId/$name") }

# ---------------------------------------------------------------------------------------------
# Strings from design-decisions.md
# ---------------------------------------------------------------------------------------------
function Get-DecSection([string]$dec) {
    $start = -1; $end = $design.Count
    for ($i = 0; $i -lt $design.Count; $i++) {
        if ($start -lt 0 -and $design[$i] -match ('^### ' + $dec + ' ')) { $start = $i; continue }
        if ($start -ge 0 -and $design[$i] -match '^#{2,3} ') { $end = $i; break }
    }
    if ($start -lt 0) { throw "design-decisions.md has no $dec" }
    return , @($design[$start..($end - 1)])
}

$dec04 = Get-DecSection 'DEC-04'
$howToRunMessage = $null
for ($i = 0; $i -lt $dec04.Count; $i++) { if ($dec04[$i] -match '^```' -and $null -eq $howToRunMessage) { $howToRunMessage = $dec04[$i + 1] } }
if ($howToRunMessage -notmatch '^How to run: ToyRobot') { throw 'DEC-04 how-to-run message not found' }
# The help text of --help (FR-18): DEC-04's second fenced block. Its lines describe the rules (some say
# "ignored"), so they are not warnings.
$helpTextLines = New-Object System.Collections.Generic.HashSet[string]
$fenceCount = 0; $inHelp = $false
foreach ($l in $dec04) {
    if ($l -match '^```') { $fenceCount++; $inHelp = ($fenceCount -eq 3); continue }
    if ($inHelp) { [void]$helpTextLines.Add($l.Trim()) }
}
if ($helpTextLines.Count -eq 0) { throw 'DEC-04 help text not found' }

$dec21 = Get-DecSection 'DEC-21'
$outputLabel = $null
for ($i = 0; $i -lt $dec21.Count; $i++) { if ($dec21[$i] -match '^```' -and $null -eq $outputLabel) { $outputLabel = $dec21[$i + 1] } }
if ($outputLabel -ne 'Output:') { throw 'DEC-21 Output: label not found' }

$dec24 = (Get-DecSection 'DEC-24') -join "`n"
$m = [regex]::Match($dec24, '^`([^`]+)` is shown before each line', 'Multiline')
if (-not $m.Success) { throw 'DEC-24 prompt not found' }
$prompt = $m.Groups[1].Value

# DEC-32 (and DEC-23): the one line written for an unexpected problem, before the exception's message.
$dec32 = (Get-DecSection 'DEC-32') -join "`n"
$m = [regex]::Match($dec32, '`(Error: [^`<]+)<exception message>`')
if (-not $m.Success) { throw 'DEC-32 unexpected-problem line not found' }
$errorPrefix = $m.Groups[1].Value

$dec22 = Get-DecSection 'DEC-22'
$dec22Texts = New-Object System.Collections.Generic.List[string]
foreach ($l in $dec22) {
    $mm = [regex]::Match($l, '^\| [^|]+ \| `(Warning: [^`]+)`')
    if ($mm.Success) { $dec22Texts.Add($mm.Groups[1].Value) }
}
if ($dec22Texts.Count -ne 12) { throw ('DEC-22 table: expected 12 warnings, found {0}' -f $dec22Texts.Count) }

# Generic patterns built from the DEC-22 texts: the quoted line, a position and the command name
# become variables; template placeholders (<line>, <COMMAND>, X,Y,F, {name}) are accepted.
$ph = '(?:<[A-Za-z][^>]*>|\{[^}]+\})'
$posRe = '(?:-?\d+,-?\d+,(?:NORTH|SOUTH|EAST|WEST)|X,Y,F|' + $ph + ')'
$cmdRe = '(?:MOVE|LEFT|RIGHT|REPORT|' + $ph + ')'
$reasonPatterns = New-Object System.Collections.Generic.List[object]
foreach ($t in $dec22Texts) {
    $r = $t.Substring('Warning: '.Length)
    $raw = [regex]::Replace($r, "'[^']*'", [string][char]1 + 'Q')
    $raw = [regex]::Replace($raw, '-?\d+,-?\d+,(?:NORTH|SOUTH|EAST|WEST)', [string][char]1 + 'P')
    if ($raw -match ' takes no values$') { $raw = $raw -replace '(MOVE|LEFT|RIGHT|REPORT) takes', ([string][char]1 + 'C takes') }
    if ($raw -match ' ignored - robot is not on the table$') { $raw = $raw -replace '^(MOVE|LEFT|RIGHT|REPORT) ignored', ([string][char]1 + 'C ignored') }
    $esc = [regex]::Escape($raw)
    $esc = $esc.Replace([string][char]1 + 'Q', "'(?<q>.+)'").Replace([string][char]1 + 'P', $posRe).Replace([string][char]1 + 'C', $cmdRe)
    $tail = ''
    $ti = $r.IndexOf('ignored - ')
    if ($ti -ge 0) { $tail = $r.Substring($ti) }
    $reasonPatterns.Add([pscustomobject]@{ Example = $r; Regex = ('^' + $esc + '$'); Quoted = ($raw.Contains([string][char]1 + 'Q')); Tail = $tail })
}
# Fragments of a reason written on their own (for example in a search list) must be a DEC-22 tail.
$fragmentOk = New-Object System.Collections.Generic.List[string]
foreach ($p in $reasonPatterns) {
    if ($p.Tail) {
        $g = [regex]::Escape($p.Tail)
        $g = [regex]::Replace($g, '-?\d+,-?\d+,(?:NORTH|SOUTH|EAST|WEST)', $posRe)
        $g = $g -replace '(MOVE|LEFT|RIGHT|REPORT)\\ takes', ($cmdRe + '\ takes')
        $fragmentOk.Add('^' + $g + '$')
        $mm = [regex]::Match($p.Tail, '^(.* at )-?\d+,-?\d+,')
        if ($mm.Success) { $fragmentOk.Add('^' + [regex]::Escape($mm.Groups[1].Value) + '$') }
    }
}
$fragmentOk.Add('^takes no values$')

# DEC-08 and DEC-22: the reason for a concrete bad line (used to check every quoted warning).
function Get-ExpectedReason([string]$line) {
    $t = $line.Trim()
    if ($t.Length -eq 0) { return 'blank line ignored' }
    $idx = -1
    for ($i = 0; $i -lt $t.Length; $i++) { if ([char]::IsWhiteSpace($t[$i])) { $idx = $i; break } }
    if ($idx -ge 0) { $word = $t.Substring(0, $idx); $rest = $t.Substring($idx + 1) } else { $word = $t; $rest = $null }
    $oic = [StringComparison]::OrdinalIgnoreCase
    if ([string]::Equals($word, 'EXIT', $oic)) { return '<EXIT>' }
    foreach ($c in @('MOVE', 'LEFT', 'RIGHT', 'REPORT')) {
        if ([string]::Equals($word, $c, $oic)) {
            if ($null -ne $rest) { return ("'{0}' ignored - {1} takes no values" -f $t, $c) }
            return '<VALID>'
        }
    }
    if ([string]::Equals($word, 'PLACE', $oic)) {
        if ($null -eq $rest) { $parts = @('') } else { $parts = $rest.Split(',') }
        if ($parts.Count -ne 3) { return ("'{0}' ignored - expected PLACE X,Y,F" -f $t) }
        [int]$v = 0
        $ns = [System.Globalization.NumberStyles]::AllowLeadingSign
        $ci = [System.Globalization.CultureInfo]::InvariantCulture
        if (-not [int]::TryParse($parts[0].Trim(), $ns, $ci, [ref]$v)) {
            if ($parts[0].Trim() -match '^[+-]?[0-9]+$') { return ("'{0}' ignored - X is too large" -f $t) }
            return ("'{0}' ignored - X must be a whole number" -f $t)
        }
        if (-not [int]::TryParse($parts[1].Trim(), $ns, $ci, [ref]$v)) {
            if ($parts[1].Trim() -match '^[+-]?[0-9]+$') { return ("'{0}' ignored - Y is too large" -f $t) }
            return ("'{0}' ignored - Y must be a whole number" -f $t)
        }
        $f = $parts[2].Trim()
        $okF = $false
        foreach ($d in @('NORTH', 'SOUTH', 'EAST', 'WEST')) { if ([string]::Equals($d, $f, $oic)) { $okF = $true } }
        if (-not $okF) { return ("'{0}' ignored - F must be NORTH, SOUTH, EAST or WEST" -f $t) }
        return '<VALID>'
    }
    return ("'{0}' ignored - unknown command" -f $t)
}
function Convert-Markers([string]$s) {
    $nb = [string][char]0xA0; $bom = [string][char]0xFEFF
    return $s.Replace('[TAB]', "`t").Replace('[NBSP]', $nb).Replace('<U+00A0>', $nb).Replace('{U+00A0}', $nb).Replace('<U+FEFF>', $bom).Replace('{U+FEFF}', $bom)
}

# =============================================================================================
Write-Host 'Checks on the whole specification (spec-kit-brief.md, checks 1 to 7)'
Write-Host ('Specs: ' + ($specIds -join ', '))

# =============================================================================================
# Check 1: Coverage
# =============================================================================================
Start-Check 'Check 1: Coverage (page 10 FR/NFR owned by exactly one spec as in the brief; every requirement cites an FR or NFR)'

$page10Ids = New-Object System.Collections.Generic.List[string]
foreach ($l in (Read-Lines $page10File)) {
    foreach ($mm in [regex]::Matches($l, '\[(N?FR-\d{2})\]\(0[23]-')) { if (-not $page10Ids.Contains($mm.Groups[1].Value)) { $page10Ids.Add($mm.Groups[1].Value) } }
}
$expectedIds = @(1..18 | ForEach-Object { 'FR-{0:D2}' -f $_ }) + @(1..9 | ForEach-Object { 'NFR-{0:D2}' -f $_ })
foreach ($id in $expectedIds) { if (-not $page10Ids.Contains($id)) { Add-Issue 'docs/solution-design/10-traceability.md' "$id missing from page 10" } }
foreach ($id in $page10Ids) { if ($expectedIds -notcontains $id) { Add-Issue 'docs/solution-design/10-traceability.md' "unexpected requirement $id on page 10" } }

function Expand-Ids([string]$text) {
    $set = New-Object System.Collections.Generic.List[string]
    foreach ($mm in [regex]::Matches($text, '\b(N?FR)-(\d{2})(?![\da-z])(?:\s+to\s+(?:N?FR-)?(\d{2}))?')) {
        $a = [int]$mm.Groups[2].Value; $b = $a
        if ($mm.Groups[3].Success) { $b = [int]$mm.Groups[3].Value }
        for ($k = $a; $k -le $b; $k++) { $set.Add(('{0}-{1:D2}' -f $mm.Groups[1].Value, $k)) }
    }
    return $set.ToArray()
}

# The ownership table as the task states it, cross-checked with the brief's own table.
$addedAfterRun = @{ '002-console-session' = @('FR-18') }
$ownTable = @{
    '001-robot-rules'     = @('FR-01', 'FR-02', 'FR-03', 'FR-04', 'FR-05', 'FR-06', 'FR-07', 'FR-09', 'FR-10', 'NFR-07')
    '002-console-session' = @('FR-08', 'FR-11', 'FR-12', 'FR-13', 'FR-14', 'FR-15', 'FR-16', 'FR-17', 'FR-18', 'NFR-01', 'NFR-04', 'NFR-05', 'NFR-06')
    '003-delivery'        = @('NFR-02', 'NFR-03', 'NFR-08', 'NFR-09')
}
foreach ($l in (Read-Lines $briefFile)) {
    $mm = [regex]::Match($l, '^\| \*\*(00\d-[a-z-]+)\*\* \| ([^|]+) \|')
    if ($mm.Success) {
        # Requirements added after the run are not in the brief, which records the run's inputs.
        $b = @(@(Expand-Ids $mm.Groups[2].Value) + @($addedAfterRun[$mm.Groups[1].Value] | Where-Object { $_ }) | Sort-Object)
        $t = @($ownTable[$mm.Groups[1].Value] | Sort-Object)
        if (($b -join ',') -ne ($t -join ',')) { Add-Issue 'docs/spec-kit/spec-kit-brief.md' ("ownership of {0} in the brief ({1}) differs from the expected table ({2})" -f $mm.Groups[1].Value, ($b -join ', '), ($t -join ', ')) }
    }
}

$owners = @{}
foreach ($s in $specIds) {
    $f = Spec-File $s 'spec.md'; $rel = Get-Rel $f
    $ownedLine = $null; $ownedLineN = 0
    $definedOwned = New-Object System.Collections.Generic.List[string]
    $heading = ''
    foreach ($o in $md[$f]) {
        if ($o.Kind -ne 'text') { continue }
        if ($o.Text -match '^#{2,4}\s+(.*)$') { $heading = $Matches[1]; continue }
        if ($null -eq $ownedLine -and $o.Text -match '^- \*\*Owned by this spec\*\*:\s*(.*)$') { $ownedLine = $Matches[1]; $ownedLineN = $o.N; continue }
        $mm = [regex]::Match($o.Text, '^- \*\*(N?FR-\d{2})(?![\da-z])')
        if ($mm.Success) {
            $id = $mm.Groups[1].Value
            if ($heading -match '\(owned') { $definedOwned.Add($id) }
            else { Add-Issue ('{0}:{1}' -f $rel, $o.N) ("$id is defined outside an '(owned)' requirements section: " + (Show $o.Text)) }
        }
    }
    if ($null -eq $ownedLine) { Add-Issue $rel 'no "**Owned by this spec**:" line'; continue }
    $fromLine = @(Expand-Ids $ownedLine | Sort-Object -Unique)
    $fromDefs = @($definedOwned | Sort-Object -Unique)
    $expected = @($ownTable[$s] | Sort-Object)
    if (($fromLine -join ',') -ne ($expected -join ',')) { Add-Issue ('{0}:{1}' -f $rel, $ownedLineN) ("owned line lists {0}; the brief gives {1}" -f ($fromLine -join ', '), ($expected -join ', ')) }
    if (($fromDefs -join ',') -ne ($expected -join ',')) { Add-Issue $rel ("requirements defined in the '(owned)' sections are {0}; the brief gives {1}" -f ($fromDefs -join ', '), ($expected -join ', ')) }
    foreach ($id in $fromLine) { if (-not $owners.ContainsKey($id)) { $owners[$id] = New-Object System.Collections.Generic.List[string] }; $owners[$id].Add($s) }
    Write-Host ('  {0} owns: {1}' -f $s, ($fromLine -join ', '))
}
foreach ($id in $page10Ids) {
    if (-not $owners.ContainsKey($id)) { Add-Issue 'specs' "$id is owned by no spec" }
    elseif ($owners[$id].Count -ne 1) { Add-Issue 'specs' ("$id is owned by {0} specs: {1}" -f $owners[$id].Count, ($owners[$id] -join ', ')) }
}

# Every requirement-like ID defined in a spec.md cites an FR-nn or NFR-nn on its defining bullet.
$citeCount = 0
foreach ($s in $specIds) {
    $f = Spec-File $s 'spec.md'; $rel = Get-Rel $f
    $model = $md[$f]
    for ($k = 0; $k -lt $model.Count; $k++) {
        $o = $model[$k]
        if ($o.Kind -ne 'text') { continue }
        $mm = [regex]::Match($o.Text, '^(\s*)- \*\*([A-Z][A-Z0-9]*-\d+[a-z]?)(?![\w-])')
        if (-not $mm.Success) { continue }
        $id = $mm.Groups[2].Value
        if ($id -match '^SC-') { continue }
        $citeCount++
        $bullet = $o.Text
        $ind = $mm.Groups[1].Length
        for ($j = $k + 1; $j -lt $model.Count; $j++) {
            $n = $model[$j]
            if ($n.Kind -eq 'text') {
                if ($n.Text.Trim().Length -eq 0) { break }
                $lead = ([regex]::Match($n.Text, '^\s*')).Length
                if ($lead -le $ind) { break }
                if ($n.Text -match '^\s*- ') { break }
            }
            $bullet += ' ' + $n.Text
        }
        $base = [regex]::Match($id, '^(N?FR-\d{2})(?:[a-z])?$')
        if ($base.Success) {
            if (-not $page10Ids.Contains($base.Groups[1].Value)) { Add-Issue ('{0}:{1}' -f $rel, $o.N) "$id is not on page 10 and does not derive from a page 10 requirement" }
            continue
        }
        $body = $bullet.Remove($mm.Groups[2].Index, $mm.Groups[2].Length)
        $cited = $false
        foreach ($c in [regex]::Matches($body, '\b(N?FR-\d{2})(?:[a-z])?\b')) { if ($page10Ids.Contains($c.Groups[1].Value)) { $cited = $true } }
        if (-not $cited) { Add-Issue ('{0}:{1}' -f $rel, $o.N) ("$id cites no FR-nn or NFR-nn: " + (Show $o.Text.Trim())) }
    }
}
Write-Host ('  {0} requirement-like IDs checked for an FR/NFR citation' -f $citeCount)
End-Check ('27 requirements from page 10 (FR-01 to FR-18, NFR-01 to NFR-09) each owned by exactly one spec as in the brief, with FR-18 added after the run; every defined requirement cites an FR or NFR')

# =============================================================================================
# Check 2: Exact strings
# =============================================================================================
Start-Check 'Check 2: Exact strings (warnings vs DEC-22, how-to-run message DEC-04, Output: DEC-21, prompt DEC-24, echo DEC-25)'
Write-Host ('  DEC-04 how-to-run message: ' + (Show $howToRunMessage))
Write-Host ('  DEC-24 prompt: ' + (Show $prompt) + '; DEC-21 label: ' + (Show $outputLabel))
Write-Host ('  DEC-32 error line: ' + (Show ($errorPrefix + '<exception message>')))
Write-Host ('  DEC-22 patterns built: {0}' -f $reasonPatterns.Count)
$script:checkedWarnings = 0

function Test-Reason([string]$r, [string]$where, [string]$shown) {
    $script:checkedWarnings++
    $hit = $null
    foreach ($p in $reasonPatterns) { $mm = [regex]::Match($r, $p.Regex); if ($mm.Success) { $hit = @{ P = $p; M = $mm }; break } }
    if ($null -eq $hit) { Add-Issue $where ('warning matches no DEC-22 pattern: ' + (Show $shown)); return }
    if (-not $hit.P.Quoted) { return }
    $q = $hit.M.Groups['q'].Value
    if ($q -match '^(?:<[A-Za-z][^>]*>|\{[^}]+\})$') { return }
    $qc = Convert-Markers $q
    if ($qc -match '<[A-Za-z][^>]*>|\{[^}]+\}') { return }
    if ($qc -ne $qc.Trim()) { Add-Issue $where ('quoted line keeps whitespace at an end (DEC-22): ' + (Show $shown)); return }
    $exp = Get-ExpectedReason $qc
    $act = $r.Replace("'" + $q + "'", "'" + $qc + "'")
    if ($exp -eq '<VALID>' -or $exp -eq '<EXIT>') { Add-Issue $where ('quotes a line that is not a bad line (DEC-08 reads it as a command): ' + (Show $shown)); return }
    if ($act -ne $exp) { Add-Issue $where ('wrong reason for this line (DEC-08/DEC-22 give ' + (Show $exp) + '): ' + (Show $shown)) }
}

function Test-Piece([string]$s, [string]$where, [bool]$inCode) {
    if ($s.StartsWith('How to run: ToyRobot')) {
        if ($s -ne $howToRunMessage) { Add-Issue $where ('how-to-run message differs from DEC-04: ' + (Show $s)) }
        return
    }
    if ($s -cmatch '^Error: ') {
        if (-not $s.StartsWith($errorPrefix)) { Add-Issue $where ('error line differs from DEC-32 (must be ' + (Show ($errorPrefix + '<exception message>')) + '): ' + (Show $s)) }
        return
    }
    if ($s -cmatch '^Output\b') {
        if ($s -ne $outputLabel) { Add-Issue $where ('Output label differs from DEC-21 (must be exactly "Output:"): ' + (Show $s)) }
        return
    }
    if ($s -match '^>(?![ &">=])\S') { Add-Issue $where ('prompt/echo must be ">" followed by one space (DEC-24, DEC-25): ' + (Show $s)) }
    if ($s -match '^> {2,}\S') { Add-Issue $where ('echo has more than one space after ">" (DEC-25 trims the line): ' + (Show $s)) }
    if ($inCode -and $s -eq '>') { Add-Issue $where ('prompt/echo line without its space (DEC-24, DEC-25): ' + (Show $s)) }
    $w = $s
    while ($w.StartsWith('> Warning')) { $w = $w.Substring(2) }
    if ($w -match '^\s*Warning\s*:') {
        if ($w -eq 'Warning: ' -or $w -eq 'Warning:') { return }
        if (-not $w.StartsWith('Warning: ')) { Add-Issue $where ('warning prefix must be "Warning: " (DEC-22): ' + (Show $s)); return }
        $r = $w.Substring('Warning: '.Length)
        if ($r -match '^(?:<[A-Za-z][^>]*>|\{[^}]+\})$') { return }
        Test-Reason $r $where $s
        return
    }
    if ($s -match '\bignored\b') {
        if ($s.StartsWith('ignored - ') -or $s -eq 'takes no values') {
            $ok = $false
            foreach ($g in $fragmentOk) { if ($s -match $g) { $ok = $true } }
            if (-not $ok) { Add-Issue $where ('warning fragment is not part of a DEC-22 text: ' + (Show $s)) }
            return
        }
        Test-Reason $s $where $s
    }
}

function Get-Pieces([string]$s) {
    $out = New-Object System.Collections.Generic.List[string]
    if ($s.Contains('"')) {
        foreach ($lit in (Get-Literals $s)) { foreach ($p in ($lit -split "`n")) { if ($p.Length -gt 0) { $out.Add($p) } } }
    } else {
        foreach ($p in ($s -split [string][char]0x23CE)) { if ($p.Length -gt 0) { $out.Add($p) } }
    }
    return , $out
}

foreach ($f in $mdFiles) {
    $rel = Get-Rel $f
    foreach ($o in $md[$f]) {
        $where = '{0}:{1}' -f $rel, $o.N
        if ($o.Kind -eq 'code') {
            if ($helpTextLines.Contains($o.Text.Trim())) { continue }
            foreach ($p in (Get-Pieces $o.Text)) { Test-Piece $p $where $true }
        } elseif ($o.Kind -eq 'text') {
            $spans = Get-Spans $o.Text
            foreach ($sp in $spans) { foreach ($p in (Get-Pieces $sp)) { Test-Piece $p $where $false } }
            $prose = [regex]::Replace($o.Text, '`[^`]*`', '')
            if ($prose -match 'Warning: \S' -or $prose -match "' ignored - " -or $prose -match 'How to run: ToyRobot') {
                Add-Issue $where ('warning or how-to-run text written outside backticks: ' + (Show $o.Text.Trim()))
            }
            foreach ($mm in [regex]::Matches($o.Text, '(?i)\bprompts?\b[^`\n]{0,12}`([^`]*)`')) {
                if ($mm.Groups[1].Value -ne $prompt) { Add-Issue $where ('prompt described as ' + (Show $mm.Groups[1].Value) + ', DEC-24 says ' + (Show $prompt)) }
            }
            foreach ($mm in [regex]::Matches($o.Text, '`([^`]*)`\s+prompt\b')) {
                if ($mm.Groups[1].Value -ne $prompt) { Add-Issue $where ('prompt described as ' + (Show $mm.Groups[1].Value) + ', DEC-24 says ' + (Show $prompt)) }
            }
            foreach ($mm in [regex]::Matches($o.Text, '(?i)`([^`]*)`\s+(?:followed by (?:the|each) line|and the line)')) {
                if ($mm.Groups[1].Value -ne $prompt) { Add-Issue $where ('echo prefix described as ' + (Show $mm.Groups[1].Value) + ', DEC-25 says ' + (Show $prompt)) }
            }
            foreach ($mm in [regex]::Matches($o.Text, '(?i)\bechoed as `([^`]*)`')) {
                if (-not $mm.Groups[1].Value.StartsWith($prompt)) { Add-Issue $where ('echo described as ' + (Show $mm.Groups[1].Value) + ', DEC-25 says ' + (Show $prompt) + ' and the line') }
            }
        }
    }
}
Write-Host ('  {0} warning texts compared with DEC-22' -f $script:checkedWarnings)

# Every DEC-22 table text appears verbatim in at least one spec file.
$allSpecText = ($mdFiles | ForEach-Object { (Read-Lines $_) -join "`n" }) -join "`n"
foreach ($t in $dec22Texts) { if (-not $allSpecText.Contains($t)) { Add-Issue 'specs' ('DEC-22 text appears in no spec: ' + (Show $t)) } }
if (-not $allSpecText.Contains('`' + $howToRunMessage + '`')) { Add-Issue 'specs' 'the DEC-04 how-to-run message appears in no spec' }
End-Check 'every warning, how-to-run message, Output: label, prompt and echo matches design-decisions.md; all 12 DEC-22 texts appear verbatim'

# =============================================================================================
# Check 3: Exit codes and channels
# =============================================================================================
Start-Check 'Check 3: Exit codes and channels (every acceptance scenario against DEC-23 and page 2)'

$positionRe = '^-?\d+,-?\d+,(?:NORTH|SOUTH|EAST|WEST)$'
$inputRe = '(?i)\binput lines? (?:is|are)\b|\bthe lines are\b|\bfile holds\b|\binput in\b'

function Get-Scenarios([string]$specId) {
    $f = Spec-File $specId 'spec.md'
    $list = New-Object System.Collections.Generic.List[object]
    $inSection = $false; $story = ''; $cur = $null
    foreach ($o in $md[$f]) {
        if ($o.Kind -eq 'text') {
            $close = $false
            if ($o.Text -match '^### (User Story \d+)') { $story = $Matches[1]; $inSection = $false; $close = $true }
            elseif ($o.Text -match '^\*\*Acceptance Scenarios\*\*') { $inSection = $true; $close = $true }
            elseif ($inSection -and ($o.Text -match '^---\s*$' -or $o.Text -match '^#{2,4} ')) { $inSection = $false; $close = $true }
            elseif ($inSection -and $o.Text -match '^(\d+[a-z]?)\. ') {
                if ($null -ne $cur) { $list.Add($cur) }
                $cur = [pscustomobject]@{ Spec = $specId; Rel = (Get-Rel $f); Story = $story; Num = $Matches[1]; Start = $o.N; Lines = (New-Object System.Collections.Generic.List[object]) }
            }
            if ($close) { if ($null -ne $cur) { $list.Add($cur) }; $cur = $null; continue }
        }
        if ($null -ne $cur) { $cur.Lines.Add($o) }
    }
    if ($null -ne $cur) { $list.Add($cur) }
    return , $list
}

function Get-ScenarioInfo($sc) {
    $info = [pscustomobject]@{
        Tag = ''; FRs = @(); Fences = (New-Object System.Collections.Generic.List[object]); EmptyOut = $false; EmptyErr = $false
        InlineOut = (New-Object System.Collections.Generic.List[object]); InlineErr = (New-Object System.Collections.Generic.List[object])
        TableOut = (New-Object System.Collections.Generic.List[object]); TableErr = (New-Object System.Collections.Generic.List[object])
        Spans = (New-Object System.Collections.Generic.List[string]); Exits = (New-Object System.Collections.Generic.List[string]); Text = ''; ExitAsIn = $false
        ExitClauses = (New-Object System.Collections.Generic.List[string])
    }
    $first = $sc.Lines[0].Text
    $mm = [regex]::Match($first, '^\d+a?\. \*\*\[([^\]]+)\]')
    if ($mm.Success) { $info.Tag = $mm.Groups[1].Value; $info.FRs = @([regex]::Matches($info.Tag, '\bN?FR-\d{2}\b') | ForEach-Object { $_.Value }) }
    # Fences and the channel each one is stated for.
    $between = New-Object System.Text.StringBuilder
    $textAll = New-Object System.Text.StringBuilder
    $lastChannel = ''
    $fence = $null
    $tableCols = $null
    foreach ($o in $sc.Lines) {
        if ($o.Kind -eq 'fence-start') {
            $b = $between.ToString()
            $pos = @{ stdout = -1; stderr = -1; input = -1 }
            foreach ($x in [regex]::Matches($b, '\(stdout\)|holds exactly')) { $pos.stdout = $x.Index }
            foreach ($x in [regex]::Matches($b, '\(stderr\)')) { $pos.stderr = $x.Index }
            foreach ($x in [regex]::Matches($b, $inputRe)) { $pos.input = $x.Index }
            $ch = $lastChannel
            $best = -1
            foreach ($k in @('stdout', 'stderr', 'input')) { if ($pos[$k] -gt $best) { $best = $pos[$k]; $ch = $k } }
            if ($best -lt 0) { $ch = $lastChannel }
            $fence = [pscustomobject]@{ Channel = $ch; Start = $o.N; Lines = (New-Object System.Collections.Generic.List[object]) }
            [void]$textAll.Append("`n[FENCE]`n")
            continue
        }
        if ($o.Kind -eq 'fence-end') {
            if ($null -ne $fence) { $info.Fences.Add($fence); $lastChannel = $fence.Channel }
            $fence = $null; [void]$between.Clear(); continue
        }
        if ($o.Kind -eq 'code') { if ($null -ne $fence) { $fence.Lines.Add($o) }; continue }
        [void]$between.Append($o.Text + "`n")
        [void]$textAll.Append($o.Text + "`n")
        foreach ($sp in (Get-Spans $o.Text)) { $info.Spans.Add($sp) }
        # Tables with a stdout or stderr column.
        if ($o.Text -match '^\s*\|') {
            $cells = @($o.Text.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() })
            if ($null -eq $tableCols) {
                $tableCols = @{}
                for ($c = 0; $c -lt $cells.Count; $c++) {
                    if ($cells[$c] -match '\(stdout\)') { $tableCols[$c] = 'stdout' }
                    if ($cells[$c] -match '\(stderr\)') { $tableCols[$c] = 'stderr' }
                }
            } elseif ($o.Text -notmatch '^\s*\|[-| :]+\|\s*$') {
                foreach ($c in $tableCols.Keys) {
                    if ($c -ge $cells.Count) { continue }
                    $vals = New-Object System.Collections.Generic.List[string]
                    foreach ($seg in ($cells[$c] -split '<br>')) {
                        $ss = Get-Spans $seg
                        if ($ss.Count -gt 0) { $vals.Add($ss[0]) }
                    }
                    $entry = [pscustomobject]@{ N = $o.N; Lines = $vals }
                    if ($tableCols[$c] -eq 'stdout') { $info.TableOut.Add($entry) } else { $info.TableErr.Add($entry) }
                }
            }
        } else { $tableCols = $null }
    }
    $info.Text = $textAll.ToString()
    $t = $info.Text
    if ($t -match '\(stdout\)(?:\*\*)?:?(?:\*\*)?\s*\(empty\)') { $info.EmptyOut = $true }
    if ($t -match '\(stderr\)(?:\*\*)?:?(?:\*\*)?\s*\(empty\)') { $info.EmptyErr = $true }
    foreach ($x in [regex]::Matches($t, '\((stdout|stderr)\)(?:\*\*)?:?(?:\*\*)?\s*(?:is|are|holds)?\s*(?=`)')) {
        $rest = $t.Substring($x.Index + $x.Length)
        $stop = [regex]::Match($rest, '\*\*|\(stdout\)|\(stderr\)|\[FENCE\]|\n\s*\n')
        if ($stop.Success) { $rest = $rest.Substring(0, $stop.Index) }
        $vals = New-Object System.Collections.Generic.List[string]
        foreach ($sp in (Get-Spans $rest)) { $vals.Add($sp) }
        if ($x.Groups[1].Value -eq 'stdout') { $info.InlineOut.Add($vals) } else { $info.InlineErr.Add($vals) }
    }
    foreach ($x in [regex]::Matches($t, '(?i)\bexit code(?:\*\*)?:?(?:\*\*)?:?\s*(?:is\s+)?(\d+|the build''s exit code|not applicable)')) {
        $info.Exits.Add($x.Groups[1].Value.ToLowerInvariant())
        # The clause the statement is in: from the previous ";" or sentence end to the next one.
        $a = $t.LastIndexOfAny([char[]]@(';', "`n"), $x.Index)
        $sentence = $t.LastIndexOf('. ', $x.Index)
        if ($sentence -gt $a) { $a = $sentence }
        $z = $t.IndexOfAny([char[]]@(';', "`n"), $x.Index)
        if ($z -lt 0) { $z = $t.Length }
        $info.ExitClauses.Add($t.Substring($a + 1, $z - $a - 1))
    }
    if ($t -match '(?i)exit codes? (?:are|is) as in scenario') { $info.ExitAsIn = $true }
    return $info
}

function Test-StdoutLines($lines, [string]$where, [bool]$allowPlaceholder) {
    $arr = @($lines)
    for ($i = 0; $i -lt $arr.Count; $i++) {
        $l = $arr[$i]
        if ($l -match '^Warning\s*:') { Add-Issue $where ('warning stated as console output (stdout): ' + (Show $l)); continue }
        if ($l -match '^>') { Add-Issue $where ('prompt or echo stated as console output (stdout): ' + (Show $l)); continue }
        if ($l -match '^How to run: ') { Add-Issue $where ('how-to-run message stated as console output (stdout): ' + (Show $l)); continue }
        if ($l -eq 'Output:') {
            $nx = $null; if ($i + 1 -lt $arr.Count) { $nx = $arr[$i + 1] }
            if ($null -eq $nx -or -not ($nx -match $positionRe -or ($allowPlaceholder -and $nx -match '^<[^>]+>$'))) { Add-Issue $where ('"Output:" on console output (stdout) not followed by a position line') }
            continue
        }
        if ($l -match $positionRe -or ($allowPlaceholder -and $l -match '^<[^>]+>$')) {
            if ($i -eq 0 -or $arr[$i - 1] -ne 'Output:') { Add-Issue $where ('position on console output (stdout) without "Output:" before it: ' + (Show $l)) }
            continue
        }
        Add-Issue $where ('console output (stdout) line that is not a REPORT result (DEC-23): ' + (Show $l))
    }
}
function Test-StderrLines($lines, [string]$where, [bool]$strict) {
    $arr = @($lines)
    for ($i = 0; $i -lt $arr.Count; $i++) {
        $l = $arr[$i]
        if ($l -eq 'Output:' -and $i + 1 -lt $arr.Count -and $arr[$i + 1] -match $positionRe) { Add-Issue $where ('REPORT result ("Output:" and a position) stated as console messages (stderr)'); continue }
        if ($l -eq 'Output:') { Add-Issue $where ('"Output:" stated as console messages (stderr)'); continue }
        if ($strict -and -not ($l.StartsWith('> ') -or $l.StartsWith('Warning: ') -or $l -eq $howToRunMessage -or $l.StartsWith($errorPrefix))) { Add-Issue $where ('console messages (stderr) line is not a prompt/echo, warning, how-to-run message or DEC-32 error line: ' + (Show $l)) }
        if ($strict -and $l.StartsWith('Warning: ')) {
            $q = [regex]::Match($l, "^Warning: '(.+)' ignored - ")
            if ($q.Success -and $i -gt 0) {
                $prev = $arr[$i - 1]
                if ($prev -ne ('> ' + $q.Groups[1].Value)) { Add-Issue $where ('warning does not quote the echo line directly above it (DEC-22, DEC-25): ' + (Show $l) + ' after ' + (Show $prev)) }
            }
        }
    }
}

$allScenarios = @{}
$exitTotal = 0
foreach ($s in $specIds) {
    $scs = Get-Scenarios $s
    $allScenarios[$s] = New-Object System.Collections.Generic.List[object]
    $specText = (Read-Lines (Spec-File $s 'spec.md')) -join "`n"
    if ($s -eq '001-robot-rules' -and $specText -notmatch 'The \*\*exit code\*\* is 0 in every scenario in this spec') {
        Add-Issue (Get-Rel (Spec-File $s 'spec.md')) 'spec 001 does not state "The **exit code** is 0 in every scenario in this spec"'
    }
    foreach ($sc in $scs) {
        $info = Get-ScenarioInfo $sc
        $allScenarios[$s].Add([pscustomobject]@{ Sc = $sc; Info = $info })
        $where = '{0}:{1} ({2} scenario {3})' -f $sc.Rel, $sc.Start, $sc.Story, $sc.Num
        foreach ($fe in $info.Fences) {
            $fl = @($fe.Lines | ForEach-Object { $_.Text })
            $fw = '{0}:{1} ({2} scenario {3})' -f $sc.Rel, $fe.Start, $sc.Story, $sc.Num
            if ($fe.Channel -eq 'stdout') { Test-StdoutLines $fl $fw $false }
            elseif ($fe.Channel -eq 'stderr') { Test-StderrLines $fl $fw $true }
            elseif ($fe.Channel -ne 'input') { Add-Issue $fw 'fenced block whose channel (input, stdout or stderr) is not stated' }
        }
        foreach ($v in $info.InlineOut) { Test-StdoutLines $v $where $true }
        foreach ($v in $info.InlineErr) { Test-StderrLines $v $where $false }
        foreach ($e in $info.TableOut) { Test-StdoutLines $e.Lines ('{0}:{1}' -f $sc.Rel, $e.N) $false }
        foreach ($e in $info.TableErr) { Test-StderrLines $e.Lines ('{0}:{1}' -f $sc.Rel, $e.N) $true }

        # Exit codes.
        $hasClaim = $info.EmptyOut -or $info.EmptyErr -or $info.InlineOut.Count -gt 0 -or $info.InlineErr.Count -gt 0 -or $info.TableOut.Count -gt 0 -or $info.TableErr.Count -gt 0 -or (@($info.Fences | Where-Object { $_.Channel -ne 'input' }).Count -gt 0)
        # A scenario that states one exit code is judged as a whole; one that states several (the
        # CI scenario) is judged clause by clause.
        $distinct = @($info.Exits | Sort-Object -Unique).Count
        for ($xi = 0; $xi -lt $info.Exits.Count; $xi++) {
            $ex = $info.Exits[$xi]
            $ctx = $info.Text
            if ($distinct -gt 1) { $ctx = $info.ExitClauses[$xi] }
            # An argument: the how-to-run message, or "started with/as" a command line with an argument.
            $isArg = ($ctx -match [regex]::Escape($howToRunMessage)) -or ($ctx -match ('(?i)started (?:with|as)[^.]*' + [char]96 + 'ToyRobot [^<' + [char]96 + ']')) -or (@($info.Fences | Where-Object { @($_.Lines | ForEach-Object { $_.Text }) -contains $howToRunMessage }).Count -gt 0)
            # --help on its own, in lower case (FR-18), exits with 0.
            $isHelp = $ctx -cmatch ([char]96 + 'ToyRobot --help' + [char]96)
            $exitTotal++
            if ($s -eq '003-delivery') {
                $isScriptError = $ctx -match '(?i)does not exist|missing file|a folder|more than one argument|two arguments|no \.NET SDK|older SDK|does not build'
                if ($ex -eq 'the build''s exit code') { if ($ctx -notmatch '(?i)does not build') { Add-Issue $where 'the build''s exit code stated outside the failed-build scenario' } }
                elseif ($ex -eq '1') { if (-not $isScriptError) { Add-Issue $where 'exit code 1 stated for a scenario that is not a script error case' } }
                elseif ($ex -eq '0') { if ($isScriptError) { Add-Issue $where 'exit code 0 stated for a script error case' } }
                else { Add-Issue $where ("exit code '$ex' is not 0, 1 or the build's exit code") }
            } else {
                if ($ex -eq 'not applicable') { if ($info.Text -notmatch '(?i)in memory') { Add-Issue $where 'exit code "not applicable" for a scenario that is not in memory only' } }
                elseif ($ex -eq '1') { if (-not $isArg -or $isHelp) { Add-Issue $where 'exit code 1 stated for a scenario without an argument other than --help (only FR-17 gives 1; FR-18, --help, gives 0)' } }
                elseif ($ex -eq '0') { if ($isArg -and -not $isHelp) { Add-Issue $where 'exit code 0 stated for a scenario started with an argument other than --help' } }
                elseif ($ex -eq '2') { if ($ctx -notmatch '(?i)unexpected problem') { Add-Issue $where 'exit code 2 stated for a scenario that is not an unexpected problem (DEC-32)' } }
                else { Add-Issue $where ("application exit code '$ex' is not 0, 1 or 2") }
                if ($s -eq '001-robot-rules' -and $ex -ne '0') { Add-Issue $where ("spec 001 states exit code '$ex'; its scenarios all end with 0") }
            }
        }
        if ($s -ne '001-robot-rules' -and $hasClaim -and $info.Exits.Count -eq 0 -and -not $info.ExitAsIn) { Add-Issue $where 'scenario states console text but no exit code' }
    }
    Write-Host ('  {0}: {1} acceptance scenarios parsed' -f $s, $scs.Count)
}
Write-Host ('  {0} exit-code statements checked' -f $exitTotal)

# Page 2: each FR's stdout and stderr appear in the owning spec's scenario for that FR.
$page2 = @{}
foreach ($l in (Read-Lines $page2File)) {
    $mm = [regex]::Match($l, '^\| \*\*(FR-\d{2})\*\*<br>')
    if (-not $mm.Success) { continue }
    $id = $mm.Groups[1].Value
    $cells = $l.Split('|')
    $acc = $cells[2]
    $e = [pscustomobject]@{ Out = (New-Object System.Collections.Generic.List[string]); Err = (New-Object System.Collections.Generic.List[string]); OutEmpty = $false; OutHelp = $false; ErrEmpty = $false; RefBoth = $null; RefErr = $null; Exit = '0' }
    $mode = ''
    foreach ($seg in ($acc -split '<br>')) {
        if ($seg -match 'console output \(stdout\) and console messages \(stderr\) are exactly as for (FR-\d{2})') { $e.RefBoth = $Matches[1]; $mode = ''; continue }
        if ($seg -match '\*\*Exit code:\*\*\s*(\d+)') { $e.Exit = $Matches[1]; $mode = ''; continue }
        if ($seg -match '(?i)console output \(stdout\)') { $mode = 'out'; if ($seg -match '\(empty\)') { $e.OutEmpty = $true }; if ($seg -match '(?i)help text') { $e.OutHelp = $true }; continue }
        if ($seg -match 'holds exactly') { $mode = 'out'; continue }
        if ($seg -match '(?i)console messages \(stderr\)') {
            $mode = 'err'
            if ($seg -match '\(empty\)') { $e.ErrEmpty = $true }
            if ($seg -match 'console messages \(stderr\) of (FR-\d{2})') { $e.RefErr = $Matches[1]; $mode = '' }
            continue
        }
        if ($mode -ne '' -and $seg.TrimStart().StartsWith('`')) {
            $sp = Get-Spans $seg
            if ($sp.Count -gt 0) { if ($mode -eq 'out') { $e.Out.Add($sp[0]) } else { $e.Err.Add($sp[0]) } }
        } elseif ($mode -ne '' -and $seg -match '^\s*and the screen') { $mode = '' }
    }
    $page2[$id] = $e
}
foreach ($id in @($page2.Keys)) {
    $e = $page2[$id]
    if ($e.RefBoth) { $r = $page2[$e.RefBoth]; foreach ($x in $r.Out) { $e.Out.Add($x) }; foreach ($x in $r.Err) { $e.Err.Add($x) }; $e.OutEmpty = $r.OutEmpty; $e.ErrEmpty = $r.ErrEmpty }
    if ($e.RefErr) { $r = $page2[$e.RefErr]; foreach ($x in $r.Err) { $e.Err.Add($x) } }
}

function Test-Channel($info, [string]$channel, $expected, [bool]$expectEmpty) {
    $exp = @($expected)
    if ($expectEmpty -or $exp.Count -eq 0) {
        if ($channel -eq 'stdout') { return $info.EmptyOut } else { return $info.EmptyErr }
    }
    $fences = @($info.Fences | Where-Object { $_.Channel -eq $channel })
    foreach ($fe in $fences) {
        $fl = @($fe.Lines | ForEach-Object { $_.Text })
        if (($fl -join "`n") -eq ($exp -join "`n")) { return $true }
    }
    if ($fences.Count -gt 0) { return $false }
    # No fenced block for this channel: the lines must appear as spans, in order.
    $k = 0
    foreach ($sp in $info.Spans) { if ($k -lt $exp.Count -and $sp -eq $exp[$k]) { $k++ } }
    return ($k -eq $exp.Count)
}

$page2Ok = 0
foreach ($id in ($page2.Keys | Sort-Object)) {
    $e = $page2[$id]
    $owner = $null
    foreach ($s in $specIds) { if ($ownTable[$s] -contains $id) { $owner = $s } }
    $cands = @($allScenarios[$owner] | Where-Object { $_.Info.FRs -contains $id })
    if ($cands.Count -eq 0) { Add-Issue (Get-Rel (Spec-File $owner 'spec.md')) "no acceptance scenario is tagged with $id"; continue }
    $found = $false
    foreach ($c in $cands) {
        if ($e.OutHelp) { $okOut = $c.Info.Text -match '(?i)help text' } else { $okOut = Test-Channel $c.Info 'stdout' $e.Out $e.OutEmpty }
        $okErr = Test-Channel $c.Info 'stderr' $e.Err $e.ErrEmpty
        $okExit = $true
        if ($owner -ne '001-robot-rules') { $okExit = ($c.Info.Exits -contains $e.Exit) }
        if ($okOut -and $okErr -and $okExit) { $found = $true; break }
    }
    if ($found) { $page2Ok++ }
    else {
        $c0 = $cands[0]
        Add-Issue ('{0}:{1}' -f $c0.Sc.Rel, $c0.Sc.Start) ("no scenario tagged $id states page 2's console output (stdout) " + (Show ($e.Out -join ' | ')) + ', console messages (stderr) ' + (Show ($e.Err -join ' | ')) + ' and exit code ' + $e.Exit)
    }
}
Write-Host ('  page 2: {0} of {1} FR acceptance criteria found in the owning spec''s scenarios' -f $page2Ok, $page2.Count)
if ($page2.Count -ne 18) { Add-Issue 'docs/solution-design/02-functional-requirements.md' ('expected 18 FR rows, found {0}' -f $page2.Count) }
End-Check 'every scenario keeps REPORT results on stdout and everything else on stderr; exit codes 0/1/2 as DEC-04, DEC-10, DEC-32 and NFR-08 say; page 2 found for FR-01 to FR-18'

# =============================================================================================
# Check 4: Names
# =============================================================================================
Start-Check 'Check 4: Names (plans, data models and contracts use only page 6 names, decided code names, .NET/xUnit names and test classes)'

$page6Names = @('Program', 'Runner', 'CommandParser', 'IRobotCommand', 'Name', 'NeedsRobotOnTable', 'Execute', 'IRobotOperator', 'CurrentState', 'TryMoveTo',
    'PlaceCommand', 'MoveCommand', 'LeftCommand', 'RightCommand', 'ReportCommand', 'RobotOperator', 'Robot', 'RobotState', 'Direction', 'Table')
$page6Text = (Read-Lines (Join-Root 'docs/solution-design/06-components.md')) -join "`n"
foreach ($n in $page6Names) { if ($page6Text -notmatch ('\b' + $n + '\b')) { Add-Issue 'docs/solution-design/06-components.md' "page 6 does not name $n" } }

function Get-NameTokens([string]$s) {
    $out = New-Object System.Collections.Generic.List[string]
    foreach ($mm in [regex]::Matches($s, '`([^`]+)`')) {
        foreach ($t in [regex]::Matches($mm.Groups[1].Value, '\b[A-Z][A-Za-z0-9]*\b')) { if ($t.Value -cmatch '[a-z]' -or $t.Value.Length -eq 1) { $out.Add($t.Value) } }
    }
    return , $out
}
# Decided code names, read from the specs themselves.
$decided = New-Object System.Collections.Generic.HashSet[string]
$decidedSources = @()
$s1 = Spec-File '001-robot-rules' 'spec.md'
foreach ($o in $md[$s1]) {
    if ($o.Kind -eq 'text' -and $o.Text -match '^- (?:<a id="[^"]*"></a>)?\*\*Decided: code names of the parse result') {
        $chosen = $o.Text
        $ci = $chosen.IndexOf('Chosen:'); $oi = $chosen.IndexOf('Other options')
        if ($ci -ge 0) { if ($oi -gt $ci) { $chosen = $chosen.Substring($ci, $oi - $ci) } else { $chosen = $chosen.Substring($ci) } }
        foreach ($t in (Get-NameTokens $chosen)) { [void]$decided.Add($t) }
        $decidedSources += ('{0}:{1}' -f (Get-Rel $s1), $o.N)
    }
}
$r1 = Spec-File '001-robot-rules' 'research.md'
$inR4 = $false
foreach ($o in $md[$r1]) {
    if ($o.Kind -ne 'text') { continue }
    if ($o.Text -match '^## R4\.') { $inR4 = $true; $decidedSources += ('{0}:{1}' -f (Get-Rel $r1), $o.N); continue }
    if ($inR4 -and $o.Text -match '^## ') { $inR4 = $false }
    if ($inR4 -and $o.Text -match '^\s*\|') {
        $cells = $o.Text.Trim().Trim('|').Split('|')
        if ($cells.Count -ge 2) { foreach ($t in (Get-NameTokens $cells[1])) { [void]$decided.Add($t) } }
    }
}
$s2 = Spec-File '002-console-session' 'spec.md'
$spec002Refs = New-Object System.Collections.Generic.List[object]
foreach ($o in $md[$s2]) {
    if ($o.Kind -ne 'text') { continue }
    if ($o.Text -match '^- <a id="decided-help-text-in-program"></a>') {
        $chosen = $o.Text; $oi = $chosen.IndexOf('Other options'); if ($oi -gt 0) { $chosen = $chosen.Substring(0, $oi) }
        foreach ($t in (Get-NameTokens $chosen)) { [void]$decided.Add($t) }
        $decidedSources += ('{0}:{1}' -f (Get-Rel $s2), $o.N)
    }
    if ($o.Text -match '^- (?:<a id="[^"]*"></a>)?\*\*Decided in the run: method names of Runner and Program') {
        $chosen = $o.Text; $ci = $chosen.IndexOf('Chosen:'); $oi = $chosen.IndexOf('Other options')
        if ($ci -ge 0 -and $oi -gt $ci) { $chosen = $chosen.Substring($ci, $oi - $ci) }
        foreach ($t in (Get-NameTokens $chosen)) { [void]$decided.Add($t) }
        $decidedSources += ('{0}:{1}' -f (Get-Rel $s2), $o.N)
    }
    if ($o.Text -match '^- (?:<a id="[^"]*"></a>)?\*\*Decided: the Runner uses the code names fixed in spec 001') {
        $part = $o.Text; $ri = $part.IndexOf('Reason:'); if ($ri -gt 0) { $part = $part.Substring(0, $ri) }
        $spec002Refs.Add([pscustomobject]@{ N = $o.N; Tokens = (Get-NameTokens $part) })
    }
}
if ($decidedSources.Count -lt 3) { Add-Issue 'specs' ('could not find all three sources of decided names (spec 001 Assumptions, research R4, spec 002 Assumptions); found: ' + ($decidedSources -join ', ')) }
foreach ($r in $spec002Refs) {
    foreach ($t in $r.Tokens) { if (-not $decided.Contains($t) -and $page6Names -notcontains $t) { Add-Issue ('{0}:{1}' -f (Get-Rel $s2), $r.N) "spec 002 uses '$t' as a spec 001 code name, but spec 001 did not decide it" } }
}
$decidedList = @($decided | Sort-Object)
Write-Host ('  decided code names ({0}): {1}' -f $decidedList.Count, ($decidedList -join ', '))

# Test classes and helpers named in the tasks.
$helpers = New-Object System.Collections.Generic.HashSet[string]
foreach ($s in $specIds) {
    $t = (Read-Lines (Spec-File $s 'tasks.md')) -join "`n"
    # test helpers, and the deliberately broken source file that the CI-05 check writes
    foreach ($mm in [regex]::Matches($t, 'test helper `([A-Z]\w+)`|Create `([A-Z]\w+)` in `tests/|writes `src/ToyRobot/([A-Z]\w+)\.cs` there containing `#error')) {
        foreach ($g in 1, 2, 3) { if ($mm.Groups[$g].Success) { [void]$helpers.Add($mm.Groups[$g].Value) } }
    }
}
Write-Host ('  test helpers named in the tasks: ' + ((@($helpers) | Sort-Object) -join ', '))

$bclRoots = @('Console', 'Encoding', 'CultureInfo', 'NumberStyles', 'StringComparison', 'AppContext', 'Environment', 'File', 'Directory', 'Path', 'Process', 'Assert', 'Record',
    'System', 'Microsoft', 'Xunit', 'Math', 'Enum', 'Convert', 'String', 'Char', 'Int32', 'ArgumentNullException', 'Activator', 'Type')
$otherAllowed = @(
    # .NET base class library and C# types used in the plans
    'TextReader', 'TextWriter', 'StringReader', 'StringWriter', 'StreamReader', 'IReadOnlyList', 'IDisposable', 'IOException', 'InvalidOperationException', 'ArgumentNullException',
    'AssemblyConfigurationAttribute', 'ProcessStartInfo', 'XDocument', 'XElement', 'List', 'IEnumerable', 'Exception', 'ToString', 'Equals', 'GetHashCode', 'Trim', 'ReadLine',
    'WriteLine', 'Write', 'Split', 'TryParse', 'IsWhiteSpace', 'ThrowIfNull', 'NewLine', 'UseShellExecute', 'ArgumentList', 'StandardInput', 'BaseStream', 'StandardOutputEncoding',
    'StandardErrorEncoding', 'IsInputRedirected', 'OpenStandardInput', 'In', 'Out', 'Error', 'BaseDirectory', 'OrdinalIgnoreCase', 'InvariantCulture', 'AllowLeadingSign', 'UTF8',
    'CurrentCulture', 'CurrentUICulture', 'Dispose', 'Length', 'Count', 'Message',
    # xUnit
    'Fact', 'Theory', 'InlineData', 'MemberData',
    # MSBuild project settings, package names and build configurations
    'Exe', 'Release', 'Debug', 'LatestMajor', 'RollForward', 'TargetFramework', 'OutputType', 'Nullable', 'ImplicitUsings', 'RootNamespace', 'AssemblyName', 'IsPackable',
    'IsTestProject', 'ProjectReference', 'PackageReference', 'ItemGroup', 'InternalsVisibleTo', 'ReferenceOutputAssembly', 'Using', 'Include', 'Sdk', 'Test', 'NET',
    # project, namespace and folder names (page 5)
    'ToyRobot', 'Tests', 'Unit', 'Support', 'RunnerWholeFlow', 'ConsoleEndToEndTests')
$extensions = @('cs', 'sln', 'slnx', 'csproj', 'exe', 'md', 'txt', 'yml', 'json', 'sh', 'cmd', 'pdf')

function Test-NameChain([string]$chain, [string]$where) {
    $segs = $chain.Split('.')
    if ($segs.Count -gt 1 -and $extensions -contains $segs[-1]) { $segs = $segs[0..($segs.Count - 2)] }
    if ($bclRoots -contains $segs[0] -or $helpers.Contains($segs[0])) { return }
    foreach ($g in $segs) {
        if ($g -cnotmatch '^[A-Z]') { continue }
        if ($g.Length -gt 1 -and $g -cmatch '^[A-Z0-9_]+$') { continue }
        if ($g.Contains('_')) { continue }
        if ($page6Names -contains $g -or $decided.Contains($g) -or $otherAllowed -contains $g -or $helpers.Contains($g) -or $g -match 'Tests$') { continue }
        Add-Issue $where ("name '$g' (in '$chain') is not on page 6, not a decided code name, not a .NET/xUnit name and not a test class or helper")
    }
}
function Get-Chains([string]$s) {
    $s = [regex]::Replace($s, '"(?:[^"\\]|\\.)*"', '""')
    $s = [regex]::Replace($s, "'[^']*'", "''")
    return , @([regex]::Matches($s, '[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*') | ForEach-Object { $_.Value })
}
$nameFiles = New-Object System.Collections.Generic.List[string]
foreach ($s in $specIds) {
    $nameFiles.Add((Spec-File $s 'plan.md')); $nameFiles.Add((Spec-File $s 'data-model.md'))
    foreach ($c in (Get-ChildItem -LiteralPath (Join-Root "specs/$s/contracts") -Filter '*.md' | Sort-Object Name)) { $nameFiles.Add($c.FullName) }
}
$skipSpan = '^\s*(?i:place|move|left|right|report|exit)\b|^(?:Output:|Warning:|How to run:|> |\[TAB\]|-)'
$namesChecked = 0
foreach ($f in $nameFiles) {
    $rel = Get-Rel $f
    foreach ($o in $md[$f]) {
        $where = '{0}:{1}' -f $rel, $o.N
        $texts = New-Object System.Collections.Generic.List[string]
        if ($o.Kind -eq 'text') { foreach ($sp in (Get-Spans $o.Text)) { if ($sp -notmatch $skipSpan) { $texts.Add($sp) } } }
        elseif ($o.Kind -eq 'code' -and $o.Lang -eq 'csharp') { $texts.Add(($o.Text -replace '//.*$', '')) }
        foreach ($mm in [regex]::Matches($o.Text, '\b(?:class|interface|record)\s+([A-Z]\w*)')) { $texts.Add($mm.Groups[1].Value) }
        foreach ($t in $texts) { foreach ($c in (Get-Chains $t)) { $namesChecked++; Test-NameChain $c $where } }
    }
}
Write-Host ('  {0} identifier chains checked in {1} plan, data-model and contract files' -f $namesChecked, $nameFiles.Count)

# Decided names used the same way in all three specs: Type.Member must be a member that spec 001
# (or spec 002 for Runner and Program) decided.
$members = @{
    'ParseResult' = @('CommandObject', 'Exit', 'Failure'); 'Outcome' = @('PositionText', 'Warning', 'Done'); 'CommandParser' = @('Parse')
    'RobotOperator' = @('Run', 'CurrentState', 'TryMoveTo'); 'Robot' = @('Placement', 'MoveTo'); 'Table' = @('Contains', 'Width', 'Height')
    'Direction' = @('North', 'East', 'South', 'West', 'All', 'Name', 'TurnLeft', 'TurnRight', 'ToString'); 'RobotState' = @('X', 'Y', 'Facing', 'ToString')
    'IRobotCommand' = @('Name', 'NeedsRobotOnTable', 'Execute'); 'IRobotOperator' = @('CurrentState', 'TryMoveTo'); 'Runner' = @('Run'); 'Program' = @('Main', 'Run')
    'PlaceCommand' = @('X', 'Y', 'F', 'Name', 'NeedsRobotOnTable', 'Execute')
}
foreach ($k in $members.Keys) { foreach ($mbr in $members[$k]) { if ($mbr -ne 'ToString' -and -not $decided.Contains($mbr) -and $page6Names -notcontains $mbr) { Add-Issue 'specs' "member $k.$mbr is used by this check but was not decided in the specs" } } }
$qualified = 0
foreach ($f in $mdFiles) {
    $rel = Get-Rel $f
    foreach ($o in $md[$f]) {
        if ($o.Kind -eq 'text') { $srcs = Get-Spans $o.Text } elseif ($o.Kind -eq 'code' -and $o.Lang -eq 'csharp') { $srcs = @($o.Text) } else { continue }
        foreach ($sp in $srcs) {
            $clean = [regex]::Replace($sp, '"(?:[^"\\]|\\.)*"', '""')
            foreach ($mm in [regex]::Matches($clean, '(?<![\w.])(ParseResult|Outcome|CommandParser|RobotOperator|Robot|Table|Direction|RobotState|IRobotCommand|IRobotOperator|Runner|Program|PlaceCommand)\.([A-Za-z_]\w*)')) {
                if ($extensions -contains $mm.Groups[2].Value) { continue }
                $qualified++
                if ($members[$mm.Groups[1].Value] -notcontains $mm.Groups[2].Value) {
                    Add-Issue ('{0}:{1}' -f $rel, $o.N) ("'{0}.{1}' is not a decided member of {0} (decided: {2})" -f $mm.Groups[1].Value, $mm.Groups[2].Value, ($members[$mm.Groups[1].Value] -join ', '))
                }
            }
        }
    }
}
Write-Host ('  {0} qualified uses of spec 001/002 type members checked across all spec files' -f $qualified)
End-Check 'only page 6 names, decided code names, .NET/xUnit names and test classes/helpers; decided names agree across the three specs'

# =============================================================================================
# Check 5: DEC-31 coverage
# =============================================================================================
Start-Check 'Check 5: DEC-31 coverage (every "Must be covered" case is matched inside a test task)'

$dec31 = (Get-DecSection 'DEC-31') -join "`n"
if ($dec31 -notmatch 'Must be covered:') { Add-Issue 'docs/design-analysis/design-decisions.md' 'DEC-31 has no "Must be covered" list' }

# Test tasks: "- [ ] Tnnn" blocks (with their indented continuation lines) that write tests in a
# *Tests.cs file or add a CI check (CI-0n) to the workflow.
$tasks = New-Object System.Collections.Generic.List[object]
foreach ($s in $specIds) {
    $f = Spec-File $s 'tasks.md'; $rel = Get-Rel $f
    $lines = Read-Lines $f
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $mm = [regex]::Match($lines[$i], '^- \[[ xX]\] (T\d{3}[a-z]?)\b(.*)$')
        if (-not $mm.Success) { continue }
        $text = $mm.Groups[2].Value
        for ($j = $i + 1; $j -lt $lines.Count; $j++) {
            if ($lines[$j] -match '^\s+\S') { $text += "`n" + $lines[$j] } else { break }
        }
        $desc = ($text -replace '^\s*(\[P\]\s*)?(\[US\d\]\s*)?', '')
        $isTest = ($text -match '\w+Tests\.cs\b') -or ($text -match 'ci\.yml' -and $text -match '\bCI-0\d')
        if ($desc -match '^(Implement|Create the test helper|Review|Confirm|Run|Commit|Push|Check|Follow|Compare|Replace|Extend|Complete)\b') { $isTest = $false }
        if ($isTest) { $tasks.Add([pscustomobject]@{ Id = ('{0} {1}' -f $s.Substring(0, 3), $mm.Groups[1].Value); Where = ('{0}:{1}' -f $rel, ($i + 1)); Text = $text }) }
    }
}
Write-Host ('  {0} test tasks found' -f $tasks.Count)

# Each case: alternatives; an alternative is one or more regexes (joined by " && ") that must all
# match inside the same test task.
$cases = @(
    @{ N = 'Challenge Example 1'; A = @('(?i)Challenge[ _]Example[ _]1\b') }
    @{ N = 'Challenge Example 2'; A = @('(?i)Challenge[ _]Example[ _]2\b') }
    @{ N = 'Challenge Example 3'; A = @('(?i)Challenge[ _]Example[ _]3\b') }
    @{ N = 'Brainstorm example 2,1,NORTH'; A = @('PLACE 0,0,EAST|US2-5 && `2,1,NORTH`') }
    @{ N = 'Brainstorm blocked move, page 3'; A = @('robot stays at 4,1,EAST') }
    @{ N = 'Brainstorm blocked move, page 5'; A = @('PLACE 0,0,SOUTH && robot stays at 0,0,SOUTH') }
    @{ N = 'Brainstorm example 0,3,WEST'; A = @('`0,3,WEST`') }
    @{ N = 'north edge'; A = @('`2,4,NORTH` && fall off|off the table') }
    @{ N = 'south edge'; A = @('`2,0,SOUTH` && fall off|off the table') }
    @{ N = 'east edge'; A = @('`4,2,EAST` && fall off|off the table') }
    @{ N = 'west edge'; A = @('`0,2,WEST` && fall off|off the table') }
    @{ N = 'south west corner'; A = @('`0,0,SOUTH` && `0,0,WEST` && fall off|off the table') }
    @{ N = 'north west corner'; A = @('`0,4,NORTH` && `0,4,WEST` && fall off|off the table') }
    @{ N = 'north east corner'; A = @('`4,4,NORTH` && `4,4,EAST` && fall off|off the table') }
    @{ N = 'south east corner'; A = @('`4,0,SOUTH` && `4,0,EAST` && fall off|off the table') }
    @{ N = 'full LEFT cycle'; A = @('full LEFT cycle', 'four `TurnLeft\(\)` calls') }
    @{ N = 'full RIGHT cycle'; A = @('full RIGHT cycle', 'four `TurnRight\(\)` calls') }
    @{ N = 'a small table such as 2 x 2'; A = @('Table\(2, 2\)|width: 2, height: 2') }
    @{ N = 'MOVE before PLACE'; A = @('Robot_not_on_table_ignores_MOVE') }
    @{ N = 'LEFT before PLACE'; A = @('Robot_not_on_table_ignores_LEFT') }
    @{ N = 'RIGHT before PLACE'; A = @('Robot_not_on_table_ignores_RIGHT') }
    @{ N = 'REPORT before PLACE'; A = @('Robot_not_on_table_ignores_REPORT') }
    @{ N = 'an invalid first PLACE'; A = @('Place_off_the_table_is_ignored_when_not_on_table', 'invalid first PLACE && REPORT ignored - robot is not on the table') }
    @{ N = 'an invalid later PLACE'; A = @('Place_off_the_table_is_ignored_when_on_table') }
    @{ N = 'a second valid PLACE'; A = @('A_second_valid_PLACE_moves_the_robot') }
    @{ N = 'a negative position'; A = @('PLACE -1,0,NORTH ignored - position is off the table') }
    @{ N = 'several REPORTs in one run'; A = @('several REPORTs && `0,0,NORTH`, `0,1,NORTH`') }
    @{ N = 'lower case'; A = @('`place 1,2,north`|`move`|`report`') }
    @{ N = 'mixed case'; A = @('`mOvE`|`rEpOrT`|`Place 1,1,East`') }
    @{ N = '`north` (direction in lower case)'; A = @('`place [^`]*\bnorth\b[^`]*`') }
    @{ N = 'REPORT in upper case after lower-case input'; A = @('`place 1,2,north`, `report` . `1,2,NORTH`', '`Place 1,1,East`, `mOvE`, `rEpOrT` . `2,1,EAST`') }
    @{ N = 'DEC-08 row `   MOVE   `'; A = @('`   MOVE   `') }
    @{ N = 'DEC-08 row `PLACE     0,0,NORTH`'; A = @('`PLACE     0,0,NORTH`') }
    @{ N = 'DEC-08 row `place 0 , 0 , north`'; A = @('`place 0 , 0 , north`') }
    @{ N = 'DEC-08 row `PLACE 01,+1,EAST`'; A = @('`PLACE 01,\+1,EAST`') }
    @{ N = 'DEC-08 row `PLACE 0 0 NORTH`'; A = @('`PLACE 0 0 NORTH` && expected PLACE X,Y,F') }
    @{ N = 'DEC-08 row `PLACE0,0,NORTH`, `PLACEHOLDER`, `PLACE,1,2`'; A = @('`PLACE0,0,NORTH` && `PLACEHOLDER` && `PLACE,1,2` && unknown command') }
    @{ N = 'DEC-08 row `PLACE 1 0,0,NORTH`'; A = @('`PLACE 1 0,0,NORTH` && X must be a whole number') }
    @{ N = 'DEC-08 row `PLACE 1,2,`'; A = @('`PLACE 1,2,` && F must be NORTH, SOUTH, EAST or WEST') }
    @{ N = 'DEC-08 row `PLACE 1,2,NORTH EXTRA`'; A = @('`PLACE 1,2,NORTH EXTRA` && F must be NORTH, SOUTH, EAST or WEST') }
    @{ N = 'tabs as whitespace'; A = @('\[TAB\]|\\t') }
    @{ N = 'non-breaking spaces as whitespace'; A = @('\[NBSP\]|<U\+00A0>') }
)
foreach ($t in $dec22Texts) {
    $r = $t.Substring('Warning: '.Length)
    $cases += @{ N = ('DEC-22 warning ' + (Show $r)); A = @([regex]::Escape($r)) }
}
$cases += @(
    @{ N = '`> MOVE`'; A = @([regex]::Escape("'> MOVE' ignored - unknown command")) }
    @{ N = '`MOVE now`'; A = @([regex]::Escape("'MOVE now' ignored - MOVE takes no values")) }
    @{ N = '`PLACEHOLDER`'; A = @('`PLACEHOLDER` && unknown command') }
    @{ N = '`PLACE 99999999999,0,NORTH`'; A = @([regex]::Escape("'PLACE 99999999999,0,NORTH' ignored - X is too large")) }
    @{ N = 'abbreviation `PLACE 0,0,N`'; A = @('`PLACE 0,0,N` && F must be NORTH, SOUTH, EAST or WEST') }
    @{ N = 'blank lines'; A = @('blank line ignored && ""|empty string') }
    @{ N = 'off-table warning with parsed values'; A = @('`place 09 , \+9 , north` && PLACE 9,9,NORTH ignored - position is off the table') }
    @{ N = 'EXIT before PLACE'; A = @('EXIT_before_PLACE') }
    @{ N = 'EXIT mid-file (later lines neither echoed nor run)'; A = @('EXIT_mid_file && `MOVE`, `REPORT`') }
    @{ N = '`EXIT now` exits'; A = @('`EXIT now` && Exit\(\)|ends_session') }
    @{ N = '`EXITNOW` warns'; A = @('EXITNOW && unknown command') }
    @{ N = 'prompt shown when interactive'; A = @('interactive: true && "> ') }
    @{ N = 'no prompt otherwise'; A = @('No_prompt_when_input_is_redirected && interactive: false') }
    @{ N = 'echo content with redirected input'; A = @('Echo_removes_whitespace_of_any_kind_from_both_ends') }
    @{ N = 'an empty input'; A = @('Empty_input && stdout `""`') }
    @{ N = 'byte-order mark (console end-to-end)'; A = @('ToyRobot\.ConsoleEndToEndTests && `EF BB BF` && `0,1,NORTH`') }
    @{ N = 'how-to-run message (console end-to-end)'; A = @('ToyRobot\.ConsoleEndToEndTests && ' + [regex]::Escape($howToRunMessage) + ' && exit code 1') }
    @{ N = 'exit code 1 (console end-to-end)'; A = @('ToyRobot\.ConsoleEndToEndTests && exit code 1') }
    @{ N = 'exit code 0 at end of input (console end-to-end)'; A = @('ToyRobot\.ConsoleEndToEndTests && Exit_code_0_at_end_of_input && exit code 0') }
    @{ N = 'exit code 0 after EXIT (console end-to-end)'; A = @('ToyRobot\.ConsoleEndToEndTests && Exit_code_0_after_EXIT && exit code 0') }
    @{ N = 'the unexpected-problem line and exit code 2 (DEC-32)'; A = @([regex]::Escape($errorPrefix) + ' && exit code 2') }
)
$covered = 0
foreach ($c in $cases) {
    $hits = New-Object System.Collections.Generic.List[string]
    foreach ($t in $tasks) {
        $ok = $false
        foreach ($alt in @($c.A)) {
            $all = $true
            foreach ($re in ($alt -split ' && ')) { if (-not [regex]::IsMatch($t.Text, $re)) { $all = $false; break } }
            if ($all) { $ok = $true; break }
        }
        if ($ok) { $hits.Add($t.Id) }
    }
    if ($hits.Count -gt 0) { $covered++; Write-Host ('  covered  {0}: {1}' -f $c.N, ($hits -join ', ')) }
    else { Write-Host ('  MISSING  {0}' -f $c.N); Add-Issue 'specs/*/tasks.md' ('no test task covers DEC-31 case: ' + $c.N) }
}
Write-Host ('  {0} of {1} DEC-31 cases covered' -f $covered, $cases.Count)
End-Check ('all {0} DEC-31 cases have a planned test task' -f $cases.Count)

# =============================================================================================
# Check 6: Analyze (not scripted)
# =============================================================================================
Write-Host ''
Write-Host 'Check 6: Analyze'
Write-Host '  NOTE: not scriptable. Run /speckit-analyze for each spec separately; it must report no CRITICAL or HIGH issue.'

# =============================================================================================
# Check 7: No code yet
# =============================================================================================
# The specification phase runs from the commit that adds the Spec Kit brief (f9e10c0) to the
# commit that adds this script (a374aaa); the code is added after it.
$specStart = 'f9e10c0'
$specEnd = 'a374aaa'
Start-Check "Check 7: No code (git diff --name-only $specStart $specEnd has nothing under src/ or tests/)"
$diff = & git -C $root diff --name-only $specStart $specEnd 2>&1
if ($LASTEXITCODE -ne 0) { Add-Issue 'git' ('git diff failed: ' + ($diff -join ' ')) }
else {
    $code = @($diff | Where-Object { $_ -match '^(src|tests)/' })
    foreach ($c in $code) { Add-Issue $c "file under src/ or tests/ changed in the specification phase ($specStart..$specEnd)" }
    Write-Host ('  {0} files changed in the specification phase, {1} under src/ or tests/' -f @($diff).Count, $code.Count)
}
End-Check 'no file under src/ or tests/ changed in the specification phase'

# =============================================================================================
Write-Host ''
Write-Host '======================================'
Write-Host ('Summary: {0} of {1} scripted checks passed (check 6 is run separately with /speckit-analyze)' -f $script:passed, ($script:passed + $script:failed))
if ($script:failed -gt 0) { Write-Host 'Status: FAILED'; exit 1 }
Write-Host 'Status: ALL SCRIPTED CHECKS PASSED'
exit 0
