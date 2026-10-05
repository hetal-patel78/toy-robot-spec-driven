# Feature Specification: Delivery

**Feature Branch**: `003-delivery`

**Created**: 2026-10-05

**Status**: Draft

**Input**: User description: "Delivery of the Toy Robot Simulator. Owns NFR-02, NFR-03, NFR-08 and NFR-09 from docs/solution-design/10-traceability.md. Covers the run scripts run.cmd and run.sh (DEC-37), the test-data files in testdata/ including the three challenge examples with matching expected-output files, the GitHub Actions CI workflow on windows-latest, ubuntu-latest and macos-latest (DEC-33), .gitattributes, and the README (DEC-37). It needs the built application and spec 002's console end-to-end tests. Tests it plans: CI runs on all three systems; each run script runs with a challenge example and its console output (stdout) is compared with the expected file line by line; the script error cases are checked by exit code. Sources: design-decisions.md and solution design pages 3, 5, 9 and 10. Every requirement cites its NFR and DEC IDs."

## Sources and ownership

- **Source of truth**: `docs/design-analysis/design-decisions.md` (DEC-nn), then the solution design in `docs/solution-design/` (pages 3, 5, 9 and 10). Where this spec and those documents differ, those documents win ([DEC-36][dec-36]).
- **Owned by this spec**: [NFR-02][nfr-02], [NFR-03][nfr-03], [NFR-08][nfr-08] and [NFR-09][nfr-09].
- **Delivers**: the run scripts `run.cmd` and `run.sh`, the test-data files in `testdata/`, the CI workflow in `.github/workflows/`, `.gitattributes`, and the README.
- **Depends on**: spec 001-robot-rules (the solution skeleton with `RollForward` = `LatestMajor`, and the robot rules) and spec 002-console-session (the built application, its exit codes and how-to-run message, and the console end-to-end tests that run every test-data file). This spec refers to their components (Program, Runner and the others on page 6) and does not restate their rules. It is implemented after both.

## Clarifications

### Session 2026-10-05

- Q: What does "builds quietly" mean for what the reviewer sees when the build succeeds and when it fails? → A: The script captures all output of its .NET check and of the build. On success it shows nothing of its own, so console messages (stderr) hold only the application's lines; on failure it writes the captured build output to console messages (stderr). The build is given no console input, so a file fed to the script reaches only the application ([DEC-37][dec-37] "builds the app quietly; if the build fails, shows the build output"; page 9 run scripts step 2; page 12 question A: wording not fixed).
- Q: What exactly does the script build? → A: Only the application project `src/ToyRobot/ToyRobot.csproj`, in the `Release` configuration, so the application is at the page 5 path; the test projects are not built by the scripts ([DEC-37][dec-37] "builds the app"; page 5 "the run scripts build the `Release` configuration").
- Q: How does a script decide that "a .NET SDK, version 8 or later, is installed"? → A: It runs `dotnet --list-sdks`; the check passes when at least one listed SDK has a major version of 8 or more. If `dotnet` cannot be found, or no listed SDK is version 8 or later, the script says where to get the .NET SDK (`https://dotnet.microsoft.com/download`) and exits with code 1 ([DEC-37][dec-37]; page 9 run scripts step 1). Decided where the sources are silent on the method.
- Q: How are the script paths that CI cannot reach by running a test-data file (no argument, no .NET SDK, failed build) tested, and which test-data files does CI run through the scripts? → A: All by automated CI steps on all three systems: the script with no argument and a test-data file redirected to its console input ([CI-03][ci-03]); `dotnet` not found and only an older SDK found ([CI-04][ci-04], by a limited `PATH` and a stand-in `dotnet` that lists only a version 7 SDK); a failed build in a temporary copy of the repository with one source file made invalid ([CI-05][ci-05]); and every test-data pair, not only the three challenge examples, through each script ([CI-01][ci-01]) (page 3 [NFR-08][nfr-08] "the error cases are checked by their exit codes"; [DEC-31][dec-31]; Spec Kit brief review checklist: every error path has a planned test). Decided where the sources are silent on where the checks run.
- Q: What does "checks the file exists" accept when the argument names a folder? → A: Only an existing file is accepted; a folder, or a path that does not exist, gives the "file not found" message on console messages (stderr) and exit code 1 ([DEC-37][dec-37] "checks the file exists"). Decided where the sources are silent.

### Session 2026-10-05 (progress lines)

