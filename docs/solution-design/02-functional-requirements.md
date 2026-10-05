# 2. Functional requirements

What the application must do, as numbered requirements, each with one acceptance criterion showing exact input and output.

## How to read this page

- **Acceptance criterion:** the check that proves a requirement is met, written as Given (the starting point), When (the input) and Then (the exact result).
- **Given**, unless a row says otherwise: a new session, the robot not on the table, and input **redirected** from a file (not typed).
- **The console** shows two kinds of text, kept on two separate channels. Both appear together on screen; they are separate so the REPORT result can be saved on its own ([DEC-23][dec-23]).

  | Term on this page | Channel | Carries |
  |---|---|---|
  | **Console output (stdout)** | standard output | Only the REPORT result: `Output:` and the position. When the application is started with `--help`, no command is read and console output carries the help text instead ([FR-18][fr-18]) |
  | **Console messages (stderr)** | standard error | Everything else: the `> ` prompt, the echo of each line, warnings, the how-to-run message and the unexpected-problem error line ([DEC-32][dec-32]) |
  | **Console input (stdin)** | standard input | The commands, typed or redirected from a file |

- **Then** shows console output (stdout) and console messages (stderr) separately and in full. "(empty)" means nothing is written to that channel. The **exit code** is 0 unless a row says otherwise.
- With redirected input, every line read is echoed to console messages (stderr) as `> ` followed by the line ([FR-14][fr-14]), so the console messages show these echo lines as well as warnings.
- Each line of input or output is shown on its own line in the cell. `ToyRobot` stands for the built application.

## The table and the robot commands

| Requirement | Acceptance criterion | Decisions |
|---|---|---|
| **FR-01**<br><a id="fr-01"></a>The table must be 5 x 5, with valid positions X = 0..4 and Y = 0..4. (0,0) is the SOUTH WEST corner; X increases towards EAST and Y towards NORTH. | **When:**<br>`PLACE 4,4,NORTH`<br>`REPORT`<br>**Then console output (stdout):**<br>`Output:`<br>`4,4,NORTH`<br>**Console messages (stderr):**<br>`> PLACE 4,4,NORTH`<br>`> REPORT` | [DEC-01][dec-01], [DEC-02][dec-02] |
| **FR-02**<br><a id="fr-02"></a>A valid PLACE must put the robot at X,Y facing F, whether it is the first PLACE or a later one. A PLACE whose position is off the table must be ignored with a warning, and the robot must stay as it was. There is no default position. | **When:**<br>`PLACE 4,2,NORTH`<br>`PLACE 9,9,NORTH`<br>`REPORT`<br>**Then console output (stdout):**<br>`Output:`<br>`4,2,NORTH`<br>**Console messages (stderr):**<br>`> PLACE 4,2,NORTH`<br>`> PLACE 9,9,NORTH`<br>`Warning: PLACE 9,9,NORTH ignored - position is off the table`<br>`> REPORT` | [DEC-11][dec-11], [DEC-15][dec-15] |
| **FR-03**<br><a id="fr-03"></a>MOVE must move the robot one unit forward in the direction it faces: NORTH Y + 1, SOUTH Y − 1, EAST X + 1, WEST X − 1. | Challenge Example 1.<br>**When:**<br>`PLACE 0,0,NORTH`<br>`MOVE`<br>`REPORT`<br>**Then console output (stdout):**<br>`Output:`<br>`0,1,NORTH`<br>**Console messages (stderr):**<br>`> PLACE 0,0,NORTH`<br>`> MOVE`<br>`> REPORT` | [DEC-02][dec-02], [DEC-16][dec-16] |
| **FR-04**<br><a id="fr-04"></a>A MOVE that would take the robot off the table must be ignored with a warning. The robot stays where it is, and later commands still work. | **When:**<br>`PLACE 0,0,SOUTH`<br>`MOVE`<br>`REPORT`<br>**Then console output (stdout):**<br>`Output:`<br>`0,0,SOUTH`<br>**Console messages (stderr):**<br>`> PLACE 0,0,SOUTH`<br>`> MOVE`<br>`Warning: MOVE ignored - next position is off the table so robot stays at 0,0,SOUTH`<br>`> REPORT` | [DEC-16][dec-16] |
| **FR-05**<br><a id="fr-05"></a>LEFT and RIGHT must turn the robot 90 degrees without moving it. LEFT: NORTH → WEST → SOUTH → EAST → NORTH. RIGHT: NORTH → EAST → SOUTH → WEST → NORTH. | Challenge Example 2.<br>**When:**<br>`PLACE 0,0,NORTH`<br>`LEFT`<br>`REPORT`<br>**Then console output (stdout):**<br>`Output:`<br>`0,0,WEST`<br>**Console messages (stderr):**<br>`> PLACE 0,0,NORTH`<br>`> LEFT`<br>`> REPORT` | [DEC-17][dec-17] |
| **FR-06**<br><a id="fr-06"></a>REPORT must print `Output:` and then the position as `X,Y,F`, both as console output (stdout), with the direction in upper case whatever case the input used. Each REPORT prints its own two lines. How a reviewer will check this format is not stated (assumption [A1](12-assumptions.md)). | **When:**<br>`place 1,2,north`<br>`report`<br>**Then console output (stdout):**<br>`Output:`<br>`1,2,NORTH`<br>**Console messages (stderr):**<br>`> place 1,2,north`<br>`> report` | [DEC-07][dec-07], [DEC-18][dec-18], [DEC-21][dec-21] |
| **FR-07**<br><a id="fr-07"></a>Until a valid PLACE, MOVE, LEFT, RIGHT and REPORT must each be ignored with a warning that names the command. | **When:**<br>`MOVE`<br>`PLACE 0,0,NORTH`<br>`REPORT`<br>**Then console output (stdout):**<br>`Output:`<br>`0,0,NORTH`<br>**Console messages (stderr):**<br>`> MOVE`<br>`Warning: MOVE ignored - robot is not on the table`<br>`> PLACE 0,0,NORTH`<br>`> REPORT` | [DEC-19][dec-19] |

