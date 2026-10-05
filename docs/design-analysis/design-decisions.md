# Toy Robot: design decisions

**This is the single source of truth** for how the Toy Robot Simulator behaves and how it is built. Each decision has a stable ID (DEC-nn) so code, tests and the README can refer to it. "The challenge" means the problem statement in `docs/challenge/README.md`; "the README" means the README at the root of the code repository.

How these decisions were reached: [review-trail.md](review-trail.md). Back to [README](README.md).

## Contents
1. [Table and coordinates](#1-table-and-coordinates)
2. [Input](#2-input)
3. [Robot model](#3-robot-model)
4. [Command rules](#4-command-rules)
5. [Output](#5-output)
6. [Components](#6-components)
7. [Quality, testing and process](#7-quality-testing-and-process)
8. [Out of scope and future](#8-out-of-scope-and-future)
9. [Assumptions](#9-assumptions)

## At a glance

**Typed session.** Lines marked ⌨ are typed by the user after the `> ` prompt; everything else is printed by the app.

```
> MOVE                                                 ⌨ MOVE
Warning: MOVE ignored - robot is not on the table
> PLACE 0,0,NORTH                                      ⌨ PLACE 0,0,NORTH
> MOVE                                                 ⌨ MOVE
> REPORT                                               ⌨ REPORT
Output:
0,1,NORTH
> EXIT                                                 ⌨ EXIT
```

**File run** (for example `./run.sh commands.txt`, where the file holds the same five lines). Nothing is typed; the app echoes each line it reads:

```
> MOVE
Warning: MOVE ignored - robot is not on the table
> PLACE 0,0,NORTH
> MOVE
> REPORT
Output:
0,1,NORTH
> EXIT
```

In both cases only `Output:` and `0,1,NORTH` are on standard output; prompts, echoes and warnings are on standard error ([DEC-23][dec-23]).

---

## 1. Table and coordinates

### DEC-01 Table size and valid positions

The table is 5 x 5. Valid positions are X = 0..4 and Y = 0..4.
- Reason: the challenge says "a square tabletop, of dimensions 5 units x 5 units". All challenge examples fit 0..4.

### DEC-02 Origin and directions

(0,0) is the SOUTH WEST corner. X increases towards EAST, Y increases towards NORTH.

| Facing | MOVE changes |
|---|---|
| NORTH | Y + 1 |
| SOUTH | Y − 1 |
| EAST | X + 1 |
| WEST | X − 1 |

- Reason: the challenge says "The origin (0,0) can be considered to be the SOUTH WEST most corner." Challenge Example 1 (`PLACE 0,0,NORTH`, `MOVE` → `0,1,NORTH`) only works with this origin.

### DEC-03 Table size is a constructor parameter

`Table` receives its width and height in its constructor. `Program` creates it as 5 x 5 and passes it in ([DEC-26][dec-26], [DEC-29][dec-29]). There is no configuration file and no feature flag.
- Reason: the challenge fixes the size at 5 x 5, so configuration would be unrequested work. A future configuration source would only change the numbers `Program` passes. Tests can build a smaller table, for example 2 x 2, and pass it through the same path.
- Trade-off: a different table size needs a code change in `Program`; accepted to keep the first build simple.

---

## 2. Input

### DEC-04 Standard input only

Commands are read from standard input: typed by a person, or redirected from a file or a pipe. The app takes **no arguments**: if any argument is given (a file name, `--help` or anything else), it prints this how-to-run message on standard error and exits with exit code 1:

```
How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)
```

How to feed a file depends on the shell:

| Shell | Command |
|---|---|
| Any (recommended) | The run scripts: `run commands.txt` (cmd), `.\run.cmd commands.txt` (PowerShell), `./run.sh commands.txt` (bash, zsh) ([DEC-37][dec-37]) |
| cmd, bash, zsh | `ToyRobot < commands.txt` |
| PowerShell | `cmd /c "ToyRobot < commands.txt"` (PowerShell does not support `<`) |

`ToyRobot` here stands for the built app; its exact path is set in the solution design. In bash and zsh it is run with its path (for example `./ToyRobot`).

- Reason: the challenge says "Input can be from a file, or from standard input, as the developer chooses." Standard input gives files for free, with one code path. The how-to-run message stops the app looking as if it hangs when given a file name.
- Trade-off: there is no file-name argument, so a file must be redirected; Windows PowerShell has no `<`, so it needs the run scripts or `cmd /c` ([DEC-37][dec-37]).

### DEC-05 One command per line

- Reason: matches the challenge examples; avoids splitting PLACE's arguments from other commands.

### DEC-06 Commands

The accepted commands are `PLACE X,Y,F`, `MOVE`, `LEFT`, `RIGHT`, `REPORT` and `EXIT`. `F` must be one of the full words `NORTH`, `SOUTH`, `EAST`, `WEST`. Abbreviations (N/S/E/W) are not accepted. `MOVE`, `LEFT`, `RIGHT` and `REPORT` take no values: `MOVE now` is a bad line ([DEC-20][dec-20]).
- Reason: the challenge command set, plus EXIT ([DEC-10][dec-10]). Abbreviations are not asked for by the challenge.

### DEC-07 Case-insensitive

`move`, `Move` and `MOVE` are the same command; `north` is the same as `NORTH`. Output always uses upper case (`0,1,NORTH`).
- Reason: there is no control over how a user types, and case carries no meaning here.

### DEC-08 How a line is read

1. Leading and trailing whitespace is removed. **Whitespace means any whitespace character:** spaces and tabs, and others such as the non-breaking space that text copied from a web page or a document can contain (in C#, what `string.Trim()` removes). Whitespace inside a line is never removed; it only separates the parts, as in steps 2 and 3.
2. The **command word** is the text up to the first whitespace (or the whole line if there is none).
3. For PLACE, the rest of the line is split on commas into **exactly three parts** (X, Y, F), and whitespace around each part is removed. At least one whitespace character is required between `PLACE` and its values, because of step 2.
4. X and Y are read with `int.TryParse` using `NumberStyles.AllowLeadingSign` and the invariant culture, so `-1`, `+1` and `01` are numbers, but `1 0` or `1.5` are not.

| Input | Result |
|---|---|
| `   MOVE   ` | Valid |
| `PLACE     0,0,NORTH` | Valid |
| `place 0 , 0 , north` | Valid |
| `PLACE 01,+1,EAST` | Valid: placed at 1,1,EAST |
| `PLACE 0 0 NORTH` | Bad line: one part instead of three |
| `PLACE0,0,NORTH`, `PLACEHOLDER`, `PLACE,1,2` | Bad line: unknown command |
| `PLACE 1 0,0,NORTH` | Bad line: X is `1 0` |
| `PLACE 1,2,` | Bad line: F is empty |
| `PLACE 1,2,NORTH EXTRA` | Bad line: F is `NORTH EXTRA` |

- Reason: friendly to people typing, without changing the command format. Accepting every kind of whitespace means a command pasted from a web page or a document works, instead of giving a warning whose quote looks correct. One simple splitting rule means every odd input has exactly one defined result. The invariant culture makes number reading the same on every machine.
- Trade-off: a wider input format than the challenge shows, and more cases to test (every row above is a test, [DEC-31][dec-31]).

### DEC-09 A leading `>` is not part of a command

A line such as `> MOVE` is a bad line (unknown command).
- Reason: the `>` in the challenge examples reads as a prompt, so this design treats it as one ([DEC-24][dec-24]). Accepting it in input would widen the format beyond what the challenge asks for. The risk that a reviewer feeds the challenge examples with their `>` is covered by assumption A1.
- Trade-off: pasting the challenge examples exactly as printed, with their `>`, gives warnings instead of results (assumption A1).

### DEC-10 Ending a session

A session ends at end of input (Ctrl+Z then Enter on Windows, Ctrl+D on Linux and macOS) or with `EXIT`. Either way the exit code is 0.
- A line that is exactly the single character Ctrl+Z (U+001A) ends the input like the end of a file: the session ends, nothing after it is read or run, no warning is written, and the exit code is 0. The Windows console gives Ctrl+Z then Enter as this line instead of the end of input. It applies to typed and redirected input alike.
- EXIT works at any point, including before the robot is placed.
- With redirected input, EXIT stops the remaining lines; they are not read, echoed or run.
- If the command word is EXIT ([DEC-08][dec-08]), the app exits and anything after it is ignored (`EXIT now` exits). `EXITNOW` and `EXITING` are not EXIT: unknown command.
- EXIT produces no warning or result of its own. (With redirected input its line is echoed like every other line, [DEC-25][dec-25].)
- When a typed session ends with Ctrl+Z / Ctrl+D, the last `> ` prompt stays on screen; this is expected.
- Ctrl+C is not handled by the app: the standard .NET behaviour applies (the app stops at once, with the non-zero exit code set by the system).
- Reason: EXIT is a choice to leave, not an error, and the intent is clear even with extra text. This is a deliberate exception to the strict format of [DEC-06][dec-06]. The challenge's "discard all commands … until a valid PLACE" is about robot commands; EXIT is about the session, so it is allowed before PLACE.
- Trade-off: a command the challenge does not define, and a looser format for EXIT than for the other commands.

---

## 3. Robot model

### DEC-11 The robot exists from the start, not on the table

`RobotOperator` creates one Robot when it is created. The Robot's placement is either `null` (not on the table) or a valid `RobotState` (`RobotState?` in C#). There is never a default position. The same Robot is used for the whole session.
- Reason: the challenge describes a robot that exists before it is placed: PLACE "will **put** the toy robot on the table", and "A robot that is **not on the table** can choose to ignore…" (emphasis added). Using `null` rather than a default such as 0,0,NORTH means an unplaced robot can never be mistaken for a placed one, and the C# compiler warns if the placement is used without checking.

### DEC-12 Robot state is an immutable value

`RobotState` holds X, Y and the facing direction and cannot be changed after it is created (for example a C# `record`).
- Reason: required by [DEC-13][dec-13] and [DEC-14][dec-14]; nobody can do `state.X = 9`.

### DEC-13 State is replaced, never changed in place

For PLACE, MOVE, LEFT and RIGHT, the same steps apply:
1. build the proposed next state,
2. ask the Table whether its position is on the table,
3. replace the robot's placement only if it is.

For LEFT and RIGHT the check always passes, because turning never changes the position.
- Reason: changing state in place means remembering to roll back when it turns out invalid, which is error-prone. One path for all four commands means nothing to keep in sync, and no risk of a future command skipping the check.
- Trade-off: a few more small types (`RobotState`, a nullable placement, a single replace method) than a simple robot with changeable fields.

### DEC-14 Only one way to change the robot

`Robot` has a single method that replaces its placement, and it accepts only a real `RobotState` (never `null`), so the robot can never be taken off the table. `RobotOperator` holds the only reference to the Robot and never hands it out. Command classes change the robot only through `IRobotOperator`, which RobotOperator implements. Others read the current position through `RobotOperator.CurrentState` (a read-only `RobotState?`).
- Reason: C# has no modifier meaning "only RobotOperator may call this", so the rule is enforced by design: no other code can reach the Robot. Exposing only the immutable state lets REPORT and tests read it without being able to change it.

---

## 4. Command rules

Every ignored command produces exactly one warning on standard error ([DEC-23][dec-23], wording in [DEC-22][dec-22]) and processing continues with the next line. Nothing is ever thrown to the user.

### DEC-15 PLACE

A valid PLACE puts the robot on the table at X,Y facing F, whether it is the first PLACE or a later one. A PLACE whose position is off the table is ignored:

| Robot before | Command | Robot after | Message |
|---|---|---|---|
| Not on the table | `PLACE 9,9,NORTH` | Still not on the table | `Warning: PLACE 9,9,NORTH ignored - position is off the table` |
| At 4,2,NORTH | `PLACE 9,9,NORTH` | Still at 4,2,NORTH | same warning |
| At 4,2,NORTH | `PLACE 1,1,EAST` | At 1,1,EAST | — |

- Reason: the challenge says "The toy robot must not fall off the table during movement. This also includes the initial placement of the toy robot." The challenge gives no default position, so none is invented.

### DEC-16 MOVE

Moves the robot one unit in the direction it faces ([DEC-02][dec-02]). A move that would leave the table is ignored with a warning; the robot can still take later commands.
- Reason: the challenge says "Any move that would cause the robot to fall must be ignored" and "further valid movement commands must still be allowed".

### DEC-17 LEFT and RIGHT

Rotate the robot 90 degrees without changing its position. LEFT: NORTH → WEST → SOUTH → EAST → NORTH. RIGHT: NORTH → EAST → SOUTH → WEST → NORTH.
- Reason: the challenge says "LEFT and RIGHT will rotate the robot 90 degrees in the specified direction without changing the position of the robot."

### DEC-18 REPORT

Prints the robot's position and direction (format: [DEC-21][dec-21]).
- Reason: the challenge says "REPORT will announce the X,Y and F of the robot."

### DEC-19 Robot commands before the robot is on the table

MOVE, LEFT, RIGHT and REPORT are ignored with a warning, for example `Warning: MOVE ignored - robot is not on the table`. (PLACE is how the robot gets on the table; EXIT is handled separately, [DEC-10][dec-10].)
- Reason: the challenge says "The application should discard all commands in the sequence until a valid PLACE command has been executed" and "A robot that is not on the table can choose to ignore the MOVE, LEFT, RIGHT and REPORT commands." Ignoring means the state does not change; the challenge does not require silence, so the user is told why.
- Trade-off: with warnings (here and in [DEC-20][dec-20]), a run prints more than the challenge examples show; the warnings go to standard error ([DEC-23][dec-23]), so the REPORT output stays exactly as expected.

### DEC-20 Bad lines

A line that is not a valid command ([DEC-08][dec-08]) is ignored with a warning, and processing continues. Typed and redirected input behave the same way. Bad lines include: an unknown command word, a line that is empty or only whitespace, PLACE without exactly three comma-separated values, X or Y that is not a whole number, X or Y that is too large to read, an invalid direction, and extra values after MOVE, LEFT, RIGHT or REPORT. The exact warning for each is in [DEC-22][dec-22].
- X and Y must fit in a C# `int` (up to about 2.1 billion). A value written as a whole number (an optional sign and digits, as in [DEC-08][dec-08]) that does not fit, in either direction, for example `PLACE 99999999999,0,NORTH` or `PLACE -99999999999,0,NORTH`, cannot be read, so it is a bad line, and its warning says `X is too large` (or `Y is too large`), meaning too large to read. Anything not written as a whole number, such as `A` or `1.5`, keeps `must be a whole number`. Numbers that do fit, including negative ones such as `PLACE -1,0,NORTH`, are read normally and rejected by the Table as off the table ([DEC-15][dec-15]).
- If the input starts with a UTF-8 byte-order mark (as some editors save files), it is removed: the UTF-8 reader that `Program` uses for redirected input removes it ([DEC-26][dec-26]), and the Runner also strips it from the start of the first line read, so in-memory tests behave the same. Nothing reads ahead, so the first prompt is never delayed.
- Reason: a bad line must not crash the app or stop the following commands. Using the standard number check keeps the parser simple, with no special number type for values that can never be on the table anyway.

---

## 5. Output

### DEC-21 REPORT format

REPORT prints a label line, then the position, always in upper case:

```
Output:
0,1,NORTH
```

Both lines go to standard output. Each REPORT prints its own pair of lines. `RobotOperator` returns only the position text (`0,1,NORTH`); the Runner writes the `Output:` label before it.
- Reason: the challenge says REPORT "can be in any form". The label makes the result easier for a person to read. Keeping the label in the Runner keeps presentation out of the rules.
- Trade-off: an automated check that expects only `0,1,NORTH` would see an extra `Output:` line (assumption A1).
- Risk: an automated check comparing against the bare `0,1,NORTH` would see an extra line. This is assumption A1; the format is documented in the README.

### DEC-22 Warning wording

Every warning is one line on standard error, starting with `Warning: `. For bad lines the line is quoted exactly as typed, with leading and trailing whitespace removed. Command names written by the app (outside the quote) are always in upper case. When a line has several problems, only the first is reported, checked in this order: command word, number of values, X, Y, F.

| Situation | Warning |
|---|---|
| Unknown command (including `> MOVE`, `EXITNOW`, `PLACE0,0,NORTH`) | `Warning: 'HELLO' ignored - unknown command` |
| Empty or whitespace-only line | `Warning: blank line ignored` |
| PLACE without exactly three comma-separated values | `Warning: 'PLACE 1,2' ignored - expected PLACE X,Y,F` |
| X is not a whole number | `Warning: 'PLACE A,0,NORTH' ignored - X must be a whole number` |
| X is written as a whole number but is too large to read (in either direction) | `Warning: 'PLACE 9999999999,0,NORTH' ignored - X is too large` |
| Y is not a whole number | `Warning: 'PLACE 0,B,NORTH' ignored - Y must be a whole number` |
| Y is written as a whole number but is too large to read (in either direction) | `Warning: 'PLACE 0,9999999999,NORTH' ignored - Y is too large` |
| Invalid direction | `Warning: 'PLACE 1,2,UP' ignored - F must be NORTH, SOUTH, EAST or WEST` |
| Extra values after MOVE, LEFT, RIGHT or REPORT | `Warning: 'move now' ignored - MOVE takes no values` |
| Off-table PLACE ([DEC-15][dec-15]) | `Warning: PLACE 9,9,NORTH ignored - position is off the table` |
| Blocked MOVE ([DEC-16][dec-16]) | `Warning: MOVE ignored - next position is off the table so robot stays at 4,2,EAST` (with the robot's current position) |
| Robot command before the robot is on the table ([DEC-19][dec-19]) | `Warning: MOVE ignored - robot is not on the table` (with LEFT, RIGHT or REPORT as the command) |

In the off-table PLACE warning the values are the **parsed** numbers and direction in upper case: `place 09 , +9 , north` gives `Warning: PLACE 9,9,NORTH ignored - position is off the table`. In the blocked MOVE warning the position is where the robot stays, in the same `X,Y,F` form as REPORT: at 4,2,EAST, `MOVE` gives `Warning: MOVE ignored - next position is off the table so robot stays at 4,2,EAST`.
- Reason: one consistent style, so tests can check exact text and users can scan warnings easily.

### DEC-23 Output channels

Standard output carries **only the REPORT output**: the `Output:` label and the position. Everything else goes to standard error.

| What the app prints | When | Channel |
|---|---|---|
| `Output:` + `X,Y,F` | Every REPORT | Standard output |
| Warnings | Ignored commands ([DEC-22][dec-22]) | Standard error |
| `> ` prompt | Only when a person is typing ([DEC-24][dec-24]) | Standard error |
| `> line` echo | Only when input is redirected ([DEC-25][dec-25]) | Standard error |
| How-to-run message | Any argument given ([DEC-04][dec-04]) | Standard error |
| `Error: unexpected problem - <exception message>` | An unexpected problem ([DEC-32][dec-32]) | Standard error |

On screen all of these appear together. The split only matters when output is saved or checked: in cmd, bash or zsh, `ToyRobot < commands.txt > results.txt` saves only the REPORT output and `2> warnings.txt` saves everything else. (In Windows PowerShell 5.1, `>` saves files as UTF-16; use cmd or the run scripts when saving output.)
- Reason: a common console convention (curl and git use standard error for progress messages). The REPORT output stays clean for saving or comparing, and saving output for later needs no extra code.

### DEC-24 Prompt

`> ` is shown before each line is read only when a person is typing; it is not shown when input is redirected. `Program` decides this with `Console.IsInputRedirected` ([DEC-26][dec-26]).
- Reason: a prompt is only useful to someone typing; with redirected input the echo takes its place ([DEC-25][dec-25]).

### DEC-25 Echo with redirected input

When input is redirected (a file or a pipe), every line read is echoed to standard error as `> ` followed by the line with leading and trailing whitespace removed, **before** it is processed. This includes bad lines, blank lines (echoed as `> `) and the EXIT line; lines after EXIT are not read, so they are not echoed. Typed input is never echoed (the terminal already shows it).
- Reason: a file run then reads like the challenge examples and like a typed session, and each warning appears directly under the line that caused it. Being on standard error, the echo never affects the REPORT output.
- Trade-off: more text on screen during a file run; it is on standard error only, so the REPORT output is unaffected.

---

## 6. Components

### DEC-26 Responsibilities

| Component | Responsibility |
|---|---|
| `Program` | Thin entry point. If any argument is given, prints the how-to-run message and exits with code 1 ([DEC-04][dec-04]). Otherwise creates `Table(5, 5)` and `RobotOperator(table)` and decides `interactive` from `Console.IsInputRedirected`. For redirected input it reads standard input through its own UTF-8 reader (which also removes a leading byte-order mark); when a person is typing it uses `Console.In`. It creates the Runner with that reader, `Console.Out` and `Console.Error`, runs it, and exits with code 0. It catches any unexpected exception that escapes this work, writes the error line `Error: unexpected problem - <exception message>` on standard error and exits with code 2 ([DEC-32][dec-32]). It does not change the console's encoding settings: on Windows that can fail when there is no console window (for example in CI), and the app's output is plain ASCII, so it needs no change. |
| `Runner` | Creates its own CommandParser (the parser has no settings or state, so it is not passed in). Strips a byte-order mark from the start of the first line read (never by reading ahead), reads lines, shows the prompt (interactive) or the echo (redirected), passes each line to the CommandParser, stops on EXIT, sends command objects to RobotOperator, and writes `Output:` plus the position to standard output and warnings to standard error. Never calls `Console` directly. |
| `CommandParser` | Turns a line into a command object, EXIT, or a failure with the reason used in the warning ([DEC-22][dec-22]). Applies [DEC-08][dec-08]. Checks the **format only**. |
| `IRobotCommand`, with `PlaceCommand`, `MoveCommand`, `LeftCommand`, `RightCommand`, `ReportCommand` | One class per robot command. Each holds only its own logic: where the robot would go next (PLACE, MOVE, LEFT, RIGHT) or the position to report (REPORT), plus its warning text. `PlaceCommand` also holds X, Y and F. It reaches the robot only through `IRobotOperator`. |
| `RobotOperator` | Created with a Table; creates and holds the only Robot. Implements `IRobotOperator`, which gives commands the current state (`CurrentState`, a read-only `RobotState?`) and one way to change the robot: check the new state with the Table, and replace the robot's state only if it is on the table. Before running a command, checks that the robot is on the table (except for PLACE). Returns an outcome for each command: the position text for REPORT (without the label), a warning, or nothing. |
| `Robot` | Holds its placement (`RobotState?`) and the single replace method. |
| `RobotState` | Immutable X, Y and facing. |
| `Direction` | NORTH, SOUTH, EAST, WEST, with turning left and right. |
| `Table` | Knows its size and answers "is this position on the table?" |

### DEC-27 Each command is a class; RobotOperator holds the shared rules

Each robot command (PLACE, MOVE, LEFT, RIGHT, REPORT) is its own class implementing `IRobotCommand`, with one method that carries the command out. For each line: the Runner reads it, the CommandParser turns it into a command object, EXIT or a failure, and the Runner passes a command object to RobotOperator, which returns an outcome that the Runner writes. RobotOperator applies the rules shared by all commands, written once: the robot must be on the table (except for PLACE), and a new state is checked with the Table before it replaces the old one. Each command class supplies only what is its own: where the robot would go next, or the position for REPORT, and its warning text. Commands reach the robot only through the `IRobotOperator` interface, never the Robot itself ([DEC-14][dec-14]). A failure is written as a warning and never reaches RobotOperator, and neither does EXIT ([DEC-30][dec-30]). The name `RobotOperator` was chosen so it is not confused with the command classes.
- Reason: follows SOLID ([DEC-34][dec-34]): each command class has one job, and a new command is a new class with no change to the existing ones (open/closed). The shared rules stay in one place, so they cannot drift apart between commands.
- Trade-off: more classes (one per command, plus two small interfaces) than a single class holding all the rules. The CommandParser still changes when a command is added, because it must recognise the new word.

### DEC-28 The parser checks format; the Table checks position

`PLACE 9,9,NORTH` is well-formed, so the parser accepts it; the Table rejects the position.
- Reason: each component has one job (single responsibility).

### DEC-29 The Runner receives everything it uses

`Runner(TextReader input, TextWriter output, TextWriter error, RobotOperator robotOperator, bool interactive)`. `interactive = true` shows the prompt; `false` echoes each line ([DEC-24][dec-24], [DEC-25][dec-25]).
- Reason: lets the whole flow run in memory in tests, with any table size ([DEC-31][dec-31]). Alternatives not chosen: redirecting the console in tests (still depends on the global `Console`) and testing only through the built app.

### DEC-30 EXIT is handled by the Runner

The Runner stops reading when it receives EXIT; RobotOperator never sees EXIT.
- Reason: EXIT is about the session, not the robot, so the "on the table?" rule never applies to it.

---

## 7. Quality, testing and process

### DEC-31 Testing

Three levels:
1. **Unit tests** for Table, Direction, CommandParser, RobotOperator and each command class.
2. **Runner whole-flow tests:** the whole application in memory, from the Runner inward (Runner → CommandParser → RobotOperator → commands → Table, and some through `Program.Run`), using a `StringReader`, `StringWriter`s and any table size, without the console app. Standard output and standard error are checked separately and exactly.
3. **Console end-to-end tests** that start the real built app as a separate process, with real standard input, standard output, standard error and exit codes: run it with redirected input for each test-data file and compare standard output with the expected file; run it with an argument and check the how-to-run message and exit code 1; check exit code 0 after end of input and after EXIT. These exercise the real console, `Console.IsInputRedirected`, the UTF-8 input reader and `Program`. They also run the test data and the run scripts.

In this project, "Runner whole-flow tests" means the in-memory tests that run the whole application from the Runner inward, and "console end-to-end tests" means the tests that run the real built application as a separate process. The Runner whole-flow tests are in `tests/ToyRobot.Tests/RunnerWholeFlow/` (the unit tests are in `tests/ToyRobot.Tests/Unit/`); the console end-to-end tests are the project `tests/ToyRobot.ConsoleEndToEndTests/`.

Expected output is compared **line by line**, so Windows and Linux line endings both pass. Test names use the challenge wording where possible (for example `Robot_not_on_table_ignores_MOVE`).

Must be covered:
- the three challenge examples, and the hand-traced Brainstorm examples (2,1,NORTH; the blocked moves on pages 3 and 5; 0,3,WEST);
- every edge (north, south, east, west) and corner; full LEFT and RIGHT cycles; a small table such as 2 x 2;
- MOVE, LEFT, RIGHT and REPORT each before PLACE; an invalid first PLACE; an invalid later PLACE; a second valid PLACE; a negative position; several REPORTs in one run;
- case-insensitivity (lower and mixed case, `north`), and REPORT in upper case after lower-case input;
- every row of the [DEC-08][dec-08] table, tabs and non-breaking spaces as whitespace;
- every warning in [DEC-22][dec-22] with its exact text, including `> MOVE`, `MOVE now`, `PLACEHOLDER`, `PLACE 99999999999,0,NORTH`, abbreviations (`PLACE 0,0,N`), blank lines, and the off-table warning with parsed values;
- EXIT before PLACE, mid-file (later lines neither echoed nor run), `EXIT now` exits, `EXITNOW` warns;
- prompt shown when interactive and not otherwise; echo content with redirected input;
- an empty input; input starting with a byte-order mark (console end-to-end, on all three systems);
- the how-to-run message and exit codes (console end-to-end);
- the unexpected-problem line and exit code 2 ([DEC-32][dec-32]).

### DEC-32 Never crash

No input, including an empty input or a very large number, makes the app crash.
- Reason: the challenge requires that invalid moves are ignored and valid commands still work; a crash would stop both.

As a last safeguard, `Program`, the thin entry point, catches any exception that escapes the work it does (the argument check, creating the parts and running the Runner). It writes exactly one line to standard error, `Error: unexpected problem - <exception message>` (where `<exception message>` is the exception's `Message`), and exits with exit code 2. If writing that line itself fails, it still exits with code 2. This is not expected for any input: every bad input has a defined result ([DEC-20][dec-20], [DEC-22][dec-22]) and no component throws for input. It covers faults outside the app's control (for example the console or a pipe closing while the app writes) and defects. Ctrl+C is still not handled ([DEC-10][dec-10]). Exit codes: 0 normal end ([DEC-10][dec-10]), 1 any argument, with the how-to-run message ([DEC-04][dec-04]), 2 unexpected problem.
- Reason: if something unforeseen happens, the user gets one clear line and a defined exit code instead of a .NET stack trace.
- Trade-off: a defect shows as a one-line error rather than a full stack trace; the tests still catch it because they check output and exit codes exactly.

### DEC-33 Cross-platform

The app must work on Windows, Linux and macOS. Evidence: a GitHub Actions workflow builds the app and runs all tests, including the console end-to-end tests ([DEC-31][dec-31]), on `windows-latest`, `ubuntu-latest` and `macos-latest` on every push. On each system it also runs that system's run script ([DEC-37][dec-37]) with a challenge example file and compares its standard output with the expected file, line by line. The README states that the solution is tested on all three systems through this workflow and links to it.
- A `.gitattributes` file keeps line endings consistent: LF for all text files, CRLF for `.cmd` files. `run.sh` is committed as executable.
- Reason: hosted CI gives real Windows, Linux and macOS machines, so macOS can be proven without owning a Mac. Docker cannot run macOS, and a README note alone would be unverified. The console end-to-end tests and script runs make sure the parts most likely to differ by system (the real console, encoding, input redirection, line endings and the scripts) are covered, not just the in-memory logic.
- Trade-off: time to set up and maintain the workflow, in exchange for real evidence on all three systems.

### DEC-34 Readable code (SOLID)

Clean code that follows the SOLID principles: each class has one job (single responsibility); new behaviour is added with new classes rather than by changing existing ones, where practical (open/closed); and parts depend on small interfaces. Classes are small and well named.
- Reason: the code is meant to be read by others, so it must be easy to follow and extend. SOLID is a widely used yardstick for object-oriented design.

### DEC-35 Typed commands only

No menu or selection interface.
- Reason: a menu would multiply the PLACE combinations and the test scope.

### DEC-36 Development approach: design first, then spec-driven with GitHub Spec Kit

The work follows this chain, each step derived from the one before:
1. **Design analysis** (this folder): the decisions in this file.
2. **Solution design document:** high-level design, logical flows (including sequence diagrams) and implementation details, written from these decisions.
3. **Spec Kit specification:** `/speckit-constitution`, `/speckit-specify`, `/speckit-plan` and `/speckit-tasks`, written from the solution design. The constitution holds only lasting principles (clean code and SOLID, the three test levels, never crash, cross-platform, no over-engineering, standard output for REPORT output only); the detailed rules stay here.
4. **Implementation** with `/speckit-implement`.

Order of precedence: this file, then the solution design, then the Spec Kit files. A later document must not contradict an earlier one; if it does, it is corrected, or the earlier decision is changed first after the developer confirms. The `/speckit-*` commands are run by the developer, or by Claude Code following a prompt the developer approved. The prompt log records the developer's prompt as typed, and the commands run under it.
- Reason: each step adds detail without re-deciding behaviour, so the specification and code can always be traced back to a decision.

### DEC-37 Deliverables

- Source code and tests. The app targets .NET 8 and is set to roll forward to a newer installed major version (`RollForward` = `LatestMajor`), so it also runs where only .NET 9 or later is installed.
  - Reason: .NET 8 support ends in November 2026, so a reviewer may have only a newer version.
- Test data: input files with matching expected-output files.
- **Run scripts** at the repository root, so a reviewer can start the app without reading the build details: `run.cmd` (Windows: cmd or PowerShell) and `run.sh` (Linux and macOS). Each one:
  - checks that a .NET SDK, version 8 or later, is installed; if not, prints where to get it and exits with code 1;
  - builds the app quietly; if the build fails, shows the build output and exits with its exit code;
  - with no argument, starts the app for typing; with one argument, checks the file exists (if not, prints a message and exits with code 1) and feeds it through standard input, so the app itself still takes no arguments ([DEC-04][dec-04]); with more than one argument, prints the script's how-to-run message and exits with code 1;
  - exits with the app's exit code;
  - writes its own messages (and any build output) to standard error, so standard output carries only the app's REPORT output.
  
  The run scripts have no demo mode of their own; the menu scripts of [DEC-41][dec-41], added later, list the test-data files, a typed session and the tests by number. CI runs each script on its own system ([DEC-33][dec-33]).
  - Reason: native on all three systems with no extra installs and no PowerShell execution-policy prompt; the scripts also hide the PowerShell `<` limitation ([DEC-04][dec-04]). A single PowerShell script would need PowerShell 7 on Linux and macOS and can be blocked on Windows.
- **The README**, at the repository root and kept short (about one screen): what it is; requirements; how to run it with the scripts, in the form for each shell (`run commands.txt` in cmd, `.\run.cmd commands.txt` in PowerShell, `./run.sh commands.txt` in bash or zsh); ready-to-copy commands that run the three challenge examples from the test data with their expected output; the build, test and direct run commands, including the PowerShell form ([DEC-04][dec-04]); the output format with an example ([DEC-21][dec-21], [DEC-23][dec-23]); a few key assumptions with a link to this file; a note that the tests run on Windows, Linux and macOS via GitHub Actions ([DEC-33][dec-33]); "How this would evolve" ([DEC-39][dec-39]); and where the development-process material and AI artefacts are. It is written in stages: a skeleton when the solution is created (with the parts already fixed by these decisions), real commands added as each one works, the CI badge and test-data list once all tests pass, and a final check that follows the README word for word on a fresh clone, in cmd, PowerShell and bash.
- A description of the development process and decisions.
- AI prompts and artefacts: `CLAUDE.md` (the project instructions given to the AI assistant), the design-phase prompt log ([prompts-design-phase.md](prompts-design-phase.md)), the Spec Kit files, and an implementation-phase prompt log.
- Development-process material: the handwritten analysis documents (Brainstorm, FR, NFR, HLD), presented as the initial analysis; the clarification documents (`docs/design-analysis/clarifications/*.pdf`) that answered the review findings; this design analysis folder; the solution design document with its sequence diagrams (main flow and the unhappy paths: commands before PLACE, invalid PLACE, blocked MOVE, bad line); and the test-flow diagram.
- Reason: the challenge's guidelines ask for source code, test data, a README, a description of the development process with notable decisions and trade-offs, and any prompts or artefacts used as inputs to code generation. The handwritten documents show where the analysis started; this folder's README states that this file wins wherever they differ.

### DEC-40 Source folders

The application's files are grouped by job in `src/ToyRobot/`, and the namespaces follow the folders:

| Folder | Namespace | Holds |
|---|---|---|
| `src/ToyRobot/` (the project root) | `ToyRobot` | Program |
| `Session/` | `ToyRobot.Session` | Runner |
| `Parsing/` | `ToyRobot.Parsing` | CommandParser and its parse result |
| `Commands/` | `ToyRobot.Commands` | the command interface, one class per command, and the outcome |
| `Domain/` | `ToyRobot.Domain` | Table, Direction, RobotState, Robot, RobotOperator and the narrow interface it gives commands |

A new command is a new class in `Commands/` plus one line in the CommandParser ([DEC-27][dec-27]). The tests follow the same rule: their folders and namespaces match (`Unit/`, `RunnerWholeFlow/` and the console end-to-end project).
- Reason: a reader, or a tool continuing the work, finds each component by its job, and the folders show the parts of the design ([DEC-26][dec-26], [DEC-34][dec-34]). Added after the implementation at the developer's request; until then all the files were directly in `src/ToyRobot/`.

### DEC-41 Menu scripts

Two menu scripts at the repository root, next to the run scripts: `menu.cmd` (Windows: cmd or PowerShell) and `menu.sh` (Linux and macOS). Each lists what can be run and runs the choice by number:

| Choice | What it does |
|---|---|
| 1 to 7 | Runs one test-data file through the run script, shows its console output (stdout), and says PASS when it matches the file's expected output and the exit code is 0, otherwise FAIL with the reason |
| 8 | Does 1 to 7 one after another, then says how many passed |
| 9 | Starts a typed session through the run script; after EXIT or the end of input, the menu comes back |
| 10 | Runs all automated tests (`dotnet test`) |
| 0 | Quits |

Any other choice shows `Not a choice: <what was typed>` and the menu again. Every run goes through the run script, so the .NET check, the build and the messages are those of [DEC-37][dec-37]. A file run and its comparison read no console input, so they cannot take the menu's own input. The end of the input ends `menu.sh`; `menu.cmd` cannot tell an empty line from the end of the input, so three empty choices in a row end it. The menu's own lines are console output (stdout); it adds no message of its own to console messages (stderr). PASS is shown in green, FAIL in red and the app's console output in bright cyan, so the result stands out. `menu.sh` uses colours only when it writes to a terminal; `menu.cmd` cannot tell, so it always uses them; both leave them out when the `NO_COLOR` environment variable is set, as CI does.
- Reason: a reviewer can try every test-data file, a typed session and the automated tests without typing file names or commands. Added after the implementation at the developer's request; until then the run scripts had no separate demo mode ([DEC-37][dec-37]).

---

## 8. Out of scope and future

### DEC-38 One robot now, room for more later

Multiple robots are not built. The design keeps a future change local to the CommandParser (reading a robot name) and RobotOperator (holding several robots); `RobotState`, `Table`, the Runner and the replace-only rule would not change. To keep it that way, avoid a static or global Robot, the Table holding the robot, and the Robot reading commands or the console.
- Reason: the rules for several robots (naming, choosing a robot, collisions, what REPORT shows) are not defined, so building them now would be guesswork and over-engineering.

### DEC-39 Not included

| Not included | Why |
|---|---|
| Configurable table size | The challenge fixes 5 x 5; [DEC-03][dec-03] keeps it easy to add |
| File-name argument | Standard input covers files ([DEC-04][dec-04]) |
| Direction abbreviations | Not asked for by the challenge ([DEC-06][dec-06]) |
| Menu interface | [DEC-35][dec-35] |
| Commands other than the six (the five robot commands and EXIT), for example BACK; obstacles | Not asked for by the challenge; any other command word is an unknown command ([DEC-20][dec-20]). A new command is a new class and one line in the parser ([DEC-27][dec-27]) |
| Logging framework, dependency injection container, Dockerfile | Too heavy for a console app of this size |
| Log file for results and warnings | The challenge says "standard output is sufficient". Redirection already saves the REPORT output and warnings separately with no code ([DEC-23][dec-23]). A log file would add decisions about location, permissions, overwrite or append and cross-platform paths, plus tests. If ever needed, only `Program` changes: it passes the Runner a writer that writes to both the console and a file ([DEC-29][dec-29]). |

The README's "How this would evolve" section describes how multiple robots, a configurable table and obstacles could be added.

---

## 9. Assumptions

| # | Assumption | Default | If it proves wrong |
|---|---|---|---|
| A1 | How the solution is checked is not stated: automatically against expected text or read by a person; standard output only or both streams; and whether the challenge examples are fed with their leading `>`. | Keep the `Output:` label ([DEC-21][dec-21]) and treat `>` as a prompt ([DEC-09][dec-09]); the format is documented in the README. | If standard output is compared automatically, print only `X,Y,F`. If both streams are merged, also stop the echo and warnings with redirected input. If the examples are fed as `> PLACE …`, accept and remove a leading `> `. |

Other assumptions, stated in the README: positions 0..4 with (0,0) at the SOUTH WEST corner; invalid or blocked commands are ignored with a warning; commands are case-insensitive with full direction words.

[dec-02]: #dec-02-origin-and-directions
[dec-03]: #dec-03-table-size-is-a-constructor-parameter
[dec-04]: #dec-04-standard-input-only
[dec-06]: #dec-06-commands
[dec-08]: #dec-08-how-a-line-is-read
[dec-09]: #dec-09-a-leading--is-not-part-of-a-command
[dec-10]: #dec-10-ending-a-session
[dec-13]: #dec-13-state-is-replaced-never-changed-in-place
[dec-14]: #dec-14-only-one-way-to-change-the-robot
[dec-15]: #dec-15-place
[dec-16]: #dec-16-move
[dec-19]: #dec-19-robot-commands-before-the-robot-is-on-the-table
[dec-20]: #dec-20-bad-lines
[dec-21]: #dec-21-report-format
[dec-22]: #dec-22-warning-wording
[dec-23]: #dec-23-output-channels
[dec-24]: #dec-24-prompt
[dec-25]: #dec-25-echo-with-redirected-input
[dec-26]: #dec-26-responsibilities
[dec-29]: #dec-29-the-runner-receives-everything-it-uses
[dec-30]: #dec-30-exit-is-handled-by-the-runner
[dec-31]: #dec-31-testing
[dec-32]: #dec-32-never-crash
[dec-33]: #dec-33-cross-platform
[dec-34]: #dec-34-readable-code-solid
[dec-35]: #dec-35-typed-commands-only
[dec-37]: #dec-37-deliverables
[dec-27]: #dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-39]: #dec-39-not-included
[dec-40]: #dec-40-source-folders
[dec-41]: #dec-41-menu-scripts
