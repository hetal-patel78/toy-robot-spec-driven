# 3. Non-functional requirements

The qualities the application must have, beyond what it does, and how each one is checked.

**CI** (continuous integration) means an automated build that compiles the application and runs all its tests on every push to the repository. **Console end-to-end tests** start the real built application as a separate program and check its real console input and output, exit codes, the test data and the run scripts. **Runner whole-flow tests** run the whole application in memory, from the Runner inward.

| Requirement | How it is checked | Decisions |
|---|---|---|
| **NFR-01 Never crash**<br><a id="nfr-01"></a>No input may make the application crash: not an empty input, a very large number, or any other text. It ends only at EXIT, at the end of the input, when started with an argument ([FR-17][fr-17], [FR-18][fr-18]), when the user presses Ctrl+C, or with an unexpected problem with exit code 2. An unexpected problem is not caused by any input: it is a fault outside the application's control (for example the console or a pipe closing while it writes) or a defect. Program's last safeguard then writes one line, `Error: unexpected problem - <exception message>`, to console messages (stderr) instead of a .NET stack trace. | Tests feed empty input, numbers too large to read and every kind of bad line, and check that processing continues and the exit code is 0. Tests give Program a reader or writer that fails and check the exact error line and exit code 2, including when writing the error line fails too. | [DEC-20][dec-20], [DEC-32][dec-32] |
| **NFR-02 Works on Windows, Linux and macOS**<br><a id="nfr-02"></a>The application, its tests and its run scripts must work on all three systems. | CI builds the application and runs all tests, including the console end-to-end tests, on Windows, Linux and macOS for every push. On each system it also runs that system's run script with a challenge example and compares console output (stdout) with the expected file, line by line. The README links to these CI runs. | [DEC-33][dec-33] |
| **NFR-03 Runs on .NET 8 or later**<br><a id="nfr-03"></a>The application targets .NET 8 and must also run on a machine where only a later major version of .NET is installed. | A review of the build settings that allow this (page [9](09-considerations.md)). | [DEC-37][dec-37] |
| **NFR-04 Same results on every machine**<br><a id="nfr-04"></a>Results must not depend on the machine's regional settings (for example how it writes numbers) or on the line endings its system uses. | A review that numbers are read with fixed rules, not the regional settings. Expected output is compared line by line, so Windows and Linux line endings both pass. CI runs the same tests and test files on all three systems. | [DEC-08][dec-08], [DEC-31][dec-31], [DEC-33][dec-33] |
| **NFR-05 Testable without the console**<br><a id="nfr-05"></a>The whole flow, from reading a line to writing the result, must be testable in memory, with any table size, without starting the application. | The Runner whole-flow tests run in memory, including on a small table such as 2 x 2, and check console output (stdout) and console messages (stderr) separately and exactly. | [DEC-03][dec-03], [DEC-29][dec-29], [DEC-31][dec-31] |
| **NFR-06 Tested at three levels**<br><a id="nfr-06"></a>There must be unit tests, Runner whole-flow tests in memory, and console end-to-end tests of the built application. They must cover every case listed in [DEC-31][dec-31], and test names use the challenge wording where possible. | CI runs all three levels on all three systems. The test list is compared with [DEC-31][dec-31]'s list of cases that must be covered. | [DEC-31][dec-31] |
| **NFR-07 Readable code (SOLID)**<br><a id="nfr-07"></a>The code must be clean and follow the SOLID principles: each class has one job, new behaviour is added with new classes where practical, parts depend on small interfaces, and classes are small and well named. | Code review against the component responsibilities on page [6](06-components.md); adding a command needs a new class and a parser change only. | [DEC-34][dec-34] |
| **NFR-08 Run scripts**<br><a id="nfr-08"></a>A reviewer must be able to start the application with one script and no knowledge of the build: `run.cmd` (Windows cmd and PowerShell) and `run.sh` (Linux and macOS). Each script checks that .NET 8 or later is installed, builds quietly, and then, with no argument, shows a menu (type commands, run a test-data file, run the tests, show the help; [DEC-41][dec-41]), or reads redirected console input; with `--help`, it starts the application with `--help`, so the application shows its help text ([FR-18][fr-18]); with one file, it feeds the file to console input (stdin). Its own messages go to console messages (stderr), so console output (stdout) carries only the application's own console output (the REPORT result, or the help text). | CI runs each system's script with a challenge example and compares console output (stdout) line by line, and runs it with `--help` and checks the help text and exit code 0. The error cases are checked by their exit codes: no .NET SDK, a missing file or more than one argument gives 1; a failed build gives the build's exit code. The message wording is not fixed. | [DEC-04][dec-04], [DEC-33][dec-33], [DEC-37][dec-37], [DEC-41][dec-41] |
| **NFR-09 Short README**<br><a id="nfr-09"></a>A reviewer must be able to build, test and run the application by reading only the README, which fits about one screen. | A final check follows the README word for word on a fresh copy of the repository, in cmd, PowerShell and bash. | [DEC-37][dec-37] |

---

Back to the [solution design overview](README.md).

[dec-03]: ../design-analysis/design-decisions.md#dec-03-table-size-is-a-constructor-parameter
[dec-04]: ../design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-08]: ../design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-20]: ../design-analysis/design-decisions.md#dec-20-bad-lines
[dec-29]: ../design-analysis/design-decisions.md#dec-29-the-runner-receives-everything-it-uses
[dec-31]: ../design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../design-analysis/design-decisions.md#dec-32-never-crash
[dec-33]: ../design-analysis/design-decisions.md#dec-33-cross-platform
[dec-34]: ../design-analysis/design-decisions.md#dec-34-readable-code-solid
[dec-37]: ../design-analysis/design-decisions.md#dec-37-deliverables
[dec-41]: ../design-analysis/design-decisions.md#dec-41-menu-in-the-run-scripts
[fr-17]: 02-functional-requirements.md#fr-17
[fr-18]: 02-functional-requirements.md#fr-18