## Reading the input

| Requirement | Acceptance criterion | Decisions |
|---|---|---|
| **FR-08**<br><a id="fr-08"></a>Commands must be read from console input (stdin), typed or redirected from a file or a pipe, one command per line. | **Given** a file redirected to console input (`ToyRobot < commands.txt`).<br>**When** the file holds:<br>`PLACE 0,0,NORTH MOVE`<br>**Then console output (stdout):** (empty)<br>**Console messages (stderr):**<br>`> PLACE 0,0,NORTH MOVE`<br>`Warning: 'PLACE 0,0,NORTH MOVE' ignored - F must be NORTH, SOUTH, EAST or WEST` | [DEC-04][dec-04], [DEC-05][dec-05] |
| **FR-09**<br><a id="fr-09"></a>Commands and directions must be accepted in any letter case. Whitespace of any kind (spaces, tabs, non-breaking spaces) at the start and end of a line and around each PLACE value must be ignored. X and Y may have a leading sign or leading zeros. | **When:**<br>`place 0 , 0 , north`<br>`REPORT`<br>**Then console output (stdout):**<br>`Output:`<br>`0,0,NORTH`<br>**Console messages (stderr):**<br>`> place 0 , 0 , north`<br>`> REPORT` | [DEC-07][dec-07], [DEC-08][dec-08] |
| **FR-10**<br><a id="fr-10"></a>A line that is not a valid command must be ignored with one warning, and processing must continue with the next line. Typed and redirected input behave the same. The warning for each kind of bad line is in the table below. | **When:**<br>`PLACE 1,0,EAST`<br>`PLACE 1,2`<br>`PLACE 3,2,NORTH`<br>`REPORT`<br>**Then console output (stdout):**<br>`Output:`<br>`3,2,NORTH`<br>**Console messages (stderr):**<br>`> PLACE 1,0,EAST`<br>`> PLACE 1,2`<br>`Warning: 'PLACE 1,2' ignored - expected PLACE X,Y,F`<br>`> PLACE 3,2,NORTH`<br>`> REPORT` | [DEC-06][dec-06], [DEC-08][dec-08], [DEC-09][dec-09], [DEC-20][dec-20], [DEC-22][dec-22] |
| **FR-11**<br><a id="fr-11"></a>A byte-order mark (an invisible marker some editors write at the start of a file) must be ignored. | **Given** a file that starts with a UTF-8 byte-order mark (UTF-8 is the usual way of storing text).<br>**When** the file holds Challenge Example 1.<br>**Then** console output (stdout) and console messages (stderr) are exactly as for FR-03, with no marker in the first echo line and no warning. | [DEC-20][dec-20] |

**Warnings for bad lines ([FR-10][fr-10]).** Each row is a one-line input; console output (stdout) is (empty), and console messages (stderr) are the echo of the line followed by the warning. The other warnings are shown under [FR-02][fr-02], [FR-04][fr-04] and [FR-07][fr-07].

