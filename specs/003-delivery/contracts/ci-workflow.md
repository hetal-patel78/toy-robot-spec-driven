# Contract: CI workflow (NFR-02, NFR-02a, NFR-08; DEC-31, DEC-33, DEC-37)

File: `.github/workflows/ci.yml`, workflow name `CI`. Trigger: `on: push` (every branch). Job matrix: `windows-latest`, `ubuntu-latest`, `macos-latest`, `fail-fast: false`, `timeout-minutes: 30` on the job (a step that hangs fails that system within 30 minutes, not after the 360-minute default). Any failed step fails that system, and the run is failed unless all three pass (page 9 test flow).

| # | Step | Shell | Passes when |
|---|---|---|---|
| 1 | Check out (`actions/checkout`) and install the .NET SDK 8 (`actions/setup-dotnet`, `8.0.x`) | — | the steps succeed |
| 2 | `dotnet build ToyRobot.sln` | default | exit code 0 |
| 3 | `dotnet test tests/ToyRobot.Tests --no-build` (unit and Runner whole-flow tests) | default | every test passes |
| 4 | `dotnet test tests/ToyRobot.ConsoleEndToEndTests --no-build` (console end-to-end tests incl. [E2E-01][e2e-01] to [E2E-10][e2e-10] of spec 002, [TD-02][td-02], [BS-01][bs-01]) | default | every test passes |
| 5 | [CI-06][ci-06]: `git ls-files --eol` (line-ending part, added before `.gitattributes`) and `git ls-files -s run.sh` (mode part, added before `run.sh`) | bash | `.cmd` files `w/crlf`; other text files `w/lf`, `w/none` or empty; `run.sh` mode `100755` |
| 6 | [CI-01][ci-01]: the script with each `testdata/<name>.txt` as its one argument, console output (stdout) saved to `out/<name>.txt`, exit code saved; and one run from inside `testdata` (`challenge-example-1.txt`, output saved to `out/other-folder.txt`, exit code saved) | Windows: cmd (`call run.cmd testdata\<name>.txt`; `cd testdata`, `call ..\run.cmd challenge-example-1.txt`); Linux, macOS: bash (`./run.sh testdata/<name>.txt`; `(cd testdata && ../run.sh challenge-example-1.txt)`) | every exit code 0 (checked in step 8) |
| 7 | [CI-03][ci-03]: the script with no argument, console input from `testdata/challenge-example-1.txt`, console output (stdout) saved to `out/no-argument.txt` | as step 6 (`call run.cmd < testdata\challenge-example-1.txt`; `./run.sh < testdata/challenge-example-1.txt`) | exit code 0 (checked in step 8) |
| 8 | Compare: for each pair, `out/<name>.txt` against `testdata/<name>.expected.txt`, and `out/no-argument.txt` and `out/other-folder.txt` against `testdata/challenge-example-1.expected.txt`, with carriage returns removed; check the saved exit codes | bash (Git Bash on Windows) | no difference, every exit code 0 |
| 9 | [CI-02][ci-02]: the script with `testdata/no-such-file.txt`, with `testdata` (a folder), with two arguments (`testdata/challenge-example-1.txt testdata/challenge-example-2.txt`), with `--HELP`, and on Windows with `testdata\challenge-example-1.*` (its message must contain `must not contain * or ?`); console output (stdout) and console messages (stderr) saved | as step 6 | each exit code 1, console output (stdout) empty, console messages (stderr) not empty |
| 10 | [CI-04][ci-04]: the script with `testdata/challenge-example-1.txt` (a) with no `dotnet` on `PATH`, (b) with a stand-in `dotnet` listing only `7.0.410`; console output (stdout) and console messages (stderr) saved | Windows: cmd; Linux, macOS: bash, starting `/bin/bash run.sh` | each exit code 1, console output (stdout) empty, console messages (stderr) contain `https://dotnet.microsoft.com/download` |
| 11 | [CI-05][ci-05]: in a copy made by `git archive HEAD` in the runner's temporary folder, with `src/ToyRobot/CiBuildBreaker.cs` holding `#error CI-05 makes this build fail on purpose`: run the script with `testdata/challenge-example-1.txt`, then `dotnet build src/ToyRobot/ToyRobot.csproj -c Release --nologo` | copy: bash; script run: as step 6 | the two exit codes are equal and not 0; console output (stdout) empty; console messages (stderr) contain `CS1029` |
| 12 | [CI-07][ci-07] (developer request 2026-10-05): the script with `testdata/challenge-example-1.txt`, console messages (stderr) and console output (stdout) saved to `out/ci07.err` and `out/ci07.out`, exit code saved; run step per system family, then one check step | cmd on Windows, bash otherwise; check in bash | with carriage returns removed, the first three lines of `ci07.err` are exactly the three progress lines of the contract run-scripts.md (the third names the file as given: `testdata\challenge-example-1.txt` on Windows, `testdata/challenge-example-1.txt` elsewhere), `ci07.out` equals `testdata/challenge-example-1.expected.txt`, exit code 0 |
| 13 | [CI-08][ci-08] (developer request 2026-10-05): the run script with no argument, with `NO_COLOR` and `TOYROBOT_MENU` set, with console input `2`, `1`, `8`, `x`, `0`, `4`, `3`, `6`, `z`, `0`, `y`, `0` (Windows: `call run.cmd < <file>` from cmd; Linux and macOS: `/bin/bash ./run.sh` with the lines piped in, so macOS uses its bash 3.2), console output (stdout) saved to `out/ci08.out`, exit code saved | cmd / bash | the output contains "PASS: challenge-example-1", "All 7 files PASS", "Toy Robot - help: choose a topic", "  PLACE X,Y,F  Put the robot on the table at X,Y facing F.", "Not a choice: x", "Not a choice: z" and "Not a choice: y"; "Toy Robot Simulator - help" appears exactly once (topic 6 only); no line starts with "FAIL: "; exit code 0 |
| 14 | [CI-09][ci-09]: the run script with `--help` (Windows: `call run.cmd --help` from cmd; Linux and macOS: `./run.sh --help`), console output (stdout) saved to `out/ci09.out`, console messages (stderr) to `out/ci09.err`, exit code saved; then the built application (`src\ToyRobot\bin\Release\net8.0\ToyRobot.exe` on Windows, `src/ToyRobot/bin/Release/net8.0/ToyRobot` elsewhere) with `--help`, console output (stdout) saved to `out/ci09.app` | cmd / bash; check in bash | with carriage returns removed, `ci09.out` equals `ci09.app`, its first line is "Toy Robot Simulator - help", `ci09.err` is exactly the first two progress lines, exit code 0 |

