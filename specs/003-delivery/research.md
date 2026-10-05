# Research: Delivery

Phase 0 of the plan. The Technical Context had no open items: the technology is fixed by pages 5 and 9 and brief rule 7. These are the decisions on how to build the delivery artefacts within those choices. Each one that the sources do not settle is also in the spec's Assumptions.

## R-01 .NET SDK check (NFR-08 step 1)

- **Decision**: run `dotnet --list-sdks` with its output captured. The check passes when at least one line starts with a whole number of 8 or more before its first dot (for example `8.0.414 [...]`, `9.0.100 [...]`). Lines that do not start with a whole number are skipped. If `dotnet` cannot be found, or no line passes, write to console messages (stderr) a message naming `https://dotnet.microsoft.com/download` and exit with code 1.
  - `run.sh`: uses only bash built-ins before and during this check (`command -v dotnet`, `[[ ]]`/`case`, parameter expansion, `read`, `printf ... >&2`), so the check works even with a `PATH` that holds nothing ([CI-04][ci-04] a).
  - `run.cmd`: `for /f "tokens=1 delims=." %%v in ('dotnet --list-sdks 2^>nul')`, keeping only tokens that are whole numbers before comparing with `GEQ 8`; a missing `dotnet` gives no lines.
- **Rationale**: [DEC-37][dec-37] asks for "a .NET SDK, version 8 or later". `--list-sdks` lists every installed SDK and is answered by the `dotnet` host itself.
- **Alternatives considered**: `dotnet --version` (reports only the SDK chosen for the folder; fails with a long text when no SDK is installed); checking only that `dotnet` exists (does not check the version).

## R-02 Quiet build (NFR-08 step 2)

- **Decision**: `dotnet build "<script folder>/src/ToyRobot/ToyRobot.csproj" -c Release --nologo`, with console input from the null device (`< /dev/null`, `< NUL`) and all output (stdout and stderr) captured: in `run.sh` into a variable; in `run.cmd` into a temporary log file in `%TEMP%` with a random name, which the script removes after use. Exit code 0: show nothing. Any other exit code: write the captured output to console messages (stderr) and exit with that code.
  - `run.cmd` exit-code rules: cmd expands `%VAR%` when a parenthesised block or a `for` body is parsed, so `%ERRORLEVEL%` inside a block is the value from before the block. Save the build's code on the line straight after `dotnet build` (`set "rc=%ERRORLEVEL%"`) and use `exit /b %rc%` after `type` and `del` (they change `%ERRORLEVEL%`).
- **Rationale**: [DEC-37][dec-37] "builds the app quietly; if the build fails, shows the build output and exits with its exit code". Capturing also hides the SDK's first-use welcome text. Giving the build no console input keeps a redirected file for the application (spec scenario 5a). Building only the application project matches page 5's Release path and keeps a reviewer's run independent of the test projects.
- **Alternatives considered**: `-v quiet` without capture (still prints some lines, and they would not be shown on failure in full); building `ToyRobot.sln` (slower; builds test projects).

## R-03 Arguments and starting the application (NFR-08 steps 3 and 4)