| Input line | Warning in console messages (stderr) |
|---|---|
| `HELLO` (also `> MOVE`, `EXITNOW`, `PLACE0,0,NORTH`) | `Warning: 'HELLO' ignored - unknown command` |
| An empty line, or only whitespace | `Warning: blank line ignored` |
| `PLACE 1,2` | `Warning: 'PLACE 1,2' ignored - expected PLACE X,Y,F` |
| `PLACE A,0,NORTH` | `Warning: 'PLACE A,0,NORTH' ignored - X must be a whole number` |
| `PLACE 9999999999,0,NORTH` (a whole number too large to read, in either direction) | `Warning: 'PLACE 9999999999,0,NORTH' ignored - X is too large` |
| `PLACE 0,B,NORTH` | `Warning: 'PLACE 0,B,NORTH' ignored - Y must be a whole number` |
| `PLACE 0,9999999999,NORTH` | `Warning: 'PLACE 0,9999999999,NORTH' ignored - Y is too large` |
| `PLACE 1,2,UP` (also `PLACE 1,2,N`) | `Warning: 'PLACE 1,2,UP' ignored - F must be NORTH, SOUTH, EAST or WEST` |
| `move now` | `Warning: 'move now' ignored - MOVE takes no values` |

For the other inputs in a row, the quote holds that line instead. The warning quotes the line as typed, without whitespace at either end, and names commands in upper case. When a line has several problems, only the first is reported, in this order: command word, number of values, X, Y, F ([DEC-22][dec-22]). A leading `>` is treated as a prompt, not part of a command ([DEC-09][dec-09], assumption [A1](12-assumptions.md)).

## Output

| Requirement | Acceptance criterion | Decisions |
|---|---|---|
| **FR-12**<br><a id="fr-12"></a>Console output (stdout) must carry only the REPORT result (`Output:` and the position), or the help text when started with `--help` ([FR-18][fr-18]). Warnings, the prompt, the echo, the how-to-run message and the error line of an unexpected problem ([NFR-01][nfr-01]) must go to console messages (stderr). | **Given** the [FR-07][fr-07] input in `commands.txt`.<br>**When** run in cmd, bash or zsh as `ToyRobot < commands.txt > results.txt`.<br>**Then** `results.txt` holds exactly:<br>`Output:`<br>`0,0,NORTH`<br>and the screen shows only the console messages (stderr) of FR-07. | [DEC-23][dec-23] |
| **FR-13**<br><a id="fr-13"></a>When a person is typing, the prompt `> ` must be written to console messages (stderr) before each line is read, and typed lines must not be echoed. With redirected input there is no prompt. | **Given** a person is typing.<br>**When** they type `MOVE`, then `EXIT`, pressing Enter after each.<br>**Then console output (stdout):** (empty)<br>**Console messages (stderr)** (`⏎` marks a line break):<br>`> Warning: MOVE ignored - robot is not on the table⏎> ` | [DEC-24][dec-24] |
| **FR-14**<br><a id="fr-14"></a>With redirected input, every line read must be echoed to console messages (stderr), before it is processed, as `> ` and the line without whitespace at either end. This includes bad lines, blank lines and EXIT; lines after EXIT are not read, so they are not echoed. | **When** the lines are: `MOVE` with spaces before and after it, an empty line, `EXIT`, `REPORT`.<br>**Then console output (stdout):** (empty)<br>**Console messages (stderr):**<br>`> MOVE`<br>`Warning: MOVE ignored - robot is not on the table`<br>`> ` (`>` and a space)<br>`Warning: blank line ignored`<br>`> EXIT` | [DEC-25][dec-25] |

## Ending a session, and starting with an argument

