# 11. Glossary

Every component name and term used in this document, defined once.

## Components

In the order a line meets them. Full descriptions are on page [6](06-components.md).

| Name | Meaning |
|---|---|
| **[Program](06-components.md#program)** | The entry point. Checks that there are no arguments, chooses the input reader, creates the Table, RobotOperator and the Runner, starts the Runner and returns the exit code. A last safeguard turns an unexpected exception into one error line and exit code 2 ([DEC-26][dec-26], [DEC-32][dec-32]). |
| **[Runner](06-components.md#runner)** | Runs the session: reads each line, shows the prompt or the echo, passes the line to the CommandParser and the command object to RobotOperator, and writes every result and warning. The only component that writes text ([DEC-26][dec-26]). |
| **[CommandParser](06-components.md#commandparser)** | Turns one line into a command object, EXIT, or a failure with its reason. Checks the format only, never the table ([DEC-28][dec-28]). |
| **[IRobotCommand](06-components.md#command-classes)** | The small interface every command class implements: `Name`, `NeedsRobotOnTable` and `Execute` ([DEC-27][dec-27]). |
| **[PlaceCommand, MoveCommand, LeftCommand, RightCommand, ReportCommand](06-components.md#command-classes)** | The command classes, one per robot command. Each holds only its own logic and its own warning text ([DEC-27][dec-27]). |
| **[IRobotOperator](06-components.md#robotoperator)** | The small interface through which a command reaches the robot: `CurrentState` (read the state) and `TryMoveTo` (check a new state with the Table, and replace the old one only if it is on the table) ([DEC-14][dec-14]). |
| **[RobotOperator](06-components.md#robotoperator)** | Implements `IRobotOperator`. Holds the only Robot and the rules shared by all commands, runs each command and returns its outcome ([DEC-27][dec-27]). |
| **[Robot](06-components.md#robot)** | Holds the robot's placement and the single method that replaces it ([DEC-14][dec-14]). |
| **[RobotState](06-components.md#robotstate)** | An immutable value: X, Y and the direction the robot faces ([DEC-12][dec-12]). |
| **[Direction](06-components.md#direction)** | NORTH, SOUTH, EAST or WEST, with turning left and right. |
| **[Table](06-components.md#table)** | Knows its size (5 x 5) and answers "is this position on the table?" ([DEC-03][dec-03], [DEC-28][dec-28]). |

## Terms

In alphabetical order.

| Term | Meaning |
|---|---|
| **Acceptance criteria** | The exact input and expected output that show a requirement is met, written as Given / When / Then (page [2](02-functional-requirements.md)). |
| **Bad line** | A line that is not a valid command, for example `HELLO` or `PLACE 1,2`. It is ignored with a warning ([DEC-20][dec-20]). |
| **Blank line** | A line that is empty or holds only whitespace. It is a bad line. |
| **Byte-order mark (BOM)** | An invisible marker some editors write at the very start of a file. It is removed before the first line is read (page [7.7](07-logical-flow/07-start-up.md#what-is-a-byte-order-mark)). |
| **CI (continuous integration)** | An automated build that compiles the application and runs all its tests on every push. Here it is GitHub Actions on Windows, Linux and macOS ([DEC-33][dec-33]). |
| **Command object** | An object of one of the command classes, created by the CommandParser for a valid robot command. |
| **Command pattern** | A design where each command is its own class with a method that carries it out. The chosen design uses it, with commands reaching the robot only through `IRobotOperator` (page [8](08-alternative-solutions.md)). |
| **Command word** | The first word of a line, up to the first whitespace, for example `PLACE` ([DEC-08][dec-08]). |
| **Console** | Where commands are typed and everything the application prints appears. |
| **Console input (stdin)** | Where the commands come from: the keyboard, or a file or pipe redirected into the application. Technically standard input. |
| **Console messages (stderr)** | Everything the application prints except the REPORT result: the prompt, the echo, warnings, the how-to-run message and the unexpected-problem error line. Technically standard error ([DEC-23][dec-23]). |
| **Console output (stdout)** | Only the REPORT result, `Output:` and the position. Technically standard output ([DEC-23][dec-23]). |
| **CRLF, LF** | The two ways of ending a line of text: CRLF on Windows, LF on Linux and macOS. Tests compare line by line, so both pass. |
| **Current state** | Where the robot is and which way it faces now; nothing while the robot is not on the table. Commands read it through `CurrentState`. |
| **DEC-nn** | A decision in [design-decisions.md][dd], for example [DEC-15][dec-15]. That file is the source of truth. |
| **Echo** | With redirected input, each line read is written back to console messages (stderr) as `> ` and the line, so a file run reads like a typed session ([DEC-25][dec-25]). |
| **End of input** | The input has no more lines: the end of a file, or Ctrl+Z then Enter (Windows) or Ctrl+D (Linux, macOS) when typing. It ends the session with exit code 0 ([DEC-10][dec-10]). |
| **End-to-end test** | A test that starts the built application as a separate program and checks its real console output (stdout) and exit code ([DEC-31][dec-31]). |
| **EXIT** | Ends the session at once with exit code 0. Handled by the Runner, never by RobotOperator ([DEC-10][dec-10], [DEC-30][dec-30]). |
| **Exit code** | The number a program returns to the system when it stops: 0 means success, 1 means it was used incorrectly, 2 means an unexpected problem ([DEC-32][dec-32]). |
| **Failure** | What the CommandParser returns for a bad line, with the reason used in the warning. It is a normal result, not an error that stops the application. |
| **FR, NFR** | A functional requirement (what the application must do, page [2](02-functional-requirements.md)) or a non-functional requirement (a quality it must have, page [3](03-non-functional-requirements.md)). |
| **How-to-run message** | The one-line help written to console messages (stderr) when the application is started with any argument ([DEC-04][dec-04]). |
| **Immutable** | Cannot be changed once created. A RobotState is replaced by a new one, never changed ([DEC-12][dec-12], [DEC-13][dec-13]). |
| **Interactive** | A person is typing, rather than input being redirected. Program decides this from `Console.IsInputRedirected` ([DEC-24][dec-24]). |
| **Interface** | In C#, a list of members a class promises to provide, without the code. Other parts depend on the interface instead of the class. |
| **Invariant culture** | Fixed rules for reading numbers, the same on every machine whatever its regional settings ([DEC-08][dec-08]). |
| **Mermaid** | A text format for diagrams that GitHub draws as pictures. All diagrams in this document use it. |
| **Next position, next state** | Where the robot would be after a command, before it is checked with the Table. The robot changes only if it is on the table ([DEC-13][dec-13]). |
| **Non-breaking space** | A space that looks normal but is a different character, often found in text copied from a web page. It counts as whitespace ([DEC-08][dec-08]). |
| **On the table** | The robot has been placed by a valid PLACE. Before that it exists but has no position ([DEC-11][dec-11]). |
| **Outcome** | What RobotOperator returns to the Runner for each command: the position text (REPORT), a warning, or "done, nothing to show" (page [6](06-components.md#outcome)). |
| **Parse result** | What the CommandParser returns for each line: a command object, EXIT or a failure (page [6](06-components.md#parse-result)). |
| **Placement** | The Robot's current state, or nothing before the first valid PLACE. |
| **Prompt** | `> `, written to console messages (stderr) before each line when a person is typing ([DEC-24][dec-24]). |
| **Proposed state** | The robot's next state, worked out by a command before it is checked with the Table. [DEC-13][dec-13]'s name for it; these pages call it the next state or next position. |
| **README** | The short README at the repository root that tells a reviewer how to build, test and run the application ([DEC-37][dec-37]). |
| **Redirected input** | Input that comes from a file or another program instead of the keyboard, for example `ToyRobot < commands.txt` ([DEC-04][dec-04]). |
| **Release build** | The optimised build of the application, which the run scripts use (page [5](05-high-level-design.md#folder-layout-and-the-built-application)). |
| **Replace** | The only way the robot changes: a new RobotState takes the place of the old one, after the Table has checked it ([DEC-13][dec-13]). |
| **RollForward** | A project setting that lets the application, built for .NET 8, run on a newer installed .NET version ([DEC-37][dec-37]). |
| **Run scripts** | `run.cmd` (Windows) and `run.sh` (Linux, macOS): check .NET, build, and start the application, optionally with a file ([DEC-37][dec-37]). |
| **SOLID** | Five principles of object-oriented design. Those used here: each class has one job (single responsibility); new behaviour is added with new classes (open/closed); parts depend on small interfaces ([DEC-34][dec-34]). |
| **Spec Kit** | GitHub's tool for spec-driven development. The next step after this document: constitution, specification, plan and tasks ([DEC-36][dec-36]). |
| **Test data** | Input files with matching expected-output files, in `testdata/` ([DEC-37][dec-37]). |
| **Trade-off** | The cost a decision accepts in exchange for its benefit, recorded on its "Trade-off" line (page [9](09-considerations.md#trade-offs-accepted)). |
| **Unit test** | A test of one component on its own ([DEC-31][dec-31]). |
| **UTF-8, UTF-16** | Ways of storing text as bytes. The application reads redirected input as UTF-8 ([DEC-26][dec-26]). |
| **Warning** | One line on console messages (stderr), starting `Warning: `, saying why a line or command was ignored ([DEC-22][dec-22]). |
| **Whitespace** | Spaces, tabs, non-breaking spaces and other blank characters: what `Trim()` removes ([DEC-08][dec-08]). |
| **Whole-flow test** | A test that runs the Runner, CommandParser, RobotOperator and Table together in memory, without the console ([DEC-31][dec-31]). |
| **xUnit** | The .NET test framework the tests use. |

---

Back to the [solution design overview](README.md).

[dd]: ../design-analysis/design-decisions.md

[dec-03]: ../design-analysis/design-decisions.md#dec-03-table-size-is-a-constructor-parameter
[dec-04]: ../design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-08]: ../design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-10]: ../design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-11]: ../design-analysis/design-decisions.md#dec-11-the-robot-exists-from-the-start-not-on-the-table
[dec-12]: ../design-analysis/design-decisions.md#dec-12-robot-state-is-an-immutable-value
[dec-13]: ../design-analysis/design-decisions.md#dec-13-state-is-replaced-never-changed-in-place
[dec-14]: ../design-analysis/design-decisions.md#dec-14-only-one-way-to-change-the-robot
[dec-15]: ../design-analysis/design-decisions.md#dec-15-place
[dec-20]: ../design-analysis/design-decisions.md#dec-20-bad-lines
[dec-22]: ../design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../design-analysis/design-decisions.md#dec-23-output-channels
[dec-24]: ../design-analysis/design-decisions.md#dec-24-prompt
[dec-25]: ../design-analysis/design-decisions.md#dec-25-echo-with-redirected-input
[dec-26]: ../design-analysis/design-decisions.md#dec-26-responsibilities
[dec-27]: ../design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-28]: ../design-analysis/design-decisions.md#dec-28-the-parser-checks-format-the-table-checks-position
[dec-30]: ../design-analysis/design-decisions.md#dec-30-exit-is-handled-by-the-runner
[dec-31]: ../design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../design-analysis/design-decisions.md#dec-32-never-crash
[dec-33]: ../design-analysis/design-decisions.md#dec-33-cross-platform
[dec-34]: ../design-analysis/design-decisions.md#dec-34-readable-code-solid
[dec-36]: ../design-analysis/design-decisions.md#dec-36-development-approach-design-first-then-spec-driven-with-github-spec-kit
[dec-37]: ../design-analysis/design-decisions.md#dec-37-deliverables