- **Decision**:
  - Script folder: `run.cmd` uses `%~dp0`; `run.sh` takes the folder part of `$0` with parameter expansion (`.` if `$0` has no `/`). The application is `<script folder>\src\ToyRobot\bin\Release\net8.0\ToyRobot.exe` (Windows) or `<script folder>/src/ToyRobot/bin/Release/net8.0/ToyRobot` (Linux and macOS), page 5.
  - No argument: start the application with the script's own console input (it is typed, or redirected if the script's input is redirected).
  - One argument `--help` (exact match; `[ "$1" = "--help" ]`; in cmd `if "%~1"=="--help"`, which is case-sensitive because it has no `/i`): start the application with `--help` and console input from `/dev/null` (`run.sh`) or `nul` (`run.cmd`), write no "Running" line, and exit with its exit code ([DEC-04][dec-04], [DEC-37][dec-37]; spec "Decided: `--help` in the run scripts").
  - Any other one argument: it must be an existing file (`[ -f "$1" ]`; in cmd `exist "%~1"` and not `exist "%~1\*"`). Otherwise write a "file not found" message naming the argument to console messages (stderr) and exit 1. If it is a file, start the application with that file redirected to its console input (stdin), never as an argument.
  - `run.cmd` only (developer request 2026-10-05): an argument containing `*` or `?` is rejected with `The file name must not contain * or ?: "<arg>"` on console messages (stderr) and exit 1, after the argument-count and no-argument checks and before the `exist` checks. Detection without echoing or piping the argument: `set "wild=1"` then `for /f "eol=| delims=*?" %%a in ("%~1") do if "%%a"=="%~1" set "wild="` (the first token equals the whole argument only when it holds no `*` or `?`; `|` cannot be in a Windows file name, so `eol=|` never cuts the line). The script starts with `setlocal DisableDelayedExpansion` so `!` in a file name is not mangled when the registry turns delayed expansion on. `run.sh` needs nothing: `[ -f "$1" ]` is false for a quoted `*`, giving file-not-found and exit 1.
  - More than one argument: write the script's how-to-run message (for example `How to run: run testdata\challenge-example-1.txt to run the commands in a file, or run with no file for the menu. In PowerShell, use .\run.cmd instead of run.` / `How to run: ./run.sh testdata/challenge-example-1.txt to run the commands in a file, or ./run.sh with no file for the menu.`) to console messages (stderr) and exit 1. In `run.cmd` "more than one" means `"%~2"` is not empty.
  - Exit with the application's exit code (`exit /b %ERRORLEVEL%`; `exit $?`). In `run.cmd` the application is started on a line outside any parenthesised block (`goto` labels for the no-argument and file cases, not `if`/`else` blocks), and `exit /b %ERRORLEVEL%` is the next line, so the code is the application's.
  - The same rule applies to the CI steps that run `run.cmd` (spec 003 tasks T015, T017, T018, T019): each exit code is saved on the line straight after the command, outside any block; a loop over names calls a subroutine (`call :run <name>`) or uses `setlocal EnableDelayedExpansion` and `!ERRORLEVEL!`.
- **Rationale**: [DEC-37][dec-37] and page 9 steps 3 and 4; the order SDK check, build, arguments is the order [DEC-37][dec-37] and page 9 list. Wording of the messages is not fixed (page 12 question A).
- **Alternatives considered**: checking the arguments before the build (a different order from [DEC-37][dec-37] and page 9); passing the file name to the application (the application takes no arguments, [DEC-04][dec-04]).

## R-04 Script channels on success

- **Decision**: on success a script writes only its three progress lines of its own (the .NET check, the build, then the running line; spec Clarifications, session 2026-10-05 (progress lines)), and no build output. Every script message goes to console messages (stderr) (`1>&2`, `>&2`); console output (stdout) is only the application's. The file name in the running line is echoed so that cmd does not parse it again (a `for` variable in `run.cmd`, `printf '%s'` in `run.sh`), so characters such as `&` stay text.
- **Rationale**: [DEC-37][dec-37] "writes its own messages (and any build output) to standard error", and [DEC-23][dec-23] keeps console output (stdout) for the REPORT output only; spec scenario 1 then has the three progress lines followed by the application's echo lines on console messages (stderr). The progress lines were added at the developer's request ([P141][p141]).

## R-05 CI workflow shape (NFR-02)

- **Decision**: one workflow `.github/workflows/ci.yml`, named `CI`, triggered `on: push` (every branch). One job with `timeout-minutes: 30` (a hanging step fails that system within 30 minutes, not after the 360-minute default) and a matrix `os: [windows-latest, ubuntu-latest, macos-latest]` and `fail-fast: false`, so each system reports its own result. Steps in the order of the page 9 test flow:
  1. `actions/checkout@v5`; `actions/setup-dotnet@v5` with `dotnet-version: 8.0.x` (if a `@v5` tag is not available when implemented, the latest major of the same action).
  2. `dotnet build ToyRobot.sln`.
  3. `dotnet test tests/ToyRobot.Tests --no-build` (unit and Runner whole-flow tests).
  4. `dotnet test tests/ToyRobot.ConsoleEndToEndTests --no-build` (console end-to-end tests, including [E2E-01][e2e-01] over every pair, [TD-02][td-02], [BS-01][bs-01]).
  5. [CI-06][ci-06] line endings and `run.sh` mode.
  6. [CI-01][ci-01] and [CI-03][ci-03] (scripts with the test data), then the comparison.
  7. [CI-02][ci-02], [CI-04][ci-04], [CI-05][ci-05] (script error paths).