| Requirement | Acceptance criterion | Decisions |
|---|---|---|
| **FR-15**<br><a id="fr-15"></a>When the command word is EXIT, in any case, the session must end at once with exit code 0, even before PLACE. Anything after EXIT on its line is ignored, and later lines are not read. EXIT gives no output of its own. `EXITNOW` is an unknown command. | **When:**<br>`exit now`<br>`REPORT`<br>**Then console output (stdout):** (empty)<br>**Console messages (stderr):**<br>`> exit now`<br>**Exit code:** 0 | [DEC-10][dec-10] |
| **FR-16**<br><a id="fr-16"></a>The session must end at the end of the input with exit code 0. A typing user ends input with Ctrl+Z then Enter (Windows) or Ctrl+D (Linux, macOS). A line that is exactly the single character Ctrl+Z (U+001A), which is how the Windows console gives Ctrl+Z then Enter, ends the input in the same way: nothing after it is read or run, and it gives no warning. Empty input must not cause an error. | **Given** an empty file.<br>**Then console output (stdout):** (empty)<br>**Console messages (stderr):** (empty)<br>**Exit code:** 0 | [DEC-10][dec-10], [DEC-32][dec-32] |
| **FR-17**<br><a id="fr-17"></a>If the application is started with any argument other than `--help` on its own, it must not read input. It must write the how-to-run message to console messages (stderr) and stop with exit code 1. This includes `-h`, `--HELP` and `--help` with another argument. | **When** started as `ToyRobot commands.txt` (or `ToyRobot -h`, `ToyRobot --HELP` or `ToyRobot --help commands.txt`).<br>**Then console output (stdout):** (empty)<br>**Console messages (stderr):**<br>`How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)` (three spaces before the bracket)<br>**Exit code:** 1 | [DEC-04][dec-04] |
| **FR-18**<br><a id="fr-18"></a>If the application is started with exactly one argument, `--help` (lower case), it must not read input. It must write the help text below to console output (stdout), write nothing to console messages (stderr), and stop with exit code 0. The help text says how to start the application, describes the table, each command and the rules, and says how a session ends. | **Given** input that holds `PLACE 0,0,NORTH` and `REPORT`.<br>**When** started as `ToyRobot --help`.<br>**Then console output (stdout):** the [help text](#help-text), exactly<br>**Console messages (stderr):** (empty)<br>**Exit code:** 0<br>The input is not read, so there is no `Output:` line. | [DEC-04][dec-04], [DEC-23][dec-23], [DEC-32][dec-32] |

### Help text

The exact help text of [FR-18][fr-18], from [DEC-04][dec-04]. It is plain ASCII. It repeats the rules of [FR-01][fr-01] to [FR-10][fr-10], [FR-15][fr-15] and [FR-16][fr-16], and how to start the run scripts ([NFR-08][nfr-08]); if one of them changes, the help text changes with it.

```text
Toy Robot Simulator - help

HOW TO START
  Type commands:        ToyRobot
  Commands from a file: ToyRobot < commands.txt
                        run commands.txt         (cmd)
                        .\run.cmd commands.txt   (PowerShell)
                        ./run.sh commands.txt    (bash, zsh)

THE TABLE
  5 x 5 units. Positions are X = 0..4 and Y = 0..4.
  0,0 is the SOUTH WEST corner; NORTH is up (Y + 1), EAST is right (X + 1).

COMMANDS (one per line, upper or lower case)
  PLACE X,Y,F  Put the robot on the table at X,Y facing F.
               F is NORTH, SOUTH, EAST or WEST (full word).
               Example: PLACE 0,0,NORTH
  MOVE         Move the robot one unit forward in the direction it faces.
  LEFT         Turn the robot 90 degrees left, without moving it.
  RIGHT        Turn the robot 90 degrees right, without moving it.
  REPORT       Show the robot's position and direction, for example:
                 Output:
                 0,1,NORTH
  EXIT         End the session.

RULES
  - The first valid PLACE puts the robot on the table. Until then
    MOVE, LEFT, RIGHT and REPORT are ignored.
  - A PLACE or MOVE that would put the robot off the table is ignored;
    the robot stays where it was and later commands still work.
  - An ignored command or a line that is not a valid command shows a
    warning, and the next line is read.

ENDING A SESSION
  EXIT                     Ends the session.
  Ctrl+Z then Enter        End of input on Windows: ends the session.
  Ctrl+D                   End of input on Linux and macOS: ends the session.
  Ctrl+C                   Stops the app at once.
```

The tests cover many more cases than the one example per requirement here, such as every corner, full turn cycles and every bad-line variant. They are listed in [DEC-31][dec-31] and planned on page [9](09-considerations.md).

---

Back to the [solution design overview](README.md).

[dec-01]: ../design-analysis/design-decisions.md#dec-01-table-size-and-valid-positions
[dec-02]: ../design-analysis/design-decisions.md#dec-02-origin-and-directions
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
[dec-31]: ../design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../design-analysis/design-decisions.md#dec-32-never-crash
[fr-02]: #fr-02
[fr-04]: #fr-04
[fr-07]: #fr-07
[fr-10]: #fr-10
[fr-14]: #fr-14
[fr-01]: #fr-01
[fr-15]: #fr-15
[fr-16]: #fr-16
[fr-18]: #fr-18
[nfr-01]: 03-non-functional-requirements.md#nfr-01
[nfr-08]: 03-non-functional-requirements.md#nfr-08
[fr-08]: #fr-08
[fr-09]: #fr-09
