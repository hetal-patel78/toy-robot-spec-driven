# Toy Robot Simulator: solution design

This document explains how the Toy Robot Simulator is built. It turns the design decisions into requirements, components, flows and tests.

- **What the system does** is decided in [design-decisions.md][dd] ([DEC-01][dec-01] to [DEC-39][dec-39]). That file is the source of truth. This document adds detail on **how**; it never changes a decision. Where the two differ, design-decisions.md wins ([DEC-36][dec-36]).
- **Order of precedence:** design-decisions.md, then this document, then the Spec Kit files written from it ([DEC-36][dec-36]).
- Every requirement, component and design point cites the decision it comes from, for example [DEC-15][dec-15].

Two terms used throughout:
- **The console** is where commands are typed and where everything the application prints appears. It prints two kinds of text: **console output**, which is only the REPORT result (`Output:` and the position), and **console messages**, which are everything else (the `> ` prompt, the echo of each line, warnings, the how-to-run message and the unexpected-problem error line). Both appear together on screen, but they are kept separate so the REPORT result can be saved on its own. Technically, **console output (stdout)** is standard output, **console messages (stderr)** are standard error, and **console input (stdin)** is standard input. Wherever the channel matters, these pages name it with its tag.
- **Exit code** is the number a program returns to the system when it stops: 0 means success, 1 means it was used incorrectly, 2 means an unexpected problem.

## The design in one minute

- A C# .NET 8 console application. The robot moves on a 5 x 5 table, positions 0..4, with (0,0) at the SOUTH WEST corner ([DEC-01][dec-01], [DEC-02][dec-02]).
- Commands come from the console, typed or redirected from a file: `PLACE X,Y,F`, `MOVE`, `LEFT`, `RIGHT`, `REPORT`, `EXIT` ([DEC-04][dec-04], [DEC-06][dec-06]).
- Any command that cannot be carried out is ignored with a one-line warning, and the next line is processed. The app never crashes ([DEC-20][dec-20], [DEC-32][dec-32]).
- REPORT prints `Output:` and the position (for example `0,1,NORTH`) as console output (stdout). Everything else is a console message (stderr) ([DEC-21][dec-21], [DEC-23][dec-23]).
- Each line passes through four parts in turn: the Runner reads it, the CommandParser checks its format and turns it into a command object, RobotOperator applies the rules shared by all commands and runs the command, and the Table checks the position ([DEC-26][dec-26], [DEC-27][dec-27]).

## Sections

### What the system must do (pages 1–4)

**1. [Request summary](01-request-summary.md)**
The challenge asks for a console application that simulates a toy robot on a 5 x 5 table and never lets it fall off. It reads the commands PLACE, MOVE, LEFT, RIGHT and REPORT. The deliverables also include test data, a README, a description of the development process and the AI prompts used.

**2. [Functional requirements](02-functional-requirements.md)**
Numbered [FR-01][fr-01] onwards. They cover the table, how a line is read, each command, ignored commands, the REPORT output, ending a session and the how-to-run message. Each one has Given / When / Then acceptance criteria with an exact input and the exact console output (stdout) and console messages (stderr), using the warning text of [DEC-22][dec-22].

**3. [Non-functional requirements](03-non-functional-requirements.md)**
Numbered [NFR-01][nfr-01] onwards: never crash, works on Windows, Linux and macOS, runs on .NET 8 or a later version, readable code, testable without the console, and run scripts for each system. Each one says how it is checked.

**4. [Scope](04-scope.md)**
In scope: one robot, a 5 x 5 table, the six commands, typed and redirected input, warnings and run scripts. Out of scope, each with its reason: more than one robot, a configurable table size, a file-name argument, direction abbreviations, a menu, BACK and obstacles, a log file, and heavy frameworks ([DEC-38][dec-38], [DEC-39][dec-39]).

### How it is built (pages 5–9)

**5. [High-level design](05-high-level-design.md)**
An architecture diagram of the components and how a line flows through them. Only `Program` touches the real console; everything else receives its input and output, so the whole flow runs in memory in tests ([DEC-26][dec-26], [DEC-29][dec-29]). Also the folder layout and the path of the built app ([DEC-04][dec-04]).

**6. [Components](06-components.md)**
One section for each component: Program, Runner, CommandParser, the command classes (`IRobotCommand`: PlaceCommand, MoveCommand, LeftCommand, RightCommand, ReportCommand), RobotOperator (`IRobotOperator`), Robot, RobotState, Direction and Table. CommandParser returns a command object, EXIT or a failure with its reason. Each command class holds only its own logic; RobotOperator holds the rules shared by all commands and returns the position text for REPORT, a warning, or nothing. Commands reach the Robot only through `IRobotOperator`, and the robot's state is replaced, never changed ([DEC-13][dec-13], [DEC-14][dec-14]).