- **Rationale**: [DEC-33][dec-33] and page 9 (test flow, "Cross-platform and CI"). `fail-fast: false` keeps evidence for all three systems when one fails; a failed step still fails that system and the run.
- **Alternatives considered**: `pull_request` as a trigger as well (not asked for); one `dotnet test ToyRobot.sln` step (works, but does not show the two levels as page 9's test flow does).

## R-06 Shells and comparison in CI

- **Decision**: on Windows the scripts run from cmd (`shell: cmd`, `call run.cmd ...`); on Linux and macOS from bash (`./run.sh ...`). Console output (stdout) is saved to files under `$RUNNER_TEMP/out/` (`%RUNNER_TEMP%\out\`). The comparison is a separate `shell: bash` step on every system (Git Bash on `windows-latest`): for each pair, `diff` of the saved file and the expected file, both with carriage returns removed (`tr -d '\r'`), so Windows and Linux line endings both pass ([DEC-31][dec-31]). Exit codes are written next to the outputs and checked in the same bash step.
- **Rationale**: Windows PowerShell 5.1 `>` writes UTF-16 ([DEC-23][dec-23]), so cmd is used for saving; one comparison step for all systems keeps it in one place. GitHub runs bash steps with `-e -o pipefail`, so commands that are expected to fail are run with their exit code captured (`set +e` around them, or `|| code=$?`).
- **Alternatives considered**: `fc` on Windows (its line-ending handling is not documented clearly enough to rely on); PowerShell comparison (a third shell for no gain).

## R-07 CI-04: no .NET SDK, in a way that does not depend on the runner image

- **Decision**:
  - (a) `dotnet` not found: Linux and macOS: `PATH="$RUNNER_TEMP/empty" /bin/bash ./run.sh testdata/challenge-example-1.txt` (an empty folder; `/bin/bash` started by its full path because `run.sh`'s `#!/usr/bin/env bash` line needs `PATH`). Windows: `set "PATH=%SystemRoot%\System32"` then `call run.cmd testdata\challenge-example-1.txt` (the SDK is not in `System32` on `windows-latest`).
  - (b) only an older SDK: a stand-in `dotnet` in `$RUNNER_TEMP/old-sdk` that prints `7.0.410 [/fake/sdk]` for any arguments (Linux and macOS: an executable `#!/bin/sh` script, run with `PATH="$RUNNER_TEMP/old-sdk:/usr/bin:/bin"`; Windows: `dotnet.cmd` printing `7.0.410 [C:\fake\sdk]`, with `PATH=%RUNNER_TEMP%\old-sdk;%SystemRoot%\System32`).
  - Each must exit with code 1 and leave console output (stdout) empty.
- **Rationale**: page 3 "no .NET SDK ... gives 1", tested on all three systems without removing the real SDK.
- **Alternatives considered**: uninstalling or renaming the SDK on the runner (fragile, differs by image).

## R-08 CI-05: failed build in a temporary copy

- **Decision**: a bash step makes the copy: `broken=$(cd "$RUNNER_TEMP/broken" && pwd)` then `git archive --format=tar HEAD | tar -x -C "$broken"` (tar on `windows-latest` rejects the Windows form of `$RUNNER_TEMP`) (committed files only, with checkout line endings, so `run.cmd` keeps CRLF and `run.sh` its mode), then writes `src/ToyRobot/CiBuildBreaker.cs` in the copy containing `#error CI-05 makes this build fail on purpose` (compiler error CS1029). The script then runs in the copy from that system's shell with `testdata/challenge-example-1.txt`, saving console output (stdout), console messages (stderr) and its exit code; then `dotnet build src/ToyRobot/ToyRobot.csproj -c Release --nologo` runs in the copy and its exit code is saved. Checks: both exit codes are equal and not 0; saved console output (stdout) is empty; saved console messages (stderr) contain `CS1029`.
- **Rationale**: page 3 "a failed build gives the build's exit code", tested without changing the checkout.
- **Alternatives considered**: breaking a file in the checkout and restoring it (risks later steps and the working copy).

## R-09 CI-06: line endings and executable mode

- **Decision**: a bash step reads `git ls-files --eol` and `git ls-files -s run.sh`: every file whose working-copy column is not `w/-text` must have `w/crlf` if its name ends in `.cmd`, and `w/lf`, `w/none` or an empty value otherwise; `run.sh` must have mode `100755`. Any other value prints the file and fails the step.
- **Rationale**: [DEC-33][dec-33] states the rules; this makes them checked on every system after a real checkout.

## R-10 `.gitattributes` content and the executable bit

- **Decision**:
  ```text
  * text=auto eol=lf
  *.cmd text eol=crlf
  *.pdf binary
  ```
  After adding it, `git add --renormalize .` is run and any file it changes is committed in the same commit (today every text file is already stored with LF in the repository, so no content changes are expected). `run.sh` is marked executable with `git update-index --chmod=+x run.sh`, because Windows does not keep the mode on disk.
- **Rationale**: [DEC-33][dec-33] "LF for all text files, CRLF for `.cmd` files"; `text=auto` leaves binary files (the PDFs) alone, and `*.pdf binary` makes that explicit. A byte-order mark is not touched by line-ending conversion, so `bom-challenge-example-1.txt` keeps it.

## R-11 `.gitignore`

- **Decision**: the root `.gitignore` is created by spec 001 (SK skeleton) with `bin/`, `obj/`, `TestResults/` and `.vs/`; this spec only checks it (task T002): it holds those four lines and `git status` lists no build output. A missing line is added; nothing is removed.
- **Rationale**: the scripts and CI build into `bin/` and `obj/`; without the file, build output could be committed. It adds no behaviour. Each requirement is owned by one spec, so the file is owned by spec 001 and checked here.
- **Alternatives considered**: no `.gitignore` (build output appears as untracked files after every run of a script).

## R-12 Test-data files and the TD-02 and BS-01 tests

- **Decision**:
  - Files are UTF-8 without byte-order mark, LF line endings, each line ending with a line break (including the last); the empty pair is two files of zero bytes. `bom-challenge-example-1.txt` is written as bytes (`EF BB BF` then the text of `challenge-example-1.txt`), not with an editor.
  - `tests/ToyRobot.ConsoleEndToEndTests/DeliveryTests.cs` holds:
    - [TD-02][td-02]: `Test_data_holds_every_input_and_expected_pair` (a theory over the six names, checking both files exist) and `Byte_order_mark_test_file_starts_with_the_mark` (first three bytes `EF BB BF`).
    - [BS-01][bs-01]: `Application_targets_net8_and_rolls_forward_to_the_latest_major_version`, reading `src/ToyRobot/ToyRobot.csproj` with `System.Xml.Linq` and checking `TargetFramework` = `net8.0` and `RollForward` = `LatestMajor`.
  - The tests find the repository root by walking up from `AppContext.BaseDirectory` to the folder holding `ToyRobot.sln` (the same rule as spec 002's AppProcess, which exposes no shared helper).
- **Rationale**: [TD-01][td-01], [TD-02][td-02], [BS-01][bs-01]; `System.Xml.Linq` is part of .NET, so no library is added (brief rule 7).

[bs-01]: spec.md#bs-01
[ci-01]: spec.md#ci-01
[ci-02]: spec.md#ci-02
[ci-03]: spec.md#ci-03
[ci-04]: spec.md#ci-04
[ci-05]: spec.md#ci-05
[ci-06]: spec.md#ci-06
[dec-04]: ../../docs/design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-31]: ../../docs/design-analysis/design-decisions.md#dec-31-testing
[dec-33]: ../../docs/design-analysis/design-decisions.md#dec-33-cross-platform
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[e2e-01]: ../002-console-session/spec.md#e2e-01
[p141]: ../../docs/design-analysis/prompts-implementation-phase.md#p141
[td-01]: spec.md#td-01
[td-02]: spec.md#td-02