- Q: Should the run scripts show progress? → A: Yes (developer's request). Each script writes these lines to console messages (stderr), in order: `Checking for a .NET SDK, version 8 or later...`; then `Building the application (Release)...`; after a successful build, with a file: `Running the application with commands from <file>` (the argument as given); with no argument: `Running the application: type commands, one per line; end with EXIT or Ctrl+Z then Enter.` (run.cmd) / `Running the application: type commands, one per line; end with EXIT or Ctrl+D.` (run.sh). The build output itself stays hidden unless the build fails. Console output (stdout) is unchanged. This replaces the earlier answer's "shows nothing of its own" on success ([DEC-37][dec-37]: the build stays quiet and the script's own messages go to console messages (stderr); [DEC-23][dec-23]).

### Session 2026-10-05 (run.cmd hardening)

- Q: What does `run.cmd` do with a file argument that contains `*` or `?`, and what does it assume about delayed expansion? → A: (developer request) `run.cmd` rejects such an argument with a message on console messages (stderr) and exit code 1, before the "Running" line and before the application starts; it does not rely on the user's setting for delayed expansion but turns it off itself (`setlocal DisableDelayedExpansion`), so `!` in a file name stays text. `run.sh` is unchanged: a quoted `*` or `?` is not a regular file, so `[ -f ]` already gives "File not found" and exit code 1 ([DEC-37][dec-37] "checks the file exists"; [NFR-08][nfr-08]).

## User Scenarios & Testing *(mandatory)*

### How to read the scenarios

- **Console output (stdout)** carries only the application's REPORT result (`Output:` and the position). **Console messages (stderr)** carry everything else: the application's prompt, echo and warnings, and the run scripts' own messages and any build output ([DEC-23][dec-23], [DEC-37][dec-37]). "(empty)" means nothing is written to that channel. Where a scenario with a test-data file states console messages (stderr), they are for reference; CI compares console output (stdout) and the exit code ([DEC-33][dec-33], page 10).
- **Given**, unless a scenario says otherwise: a fresh clone of the repository, a .NET SDK version 8 or later installed, and the current folder is the repository root.
- Page 2 has no scenarios for these non-functional requirements; the scenarios below come from the checks on page 3 ([NFR-02][nfr-02], [NFR-03][nfr-03], [NFR-08][nfr-08], [NFR-09][nfr-09]), [DEC-33][dec-33], [DEC-37][dec-37] and [DEC-31][dec-31] ("an empty input; input starting with a byte-order mark (console end-to-end, on all three systems)").
- The run scripts' progress lines are fixed (see the clarification of 2026-10-05, "progress lines"); the wording of their error messages (how-to-run message, file not found, no .NET SDK) is not fixed (page 3 [NFR-08][nfr-08], page 12 question A); their exit codes are. The progress lines for a run with the file `testdata/challenge-example-1.txt` are, in order: `Checking for a .NET SDK, version 8 or later...`, `Building the application (Release)...`, `Running the application with commands from testdata/challenge-example-1.txt` (the argument as given).

### User Story 1 - Run a challenge example with one script on any system (Priority: P1)

A reviewer who knows nothing about the build runs one script with an example file and sees the result. The script checks .NET, builds quietly and feeds the file to the application through console input (stdin), so the application itself still takes no arguments.

**Why this priority**: This is the first thing a reviewer does; it must work on Windows, Linux and macOS.

**Independent Test**: Run each system's script with each challenge example file and compare console output (stdout) with the expected file line by line (CI, User Story 2); run the error cases and check their exit codes.

**Acceptance Scenarios**:

1. **[NFR-08; DEC-04, DEC-37]** **Given** Windows cmd. **When** `run testdata\challenge-example-1.txt` is run. **Then console output (stdout):**
   ```text
   Output:
   0,1,NORTH
   ```
   **Console messages (stderr):** first the script's three progress lines, `Checking for a .NET SDK, version 8 or later...`, `Building the application (Release)...` and `Running the application with commands from testdata\challenge-example-1.txt`; then the application's echo lines:
   ```text
   > PLACE 0,0,NORTH
   > MOVE
   > REPORT
   ```
   No build output is shown, because the build succeeded; the script shows only its progress lines. **Exit code:** 0 (the application's exit code).

2. **[NFR-08; DEC-04, DEC-37]** **Given** Windows PowerShell. **When** `.\run.cmd testdata\challenge-example-1.txt` is run. **Then** console output (stdout), console messages (stderr) and exit code are as in scenario 1.

3. **[NFR-08; DEC-04, DEC-37]** **Given** bash or zsh on Linux or macOS. **When** `./run.sh testdata/challenge-example-1.txt` is run. **Then** console output (stdout), console messages (stderr) and exit code are as in scenario 1.

4. **[NFR-08; DEC-37, DEC-31 "the three challenge examples"]** **When** each script is run with `testdata/challenge-example-2.txt` and `testdata/challenge-example-3.txt` (in the form for its shell). **Then console output (stdout)** is, respectively:
   ```text
   Output:
   0,0,WEST
   ```
   ```text
   Output:
   3,3,NORTH
   ```
   **Console messages (stderr):** the script's three progress lines (the third names the file, as given) and then the application's echo lines; no build output. For `challenge-example-2.txt` the echo lines are:
   ```text
   > PLACE 0,0,NORTH
   > LEFT
   > REPORT
   ```
   For `challenge-example-3.txt` the echo lines are:
   ```text
   > PLACE 1,2,EAST
   > MOVE
   > MOVE
   > LEFT
   > MOVE
   > REPORT
   ```
   **Exit code:** 0

5. **[NFR-08; DEC-24, DEC-37]** **When** a script is run with no argument (`run`, `.\run.cmd` or `./run.sh`). **Then** after the quiet build the application starts for typing: console messages (stderr) show the progress lines `Checking for a .NET SDK, version 8 or later...`, `Building the application (Release)...` and then `Running the application: type commands, one per line; end with EXIT or Ctrl+Z then Enter.` (`run.cmd`) or `Running the application: type commands, one per line; end with EXIT or Ctrl+D.` (`run.sh`), then the prompt `> `, and typing `EXIT` and Enter ends it. **Console output (stdout):** (empty). **Exit code:** 0

5a. **[NFR-08; DEC-04, DEC-25, DEC-37]** **When** a script is run with no argument and its own console input redirected from `testdata/challenge-example-1.txt` (`run < testdata\challenge-example-1.txt` in cmd, `./run.sh < testdata/challenge-example-1.txt` in bash). **Then** the application reads that console input as redirected input: **console output (stdout)** is `Output:` and `0,1,NORTH`, **console messages (stderr)** are the two first progress lines, the no-argument `Running the application: type commands...` line of scenario 5 and the three echo lines of scenario 1, and the **exit code** is 0. (This is how CI proves the no-argument path, [CI-03][ci-03], since CI cannot type.)

6. **[NFR-08; DEC-37]** **When** a script is run with one argument naming a file that does not exist (for example `run missing.txt`), or naming a folder (for example `run testdata`). **Then console output (stdout):** (empty). **Console messages (stderr):** a message saying the file was not found (wording not fixed). The application is not started. **Exit code:** 1

6a. **[NFR-08; DEC-37; developer request 2026-10-05]** **When** `run.cmd` is run with one argument naming a file that does not exist as such because the name contains `*` or `?` (for example `run.cmd testdata\challenge-example-1.*`). **Then console output (stdout):** (empty). **Console messages (stderr):** a message saying the file name must not contain `*` or `?` (it contains `must not contain * or ?`; no "Running" line). The application is not started. **Exit code:** 1. `run.sh` needs no such rule: a quoted `*` or `?` names no regular file, so scenario 6 applies ("File not found", exit code 1; confirmed in Git Bash).

7. **[NFR-08; DEC-37]** **When** a script is run with more than one argument (for example `./run.sh a.txt b.txt`). **Then console output (stdout):** (empty). **Console messages (stderr):** the script's how-to-run message (wording not fixed). **Exit code:** 1

8. **[NFR-08; DEC-37]** **Given** no .NET SDK, version 8 or later, is installed: either `dotnet` cannot be found, or `dotnet --list-sdks` lists only SDKs older than version 8. **When** a script is run (with or without a file). **Then console output (stdout):** (empty). **Console messages (stderr):** a message saying where to get the .NET SDK, `https://dotnet.microsoft.com/download` (wording not fixed). Nothing is built and the application is not started. **Exit code:** 1

9. **[NFR-08; DEC-37]** **Given** the application does not build. **When** a script is run (with or without a file). **Then console output (stdout):** (empty). **Console messages (stderr):** the build output. The application is not started. **Exit code:** the build's exit code (the exit code that `dotnet build` of the application project in `Release` gives for the same source).

10. **[NFR-08; DEC-10, DEC-20, DEC-31 "an empty input", "input starting with a byte-order mark"]** **When** each script is run with `testdata/empty.txt`, `testdata/bom-challenge-example-1.txt` and `testdata/exit-mid-file.txt`. **Then console output (stdout)** matches each file's expected-output file line by line (empty; `Output:` and `0,1,NORTH`; `Output:` and `0,0,NORTH`), and the **exit code** is 0. **Console messages (stderr)**, in every case the script's three progress lines first (as in scenario 1, naming the file), then: for `empty.txt` nothing more; for `bom-challenge-example-1.txt` the lines `> PLACE 0,0,NORTH`, `> MOVE`, `> REPORT` (no marker in the first echo line and no warning, [FR-11][fr-11]); for `exit-mid-file.txt` the lines `> PLACE 0,0,NORTH`, `> REPORT`, `> EXIT`.

---

### User Story 2 - Proof that it works on Windows, Linux and macOS (Priority: P1)

Every push runs a GitHub Actions workflow on Windows, Linux and macOS that builds the application, runs all three test levels and runs that system's script with the challenge examples, so a reviewer can see real evidence for each system without owning all three.

**Why this priority**: [DEC-33][dec-33] requires evidence, not a note; macOS can be proven only this way.

**Independent Test**: Push a commit and watch the workflow pass on all three systems.

**Acceptance Scenarios**:

1. **[NFR-02; DEC-31, DEC-33]** **When** any commit is pushed to GitHub. **Then** the workflow runs on `windows-latest`, `ubuntu-latest` and `macos-latest`, and on each: builds the solution; runs the unit tests and Runner whole-flow tests in memory; runs the console end-to-end tests of the built application (including every test-data file and the byte-order-mark input, spec 002); runs that system's run script (`run.cmd` from cmd on Windows, `run.sh` on Linux and macOS) with each test-data input file, including the three challenge example files, and compares console output (stdout) with the expected file line by line and checks exit code 0 ([CI-01][ci-01]); runs the script from inside `testdata` ([CI-01][ci-01]); runs the script with a missing file, with a folder and with two arguments and checks exit code 1 ([CI-02][ci-02]); runs the script with no argument and a test-data file redirected to its console input ([CI-03][ci-03]); runs the script with no .NET SDK found and with only an older SDK found and checks exit code 1 ([CI-04][ci-04]); runs the script on a copy of the repository that does not build and checks it exits with the build's exit code ([CI-05][ci-05]); and checks the line endings and the executable `run.sh` ([CI-06][ci-06]). The run passes only when every step passes on all three systems.

2. **[NFR-02; DEC-33]** **When** any build, test, comparison or exit-code check fails on a system. **Then** the run on that system stops and is marked failed, and the push is shown as failed (page 9, test flow).

3. **[NFR-02, NFR-04 (spec 002); DEC-33]** **Given** the repository's `.gitattributes`. **Then** every text file is checked out with LF line endings and every `.cmd` file with CRLF, on every system, and `run.sh` is stored as executable, so `./run.sh` runs straight after a clone on Linux and macOS (checked by [CI-06][ci-06], and by [CI-01][ci-01] running `./run.sh` directly).

---

### User Story 3 - Build, test and run from the README alone (Priority: P2)

A reviewer opens the repository and, reading only the short README (about one screen), builds, tests and runs the application in cmd, PowerShell or bash, and runs the three challenge examples with ready-to-copy commands that show the expected output.

**Why this priority**: the challenge asks for a short README with build and run instructions; it is the reviewer's entry point.

**Independent Test**: Follow the README word for word on a fresh clone in cmd, PowerShell and bash.

**Acceptance Scenarios**:

1. **[NFR-09; DEC-37]** **Given** a fresh clone. **When** a reviewer follows the README word for word in cmd, in PowerShell and in bash. **Then** every command in it works as written, and every output it shows (including the challenge examples' expected output and the `Output:` format) matches what the application prints.

2. **[NFR-09; DEC-33, DEC-37]** **Given** all tests pass in CI. **Then** the README shows the CI status badge, states that the solution is tested on Windows, Linux and macOS through the workflow, and links to it.

---

### User Story 4 - Runs where only a newer .NET is installed (Priority: P3)

A reviewer may have only .NET 9 or later, because .NET 8 support ends in November 2026. The application targets .NET 8 but still runs.

**Why this priority**: A risk on the review date, handled by a single build setting.

**Independent Test**: Review of the build settings (page 3), backed by the test [BS-01][bs-01] that reads the application's project file.

**Acceptance Scenarios**:

1. **[NFR-03; DEC-37]** **Given** the application's project file (created by spec 001, [SK-02][sk-02]). **Then** it targets .NET 8 and sets `RollForward` = `LatestMajor`, so the built application runs where only a later major version of .NET is installed.

---

### Edge Cases

- A test-data file that starts with a byte-order mark keeps it in the repository (`.gitattributes` changes line endings only), so the byte-order-mark run tests what [DEC-20][dec-20] describes ([DEC-31][dec-31], [DEC-33][dec-33]).
- The empty test-data input and its expected output are both empty files ([FR-16][fr-16], spec 002).
- In Windows PowerShell 5.1, `>` saves output as UTF-16, a known limitation ([DEC-23][dec-23], page 9); so CI runs `run.cmd` from cmd when it saves console output (stdout) for its comparison.
- The scripts never pass an argument to the application; a file is always fed through console input (stdin), so the application's how-to-run message ([FR-17][fr-17], spec 002) is never triggered by a script ([DEC-04][dec-04], [DEC-37][dec-37]).
- The build inside a script reads no console input, so with no argument and redirected console input (scenario 5a) every line reaches the application.
- A script finds the application relative to its own folder, so it also works when called from another folder; the file argument is taken relative to the current folder (see Assumptions). [CI-01][ci-01] tests this with one run from inside `testdata`.
- The first use of the .NET SDK on a machine can print a welcome text; the script captures it with the build output, so it never reaches console output (stdout).

## Requirements *(mandatory)*

### Non-Functional Requirements (owned)

- **NFR-02 Works on Windows, Linux and macOS**<a id="nfr-02"></a> ([DEC-31][dec-31], [DEC-33][dec-33]): The application, its tests and its run scripts MUST work on all three systems. A GitHub Actions workflow in `.github/workflows/` MUST, on every push, on `windows-latest`, `ubuntu-latest` and `macos-latest`: build the application; run all tests, including the console end-to-end tests (spec 002); run that system's run script with a challenge example file and compare console output (stdout) with the expected file, line by line. Any failure fails the run on that system. The README MUST link to these CI runs.
  - **NFR-02a**<a id="nfr-02a"></a> ([DEC-33][dec-33]): A `.gitattributes` file MUST keep line endings consistent: LF for all text files, CRLF for `.cmd` files. `run.sh` and `menu.sh` MUST be committed as executable.
- **NFR-03 Runs on .NET 8 or later**<a id="nfr-03"></a> ([DEC-37][dec-37]): The application MUST target .NET 8 and also run on a machine where only a later major version of .NET is installed (`RollForward` = `LatestMajor` in the application's project file, set up by spec 001, [SK-02][sk-02]). Checked by a review of the build settings (page 9), and by test [BS-01][bs-01].
  - **BS-01**<a id="bs-01"></a> ([NFR-03][nfr-03]; [DEC-37][dec-37]): A console end-to-end-project test MUST read `src/ToyRobot/ToyRobot.csproj` and check that its target framework is `net8.0` and its `RollForward` is `LatestMajor`, so a change to either fails the tests on every system.
- **NFR-08 Run scripts**<a id="nfr-08"></a> ([DEC-04][dec-04], [DEC-33][dec-33], [DEC-37][dec-37]): A reviewer MUST be able to start the application with one script and no knowledge of the build: `run.cmd` (Windows cmd and PowerShell) and `run.sh` (Linux and macOS), at the repository root. Each script, in order:
  1. checks that a .NET SDK, version 8 or later, is installed: `dotnet` must be found and `dotnet --list-sdks` must list at least one SDK whose major version is 8 or more; if not, says where to get it (`https://dotnet.microsoft.com/download`) and exits with code 1;
  2. builds the application project `src/ToyRobot/ToyRobot.csproj` quietly in the `Release` configuration: all build output is captured and the build reads no console input; if the build succeeds no build output is shown; if it fails, the captured build output is written to console messages (stderr) and the script exits with the build's exit code;
  3. checks the arguments: with none, starts the application with the script's own console input (typed, or redirected if the script's input is redirected); with one, checks it names an existing file (a missing path or a folder: prints a message and exits with code 1; `run.cmd` also rejects a name containing `*` or `?` with a message and exit code 1, before the "Running" line) and feeds that file to the application through console input (stdin), so the application itself still takes no arguments; with more than one, prints the script's how-to-run message and exits with code 1;
  4. exits with the application's exit code.

  Progress lines (developer's request, 2026-10-05): each script writes these lines to console messages (stderr), in order: `Checking for a .NET SDK, version 8 or later...` before step 1; `Building the application (Release)...` before step 2; and, once the build has succeeded and the arguments are checked, just before the application starts, with a file `Running the application with commands from <file>` (the argument as given), with no argument `Running the application: type commands, one per line; end with EXIT or Ctrl+Z then Enter.` (`run.cmd`) or `Running the application: type commands, one per line; end with EXIT or Ctrl+D.` (`run.sh`). A how-to-run message or file-not-found error, or a failed build or missing SDK, is reported without the "Running" line. On success a script writes nothing to console output (stdout) and nothing else to console messages (stderr).

  The application started is the Release build at the path on page 5: `src\ToyRobot\bin\Release\net8.0\ToyRobot.exe` (Windows) or `src/ToyRobot/bin/Release/net8.0/ToyRobot` (Linux and macOS). Each script MUST write its own messages and any build output to console messages (stderr), so console output (stdout) carries only the application's REPORT result. The wording of the error messages is not fixed; the progress lines are fixed as above. There is no demo mode.
- **NFR-09 Short README**<a id="nfr-09"></a> ([DEC-37][dec-37]): A reviewer MUST be able to build, test and run the application by reading only the README at the repository root, which fits about one screen and holds: what it is; requirements; how to run it with the scripts, in the form for each shell (`run commands.txt` in cmd, `.\run.cmd commands.txt` in PowerShell, `./run.sh commands.txt` in bash or zsh); ready-to-copy commands that run the three challenge examples from the test data with their expected output; the build, test and direct run commands, including the PowerShell form (`cmd /c "ToyRobot < commands.txt"`, with the built application's path from page 5); the output format with an example (`Output:` then `X,Y,F` on console output (stdout), everything else on console messages (stderr)); a few key assumptions with a link to `docs/design-analysis/design-decisions.md`; a note that the tests run on Windows, Linux and macOS via GitHub Actions, with the CI badge and a link; "How this would evolve" (multiple robots, a configurable table and obstacles, none built, [DEC-39][dec-39]); and where the development-process material and AI artefacts are (`CLAUDE.md`, the design-phase and implementation-phase prompt logs, the Spec Kit files, the handwritten analysis, the clarifications, the design analysis and the solution design). It is written in stages: a skeleton with the parts already fixed by the decisions, real commands added as each one works, the CI badge and test-data list once all tests pass, and a final check that follows it word for word on a fresh clone in cmd, PowerShell and bash.

### Menu scripts (developer request, 2026-10-05)

- **MN-01**<a id="mn-01"></a> ([NFR-08][nfr-08]; [DEC-41][dec-41]): `menu.cmd` (Windows: cmd and PowerShell) and `menu.sh` (Linux and macOS) at the repository root MUST show a numbered list and run the choice: 1 to 7 run one test-data file of [TD-01][td-01] through the run script with no console input, show its console output (stdout), and write `PASS: <name>` when it matches `testdata/<name>.expected.txt` line by line (carriage returns ignored) and the exit code is 0, otherwise `FAIL: <name> - <reason>`; 8 runs 1 to 7 in order and then writes `All 7 files PASS` or `<n> of 7 files FAIL`; 9 runs the run script with no argument (a typed session) and then writes `Typed session ended.`; 10 runs `dotnet test` on `ToyRobot.sln` with no console input; 0 ends the menu with exit code 0; any other choice writes `Not a choice: <choice>`. After every choice except 0 the list is shown again. The end of the input ends `menu.sh`; in `menu.cmd` three empty choices in a row end it. The menu's own lines are console output (stdout); the run script's messages stay on console messages (stderr). Each script works when called from another folder. Colours (developer request, 2026-10-05): `PASS` lines green, `FAIL` lines red and the run's console output bright cyan, with ANSI colour codes; `menu.sh` only when its console output is a terminal; neither script when the environment variable `NO_COLOR` is set.

### Test data (delivered by this spec; run by spec 002's console end-to-end tests and by CI)

- **TD-01**<a id="td-01"></a> ([DEC-31][dec-31], [DEC-37][dec-37]; [NFR-02][nfr-02]): `testdata/` MUST hold input files with matching expected-output files. Each pair is `testdata/<name>.txt` (the input) and `testdata/<name>.expected.txt` (the exact console output (stdout) for that input). The set is:

  | Name | Input lines | Expected console output (stdout) |
  |---|---|---|
  | `challenge-example-1` | `PLACE 0,0,NORTH`<br>`MOVE`<br>`REPORT` | `Output:`<br>`0,1,NORTH` |
  | `challenge-example-2` | `PLACE 0,0,NORTH`<br>`LEFT`<br>`REPORT` | `Output:`<br>`0,0,WEST` |
  | `challenge-example-3` | `PLACE 1,2,EAST`<br>`MOVE`<br>`MOVE`<br>`LEFT`<br>`MOVE`<br>`REPORT` | `Output:`<br>`3,3,NORTH` |
  | `bom-challenge-example-1` | A UTF-8 byte-order mark, then the lines of `challenge-example-1` | `Output:`<br>`0,1,NORTH` |
  | `exit-mid-file` | `PLACE 0,0,NORTH`<br>`REPORT`<br>`EXIT`<br>`MOVE`<br>`REPORT` | `Output:`<br>`0,0,NORTH` |
  | `empty` | (no lines; an empty file) | (empty file) |
  | `long-session` | 36 lines: robot commands before any PLACE, a PLACE off the table, a PLACE on the table, the robot walked along two edges with a MOVE blocked at each corner, an unknown command, a PLACE with values missing, a blank line, a second PLACE in lower case, a line with leading spaces, a full turn, a negative PLACE, and four REPORTs | `Output:`<br>`0,0,WEST`<br>`Output:`<br>`4,4,EAST`<br>`Output:`<br>`3,0,EAST`<br>`Output:`<br>`3,0,EAST` |

- **TD-02**<a id="td-02"></a> ([DEC-31][dec-31]; [NFR-02][nfr-02]): A console end-to-end test MUST check that all seven pairs of [TD-01][td-01], including `challenge-example-1`, `challenge-example-2` and `challenge-example-3`, are present in `testdata/` (each input file and its expected-output file), and that `bom-challenge-example-1.txt` starts with the UTF-8 byte-order mark bytes `EF BB BF`, so that spec 002's test over every test-data pair ([E2E-01][e2e-01]) can never pass with a pair missing or the byte-order mark lost.

### CI checks of the run scripts (planned by this spec)

- **CI-01**<a id="ci-01"></a> ([NFR-02][nfr-02], [NFR-08][nfr-08]; [DEC-31][dec-31], [DEC-33][dec-33]): On each system, CI MUST run that system's script (`run.cmd` from cmd on Windows; `./run.sh` on Linux and macOS) with each test-data input file of [TD-01][td-01] as its one argument, save console output (stdout) only, compare it with the expected file line by line (Windows and Linux line endings both pass), and check exit code 0; a mismatch or another exit code fails the run. Console messages (stderr) are not compared here (the separate check [CI-07][ci-07] compares the progress lines): the lines stated in User Story 1 scenarios 1, 4 and 10 are for reference, as in spec 002 User Story 1 scenario 5 (page 9 level 3, page 10, [DEC-33][dec-33]). CI MUST also run the script once from inside `testdata` (`challenge-example-1.txt` as its one argument, the script reached as `..\run.cmd` or `../run.sh`), compare its console output (stdout) with `testdata/challenge-example-1.expected.txt`, and check exit code 0.
- **CI-02**<a id="ci-02"></a> ([NFR-08][nfr-08]; [DEC-37][dec-37]): On each system, CI MUST run the script with a file that does not exist, with a folder (`testdata`) and with two arguments, and check that each exits with code 1, writes nothing to console output (stdout) and writes a message (not empty) to console messages (stderr). On Windows, CI MUST also run `run.cmd` with `testdata\challenge-example-1.*` and check the same, and that the message contains `must not contain * or ?` (developer request 2026-10-05).
- **CI-03**<a id="ci-03"></a> ([NFR-08][nfr-08]; [DEC-04][dec-04], [DEC-37][dec-37]): On each system, CI MUST run the script with no argument and its console input redirected from `testdata/challenge-example-1.txt`, compare console output (stdout) with `testdata/challenge-example-1.expected.txt` line by line, and check exit code 0.
- **CI-04**<a id="ci-04"></a> ([NFR-08][nfr-08]; [DEC-37][dec-37]): On each system, CI MUST run the script with a test-data file (a) with a `PATH` in which `dotnet` cannot be found and (b) with a `PATH` whose only `dotnet` is a stand-in that lists one SDK of version 7 for `--list-sdks`, and check that each exits with code 1, writes nothing to console output (stdout) and writes console messages (stderr) that contain `https://dotnet.microsoft.com/download`.
- **CI-05**<a id="ci-05"></a> ([NFR-08][nfr-08]; [DEC-37][dec-37]): On each system, CI MUST make a temporary copy of the repository's committed files outside the checkout, add one source file that does not compile to the application project in that copy, run the script there with a test-data file, and check that it writes nothing to console output (stdout), writes the build output to console messages (stderr), and exits with the same non-zero exit code that `dotnet build` of the application project in `Release` gives in that copy. The checkout itself is never changed.
- **CI-06**<a id="ci-06"></a> ([NFR-02a][nfr-02a]; [DEC-33][dec-33]): On each system, CI MUST check from git's view of the checkout that every `.cmd` file has CRLF line endings, every other text file has LF line endings (or none), and `run.sh` and `menu.sh` are stored with the executable mode.
- **CI-07**<a id="ci-07"></a> ([NFR-08][nfr-08]; [DEC-37][dec-37], [DEC-23][dec-23]; developer request 2026-10-05): On each system, CI MUST run the script once with `testdata/challenge-example-1.txt`, save console messages (stderr) and console output (stdout), and check, with carriage returns removed, that the first three lines of console messages (stderr) are exactly `Checking for a .NET SDK, version 8 or later...`, `Building the application (Release)...` and `Running the application with commands from testdata/challenge-example-1.txt` (on Windows, `testdata\challenge-example-1.txt`, the argument as given), and that console output (stdout) still equals `testdata/challenge-example-1.expected.txt`.
- **CI-08**<a id="ci-08"></a> ([NFR-08][nfr-08]; [MN-01][mn-01]; [DEC-41][dec-41]; developer request 2026-10-05): On each system, CI MUST run that system's menu script, with `NO_COLOR` set, with console input holding the choices `1`, `8`, `x` and `0`, one per line (from a file on Windows), save console output (stdout), and check that it contains `PASS: challenge-example-1`, `All 7 files PASS` and `Not a choice: x`, has no line starting with `FAIL: `, and that the exit code is 0.

### Key Entities

- **Run script**: `run.cmd` or `run.sh`; checks .NET, builds, runs the application with or without a file.
- **Test-data pair**: an input file and its expected console output (stdout) file in `testdata/`.
- **CI workflow**: the GitHub Actions workflow that runs on Windows, Linux and macOS on every push.
- **`.gitattributes`**: the line-ending rules (LF for text, CRLF for `.cmd`).
- **README**: the root `README.md` that tells a reviewer how to build, test and run the application.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**<a id="sc-001"></a> ([NFR-02][nfr-02]): The CI workflow passes on all 3 systems (Windows, Linux, macOS) for the final commit, with every test level and every script check green.
- **SC-002**<a id="sc-002"></a> ([NFR-08][nfr-08], [CI-01][ci-01], [CI-03][ci-03]): On each of the 3 systems, the run script's console output (stdout) for each of the 7 test-data files (including the 3 challenge examples) matches the expected file line by line with exit code 0 (21 of 21 comparisons), the no-argument run with redirected console input matches too (3 of 3), and so does the run from inside `testdata` (3 of 3).
- **SC-003**<a id="sc-003"></a> ([NFR-08][nfr-08], [CI-02][ci-02], [CI-04][ci-04], [CI-05][ci-05]): On all 3 systems, the script error cases give exit code 1 for a missing file, a folder, more than one argument (and, for `run.cmd`, a file name with `*` or `?`), no `dotnet` found and only an older SDK found, and a failed build gives the build's exit code; in every error case console output (stdout) is empty and console messages (stderr) are not empty (a failed build shows `CS1029`, and the missing-SDK message names `https://dotnet.microsoft.com/download`).
- **SC-004**<a id="sc-004"></a> ([NFR-09][nfr-09]): A reviewer following only the README, word for word, on a fresh clone, completes build, test and the three challenge example runs in each of cmd, PowerShell and bash with no step failing, and the README fits about one screen.
- **SC-005**<a id="sc-005"></a> ([NFR-03][nfr-03]): The application's project file sets the target to .NET 8 and `RollForward` = `LatestMajor` (review: 1 of 1 setting present; [BS-01][bs-01] passes on all 3 systems).
- **SC-007**<a id="sc-007"></a> ([NFR-02a][nfr-02a], [CI-06][ci-06]): On all 3 systems, 0 `.cmd` files without CRLF, 0 other text files with CRLF, and `run.sh` and `menu.sh` stored as executable.
- **SC-006**<a id="sc-006"></a> ([TD-01][td-01], [TD-02][td-02]): All 7 test-data pairs exist, and spec 002's console end-to-end tests pass for every one of them on all 3 systems.
- **SC-008**<a id="sc-008"></a> ([MN-01][mn-01], [CI-08][ci-08]): On all 3 systems, the menu script with the choices `1`, `8`, `x`, `0` reports PASS for every test-data file and ends with exit code 0.

## Assumptions

- The CI workflow uses only GitHub Actions' standard steps to check out the repository and install the .NET SDK; no other tool or library is added (Spec Kit brief rule 7; [DEC-39][dec-39]).
- The repository is hosted at `https://github.com/hetal-patel78/toy-robot-spec-driven`; the README's CI badge and link point to its Actions page.
- <a id="decided-testdata-set"></a>**Decided in the run: the test-data set and file names.** [DEC-37][dec-37] asks for "input files with matching expected-output files" and [DEC-31][dec-31] for the challenge examples, an empty input and a byte-order-mark input console end-to-end; the brief's real runs (step 10) also use a file with EXIT in the middle. Chosen: the six pairs in [TD-01][td-01], named `<name>.txt` and `<name>.expected.txt`. Other options: also add the Brainstorm traces or a file of bad lines (already covered by spec 001's and spec 002's tests; more files for the README to list); a separate `expected/` folder (two places to keep in step). The seventh pair, `long-session`, was added later at the developer's request (see "Decided (developer, 2026-10-05): a long mixed session" below).
- <a id="decided-challenge-examples-in-ci"></a>**Decided in the run: which challenge examples CI runs through the scripts.** [DEC-33][dec-33] says "a challenge example file". Chosen: all three challenge examples on each system, because the README gives ready-to-copy commands for all three and CI then proves each of them on every system; [CI-01][ci-01] also runs the other three test-data files (see "CI runs every test-data file through the scripts" below). Other options: one example only (meets [DEC-33][dec-33]'s minimum with less evidence).
- <a id="decided-windows-shell"></a>**Decided in the run: Windows shell in CI.** [DEC-37][dec-37] says `run.cmd` works in cmd and PowerShell. Chosen: CI runs `run.cmd` from cmd, because Windows PowerShell 5.1 `>` saves UTF-16 ([DEC-23][dec-23]) and would make the saved console output (stdout) differ byte for byte; the PowerShell form is checked in the real runs on Windows (brief step 10) and in the README check. Other options: also run it from PowerShell in CI (needs an encoding-aware comparison).
- <a id="decided-error-paths-checks"></a>**Decided: how the "no .NET SDK", "failed build" and "no argument" paths are checked.** Page 3 says the script error cases are checked by their exit codes but does not say where. Chosen: automated CI steps on all three systems that do not depend on where the runner image installs the SDK: [CI-04][ci-04] runs the script with a `PATH` that holds no `dotnet` (on Linux and macOS the script is started as `/bin/bash run.sh`, and the script uses only shell built-ins before its .NET check, so it runs with that `PATH`; on Windows the `PATH` is only `%SystemRoot%\System32`) and with a `PATH` whose only `dotnet` is a stand-in listing a version 7 SDK; [CI-05][ci-05] breaks the build in a temporary copy of the repository, never in the checkout; [CI-03][ci-03] feeds a test-data file to the script's console input with no argument, because CI cannot type. The same cases are also run by hand on Windows during the real runs (brief step 10). Other options: check them by hand only (leaves error paths without an automated test); hide the real SDK on the runner (fragile across runner images).
- <a id="decided-long-session"></a>**Decided (developer, 2026-10-05): a long mixed session.** The six pairs each show one thing. Chosen: a seventh pair, `long-session`, that runs many kinds of line in one file, so one file shows the rules working together: ignored commands before PLACE, PLACE off and on the table, MOVE blocked at the edges, bad lines, a blank line, lower case and spaces, a full turn, and REPORT more than once. Its expected file holds only the console output (stdout), like every pair; the warnings go to console messages (stderr), which [CI-01][ci-01] does not compare. The expected output was produced by running the input through the built application and checked by hand, step by step. Other options: also compare console messages (stderr) for this file (a new kind of expected file and comparison).
- <a id="decided-menu-scripts"></a>**Decided (developer, 2026-10-05): menu scripts ([DEC-41][dec-41]).** Chosen: `menu.cmd` and `menu.sh`, which list the seven test-data files, all of them, a typed session and the automated tests by number, and run every choice through the run script. A file run and its comparison get no console input (in `menu.cmd`, `fc` would otherwise read the menu's input). Other options: a PowerShell menu (needs PowerShell 7 on Linux and macOS, and can be blocked by the execution policy on Windows, the reason for [DEC-37][dec-37]'s scripts); options on the run scripts (would change their arguments, which [NFR-08][nfr-08] fixes).
- <a id="decided-every-testdata-file"></a>**Decided: CI runs every test-data file through the scripts.** [DEC-33][dec-33] asks for "a challenge example file"; spec scenario 10 also names the empty, byte-order-mark and EXIT-mid-file inputs. Chosen: [CI-01][ci-01] runs every [TD-01][td-01] input through each script on each system (seven inputs, 21 comparisons), so the empty input and the byte-order mark are proved through the scripts as well as console end-to-end. Other options: only the three challenge examples (scenario 10 left without an automated test).
- <a id="decided-script-build"></a>**Decided: the script build.** Chosen: the scripts build only `src/ToyRobot/ToyRobot.csproj` in `Release` (page 5 path), capture all output of the .NET check and the build, show it only when the build fails, and give the build no console input. On success the scripts show no build output, only their progress lines (see "Decided (developer, 2026-10-05)" below). Other options: build `ToyRobot.sln` (also builds the test projects, slower, and a test-project error would stop a reviewer running the application); show the build output as progress (would mix build text into console messages (stderr) on every run).
- <a id="decided-sdk-check"></a>**Decided: the .NET SDK check.** Chosen: `dotnet --list-sdks` must list an SDK with major version 8 or more; the message names `https://dotnet.microsoft.com/download`. Other options: `dotnet --version` (reports the SDK picked for the current folder, which can be lower than an installed one and fails differently when no SDK is installed); only check that `dotnet` exists (does not check "version 8 or later").
- <a id="decided-folder-not-file"></a>**Decided: a folder is not a file.** Chosen: a script's single argument must name an existing file; a folder gives the "file not found" message and exit code 1 (tested by [CI-02][ci-02]). Other options: let the shell fail on redirecting a folder (exit code and message differ by shell).
- <a id="decided-ci-comparison"></a>**Decided: how CI compares console output.** Chosen: the saved console output (stdout) and the expected file are compared with carriage returns removed, in a bash step on every system (Git Bash on Windows, which `windows-latest` provides), so Windows and Linux line endings both pass ([DEC-31][dec-31]). The run itself is always from that system's shell (`run.cmd` from cmd). Other options: a separate comparison per shell (more code to keep in step).
- <a id="decided-gitignore"></a>**Decided: `.gitignore`.** The scripts and CI build into `bin/` and `obj/`. Chosen: the root `.gitignore` is created by spec 001 (SK skeleton) with `bin/`, `obj/`, `TestResults/` and `.vs/`; this spec only checks it (it holds those four lines and `git status` lists no build output) and adds only a missing line, removing nothing. It changes no behaviour. Other options: create it here (two specs would create the same file; each requirement is owned by one spec); none (build output shows as untracked files and can be committed by mistake).
- <a id="decided-ci01-ci03-stdout-only"></a>**Decided: [CI-01][ci-01] and [CI-03][ci-03] compare console output (stdout) and the exit code only.** Page 3 and [NFR-08][nfr-08] say a script writes its messages to console messages (stderr), but not how that is tested. Chosen: [CI-01][ci-01] and [CI-03][ci-03] compare console output (stdout) line by line and the exit code only ([DEC-33][dec-33]; page 3; page 9 level 3; page 10); console messages (stderr) are checked only in the script error cases, where [DEC-37][dec-37] states what is shown: [CI-02][ci-02] not empty, [CI-04][ci-04] contains `https://dotnet.microsoft.com/download`, [CI-05][ci-05] contains `CS1029`. Other options: compare the echo lines on console messages (stderr) for every test-data file (contradicts page 10 and page 9, which leave the echo to the Runner whole-flow tests, and spec 002's console end-to-end decision).
- <a id="decided-other-folder"></a>**Decided: [CI-01][ci-01] also runs a script from another folder.** The scripts find the application relative to their own folder (see the assumption on where a script looks). Chosen: [CI-01][ci-01] adds one run from inside `testdata` (`call ..\run.cmd challenge-example-1.txt` in cmd, `(cd testdata && ../run.sh challenge-example-1.txt)` in bash), compared with `testdata/challenge-example-1.expected.txt` with exit code 0, on each system. Other options: leave the claim untested; run every pair from another folder (more steps for the same path).
- <a id="decided-local-ci06-check"></a>**Decided: where the local check of [CI-06][ci-06] runs.** The working copy was checked out with `core.autocrlf=true` before `.gitattributes` existed, and `git add --renormalize` does not rewrite files on disk, so its text files stay `w/crlf` locally while the repository is correct; the usual fixes (`reset --hard`, deleting files) are not allowed (brief rule 12). Chosen: the local [CI-06][ci-06] check runs in a fresh clone under `%TEMP%` (`git clone -c core.autocrlf=true <repository root>` into a new folder under `%TEMP%` named after the task, for example `%TEMP%\ci06-T013a`; an existing folder is never reused, because `git clone` refuses a non-empty folder and nothing is deleted), not in the working copy; the working copy's `w/crlf` values are expected and are not changed. Other options: re-checkout the working copy (forbidden); skip the local run and rely on CI (the check would first be seen on GitHub only).
- <a id="decided-workflow-shape"></a>**Decided: CI workflow shape.** Chosen (research [R-05][r-05]): one workflow `CI` in `.github/workflows/ci.yml`, triggered `on: push` (every branch); one job with `strategy.fail-fast: false` and `matrix.os` of `windows-latest`, `ubuntu-latest`, `macos-latest`; `timeout-minutes: 30` on the job, so a step that hangs (a script waiting for input, a build server holding a pipe open) fails that system within 30 minutes and not after GitHub's 360-minute default; `actions/checkout@v5` and `actions/setup-dotnet@v5` with `dotnet-version: 8.0.x`; the unit and Runner whole-flow tests and the console end-to-end tests as two separate steps. Other options: also trigger on `pull_request` (not asked for); one `dotnet test ToyRobot.sln` step (does not show the two levels of page 9's test flow); no job timeout (a hang can run up to 6 hours).
- <a id="decided-gitattributes"></a>**Decided: `.gitattributes` content.** Chosen (research [R-10][r-10]): the three lines `* text=auto eol=lf`, `*.cmd text eol=crlf`, `*.pdf binary`. Other option: without `*.pdf binary`, relying on `text=auto` to leave the PDFs alone (works, but the PDFs' handling is then implicit).
- <a id="decided-td02-bs01-location"></a>**Decided: where [TD-02][td-02] and [BS-01][bs-01] live.** Chosen (research [R-12][r-12]): both in `tests/ToyRobot.ConsoleEndToEndTests/DeliveryTests.cs`, as `Test_data_holds_every_input_and_expected_pair`, `Byte_order_mark_test_file_starts_with_the_mark` and `Application_targets_net8_and_rolls_forward_to_the_latest_major_version`; the repository root is found by walking up from `AppContext.BaseDirectory` to the folder holding `ToyRobot.sln`; spec 002 exposes no shared helper for it. Other options: add a public `AppProcess.RepositoryRoot` in spec 002 (changes a finished spec for one call); a separate test project (not on page 5; one more project to build and run).
- <a id="decided-deliverytests-namespace"></a>Decided: DeliveryTests is in namespace ToyRobot.ConsoleEndToEndTests, following spec 001's folder-equals-namespace rule and spec 002's namespaces. Other options: a separate namespace for delivery tests (not a folder of its own).
- <a id="decided-bs01-ci06"></a>**Decided: [BS-01][bs-01] and [CI-06][ci-06].** Page 3 checks [NFR-03][nfr-03] by review and [NFR-02][nfr-02]'s line endings are stated by [DEC-33][dec-33] without a check. Chosen: the review stays, and [BS-01][bs-01] (a test that reads the project file) and [CI-06][ci-06] (a check of git's view of line endings and `run.sh`'s mode) make both repeatable on every push. Other options: review only (a later change could go unnoticed until a reviewer's machine fails).
- <a id="decided-script-lookup"></a>**Decided in the run: where the script looks for the solution and the file.** The sources do not say. Chosen: each script finds the solution relative to its own folder (so it also works when called from another folder), and takes the file argument as given, relative to the current folder; the README always runs it from the repository root. Other options: require the current folder to be the repository root (fails in a confusing way otherwise).
- <a id="decided-readme-root"></a>**Decided in the run: the README is the existing root `README.md`.** The root `README.md` today describes the design stage. Chosen: it becomes the README by being changed in place; its "Where to look" links are kept as the part that says where the development-process material and AI artefacts are, and its "Status" text is updated to describe the finished application. Nothing is moved or deleted. Other options: a new file (two READMEs at the root would confuse a reviewer).
- <a id="decided-readme-stages"></a>**Decided in the run: README stages and the Release build.** [DEC-37][dec-37] says the README skeleton is written "when the solution is created", which happens in spec 001, but the README is owned by this spec. Chosen: all README stages are done by this spec's tasks, in [DEC-37][dec-37]'s order (skeleton, commands as each works, badge and test-data list once CI passes); the implementation follows the README word for word on a fresh local clone on Windows in cmd, PowerShell and Git Bash and records the result, and the final word-for-word check on a fresh clone in cmd, PowerShell and bash is repeated by the developer before finishing (brief, "Left for the developer"). The README's build command builds the `Release` configuration, so its direct run commands use the page 5 path. Other options: a README skeleton task in spec 001 (crosses the ownership in the brief's table); README direct-run commands with the Debug path (does not match page 5).
- <a id="decided-cmd-exit-code"></a>**Decided: how a cmd step saves an exit code.** Chosen: the redirection is written first (`>"<file>" echo %ERRORLEVEL%`, or `set "rc=%ERRORLEVEL%"` then `>"<file>" echo %rc%`), and every saved code is read with `tr -d '\r\n '` before it is compared with 0 or 1. Other options: `echo %ERRORLEVEL%> "<file>"` (a one-digit code before `>` is read as a handle number, so the code is not saved); comparing the raw file (it ends in a carriage return).
- <a id="decided-ci06-failing-locally"></a>**Decided: the [CI-06][ci-06] line-ending test is shown to fail locally, not on GitHub.** Chosen: T013a is committed but not pushed, and its failure is shown in a fresh clone made with `git clone -c core.autocrlf=true`; T013a and T003 are pushed together. Other options: push the failing check and watch `windows-latest` fail (the brief forbids pushing work that is not passing).
- <a id="decided-us1-stderr-reference"></a>**Decided: User Story 1 scenarios 1, 4, 5a and 10 state console messages (stderr) for reference; CI does not compare them,** as spec 002 User Story 1 scenario 5 (page 10). Other options: compare them in [CI-01][ci-01] and [CI-03][ci-03] (contradicts page 9, page 10 and [DEC-33][dec-33] "compares its standard output").
- **Decided: [CI-05][ci-05] copy folder form.** On `windows-latest`, `tar -x -C "$RUNNER_TEMP/broken"` failed because Git Bash passed the Windows form of `$RUNNER_TEMP` (`D:\a\_temp`) to tar unchanged. Chosen: the copy folder is given in the shell's own form (`broken=$(cd "$RUNNER_TEMP/broken" && pwd)`, then `tar -x -C "$broken"`); tasks T019 and research [R-08][r-08] say so. Other options: `cygpath` (not on Linux and macOS); `git worktree` or `git clone` for the copy (differs from the `git archive HEAD` of the task).
- **Decided: CI step layout.** Each script check is a run step per system family (`if: runner.os == 'Windows'` in cmd, otherwise bash) followed by one bash check step, and the output folder is made by its own first step ("Prepare output folder"). [CI-06][ci-06]'s two parts are one step, "[CI-06][ci-06] line endings and executable mode". The steps keep the order of contracts/ci-workflow.md. Other options: one long step per check (harder to see which part failed).
- **Decided: the typed session of scenario 5 is run by hand only.** In the automated run no person can type at the prompt, so only the redirected-input form ([CI-03][ci-03], and the same through a pipe by hand) was run; the typed run is left to the developer's check on a fresh clone (spec Assumptions; [NFR-09][nfr-09]).
- **Decided (developer, 2026-10-05): progress lines in the run scripts.** The developer asked that the scripts show what is happening while they run. Chosen: each script writes short progress lines to console messages (stderr), exactly as in [NFR-08][nfr-08] and the clarification "progress lines": the .NET check, the build, then the start of the application (with the file as given, or the typing hint). The build output itself stays hidden unless the build fails; console output (stdout) is unchanged ([DEC-37][dec-37], [DEC-23][dec-23]). [CI-07][ci-07] checks the lines. This changes the earlier decision that a successful run shows nothing from the script. Other options: show the build output (mixes build text into console messages (stderr)); show nothing (the reviewer cannot tell a slow build from a hang).
- **Decided (developer, 2026-10-05): `run.cmd` rejects `*` and `?` in its file argument and turns delayed expansion off.** A wildcard such as `testdata\challenge-example-1.*` passed the existence checks, and the progress line then expanded it. Chosen: a message on console messages (stderr) containing `must not contain * or ?` and exit code 1, before the "Running" line; `setlocal DisableDelayedExpansion` so `!` in a file name is not mangled. Other options: let the redirect fail (exit code 0 and misleading "Running" lines).

[bs-01]: #bs-01
[ci-01]: #ci-01
[ci-02]: #ci-02
[ci-03]: #ci-03
[ci-04]: #ci-04
[ci-05]: #ci-05
[ci-06]: #ci-06
[ci-07]: #ci-07
[ci-08]: #ci-08
[dec-04]: ../../docs/design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-20]: ../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-31]: ../../docs/design-analysis/design-decisions.md#dec-31-testing
[dec-33]: ../../docs/design-analysis/design-decisions.md#dec-33-cross-platform
[dec-36]: ../../docs/design-analysis/design-decisions.md#dec-36-development-approach-design-first-then-spec-driven-with-github-spec-kit
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[dec-39]: ../../docs/design-analysis/design-decisions.md#dec-39-not-included
[dec-41]: ../../docs/design-analysis/design-decisions.md#dec-41-menu-scripts
[e2e-01]: ../002-console-session/spec.md#e2e-01
[fr-11]: ../../docs/solution-design/02-functional-requirements.md#fr-11
[fr-16]: ../../docs/solution-design/02-functional-requirements.md#fr-16
[fr-17]: ../../docs/solution-design/02-functional-requirements.md#fr-17
[nfr-02]: #nfr-02
[nfr-02a]: #nfr-02a
[nfr-03]: #nfr-03
[nfr-08]: #nfr-08
[nfr-09]: #nfr-09
[mn-01]: #mn-01
[r-05]: research.md#r-05-ci-workflow-shape-nfr-02
[r-08]: research.md#r-08-ci-05-failed-build-in-a-temporary-copy
[r-10]: research.md#r-10-gitattributes-content-and-the-executable-bit
[r-12]: research.md#r-12-test-data-files-and-the-td-02-and-bs-01-tests
[sk-02]: ../001-robot-rules/spec.md#sk-02
[td-01]: #td-01
[td-02]: #td-02
