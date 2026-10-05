# 10. Traceability

Links each requirement to the decisions it comes from, the components that implement it and the tests that prove it.

## How to read the tables

- **Requirement** links to its full text and acceptance criteria on page [2](02-functional-requirements.md) (FR) or page [3](03-non-functional-requirements.md) (NFR).
- **Components** are the parts that carry the requirement out, as described on page [6](06-components.md). "Command classes" means `PlaceCommand`, `MoveCommand`, `LeftCommand`, `RightCommand` and `ReportCommand`.
- **Test levels** are the three levels in [DEC-31][dec-31], described on page [9](09-considerations.md#testing-strategy):
  - **Unit:** one component on its own.
  - **Runner whole-flow:** the whole application in memory, from the Runner inward (Runner → CommandParser → RobotOperator → commands → Table), with console output (stdout) and console messages (stderr) checked separately and exactly.
  - **Console end-to-end:** the real built application run as a separate process, with the real console, on Windows, Linux and macOS in CI.
- ✓ marks the levels planned to test the requirement. A blank cell means that level is not used for it.

## Functional requirements

| Requirement | Decisions | Components | Unit | Runner whole-flow | Console end-to-end |
|---|---|---|:-:|:-:|:-:|
| [FR-01](02-functional-requirements.md) Table 5 x 5, origin SOUTH WEST | [DEC-01][dec-01], [DEC-02][dec-02] | Table, Program (creates 5 x 5) | ✓ | ✓ | |
| [FR-02](02-functional-requirements.md) PLACE, valid and off the table | [DEC-11][dec-11], [DEC-15][dec-15] | CommandParser, `PlaceCommand`, RobotOperator, Table, Robot | ✓ | ✓ | ✓ |
| [FR-03](02-functional-requirements.md) MOVE one unit forward | [DEC-02][dec-02], [DEC-16][dec-16] | `MoveCommand`, RobotOperator, Table | ✓ | ✓ | ✓ |
| [FR-04](02-functional-requirements.md) MOVE off the table is ignored | [DEC-16][dec-16] | `MoveCommand`, RobotOperator, Table | ✓ | ✓ | |
| [FR-05](02-functional-requirements.md) LEFT and RIGHT turn | [DEC-17][dec-17] | `LeftCommand`, `RightCommand`, Direction | ✓ | ✓ | |
| [FR-06](02-functional-requirements.md) REPORT prints `Output:` and the position | [DEC-07][dec-07], [DEC-18][dec-18], [DEC-21][dec-21] | `ReportCommand`, RobotOperator, Runner (adds `Output:`) | ✓ | ✓ | ✓ |
| [FR-07](02-functional-requirements.md) Robot commands before PLACE are ignored | [DEC-19][dec-19] | RobotOperator | ✓ | ✓ | |
| [FR-08](02-functional-requirements.md) Read console input (stdin), one command per line | [DEC-04][dec-04], [DEC-05][dec-05] | Program, Runner | | ✓ | ✓ |
| [FR-09](02-functional-requirements.md) Any letter case; whitespace ignored | [DEC-07][dec-07], [DEC-08][dec-08] | CommandParser, Runner (echo) | ✓ | ✓ | |
| [FR-10](02-functional-requirements.md) Bad lines ignored with one warning | [DEC-06][dec-06], [DEC-08][dec-08], [DEC-09][dec-09], [DEC-20][dec-20], [DEC-22][dec-22] | CommandParser, Runner | ✓ | ✓ | |
| [FR-11](02-functional-requirements.md) Byte-order mark ignored | [DEC-20][dec-20] | Program (UTF-8 reader), Runner (first line) | | ✓ | ✓ |
| [FR-12](02-functional-requirements.md) Console output (stdout) carries only the REPORT result | [DEC-23][dec-23] | Runner, Program | | ✓ | ✓ |
| [FR-13](02-functional-requirements.md) Prompt when typing, none when redirected | [DEC-24][dec-24] | Runner, Program (decides `interactive`) | | ✓ | |
| [FR-14](02-functional-requirements.md) Echo of each line when redirected | [DEC-25][dec-25] | Runner | | ✓ | |
| [FR-15](02-functional-requirements.md) EXIT ends the session | [DEC-10][dec-10] | CommandParser, Runner, Program | ✓ | ✓ | ✓ |
| [FR-16](02-functional-requirements.md) End of input ends the session | [DEC-10][dec-10], [DEC-32][dec-32] | Runner, Program | | ✓ | ✓ |
| [FR-17](02-functional-requirements.md) Any other argument: how-to-run message, exit code 1 | [DEC-04][dec-04] | Program | | ✓ | ✓ |
| [FR-18](02-functional-requirements.md) `--help`: help text, exit code 0 | [DEC-04][dec-04], [DEC-23][dec-23], [DEC-32][dec-32] | Program | | ✓ | ✓ |

Notes:
- **Exit codes** ([FR-15][fr-15], [FR-16][fr-16], [FR-17][fr-17], [FR-18][fr-18]) are returned by Program, so the console end-to-end tests check them ([DEC-31][dec-31]). Exit code 2 for an unexpected problem ([NFR-01][nfr-01]) cannot be caused by any input, so it is checked in memory through Program's internal `Run` method, with a reader or writer that fails ([DEC-32][dec-32]).
- **The prompt and the echo** ([FR-13][fr-13], [FR-14][fr-14]) are on console messages (stderr). The Runner whole-flow tests check them exactly, giving the Runner `interactive` as true or false. The console end-to-end tests compare only console output (stdout), so they do not check them.
- **Every row of the [DEC-08][dec-08] table** and **every warning in [DEC-22][dec-22]**, with its exact text, must have a test ([FR-09][fr-09], [FR-10][fr-10], [DEC-31][dec-31]).
- **The three challenge examples** are test-data files, so they run in the console end-to-end tests and are compared with their expected output ([FR-02][fr-02], [FR-03][fr-03], [FR-06][fr-06]).

## Non-functional requirements

These are mostly proved by how the tests run, or by a review, rather than by one test.

| Requirement | Decisions | Implemented by | Proved by |
|---|---|---|---|
| [NFR-01](03-non-functional-requirements.md) Never crash | [DEC-20][dec-20], [DEC-32][dec-32] | CommandParser (failures, not exceptions; `int.TryParse`), RobotOperator (checks before it replaces), Program (last safeguard: one error line, exit code 2) | Unit, Runner whole-flow and console end-to-end tests with empty input, very large numbers and every kind of bad line; tests of Program's `Run` with a reader or writer that fails |
| [NFR-02](03-non-functional-requirements.md) Windows, Linux and macOS | [DEC-33][dec-33] | The CI workflow, `.gitattributes` | All three test levels and the run scripts, in CI on all three systems |
| [NFR-03](03-non-functional-requirements.md) .NET 8 or later | [DEC-37][dec-37] | The project file (`RollForward` = `LatestMajor`) | Review of the build settings |
| [NFR-04](03-non-functional-requirements.md) Same results on every machine | [DEC-08][dec-08], [DEC-31][dec-31], [DEC-33][dec-33] | CommandParser (invariant culture); tests compare line by line | Review, and the same tests on all three systems in CI |
| [NFR-05](03-non-functional-requirements.md) Testable without the console | [DEC-03][dec-03], [DEC-29][dec-29], [DEC-31][dec-31] | Runner (given its input and outputs), Table (size in the constructor) | Runner whole-flow tests, including a 2 x 2 table |
| [NFR-06](03-non-functional-requirements.md) Tested at three levels | [DEC-31][dec-31] | `ToyRobot.Tests`, `ToyRobot.ConsoleEndToEndTests` | CI runs all three levels; the test list is compared with [DEC-31][dec-31] |
| [NFR-07](03-non-functional-requirements.md) Readable code (SOLID) | [DEC-34][dec-34], [DEC-40][dec-40] | All components | Code review against page [6](06-components.md) |
| [NFR-08](03-non-functional-requirements.md) Run scripts | [DEC-04][dec-04], [DEC-33][dec-33], [DEC-37][dec-37], [DEC-41][dec-41] | `run.cmd`, `run.sh`, with the menu | CI runs each system's script with a challenge example and with `--help`; the error cases are checked by their exit codes |
| [NFR-09](03-non-functional-requirements.md) Short README | [DEC-37][dec-37] | The README | Following the README word for word on a fresh copy, in cmd, PowerShell and bash |

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
[dec-11]: ../design-analysis/design-decisions.md#dec-11-the-robot-exists-from-the-start-not-on-the-table
[dec-15]: ../design-analysis/design-decisions.md#dec-15-place
[dec-16]: ../design-analysis/design-decisions.md#dec-16-move
[dec-17]: ../design-analysis/design-decisions.md#dec-17-left-and-right
[dec-18]: ../design-analysis/design-decisions.md#dec-18-report
[dec-19]: ../design-analysis/design-decisions.md#dec-19-robot-commands-before-the-robot-is-on-the-table
[dec-20]: ../design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../design-analysis/design-decisions.md#dec-23-output-channels
[dec-24]: ../design-analysis/design-decisions.md#dec-24-prompt
[dec-25]: ../design-analysis/design-decisions.md#dec-25-echo-with-redirected-input
[dec-29]: ../design-analysis/design-decisions.md#dec-29-the-runner-receives-everything-it-uses
[dec-31]: ../design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../design-analysis/design-decisions.md#dec-32-never-crash
[dec-33]: ../design-analysis/design-decisions.md#dec-33-cross-platform
[dec-34]: ../design-analysis/design-decisions.md#dec-34-readable-code-solid
[dec-37]: ../design-analysis/design-decisions.md#dec-37-deliverables
[dec-40]: ../design-analysis/design-decisions.md#dec-40-source-folders
[dec-41]: ../design-analysis/design-decisions.md#dec-41-menu-in-the-run-scripts
[fr-02]: 02-functional-requirements.md#fr-02
[fr-03]: 02-functional-requirements.md#fr-03
[fr-06]: 02-functional-requirements.md#fr-06
[fr-09]: 02-functional-requirements.md#fr-09
[fr-10]: 02-functional-requirements.md#fr-10
[fr-13]: 02-functional-requirements.md#fr-13
[fr-14]: 02-functional-requirements.md#fr-14
[fr-15]: 02-functional-requirements.md#fr-15
[fr-16]: 02-functional-requirements.md#fr-16
[fr-17]: 02-functional-requirements.md#fr-17
[nfr-01]: 03-non-functional-requirements.md#nfr-01
[fr-18]: 02-functional-requirements.md#fr-18
