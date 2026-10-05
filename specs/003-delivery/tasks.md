---

description: "Tasks for spec 003-delivery: run scripts, test data, CI workflow, .gitattributes, README"
---

# Tasks: Delivery

**Input**: Design documents from `specs/003-delivery/` (spec.md, plan.md, research.md, data-model.md, contracts/, quickstart.md)

**Prerequisites**: specs 001-robot-rules and 002-console-session fully implemented, with `dotnet build` and `dotnet test` passing at the repository root (T001). This spec is implemented third (order 001, 002, 003).

**Tests**: Required (constitution II; Spec Kit brief rule 8). In every phase the tests are written before the files that make them pass, and the phase ends with all its tests passing. For the run scripts and the CI workflow the tests are CI steps ([CI-01][ci-01] to [CI-06][ci-06]), so "passing" means passing locally where a check can run on Windows and in the `CI` workflow on `windows-latest`, `ubuntu-latest` and `macos-latest`.

**Organization**: by user story (spec.md). US1 and US2 are both P1; US1 (scripts) comes first because [CI-01][ci-01] to [CI-05][ci-05] need the scripts, and the CI skeleton they run in is foundational.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: can run in parallel (different files, no dependency on an unfinished task)
- **[Story]**: US1 to US4, as in spec.md
- Every task cites its requirement (NFR, TD, CI, BS) and DEC IDs

---

## Phase 1: Setup

**Purpose**: confirm what this spec builds on; check that build output stays out of the repository.

- [x] T001 Confirm the dependencies on specs 001 and 002: from the repository root run `dotnet build` and `dotnet test`; both succeed, `ToyRobot.sln`, `src/ToyRobot/ToyRobot.csproj` (with `net8.0` and `RollForward` = `LatestMajor`, spec 001 [SK-02][sk-02]), `tests/ToyRobot.Tests/` and `tests/ToyRobot.ConsoleEndToEndTests/` exist, and the console end-to-end project holds spec 002's [E2E-01][e2e-01] (runs every `testdata/<name>.txt` that has a `testdata/<name>.expected.txt`). If any of this is missing, stop: spec 003 must not start (plan "Dependencies on the other specs") ([NFR-02][nfr-02], [NFR-08][nfr-08]; [DEC-31][dec-31], [DEC-37][dec-37]).
- [x] T002 Check the `.gitignore` that spec 001 creates (project skeleton; research [R-11][r-11]; spec Assumptions "Decided: `.gitignore`"): it holds `bin/`, `obj/`, `TestResults/` and `.vs/`. Add only a missing line and remove nothing. Check with `git status` that no `bin/`, `obj/` or `TestResults/` path is listed ([NFR-02][nfr-02]; [DEC-33][dec-33]).

**Checkpoint**: `dotnet build` and `dotnet test` pass; `git status` shows no build output.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: line-ending rules, the six test-data pairs, the CI skeleton and the README skeleton, which every user story uses.

### Test data, test first (TD-01, TD-02; DEC-31, DEC-37)

