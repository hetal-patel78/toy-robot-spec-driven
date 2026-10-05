# 6. Components

What each component does, what goes in and comes out, and what it does not do.

The components and their jobs come from [DEC-26][dec-26]. How they fit together is on page [5](05-high-level-design.md).

## Program

| | |
|---|---|
| **Folder** | `src/ToyRobot/`, the project root (namespace `ToyRobot`) ([DEC-40][dec-40]). |
| **Job** | The thin entry point. It checks the arguments, creates the other parts and starts the Runner, inside a last safeguard against unexpected exceptions ([DEC-04][dec-04], [DEC-26][dec-26], [DEC-32][dec-32]). |
| **Takes in** | The command-line arguments and the real console. |
| **Gives back** | Exit code 0 with the help text in console output (stdout) if the only argument is `--help`; exit code 1 with the how-to-run message in console messages (stderr) if any other argument is given; otherwise exit code 0 when the Runner returns. Exit code 2 with the line `Error: unexpected problem - <exception message>` in console messages (stderr) if an unexpected exception escapes this work ([DEC-23][dec-23], [DEC-32][dec-32]). |
| **Does not** | Read or parse commands, apply rules, change the console's encoding settings, or handle Ctrl+C ([DEC-10][dec-10], [DEC-26][dec-26]). |

Program has two methods. `Main` is the thin wiring of the real console: it decides `interactive` and chooses the input reader (steps 2 and 3 below), then calls `Run(args, input, output, error, interactive)` with that reader, `Console.Out` and `Console.Error`, and returns its exit code. Choosing the reader reads nothing, so it is done before the argument check, outside the safeguard. `Run` is internal, so the tests can call it with in-memory readers and writers; it does the argument check, steps 1 and 4, and the safeguard.