Outputs are saved under the runner's temporary folder (`$RUNNER_TEMP`), never in the checkout. Commands expected to fail are run with their exit code captured, because GitHub runs bash steps with `-e -o pipefail`. In `shell: cmd` steps each exit code is saved on the line straight after the command, outside any parenthesised block or `for` body (GitHub runs cmd with delayed expansion off, so `%ERRORLEVEL%` in a block holds the value from before the block); a loop over names calls a subroutine (`call :run <name>`), and the redirection is written first (`>"<file>" echo %ERRORLEVEL%`), because a one-digit code directly before `>` is read as a handle number. Each saved exit code is read with carriage returns, line breaks and spaces removed (`tr -d '\r\n '`) before it is compared with 0 or 1.

[bs-01]: ../spec.md#bs-01
[ci-01]: ../spec.md#ci-01
[ci-02]: ../spec.md#ci-02
[ci-03]: ../spec.md#ci-03
[ci-04]: ../spec.md#ci-04
[ci-05]: ../spec.md#ci-05
[ci-06]: ../spec.md#ci-06
[ci-07]: ../spec.md#ci-07
[ci-08]: ../spec.md#ci-08
[e2e-01]: ../../002-console-session/spec.md#e2e-01
[e2e-09]: ../../002-console-session/spec.md#e2e-09
[td-02]: ../spec.md#td-02
[ci-09]: ../spec.md#ci-09
[e2e-10]: ../../002-console-session/spec.md#e2e-10