- [x] T004 Write the [TD-02][td-02] tests first in `tests/ToyRobot.ConsoleEndToEndTests/DeliveryTests.cs`, class `DeliveryTests` in namespace `ToyRobot.ConsoleEndToEndTests` (research [R-12][r-12]): `Test_data_holds_every_input_and_expected_pair` as an xUnit theory over `challenge-example-1`, `challenge-example-2`, `challenge-example-3`, `bom-challenge-example-1`, `exit-mid-file`, `empty`, checking `testdata/<name>.txt` and `testdata/<name>.expected.txt` both exist; and `Byte_order_mark_test_file_starts_with_the_mark`, checking the first three bytes of `testdata/bom-challenge-example-1.txt` are `EF BB BF`. Find the repository root by walking up from `AppContext.BaseDirectory` to the folder holding `ToyRobot.sln` (the same rule as spec 002's AppProcess, which exposes no shared helper). Run `dotnet test tests/ToyRobot.ConsoleEndToEndTests` and confirm these tests FAIL (no `testdata/` yet) ([TD-02][td-02]; [DEC-31][dec-31]).
- [x] T005 [P] Create `testdata/challenge-example-1.txt` (lines `PLACE 0,0,NORTH`, `MOVE`, `REPORT`) and `testdata/challenge-example-1.expected.txt` (lines `Output:`, `0,1,NORTH`): UTF-8 without byte-order mark, LF, every line ending with a line break (contracts/testdata.md; [TD-01][td-01]; [DEC-31][dec-31] "the three challenge examples", [DEC-21][dec-21]).
- [x] T006 [P] Create `testdata/challenge-example-2.txt` (`PLACE 0,0,NORTH`, `LEFT`, `REPORT`) and `testdata/challenge-example-2.expected.txt` (`Output:`, `0,0,WEST`), same format as T005 ([TD-01][td-01]; [DEC-31][dec-31], [DEC-21][dec-21]).
- [x] T007 [P] Create `testdata/challenge-example-3.txt` (`PLACE 1,2,EAST`, `MOVE`, `MOVE`, `LEFT`, `MOVE`, `REPORT`) and `testdata/challenge-example-3.expected.txt` (`Output:`, `3,3,NORTH`), same format as T005 ([TD-01][td-01]; [DEC-31][dec-31], [DEC-21][dec-21]).
- [x] T008 [P] Create `testdata/bom-challenge-example-1.txt` by writing bytes (for example `[System.IO.File]::WriteAllBytes` in PowerShell): `EF BB BF` followed by the exact text of `testdata/challenge-example-1.txt`; create `testdata/bom-challenge-example-1.expected.txt` (`Output:`, `0,1,NORTH`). Confirm with `Format-Hex` (or `od -An -tx1 -N3`) that the input starts with `ef bb bf` ([TD-01][td-01]; [DEC-20][dec-20], [DEC-31][dec-31] "input starting with a byte-order mark").
- [x] T009 [P] Create `testdata/exit-mid-file.txt` (`PLACE 0,0,NORTH`, `REPORT`, `EXIT`, `MOVE`, `REPORT`) and `testdata/exit-mid-file.expected.txt` (`Output:`, `0,0,NORTH`), same format as T005 ([TD-01][td-01]; [DEC-10][dec-10], [DEC-31][dec-31] "EXIT ... mid-file").
- [x] T010 [P] Create `testdata/empty.txt` and `testdata/empty.expected.txt` as zero-byte files ([TD-01][td-01]; [DEC-10][dec-10], [DEC-31][dec-31] "an empty input", [DEC-32][dec-32]).
- [x] T011 Run `dotnet build` and `dotnet test` at the repository root: [TD-02][td-02] now passes, spec 002's [E2E-01][e2e-01] runs all six pairs and passes, and every other test still passes. Commit the test data and `DeliveryTests.cs` ([TD-01][td-01], [TD-02][td-02]; [DEC-31][dec-31]).

### CI skeleton (NFR-02; DEC-31, DEC-33)

- [x] T012 Create `.github/workflows/ci.yml` with steps 1 to 4 of contracts/ci-workflow.md: workflow name `CI`; `on: push`; one job with `timeout-minutes: 30` (so a step that hangs, for example a script waiting for input, fails that system within 30 minutes instead of the 360-minute default), `strategy.fail-fast: false` and `matrix.os: [windows-latest, ubuntu-latest, macos-latest]`, `runs-on: ${{ matrix.os }}`; `actions/checkout@v5`; `actions/setup-dotnet@v5` with `dotnet-version: 8.0.x` (latest major of each action if `@v5` does not exist, research [R-05][r-05]); `dotnet build ToyRobot.sln`; `dotnet test tests/ToyRobot.Tests --no-build`; `dotnet test tests/ToyRobot.ConsoleEndToEndTests --no-build` ([NFR-02][nfr-02]; [DEC-31][dec-31] three levels, [DEC-33][dec-33]).
- [x] T013 Push the branch and watch the run (`gh run watch`): build, unit and Runner whole-flow tests, and console end-to-end tests (including [E2E-01][e2e-01] over the six pairs, [E2E-05][e2e-05] byte-order mark, [TD-02][td-02]) pass on all three systems ([NFR-02][nfr-02]; [DEC-31][dec-31] "input starting with a byte-order mark (console end-to-end, on all three systems)", [DEC-33][dec-33]).

### Line endings, test first then rule (NFR-02a; DEC-33)

- [x] T013a [US2] [CI-06][ci-06], line-ending part, test first (contracts/ci-workflow.md step 5; research [R-09][r-09]): add to `.github/workflows/ci.yml`, after the test steps, a `shell: bash` step that reads `git ls-files --eol` and fails, naming the file, for any file not marked `w/-text` whose working-copy value is not `w/crlf` for a `.cmd` file, or not `w/lf`, `w/none` or empty for any other file. Commit locally; do not push (brief: never push work that is not passing). Show that it fails by running the same commands in Git Bash in a fresh clone made with `git clone -c core.autocrlf=true <repository root> %TEMP%\ci06-T013a` (the setting `windows-latest` uses), not in the working copy (see T003): text files show `w/crlf`. T003 makes it pass ([NFR-02a][nfr-02a], [CI-06][ci-06]; [DEC-33][dec-33]).
- [x] T003 Add `.gitattributes` at the repository root with exactly the three lines of research [R-10][r-10] (`* text=auto eol=lf`, `*.cmd text eol=crlf`, `*.pdf binary`). Run `git add --renormalize .` and `git status`; commit `.gitattributes` together with any file the renormalization changed (line endings only; no content is removed, brief rule 12). Run `git ls-files --eol` and confirm no text file shows `i/crlf` ([NFR-02a][nfr-02a]; [DEC-33][dec-33]). The working copy was checked out with `core.autocrlf=true` before `.gitattributes` existed, and neither `git add --renormalize` nor this task rewrites files on disk, so its `w/crlf` values are expected and are NOT changed (no `reset --hard`, no deleting files, brief rule 12). Run the check again in a new fresh clone, `git clone -c core.autocrlf=true <repository root> %TEMP%\ci06-T003`, and see it pass; then push the commits of T013a and T003 together and watch the run: the line-ending part of [CI-06][ci-06] passes on all three systems, so Phase 2 ends green ([NFR-02a][nfr-02a], [CI-06][ci-06]; [DEC-33][dec-33]).

### README skeleton (NFR-09; DEC-37 stage 1)

- [x] T014 Change the root `README.md` in place into the README skeleton (spec Assumptions "Decided in the run: the README is the existing root `README.md`"): keep the title, the first paragraph and every row of the "Where to look" table; update the "Status" text to say the application is being delivered and that commands are added as each one works; add headings, in the order of quickstart.md section 5, for the parts fixed by the decisions now: what it is; requirements (.NET SDK 8 or later, `https://dotnet.microsoft.com/download`, runs on a later major version too, [DEC-37][dec-37]); output format with the example `Output:` / `0,1,NORTH` on console output (stdout) and everything else on console messages (stderr) ([DEC-21][dec-21], [DEC-23][dec-23]); key assumptions with a link to `docs/design-analysis/design-decisions.md`; "How this would evolve" (multiple robots, a configurable table, obstacles; none built, [DEC-38][dec-38], [DEC-39][dec-39]); and add to "Where to look" the rows for the design-phase prompt log, the implementation-phase prompt log, the Spec Kit files (`.specify/memory/constitution.md`, `specs/`) and the solution design (`docs/solution-design/README.md`). No command is written yet that has not been run ([NFR-09][nfr-09]; [DEC-37][dec-37]).

**Checkpoint**: `dotnet test` passes locally; the `CI` run passes on all three systems.

---

## Phase 3: User Story 1 - Run a challenge example with one script on any system (Priority: P1) MVP

**Goal**: `run.cmd` and `run.sh` check .NET, build quietly and run the application with or without a file, with the exit codes of [NFR-08][nfr-08] ([DEC-04][dec-04], [DEC-37][dec-37]).

**Independent Test**: [CI-01][ci-01] to [CI-05][ci-05] on all three systems, plus the same cases run locally on Windows.

### Tests for User Story 1 (write first; they fail until T021 and T022; T015 to T019a)

Rules for every cmd step below (GitHub's `shell: cmd` runs with delayed expansion off, and `%VAR%` is expanded when a line, a parenthesised block or a `for` body is parsed): save each exit code on the line straight after the command, outside any parenthesised block, as `>"<file>" echo %ERRORLEVEL%` (redirection written first; never `echo %ERRORLEVEL%> "<file>"`, where a one-digit code before `>` is read as a handle number and the code is not saved), or `set "rc=%ERRORLEVEL%"` then `>"<file>" echo %rc%`; for a loop over names use `call :run <name>` to a subroutine (or `setlocal EnableDelayedExpansion` and `!ERRORLEVEL!`); end the step with `exit /b 0`, because the codes are checked in the bash comparison step (research [R-02][r-02], [R-03][r-03], [R-06][r-06]).

- [x] T015 [US1] Add [CI-01][ci-01] and [CI-03][ci-03] to `.github/workflows/ci.yml` (contracts/ci-workflow.md steps 6 and 7): first create the folder `$RUNNER_TEMP/out` (`%RUNNER_TEMP%\out`); on Windows a `shell: cmd` step that, for each of the six names (one `call :run <name>` line each, then a subroutine `:run`; see the rules above), runs `call run.cmd testdata\%1.txt > "%RUNNER_TEMP%\out\%1.txt"` and on the next line writes its exit code to `"%RUNNER_TEMP%\out\%1.code"` (console messages (stderr) stay in the step log; they are not compared, page 10); then runs `call run.cmd < testdata\challenge-example-1.txt > "%RUNNER_TEMP%\out\no-argument.txt"` and saves its code; then one run from inside `testdata`: `cd testdata`, then `call ..\run.cmd challenge-example-1.txt > "%RUNNER_TEMP%\out\other-folder.txt"`, its code saved on the next line to `other-folder.code`, then `cd ..` (the script finds the application relative to its own folder); on Linux and macOS a `shell: bash` step doing the same with `./run.sh testdata/<name>.txt`, `./run.sh < testdata/challenge-example-1.txt` and `(cd testdata && ../run.sh challenge-example-1.txt) > "$RUNNER_TEMP/out/other-folder.txt"`, exit codes captured despite `-e` (research [R-06][r-06]). Use `if: runner.os == 'Windows'` / `if: runner.os != 'Windows'` ([NFR-08][nfr-08], [CI-01][ci-01], [CI-03][ci-03]; [DEC-04][dec-04], [DEC-33][dec-33], [DEC-37][dec-37]).
- [x] T016 [US1] Add the comparison step to `.github/workflows/ci.yml` (contracts/ci-workflow.md step 8): one `shell: bash` step on all three systems that, for every pair, runs `diff <(tr -d '\r' < "$RUNNER_TEMP/out/<name>.txt") <(tr -d '\r' < testdata/<name>.expected.txt)`, compares `no-argument.txt` and `other-folder.txt` with `testdata/challenge-example-1.expected.txt` the same way, checks every saved exit code (the six pairs, `no-argument`, `other-folder`) is `0` (each saved exit code is read with carriage returns, line breaks and spaces removed (`tr -d '\r\n '`) before it is compared with 0 or 1), prints each difference, and fails if any check fails ([CI-01][ci-01], [CI-03][ci-03]; [DEC-23][dec-23], [DEC-31][dec-31] line by line, [DEC-33][dec-33]).
- [x] T017 [US1] Add [CI-02][ci-02] to `.github/workflows/ci.yml` (contracts/ci-workflow.md step 9): for each of `testdata/no-such-file.txt`, `testdata` (a folder) and the two arguments `testdata/challenge-example-1.txt testdata/challenge-example-2.txt`, run the script (cmd: `call run.cmd ...`; bash: `./run.sh ...`) with console output (stdout) and console messages (stderr) saved to files, the exit code saved on the line straight after the command (see the rules above), and fail unless the exit code (each saved exit code is read with carriage returns, line breaks and spaces removed (`tr -d '\r\n '`) before it is compared with 0 or 1) is 1, the saved console output (stdout) is empty and the saved console messages (stderr) are not empty ([NFR-08][nfr-08], [CI-02][ci-02]; [DEC-23][dec-23], [DEC-37][dec-37]).
- [x] T018 [US1] Add [CI-04][ci-04] to `.github/workflows/ci.yml` (contracts/ci-workflow.md step 10; research [R-07][r-07]): (a) no `dotnet`: bash `PATH="$RUNNER_TEMP/empty" /bin/bash ./run.sh testdata/challenge-example-1.txt`; cmd `set "PATH=%SystemRoot%\System32"` then `call run.cmd testdata\challenge-example-1.txt`; (b) only an older SDK: a stand-in `dotnet` printing `7.0.410 [/fake/sdk]` (bash: executable `#!/bin/sh` file in `$RUNNER_TEMP/old-sdk`, `PATH="$RUNNER_TEMP/old-sdk:/usr/bin:/bin" /bin/bash ./run.sh ...`; cmd: `dotnet.cmd` printing `7.0.410 [C:\fake\sdk]` in `%RUNNER_TEMP%\old-sdk`, `PATH=%RUNNER_TEMP%\old-sdk;%SystemRoot%\System32`). Save console output (stdout) and console messages (stderr) to files and each exit code on the line straight after the command (see the rules above). Each must give exit code 1 (each saved exit code is read with carriage returns, line breaks and spaces removed (`tr -d '\r\n '`) before it is compared with 0 or 1), empty console output (stdout), and console messages (stderr) that contain `https://dotnet.microsoft.com/download` ([NFR-08][nfr-08], [CI-04][ci-04]; [DEC-23][dec-23], [DEC-37][dec-37]).
- [x] T019 [US1] Add [CI-05][ci-05] to `.github/workflows/ci.yml` (contracts/ci-workflow.md step 11; research [R-08][r-08]): a bash step makes `$RUNNER_TEMP/broken` with `broken=$(cd "$RUNNER_TEMP/broken" && pwd)` then `git archive --format=tar HEAD | tar -x -C "$broken"` (tar on `windows-latest` rejects the Windows form of `$RUNNER_TEMP`) and writes `src/ToyRobot/CiBuildBreaker.cs` there containing `#error CI-05 makes this build fail on purpose`; then, from that system's shell in `$RUNNER_TEMP/broken`, run the script with `testdata/challenge-example-1.txt` saving stdout, stderr and the exit code (on the line straight after the command, outside any block; see the rules above), then run `dotnet build src/ToyRobot/ToyRobot.csproj -c Release --nologo` there saving its exit code the same way; fail unless both codes (each saved exit code is read with carriage returns, line breaks and spaces removed (`tr -d '\r\n '`) before it is compared with 0 or 1) are equal and not 0, the saved stdout is empty and the saved stderr contains `CS1029`. The checkout is never changed ([NFR-08][nfr-08], [CI-05][ci-05]; [DEC-37][dec-37]).
- [x] T019a [US1] [CI-06][ci-06], executable-mode part, test first (contracts/ci-workflow.md step 5; research [R-09][r-09], [R-10][r-10]): extend the [CI-06][ci-06] step added in T013a so that it also fails unless `git ls-files -s run.sh` starts with `100755`. It is red until T022 (there is no `run.sh` yet, so the output is empty). Run the same command locally in Git Bash in a fresh clone under `%TEMP%` (`git clone -c core.autocrlf=true <repository root> %TEMP%\ci06-T019a`, not the working copy) and see it fail ([NFR-02a][nfr-02a], [CI-06][ci-06]; [DEC-33][dec-33]).
- [x] T020 [US1] Run locally on Windows, in cmd, the [CI-01][ci-01], [CI-02][ci-02] and [CI-03][ci-03] commands from T015 to T017 before the scripts exist and confirm they fail (`run.cmd` is not found). This shows the checks can fail (constitution II).

### Implementation for User Story 1

- [x] T021 [US1] Write `run.cmd` at the repository root (contracts/run-scripts.md; research [R-01][r-01] to [R-04][r-04]; [DEC-04][dec-04], [DEC-23][dec-23], [DEC-37][dec-37], [NFR-08][nfr-08]), written to disk with CRLF line endings (`.gitattributes` applies CRLF only at checkout; cmd can misread `goto` labels in a file with LF line endings), checked after `git add run.cmd` with `git ls-files --eol run.cmd` showing `w/crlf` before T023 runs it, starting `@echo off` and `setlocal`, in this order: (1) .NET check: `for /f "tokens=1 delims=." %%v in ('dotnet --list-sdks 2^>nul')`, counting only whole-number tokens `GEQ 8`; none: write a message naming `https://dotnet.microsoft.com/download` to stderr (`1>&2`), `exit /b 1`; (2) build: `dotnet build "%~dp0src\ToyRobot\ToyRobot.csproj" -c Release --nologo < NUL > "<log>" 2>&1` with `<log>` a randomly named file in `%TEMP%`; save the build's code on the line straight after `dotnet build` (`set "rc=%ERRORLEVEL%"`, never inside a parenthesised block, where `%ERRORLEVEL%` is expanded before the block runs); non-zero `rc`: `type` the log to stderr, delete the log, `exit /b %rc%` (the code is taken from `rc`, because `%ERRORLEVEL%` is not the build's after `type` and `del`); zero: delete the log, show nothing; (3) arguments: `"%~2"` not empty: the how-to-run message (for example `How to run: run testdata\challenge-example-1.txt to run the commands in a file, or run with no file to type the commands. In PowerShell, use .\run.cmd instead of run.` (changed later at the developer's request, P188; first wording `How to run: run.cmd [commands.txt]`)) to stderr, `exit /b 1`; `"%~1"` empty: run `"%~dp0src\ToyRobot\bin\Release\net8.0\ToyRobot.exe"` with the script's console input; otherwise if `"%~1"` does not exist or `"%~1\*"` exists (a folder): a "file not found" message naming it to stderr, `exit /b 1`; else run the application with `< "%~1"`; use `goto` labels for the no-argument and the file cases instead of `if`/`else` blocks, so that the application is started on a line outside any parenthesised block and is followed directly by (4) `exit /b %ERRORLEVEL%` with the application's code (nothing between the two lines). Nothing of the script's own is written on success; nothing at all is written to stdout by the script.
- [x] T022 [US1] Write `run.sh` at the repository root (contracts/run-scripts.md; research [R-01][r-01] to [R-04][r-04]; [DEC-04][dec-04], [DEC-23][dec-23], [DEC-37][dec-37], [NFR-08][nfr-08]): `#!/usr/bin/env bash`, LF, bash 3.2 compatible, using only bash built-ins up to and including the .NET check: script folder from `$0` by parameter expansion (`.` when `$0` has no `/`); (1) `command -v dotnet`, then `dotnet --list-sdks` captured, passing if a line starts with a whole number `>= 8` before its first dot; otherwise a message naming `https://dotnet.microsoft.com/download` to stderr (`>&2`), `exit 1`; (2) `out=$(dotnet build "$dir/src/ToyRobot/ToyRobot.csproj" -c Release --nologo 2>&1 < /dev/null)`; non-zero: `printf '%s\n' "$out" >&2` and exit with the build's code; (3) `$# -gt 1`: the how-to-run message (for example `How to run: ./run.sh testdata/challenge-example-1.txt to run the commands in a file, or ./run.sh with no file to type the commands.` (changed later, P188; first wording `How to run: ./run.sh [commands.txt]`)) to stderr, `exit 1`; `$# -eq 0`: run `"$dir/src/ToyRobot/bin/Release/net8.0/ToyRobot"` with the script's console input; `$# -eq 1`: `[ -f "$1" ]` or a "file not found" message to stderr and `exit 1`; else run the application with `< "$1"`; (4) exit with the application's code. Mark it executable in git: `git update-index --chmod=+x run.sh` ([NFR-02a][nfr-02a]; [DEC-33][dec-33]).
- [x] T023 [US1] Run locally on Windows, in cmd, the checks of T015 to T018 ([CI-01][ci-01] every pair and the run from inside `testdata` with the comparisons of T016 run in Git Bash; [CI-02][ci-02] with its console-message check, [CI-03][ci-03], [CI-04][ci-04] a and b) against `run.cmd`; then in PowerShell `.\run.cmd testdata\challenge-example-1.txt` and check `$LASTEXITCODE` is 0 and stdout is `Output:` / `0,1,NORTH` (spec US1 scenario 2). In Git Bash, run `./run.sh testdata/challenge-example-1.txt` and [CI-02][ci-02] against `run.sh` where Git Bash can start the application. Fix the scripts until all pass ([NFR-08][nfr-08]; [DEC-37][dec-37]).
- [x] T023a [US1] Commit `run.cmd`, `run.sh` (mode 100755, `git ls-files -s run.sh` shows `100755`) and the workflow changes locally; do not push yet. `git archive HEAD` in T024 then holds the scripts ([NFR-02][nfr-02], [NFR-08][nfr-08]; [DEC-33][dec-33], [DEC-37][dec-37]).
- [x] T024 [US1] Run [CI-05][ci-05] locally on Windows (after T023a) in a temporary copy (`git archive HEAD` into a folder under `%TEMP%`, never the working copy; the copy holds `run.cmd` and `run.sh`) and check the script's exit code equals that of `dotnet build src\ToyRobot\ToyRobot.csproj -c Release --nologo` there, stdout is empty and stderr contains `CS1029` ([NFR-08][nfr-08], [CI-05][ci-05]; [DEC-37][dec-37]).
- [x] T025 [US1] After T024, push the commits of T023a and watch the `CI` run: [CI-01][ci-01] to [CI-05][ci-05] and the executable-mode part of [CI-06][ci-06] pass on all three systems, with the earlier steps still passing ([NFR-02][nfr-02], [NFR-02a][nfr-02a], [NFR-08][nfr-08]; [DEC-33][dec-33], [DEC-37][dec-37]).

**Checkpoint**: on all three systems the scripts give the right console output (stdout) for all six pairs and with no argument, and exit code 1 or the build's exit code in every error case.

---

## Phase 4: User Story 2 - Proof that it works on Windows, Linux and macOS (Priority: P1)

**Goal**: the workflow is complete as in contracts/ci-workflow.md and also proves the line-ending rules ([NFR-02][nfr-02], [NFR-02a][nfr-02a]; [DEC-33][dec-33]).

**Independent Test**: a push gives a green `CI` run on all three systems with every step of contracts/ci-workflow.md.

### Tests for User Story 2

- [x] T026 [US2] Review that both parts of [CI-06][ci-06] are present in `.github/workflows/ci.yml` as contracts/ci-workflow.md step 5 (research [R-09][r-09]): the line-ending part (T013a, red before T003) and the executable-mode part (T019a, red before T022), placed after the test steps, on all three systems. Run both parts locally in Git Bash in a fresh clone under `%TEMP%` (`git clone -c core.autocrlf=true <repository root> %TEMP%\ci06-T026`; commit first, so the clone holds `.gitattributes` and `run.sh`), not in the working copy, whose `w/crlf` values are expected (T003), and confirm they pass there ([NFR-02a][nfr-02a], [CI-06][ci-06]; [DEC-33][dec-33]).

### Implementation for User Story 2

- [x] T027 [US2] Review `.github/workflows/ci.yml` against contracts/ci-workflow.md: trigger `on: push`; the three systems; `fail-fast: false`; steps 1 to 11 in that order; outputs only under `$RUNNER_TEMP`; Windows script runs from cmd, comparison in bash. Fix any difference ([NFR-02][nfr-02]; [DEC-33][dec-33]).
- [x] T028 [US2] Commit, push and watch the run: every step passes on `windows-latest`, `ubuntu-latest` and `macos-latest`. Record the run's link for the README (T031) and the run report ([NFR-02][nfr-02], [SC-001][sc-001], [SC-007][sc-007]; [DEC-33][dec-33]).

**Checkpoint**: the `CI` run is green on all three systems with all steps.

---

## Phase 5: User Story 3 - Build, test and run from the README alone (Priority: P2)

**Goal**: the README is complete and works word for word ([NFR-09][nfr-09]; [DEC-37][dec-37] stages 2 to 4).

**Independent Test**: quickstart.md section 5 ("README check"), on a fresh clone in cmd, PowerShell and bash.

### Test for User Story 3

- [x] T029 [US3] Use quickstart.md section 5 as the README's test: before each command is added to the README (T030), run it exactly as it will be written, in the shell it is written for, from the repository root, and record the output; a command that does not give the shown output is not added until fixed ([NFR-09][nfr-09]; [DEC-37][dec-37] "real commands added as each one works").

### Implementation for User Story 3

- [x] T030 [US3] Add to `README.md` ([DEC-37][dec-37] stage 2), each command as run in T029: how to run it with the scripts in each shell form (`run commands.txt`, `.\run.cmd commands.txt`, `./run.sh commands.txt`; with no file, type commands and end with `EXIT`); ready-to-copy commands for the three challenge examples in each shell form (`testdata\challenge-example-1.txt` to `-3.txt` for cmd and PowerShell, `testdata/...` for bash) with their expected console output (stdout) `Output:` `0,1,NORTH`, `Output:` `0,0,WEST`, `Output:` `3,3,NORTH`; build `dotnet build -c Release`, test `dotnet test`, and the direct run commands for cmd (`src\ToyRobot\bin\Release\net8.0\ToyRobot.exe < testdata\challenge-example-1.txt`), bash (`./src/ToyRobot/bin/Release/net8.0/ToyRobot < testdata/challenge-example-1.txt`) and PowerShell (`cmd /c "src\ToyRobot\bin\Release\net8.0\ToyRobot.exe < testdata\challenge-example-1.txt"`) ([NFR-09][nfr-09]; [DEC-04][dec-04], [DEC-21][dec-21], [DEC-37][dec-37]).
- [x] T031 [US3] After T028 (all tests pass in CI), add to `README.md` ([DEC-37][dec-37] stage 3): the CI badge `[![CI](https://github.com/hetal-patel78/toy-robot-spec-driven/actions/workflows/ci.yml/badge.svg)](https://github.com/hetal-patel78/toy-robot-spec-driven/actions/workflows/ci.yml)`, the sentence that the solution is tested on Windows, Linux and macOS through this workflow, with the link; and the test-data list (the six pairs and what each shows). Update the "Status" text to describe the finished application. Keep the README to about one screen ([NFR-02][nfr-02], [NFR-09][nfr-09]; [DEC-33][dec-33], [DEC-37][dec-37]).
- [x] T032 [US3] README check ([DEC-37][dec-37] stage 4, local part): clone the repository into a new folder under `%TEMP%` (`git clone <this repository> <folder>`), and follow `README.md` word for word there in cmd, in PowerShell and in Git Bash: every command works and every shown output matches; check every part of quickstart.md section 5 is present. Record the result for the run report; the developer repeats this on a fresh clone before finishing (spec Assumptions; [NFR-09][nfr-09], [SC-004][sc-004]; [DEC-37][dec-37]). Done by the developer on Windows, 5 October 2026: the README followed on a fresh clone in cmd and PowerShell; the bash commands are run through `run.sh` in CI on Linux and macOS.

**Checkpoint**: the README check passes in cmd, PowerShell and Git Bash on the fresh local clone.

---

## Phase 6: User Story 4 - Runs where only a newer .NET is installed (Priority: P3)

**Goal**: [NFR-03][nfr-03] is guarded by a test and reviewed ([DEC-37][dec-37]).

**Independent Test**: [BS-01][bs-01] passes on all three systems; the review confirms the setting.

### Test for User Story 4

- [x] T033 [US4] Add [BS-01][bs-01] to `tests/ToyRobot.ConsoleEndToEndTests/DeliveryTests.cs` (namespace `ToyRobot.ConsoleEndToEndTests`): `Application_targets_net8_and_rolls_forward_to_the_latest_major_version`, reading `src/ToyRobot/ToyRobot.csproj` with `System.Xml.Linq` (repository root found as in T004) and asserting the `TargetFramework` element is `net8.0` and the `RollForward` element is `LatestMajor`. The setting already exists (spec 001 [SK-02][sk-02]), so the test passes at once and from then on guards it; to show it can fail, change its expected `RollForward` value to `Disable` in one local run, see it fail, and put `LatestMajor` back before committing ([NFR-03][nfr-03], [BS-01][bs-01]; [DEC-37][dec-37]).

### Implementation for User Story 4

- [x] T034 [US4] Review the build settings (page 3, [NFR-03][nfr-03]): `src/ToyRobot/ToyRobot.csproj` has `<TargetFramework>net8.0</TargetFramework>` and `<RollForward>LatestMajor</RollForward>`; nothing in this spec changes them. Run `dotnet test`; push; [BS-01][bs-01] passes on all three systems ([NFR-03][nfr-03], [SC-005][sc-005]; [DEC-37][dec-37]).

**Checkpoint**: [BS-01][bs-01] passes locally and in CI.

---

## Phase 7: Polish & cross-cutting

- [x] T035 Real runs by hand on Windows (Spec Kit brief step 10), from cmd and from PowerShell with `run.cmd`: each of the six test-data files, stdout compared with the expected file line by line (save stdout from cmd, not with PowerShell `>`, [DEC-23][dec-23]); `run` with no argument in cmd, type `PLACE 0,0,NORTH`, `REPORT`, `EXIT`: the prompt `> ` appears on stderr, stdout shows `Output:` / `0,0,NORTH`, exit code 0 (spec US1 scenario 5); the no-`dotnet` case and the failed-build case (in a temporary copy) by hand. Record every result for the run report ([NFR-08][nfr-08]; [DEC-24][dec-24], [DEC-37][dec-37]). Done by the developer on Windows, 5 October 2026: every test-data file and the typed session in cmd and PowerShell, with the script error cases (two files, a missing file, a folder); the no-`dotnet` and failed-build cases are checked by CI-04 and CI-05.
  - Status: every part done except the typed session (no argument, commands typed by a person), which needs a person at a console and is left for the developer (docs/spec-kit/implementation-run-report.md, sections 7 and 8).
- [x] T036 Run quickstart.md sections 1 to 4 in full; `dotnet build` and `dotnet test` pass at the repository root; `git status` shows no build output and no stray file; nothing was deleted (brief rule 12) ([NFR-02][nfr-02], [NFR-08][nfr-08], [NFR-09][nfr-09]; [DEC-37][dec-37]).
- [x] T037 Confirm the last `CI` run for the final commit is green on all three systems ([SC-001][sc-001] to [SC-003][sc-003], [SC-006][sc-006], [SC-007][sc-007]) and record its link for the run report ([NFR-02][nfr-02]; [DEC-33][dec-33]).

## Phase 8: Progress lines (developer request 2026-10-05)

- [x] T038 [US1] Progress lines in `run.cmd` and `run.sh` (developer request 2026-10-05; [NFR-08][nfr-08]; [DEC-37][dec-37], [DEC-23][dec-23]). Test first: add the CI step "[CI-07][ci-07] Progress lines" (run step per system family and one bash check step, after "[CI-01][ci-01] and [CI-03][ci-03] compare") that runs each system's script once with `testdata/challenge-example-1.txt`, saves console messages (stderr), and checks (carriage returns removed) that its first three lines are exactly `Checking for a .NET SDK, version 8 or later...`, `Building the application (Release)...` and `Running the application with commands from <file as given>`, and that console output (stdout) still equals `testdata/challenge-example-1.expected.txt`; confirm locally on Windows (cmd form, `set NoDefaultCurrentDirectoryInExePath=` first) that the check fails before the script change (the first stderr lines are the echo lines). Then add the lines to both scripts: the SDK line before the SDK check, the build line before the build, the running line just before the application starts (after the argument checks, so the how-to-run message and file-not-found errors have no "Running" line; no argument: the typing hint, `Ctrl+Z then Enter` in `run.cmd`, `Ctrl+D` in `run.sh`). Keep `run.cmd` CRLF, `run.sh` LF and mode 100755; add the one README sentence after the "Run it" table; run build, tests and the specification check; run both scripts by hand.

## Phase 9: run.cmd hardening (developer request 2026-10-05)

- [x] T039 [US1] `run.cmd` rejects `*` and `?` in its file argument and turns delayed expansion off (developer request 2026-10-05; [NFR-08][nfr-08]; [DEC-37][dec-37]). Test first: add to "[CI-02][ci-02] Windows" the case `testdata\challenge-example-1.*`, and to "[CI-02][ci-02] check" (Windows only) the same checks plus that stderr contains `must not contain * or ?`; confirm locally on Windows (cmd form, `set NoDefaultCurrentDirectoryInExePath=` first) that the check fails before the script change (exit code 0). Then change `run.cmd`: `setlocal DisableDelayedExpansion`, and, after the argument-count and no-argument checks and before the `exist` checks, a wildcard check with the message on stderr and exit 1. Keep CRLF. `run.sh` is unchanged (confirm in Git Bash that `./run.sh 'testdata/challenge-example-1.*'` gives file-not-found, exit 1). Run build, tests, the specification check and the hand runs (names with `&`, `^`, `%`, `!`, also with delayed expansion forced on).
- [x] T040 [US1] A seventh test-data pair, `long-session` (developer request 2026-10-05; [TD-01][td-01]; see the spec's "Decided (developer, 2026-10-05): a long mixed session"). Test first: add `long-session` to the [TD-02][td-02] theory in `tests/ToyRobot.ConsoleEndToEndTests/DeliveryTests.cs` and to both [CI-01][ci-01] lists in `.github/workflows/ci.yml`; the test fails. Then create `testdata/long-session.txt` (the 36 lines of [TD-01][td-01]) and `testdata/long-session.expected.txt`, from running the input through the built application, checked by hand; LF line endings, no byte-order mark. `dotnet test` passes, including [E2E-01][e2e-01] over all seven pairs.

---

## Phase 10: Menu scripts (developer request 2026-10-05)

- [x] T041 [US1] Menu scripts ([MN-01][mn-01]; [NFR-08][nfr-08]; [DEC-41][dec-41]). Test first: add the CI steps "[CI-08][ci-08] Menu" for Windows (cmd, input from a file) and for Linux and macOS (bash, input piped), and the check step; add `menu.sh` to the mode check of [CI-06][ci-06]. Then write `menu.cmd` (CRLF) and `menu.sh` (LF, executable) at the repository root: each calls the run script by its own folder, gives a file run and the comparison no console input, and shows the list again after every choice except 0. Run by hand on Windows from cmd and PowerShell and in Git Bash: choices 1, 8, 9 (typed session), `x` and 0, and a copy with a changed expected file (FAIL). Colours (developer request 2026-10-05): PASS green, FAIL red, the run's console output bright cyan; `menu.sh` only on a terminal; none with `NO_COLOR` set, which both CI-08 steps set.

## Test coverage of every behaviour in spec.md

| Behaviour (spec) | Planned test | Task |
|---|---|---|
| US1 sc. 1 to 4, 10: file argument, every pair incl. Challenge examples, empty, byte-order mark, EXIT mid-file; console messages (stderr) stated for reference | [CI-01][ci-01] (all systems; stdout line by line and exit code); local cmd and PowerShell runs | T015, T016, T023, T035 |
| Edge case: script works when called from another folder | [CI-01][ci-01] run from inside `testdata` | T015, T016, T023 |
| US1 sc. 5: no argument, typed | real run by hand (CI cannot type; prompt itself in spec 002) | T035 |
| US1 sc. 5a: no argument, redirected console input (stdout and exit code) | [CI-03][ci-03] | T015, T016 |
| US1 sc. 6: missing file, folder (message on stderr) | [CI-02][ci-02] | T017, T023 |
| US1 sc. 7: more than one argument (how-to-run message on stderr) | [CI-02][ci-02] | T017, T023 |
| US1 sc. 8: no `dotnet`; only an older SDK (message naming `https://dotnet.microsoft.com/download` on stderr) | [CI-04][ci-04] (a), (b) | T018, T023, T035 |
| US1 sc. 9: build fails | [CI-05][ci-05] | T019, T024, T035 |
| US2 sc. 1, 2: CI on three systems, failure fails the run | the `CI` workflow itself | T012, T013, T025, T028, T037 |
| US2 sc. 3: line endings, executable `run.sh` | [CI-06][ci-06] (line endings, test first T013a, then T003; mode, test first T019a, then T022; review T026); [CI-01][ci-01] runs `./run.sh` directly | T013a, T003, T019a, T022, T026, T025 |
| US3 sc. 1, 2: README works word for word; badge and link | README check | T029 to T032 |
| US4 sc. 1: `net8.0` and `RollForward` = `LatestMajor` | [BS-01][bs-01]; review | T033, T034 |
| [TD-01][td-01], [TD-02][td-02]: six pairs present, byte-order mark kept | [TD-02][td-02]; spec 002 [E2E-01][e2e-01] | T004, T011 |

## Dependencies & execution order

- **Specs**: 001 and 002 complete first (T001). This spec changes none of their source; it adds one test file to spec 002's console end-to-end project.
- **Phase 1 → Phase 2 → Phase 3 (US1) → Phase 4 (US2) → Phase 5 (US3) → Phase 6 (US4) → Phase 7.**
- Within Phase 2: T004 before T005 to T010; T011 after T010; T012 after T011; T013 after T012; T013a ([CI-06][ci-06] line-ending test, red on Windows) after T013; T003 (`.gitattributes`) directly after T013a and before `run.cmd` is committed in T023a (so `run.cmd` is stored under the CRLF rules); T014 any time in Phase 2.
- Within US1: T015 to T019a (tests) and T020 before T021 and T022; T023 after both scripts; T023a (commit locally) after T023; T024 after T023a (`git archive HEAD` must hold the scripts); T025 (push and watch) last.
- US2 needs US1 (T019a and T026: [CI-06][ci-06]'s mode part checks `run.sh`; T027 reviews all steps).
- US3: T030 needs US1 (script commands) and T029; T031 needs T028; T032 needs T031.
- US4: T033 after T004 (same file); T034 after T033.
- T035 to T037 after all stories. T038 (developer request) after T037: its [CI-07][ci-07] step is written and shown failing before the scripts change. T039 (developer request) after T038: its [CI-02][ci-02] case is written and shown failing before `run.cmd` changes.

## Parallel opportunities

- T005 to T010 (one pair each, different files).
- T014 (README skeleton) in parallel with T005 to T013.
- T021 (`run.cmd`) and T022 (`run.sh`) are different files and can be written in parallel once T015 to T020 are done.

```text
Phase 2 example:
  Task: "Create testdata/challenge-example-1.txt and .expected.txt (T005)"
  Task: "Create testdata/challenge-example-2.txt and .expected.txt (T006)"
  Task: "Create testdata/bom-challenge-example-1.txt by bytes (T008)"
US1 example:
  Task: "Write run.cmd (T021)"
  Task: "Write run.sh (T022)"
```

## Implementation strategy

1. Phase 1 and 2: dependencies confirmed, test data with [TD-02][td-02], CI skeleton green, [CI-06][ci-06] line-ending test (red locally in a fresh autocrlf clone) then `.gitattributes` (green), README skeleton.
2. US1 (MVP): scripts with [CI-01][ci-01] to [CI-05][ci-05] green on all three systems; a reviewer can already run the challenge examples.
3. US2: CI complete; both parts of [CI-06][ci-06] reviewed.
4. US3: README complete and checked on a fresh clone.
5. US4: [BS-01][bs-01] and the review.
6. Polish: real runs by hand, quickstart, final green CI; results go to the run report.

## Notes

- Commit after each task or group, naming the task IDs (brief step 9.2). Push only when everything that can run locally passes.
- If a task conflicts with design-decisions.md or the solution design, the sources win: fix spec, plan or tasks first, then the files (brief step 9.3).
- Nothing is deleted: no test-data file, script or document is removed, and the README is changed in place (brief rule 12).

[bs-01]: spec.md#bs-01
[ci-01]: spec.md#ci-01
[ci-02]: spec.md#ci-02
[ci-03]: spec.md#ci-03
[ci-04]: spec.md#ci-04
[ci-05]: spec.md#ci-05
[ci-06]: spec.md#ci-06
[ci-07]: spec.md#ci-07
[ci-08]: spec.md#ci-08
[dec-04]: ../../docs/design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-10]: ../../docs/design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-20]: ../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-24]: ../../docs/design-analysis/design-decisions.md#dec-24-prompt
[dec-31]: ../../docs/design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../../docs/design-analysis/design-decisions.md#dec-32-never-crash
[dec-33]: ../../docs/design-analysis/design-decisions.md#dec-33-cross-platform
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[dec-38]: ../../docs/design-analysis/design-decisions.md#dec-38-one-robot-now-room-for-more-later
[dec-39]: ../../docs/design-analysis/design-decisions.md#dec-39-not-included
[dec-41]: ../../docs/design-analysis/design-decisions.md#dec-41-menu-scripts
[e2e-01]: ../002-console-session/spec.md#e2e-01
[e2e-05]: ../002-console-session/spec.md#e2e-05
[nfr-02]: spec.md#nfr-02
[nfr-02a]: spec.md#nfr-02a
[nfr-03]: spec.md#nfr-03
[mn-01]: spec.md#mn-01
[nfr-08]: spec.md#nfr-08
[nfr-09]: spec.md#nfr-09
[r-01]: research.md#r-01-net-sdk-check-nfr-08-step-1
[r-02]: research.md#r-02-quiet-build-nfr-08-step-2
[r-03]: research.md#r-03-arguments-and-starting-the-application-nfr-08-steps-3-and-4
[r-04]: research.md#r-04-script-channels-on-success
[r-05]: research.md#r-05-ci-workflow-shape-nfr-02
[r-06]: research.md#r-06-shells-and-comparison-in-ci
[r-07]: research.md#r-07-ci-04-no-net-sdk-in-a-way-that-does-not-depend-on-the-runner-image
[r-08]: research.md#r-08-ci-05-failed-build-in-a-temporary-copy
[r-09]: research.md#r-09-ci-06-line-endings-and-executable-mode
[r-10]: research.md#r-10-gitattributes-content-and-the-executable-bit
[r-11]: research.md#r-11-gitignore
[r-12]: research.md#r-12-test-data-files-and-the-td-02-and-bs-01-tests
[sc-001]: spec.md#sc-001
[sc-003]: spec.md#sc-003
[sc-004]: spec.md#sc-004
[sc-005]: spec.md#sc-005
[sc-006]: spec.md#sc-006
[sc-007]: spec.md#sc-007
[sk-02]: ../001-robot-rules/spec.md#sk-02
[td-01]: spec.md#td-01
[td-02]: spec.md#td-02