**7. [Logical flow and sequence diagrams](07-logical-flow.md)**
A flowchart of how one line is processed, and sequence diagrams for seven scenarios: valid PLACE then REPORT, MOVE blocked at the edge, commands before a valid PLACE, a bad line, PLACE off the table, EXIT, and start-up.

**8. [Alternative solutions](08-alternative-solutions.md)**
Five alternatives that were considered and not chosen, each on its own page with a sequence diagram, pros, cons, the decision it relates to and the trade-off the chosen design accepts. Three are other overall designs (everything in one class, commands as data with all rules in RobotOperator, an event-based design with a pure reducer); two are other choices within the chosen design (each command given the Robot and the Table, and a separate validator before the parser).

**9. [Considerations](09-considerations.md)**
Error handling, the two output streams and exit codes, the testing strategy (three levels, with a test-flow diagram, using xUnit), cross-platform checks with GitHub Actions, the run scripts, and how a new command, more robots, a configurable table or a log file could be added later (none is built). Also assumptions, the trade-offs the design accepts, risks and known limitations.

### Reference (pages 10–12)

**10. [Traceability](10-traceability.md)**
A table linking each FR and NFR to its decisions, the components that implement it and the test levels that prove it: unit, Runner whole-flow in memory, or console end-to-end.

**11. [Glossary](11-glossary.md)**
Every term and component name, defined once.

**12. [Assumptions](12-assumptions.md)**
A1 records how a reviewer is assumed to check the solution. Its default keeps the `Output:` label and treats `>` as a prompt ([DEC-09][dec-09], [DEC-21][dec-21]). The page also lists the questions settled while writing this document.

## Where to find what

| Looking for | Page |
|---|---|
| What the app must do, with exact input and output | [2](02-functional-requirements.md) |
| Component names and responsibilities | [5](05-high-level-design.md), [6](06-components.md) |
| Folder layout and the path of the built app | [5](05-high-level-design.md) |
| Step-by-step flows | [7](07-logical-flow.md) |
| Why this design and not another | [8](08-alternative-solutions.md) |
| Test framework, test levels, CI on three systems, .NET roll-forward | [9](09-considerations.md) |
| Which requirement is implemented and tested where | [10](10-traceability.md) |
| Terms and component names | [11](11-glossary.md) |
| Assumptions | [12](12-assumptions.md) |

## Sources

| Source | Role |
|---|---|
| [design-decisions.md][dd] | Source of truth for every rule and design decision |
| [solution-design-brief.md](solution-design-brief.md) | The developer's instructions for writing this document |
| [The challenge statement](../challenge/README.md) | The challenge |
| [handwritten/](../design-analysis/handwritten/), [clarifications/](../design-analysis/clarifications/), [review-trail.md](../design-analysis/review-trail.md) | Background: the initial analysis and how the decisions were reached |

[dd]: ../design-analysis/design-decisions.md

[dec-01]: ../design-analysis/design-decisions.md#dec-01-table-size-and-valid-positions
[dec-02]: ../design-analysis/design-decisions.md#dec-02-origin-and-directions
[dec-04]: ../design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-06]: ../design-analysis/design-decisions.md#dec-06-commands
[dec-09]: ../design-analysis/design-decisions.md#dec-09-a-leading--is-not-part-of-a-command
[dec-13]: ../design-analysis/design-decisions.md#dec-13-state-is-replaced-never-changed-in-place
[dec-14]: ../design-analysis/design-decisions.md#dec-14-only-one-way-to-change-the-robot
[dec-15]: ../design-analysis/design-decisions.md#dec-15-place
[dec-20]: ../design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../design-analysis/design-decisions.md#dec-23-output-channels
[dec-26]: ../design-analysis/design-decisions.md#dec-26-responsibilities
[dec-27]: ../design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-29]: ../design-analysis/design-decisions.md#dec-29-the-runner-receives-everything-it-uses
[dec-32]: ../design-analysis/design-decisions.md#dec-32-never-crash
[dec-36]: ../design-analysis/design-decisions.md#dec-36-development-approach-design-first-then-spec-driven-with-github-spec-kit
[dec-38]: ../design-analysis/design-decisions.md#dec-38-one-robot-now-room-for-more-later
[dec-39]: ../design-analysis/design-decisions.md#dec-39-not-included
[fr-01]: 02-functional-requirements.md#fr-01
[nfr-01]: 03-non-functional-requirements.md#nfr-01
