# 4. Scope

What this solution includes, and what it leaves out on purpose, with the reason for each item left out.

## In scope

| Included | Decisions |
|---|---|
| One robot on a 5 x 5 table, positions 0..4, with (0,0) at the SOUTH WEST corner | [DEC-01][dec-01], [DEC-02][dec-02], [DEC-38][dec-38] |
| The challenge commands PLACE, MOVE, LEFT, RIGHT and REPORT, plus EXIT to end a session | [DEC-06][dec-06], [DEC-10][dec-10] |
| Input from the console, typed by a person or redirected from a file or a pipe, one command per line | [DEC-04][dec-04], [DEC-05][dec-05] |
| Commands in any letter case, with extra spaces allowed | [DEC-07][dec-07], [DEC-08][dec-08] |
| A one-line warning for every ignored command or bad line, after which processing continues | [DEC-19][dec-19], [DEC-20][dec-20], [DEC-22][dec-22] |
| The REPORT result as console output (stdout); warnings, the prompt, the echo and the how-to-run message as console messages (stderr), so the REPORT result can be saved on its own | [DEC-21][dec-21], [DEC-23][dec-23] |
| A prompt when a person is typing, and an echo of each line when input is redirected | [DEC-24][dec-24], [DEC-25][dec-25] |
| A how-to-run message for any argument, and exit codes 0 (normal end), 1 (wrong use) and 2 (unexpected problem, shown as one error line instead of a .NET stack trace) | [DEC-04][dec-04], [DEC-10][dec-10], [DEC-32][dec-32] |
| Run scripts for Windows, Linux and macOS | [DEC-37][dec-37] |
| Tests at three levels, run by CI (an automated build and test run) on Windows, Linux and macOS | [DEC-31][dec-31], [DEC-33][dec-33] |
| The challenge deliverables: source code, test data, a short README, a description of the development process, and the AI prompts and artefacts | [DEC-37][dec-37] |

## Out of scope

| Not included | Reason | Decisions |
|---|---|---|
| More than one robot | The challenge asks for one robot. The rules for several (naming, choosing a robot, collisions, what REPORT shows) are not defined, so building them now would be guesswork. The design keeps room to add them (page [9](09-considerations.md)). | [DEC-38][dec-38] |
| A configurable table size | The challenge fixes the table at 5 x 5. The design keeps it easy to add later. | [DEC-03][dec-03], [DEC-39][dec-39] |
| A file name as an argument | Console input already covers files: a file can be redirected to it. | [DEC-04][dec-04], [DEC-39][dec-39] |
| Direction abbreviations (N, S, E, W) | Not asked for by the challenge. | [DEC-06][dec-06], [DEC-39][dec-39] |
| A menu for choosing commands | It would multiply the PLACE combinations and the test scope. Commands are typed. | [DEC-35][dec-35], [DEC-39][dec-39] |
| A BACK command, and obstacles on the table | Not asked for by the challenge. | [DEC-39][dec-39] |
| A logging library, a dependency injection container (a library that connects the parts of a program), and a Dockerfile (a recipe for packaging the program in a container) | Too heavy for a console application of this size. | [DEC-39][dec-39] |
| A log file for results and warnings | The challenge says printing to the console is enough. Redirecting console output (stdout) and console messages (stderr) already saves the REPORT result and the warnings separately. A log file would add choices about its location, permissions, overwriting or appending, and paths on each system, plus tests. | [DEC-23][dec-23], [DEC-39][dec-39] |
| Accepting a leading `>` in input | The `>` in the challenge examples is read as a prompt, so `> MOVE` is a bad line. This may change if assumption [A1](12-assumptions.md) proves wrong. | [DEC-09][dec-09] |
| Handling Ctrl+C | The standard behaviour applies: the application stops at once, with the exit code the system sets. | [DEC-10][dec-10] |
| A demo mode in the run scripts | The README gives ready-to-copy commands that run the three challenge examples instead. | [DEC-37][dec-37] |

---

Back to the [solution design overview](README.md).

[dec-01]: ../design-analysis/design-decisions.md#dec-01-table-size-and-valid-positions
[dec-02]: ../design-analysis/design-decisions.md#dec-02-origin-and-directions
[dec-03]: ../design-analysis/design-decisions.md#dec-03-table-size-is-a-constructor-parameter
[dec-04]: ../design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-05]: ../design-analysis/design-decisions.md#dec-05-one-command-per-line
[dec-06]: ../design-analysis/design-decisions.md#dec-06-commands
[dec-07]: ../design-analysis/design-decisions.md#dec-07-case-insensitive
[dec-08]: ../design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-09]: ../design-analysis/design-decisions.md#dec-09-a-leading--is-not-part-of-a-command
[dec-10]: ../design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-19]: ../design-analysis/design-decisions.md#dec-19-robot-commands-before-the-robot-is-on-the-table
[dec-20]: ../design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../design-analysis/design-decisions.md#dec-23-output-channels
[dec-24]: ../design-analysis/design-decisions.md#dec-24-prompt
[dec-25]: ../design-analysis/design-decisions.md#dec-25-echo-with-redirected-input
[dec-31]: ../design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../design-analysis/design-decisions.md#dec-32-never-crash
[dec-33]: ../design-analysis/design-decisions.md#dec-33-cross-platform
[dec-35]: ../design-analysis/design-decisions.md#dec-35-typed-commands-only
[dec-37]: ../design-analysis/design-decisions.md#dec-37-deliverables
[dec-38]: ../design-analysis/design-decisions.md#dec-38-one-robot-now-room-for-more-later
[dec-39]: ../design-analysis/design-decisions.md#dec-39-not-included