The argument check, the first thing `Run` does ([DEC-04][dec-04]):
- **Exactly one argument, and it is `--help`** (an exact, case-sensitive match): write the help text to console output (`output`) and return exit code 0. The help text is held in Program as its lines, each written with `WriteLine`, so it ends with a line break like every other line the application writes. Its exact text is on page [2](02-functional-requirements.md#help-text).
- **Any other argument, or more than one:** write the how-to-run message to console messages (`error`) and return exit code 1.
- In both cases `Run` reads nothing from `input` and creates no other part, so typed or redirected input is left unread.

Steps when there is no argument:
1. Create `Table(5, 5)` and `RobotOperator(table)` ([DEC-03][dec-03]).
2. Decide whether a person is typing: `interactive` is true when `Console.IsInputRedirected` is false ([DEC-24][dec-24]).
3. Choose the input reader. For redirected input, Program opens console input (stdin) with its own UTF-8 reader, which also removes a leading byte-order mark. When a person is typing, it uses `Console.In` ([DEC-20][dec-20], [DEC-26][dec-26]).
4. Create the Runner with that reader, `Console.Out` (console output, stdout), `Console.Error` (console messages, stderr), RobotOperator and `interactive`, and run it ([DEC-29][dec-29]).

The safeguard: `Run` does the argument check (including writing the help text), creates the parts and runs the Runner inside one `try`. If any exception escapes, it writes exactly one line to console messages (stderr), `Error: unexpected problem - <exception message>` (the exception's `Message`), and returns exit code 2. If writing that line fails too, it still returns 2 and does not throw. No input causes this: every bad input has a defined result and no component throws for input. It is there for faults outside the application's control, such as the console or a pipe closing while it writes, and for defects. Ctrl+C is still not handled ([DEC-10][dec-10], [DEC-32][dec-32]).

It does not change the console's encoding: on Windows that can fail when there is no console window (for example in CI), and the output is plain ASCII, so no change is needed ([DEC-26][dec-26]).

## Runner

| | |
|---|---|
| **Folder** | `src/ToyRobot/Session/` (namespace `ToyRobot.Session`) ([DEC-40][dec-40]). |
| **Job** | Runs the session: reads lines, shows the prompt or the echo, passes each line to the CommandParser and each command object to RobotOperator, and writes the results ([DEC-26][dec-26]). |
| **Takes in** | `Runner(TextReader input, TextWriter output, TextWriter error, RobotOperator robotOperator, bool interactive)` ([DEC-29][dec-29]). `output` is console output (stdout) and `error` is console messages (stderr). It creates its own CommandParser, because the parser has no settings or state. |
| **Gives back** | Text on the two writers. It returns when it reads EXIT or reaches the end of the input. |
| **Does not** | Use `Console` directly, know any robot rule or the table size, or pass EXIT or a parse failure to RobotOperator ([DEC-27][dec-27], [DEC-30][dec-30]). |

For each line:
1. If `interactive`, write `> ` to `error` ([DEC-24][dec-24]).
2. Read a line. If there is none (end of input), or the line is exactly the single character Ctrl+Z (U+001A, how the Windows console gives Ctrl+Z then Enter), return ([DEC-10][dec-10]).
3. If this is the first line, remove a byte-order mark from its start. Nothing is read ahead, so the first prompt is never delayed ([DEC-20][dec-20]).
4. If not `interactive`, write `> ` and the line, with whitespace removed from both ends, to `error` ([DEC-25][dec-25]).
5. Pass the line to the CommandParser.
   - A failure: write `Warning: ` and its reason to `error`.
   - EXIT: return ([DEC-30][dec-30]).
   - A command object: pass it to RobotOperator and write the outcome (see [RobotOperator](#robotoperator)).

Trimming uses `Trim()`, which removes every kind of whitespace (spaces, tabs, non-breaking spaces) from both ends, and nothing inside the line ([DEC-08][dec-08]).

## CommandParser

| | |
|---|---|
| **Folder** | `src/ToyRobot/Parsing/`, with the parse result (namespace `ToyRobot.Parsing`) ([DEC-40][dec-40]). |
| **Job** | Turns one line into a command object, EXIT, or a failure with its reason. It creates the command object for the command it reads. It checks the **format only** ([DEC-26][dec-26], [DEC-28][dec-28]). |
| **Takes in** | One line of text. |
| **Gives back** | A parse result (below). |
| **Does not** | Check whether a position is on the table, know where the robot is, write anything, or throw an exception for bad input ([DEC-20][dec-20], [DEC-28][dec-28]). |

How a line is read ([DEC-08][dec-08]):
1. Remove whitespace of any kind from both ends with `Trim()`. An empty result is a blank line.
2. The **command word** is the text up to the first whitespace character, or the whole line. It is compared with the six command names ignoring case ([DEC-07][dec-07]).
3. `MOVE`, `LEFT`, `RIGHT` and `REPORT` must have nothing after the command word ([DEC-06][dec-06]).
4. `EXIT` ignores anything after it ([DEC-10][dec-10]).
5. For `PLACE`, the rest of the line is split on commas into exactly three parts, and each part is trimmed. X and Y are read with `int.TryParse`, using `NumberStyles.AllowLeadingSign` and the invariant culture. A part that cannot be read but is written as a whole number (an optional sign and digits only) is too large to read, and the reason says so ([DEC-20][dec-20]). F must be one of the four full direction names, in any case.
6. Only the first problem is reported, checked in this order: command word, number of values, X, Y, F ([DEC-22][dec-22]).

### Parse result

The parser returns exactly one of these:

| Result | When | Contents |
|---|---|---|
| **A command object** | The line is a valid PLACE, MOVE, LEFT, RIGHT or REPORT | A new `PlaceCommand` (with X, Y and F), `MoveCommand`, `LeftCommand`, `RightCommand` or `ReportCommand`; see [Command classes](#command-classes) |
| **EXIT** | The command word is EXIT | Nothing more; the Runner stops ([DEC-30][dec-30]) |
| **A failure** | The line is not a valid command | The reason, as used in the warning ([DEC-22][dec-22]) |

The reason is the warning text without the `Warning: ` prefix, for example `'PLACE 1,2' ignored - expected PLACE X,Y,F` or `blank line ignored`. The Runner adds the prefix when it writes the warning.

| Failure | Reason |
|---|---|
| Unknown command word | `'<line>' ignored - unknown command` |
| Blank line | `blank line ignored` |
| PLACE without exactly three values | `'<line>' ignored - expected PLACE X,Y,F` |
| X is not a whole number | `'<line>' ignored - X must be a whole number` |
| X is written as a whole number but is too large to read (either direction) | `'<line>' ignored - X is too large` |
| Y is not a whole number | `'<line>' ignored - Y must be a whole number` |
| Y is written as a whole number but is too large to read (either direction) | `'<line>' ignored - Y is too large` |
| Invalid direction | `'<line>' ignored - F must be NORTH, SOUTH, EAST or WEST` |
| Values after MOVE, LEFT, RIGHT or REPORT | `'<line>' ignored - <COMMAND> takes no values` |

`<line>` is the line exactly as typed, with whitespace removed from both ends. `<COMMAND>` is the command name in upper case.

## Command classes

| | |
|---|---|
| **Folder** | `src/ToyRobot/Commands/`, with `IRobotCommand` and the outcome (namespace `ToyRobot.Commands`) ([DEC-40][dec-40]). |
| **Job** | One class per robot command: `PlaceCommand`, `MoveCommand`, `LeftCommand`, `RightCommand` and `ReportCommand`. Each holds **only its own logic**: where the robot would go next, or the position to report, and its own warning text ([DEC-26][dec-26], [DEC-27][dec-27]). |
| **Takes in** | Created by the CommandParser. `PlaceCommand` holds X, Y (whole numbers) and F (a Direction); the others hold nothing. When run, a command receives `IRobotOperator`, never the Robot itself ([DEC-14][dec-14]). |
| **Gives back** | An outcome (see [Outcome](#outcome)). |
| **Does not** | Check the table itself, change the robot directly, check whether the robot is on the table (RobotOperator does that once, for all commands), parse text or write output ([DEC-13][dec-13], [DEC-27][dec-27]). |

Every command class implements the same small interface, `IRobotCommand`:

| Member | What it is |
|---|---|
| `Name` | The command name in upper case, for example `MOVE`, used in warnings |
| `NeedsRobotOnTable` | True for MOVE, LEFT, RIGHT and REPORT; false for PLACE, which is how the robot gets on the table ([DEC-19][dec-19]) |
| `Execute(IRobotOperator robot)` | Carries out the command and returns an outcome |

What each command does when run:

| Command class | Works out | Then | Warning if refused |
|---|---|---|---|
| `PlaceCommand` | The new state X,Y,F | Asks RobotOperator to move the robot there | `PLACE 9,9,NORTH ignored - position is off the table`, using its X, Y and F in upper case ([DEC-15][dec-15]) |
| `MoveCommand` | One step forward from the current state: NORTH Y + 1, SOUTH Y − 1, EAST X + 1, WEST X − 1 ([DEC-02][dec-02]) | Asks RobotOperator to move the robot there | `MOVE ignored - next position is off the table so robot stays at 4,2,EAST`, using the current position ([DEC-16][dec-16]) |
| `LeftCommand`, `RightCommand` | The same position, turned left or right using Direction ([DEC-17][dec-17]) | Asks RobotOperator to move the robot there; this always succeeds | None |
| `ReportCommand` | The current position as text, for example `0,1,NORTH` | Returns it as the outcome; nothing changes ([DEC-18][dec-18]) | None |

PLACE, MOVE, LEFT and RIGHT all change the robot the same way, through RobotOperator's single "check, then replace" step. [DEC-13][dec-13] calls the next state the "proposed state" ([DEC-13][dec-13]). Adding a command means adding one class and teaching the CommandParser its word; the existing classes do not change ([DEC-27][dec-27]).

## RobotOperator

| | |
|---|---|
| **Folder** | `src/ToyRobot/Domain/`, with `IRobotOperator` (namespace `ToyRobot.Domain`) ([DEC-40][dec-40]). |
| **Job** | Holds the only Robot and the rules **shared by all commands**: the robot must be on the table (except for PLACE), and a new state is checked with the Table before it replaces the old one ([DEC-13][dec-13], [DEC-26][dec-26], [DEC-27][dec-27]). |
| **Takes in** | The Table, when it is created. Then one command object at a time, never EXIT and never a failure ([DEC-27][dec-27], [DEC-30][dec-30]). |
| **Gives back** | An outcome for each command (below). |
| **Does not** | Parse text, write output, add the `Output:` label or the `Warning: ` prefix, handle EXIT, hold any one command's own logic, or hand out the Robot ([DEC-14][dec-14], [DEC-21][dec-21]). |

It implements `IRobotOperator`, the only way commands can reach the robot ([DEC-14][dec-14]):

| Member | What it does |
|---|---|
| `CurrentState` | The robot's current state, read-only: a `RobotState`, or `null` when the robot is not on the table |
| `TryMoveTo(next)` | Asks the Table whether `next` is on the table. If it is, replaces the Robot's state with it and answers yes; if not, leaves the robot unchanged and answers no |

When the Runner passes it a command object:
1. If the command needs the robot on the table and the robot is not on the table, return the warning `<NAME> ignored - robot is not on the table`, for example `MOVE ignored - robot is not on the table` ([DEC-19][dec-19]).
2. Otherwise run the command, passing itself as `IRobotOperator`, and return the command's outcome.

### Outcome

RobotOperator returns exactly one of these to the Runner ([DEC-26][dec-26]):

| Outcome | When | What the Runner writes |
|---|---|---|
| **The position text** | REPORT with the robot on the table | `Output:` then the text, for example `0,1,NORTH`, to console output (stdout) ([DEC-21][dec-21]) |
| **A warning** | The command is ignored | `Warning: ` then the text, to console messages (stderr) ([DEC-22][dec-22]) |
| **Done, nothing to show** | PLACE, MOVE, LEFT or RIGHT carried out | Nothing; it reads the next line |

The warning texts, without the `Warning: ` prefix, and who produces them:

| Situation | Warning text | Produced by |
|---|---|---|
| Off-table PLACE ([DEC-15][dec-15]) | `PLACE 9,9,NORTH ignored - position is off the table`, using the values as read, in upper case | `PlaceCommand` |
| MOVE that would take the robot off the table ([DEC-16][dec-16]) | `MOVE ignored - next position is off the table so robot stays at 4,2,EAST`, using the robot's current position in the same form as REPORT | `MoveCommand` |
| Robot command before PLACE ([DEC-19][dec-19]) | `MOVE ignored - robot is not on the table`, with LEFT, RIGHT or REPORT as the command | RobotOperator |

## Robot

| | |
|---|---|
| **Folder** | `src/ToyRobot/Domain/` (namespace `ToyRobot.Domain`) ([DEC-40][dec-40]). |
| **Job** | Holds its placement and the single method that replaces it ([DEC-14][dec-14], [DEC-26][dec-26]). |
| **Takes in** | Created by RobotOperator with no placement. Its replace method accepts only a real `RobotState`, never `null` ([DEC-11][dec-11], [DEC-14][dec-14]). |
| **Gives back** | Its placement: `null` (not on the table) or a `RobotState`. |
| **Does not** | Check the table, know the rules, read commands or use the console. It can never be taken off the table once placed ([DEC-14][dec-14], [DEC-38][dec-38]). |

## RobotState

| | |
|---|---|
| **Folder** | `src/ToyRobot/Domain/` (namespace `ToyRobot.Domain`) ([DEC-40][dec-40]). |
| **Job** | An immutable value: X, Y and the facing Direction ([DEC-12][dec-12]). |
| **Takes in** | X, Y and the Direction, when it is created, for example as a C# `record`. |
| **Gives back** | X, Y and the Direction. |
| **Does not** | Change after it is created, or check whether it is on the table ([DEC-12][dec-12], [DEC-28][dec-28]). |

## Direction

| | |
|---|---|
| **Folder** | `src/ToyRobot/Domain/` (namespace `ToyRobot.Domain`) ([DEC-40][dec-40]). |
| **Job** | The four directions, NORTH, SOUTH, EAST and WEST, with turning left and right ([DEC-26][dec-26]). |
| **Takes in** | Nothing; it is a fixed set of four values. |
| **Gives back** | The direction after a left turn or a right turn ([DEC-17][dec-17]), and its name in upper case for REPORT and warnings ([DEC-07][dec-07]). |
| **Does not** | Know positions, the table or the robot. |

## Table

| | |
|---|---|
| **Folder** | `src/ToyRobot/Domain/` (namespace `ToyRobot.Domain`) ([DEC-40][dec-40]). |
| **Job** | Knows its size and answers "is this position on the table?" ([DEC-26][dec-26], [DEC-28][dec-28]). |
| **Takes in** | Its width and height, in its constructor ([DEC-03][dec-03]). A position X,Y is on the table when 0 ≤ X < width and 0 ≤ Y < height ([DEC-01][dec-01]). |
| **Gives back** | Yes or no for a position. |
| **Does not** | Hold the robot, know about commands, or read any configuration ([DEC-03][dec-03], [DEC-38][dec-38]). |

---

Back to the [solution design overview](README.md).

[dec-01]: ../design-analysis/design-decisions.md#dec-01-table-size-and-valid-positions
[dec-02]: ../design-analysis/design-decisions.md#dec-02-origin-and-directions
[dec-03]: ../design-analysis/design-decisions.md#dec-03-table-size-is-a-constructor-parameter
[dec-04]: ../design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-06]: ../design-analysis/design-decisions.md#dec-06-commands
[dec-07]: ../design-analysis/design-decisions.md#dec-07-case-insensitive
[dec-08]: ../design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-10]: ../design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-11]: ../design-analysis/design-decisions.md#dec-11-the-robot-exists-from-the-start-not-on-the-table
[dec-12]: ../design-analysis/design-decisions.md#dec-12-robot-state-is-an-immutable-value
[dec-13]: ../design-analysis/design-decisions.md#dec-13-state-is-replaced-never-changed-in-place
[dec-14]: ../design-analysis/design-decisions.md#dec-14-only-one-way-to-change-the-robot
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
[dec-26]: ../design-analysis/design-decisions.md#dec-26-responsibilities
[dec-27]: ../design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-28]: ../design-analysis/design-decisions.md#dec-28-the-parser-checks-format-the-table-checks-position
[dec-29]: ../design-analysis/design-decisions.md#dec-29-the-runner-receives-everything-it-uses
[dec-30]: ../design-analysis/design-decisions.md#dec-30-exit-is-handled-by-the-runner
[dec-32]: ../design-analysis/design-decisions.md#dec-32-never-crash
[dec-38]: ../design-analysis/design-decisions.md#dec-38-one-robot-now-room-for-more-later
[dec-40]: ../design-analysis/design-decisions.md#dec-40-source-folders
