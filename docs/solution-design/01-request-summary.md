# 1. Request summary

What the challenge asks for, in a few sentences, before any design choice is made.

Source: [The challenge statement](../challenge/README.md).

## The application

The challenge asks for a console application that simulates a toy robot on a square table of 5 x 5 units. There is nothing else on the table. The robot can roam freely, but it must never fall off.

The application reads these commands:

| Command | What it does |
|---|---|
| `PLACE X,Y,F` | Puts the robot on the table at position X,Y, facing NORTH, SOUTH, EAST or WEST. (0,0) is the SOUTH WEST corner. |
| `MOVE` | Moves the robot one unit forward in the direction it is facing. |
| `LEFT`, `RIGHT` | Turns the robot 90 degrees to the left or right, without moving it. |
| `REPORT` | Announces the robot's X, Y and facing direction. |

The rules the challenge sets:
- The first command must be a PLACE. All commands before a valid PLACE are discarded. After that, any command can come in any order, including another PLACE.
- A robot that is not on the table may ignore MOVE, LEFT, RIGHT and REPORT.
- The robot must not fall off the table, either when it moves or when it is first placed. Any move that would make it fall is ignored, and later valid commands still work.
- Input can come from a file or be typed at the console, as the developer chooses. (The challenge calls the console input "standard input".)
- REPORT output can be in any form; printing it on the console is enough. (The challenge calls this "standard output".)

The challenge gives three examples with their expected result. For example, `PLACE 0,0,NORTH`, `MOVE`, `REPORT` gives `0,1,NORTH`.

The language is the developer's choice. This solution is a C# .NET 8 console application ([DEC-37][dec-37]).

## What must be delivered

The challenge asks for ([DEC-37][dec-37]):
1. The complete source code.
2. Test data to exercise the application.
3. A short README explaining how to build and run it.
4. A brief description of the development process: tools used, resources consulted, and notable decisions and trade-offs.
5. If AI was used to write any part, the prompts and artefacts used as inputs to code generation.

## What the request leaves open

The challenge does not settle every detail. Some points it leaves to the developer, some it only implies, and some it does not mention. Each one is settled by a decision, and the exact behaviour is in the [functional requirements](02-functional-requirements.md).

| Point | What the challenge says | Settled by |
|---|---|---|
| Which positions are on the table | A table of 5 x 5 units with (0,0) at the SOUTH WEST corner. This implies positions 0..4, but the range is not written out. | [DEC-01][dec-01] |
| Where the input comes from | "from a file, or from standard input, as the developer chooses". | [DEC-04][dec-04] |
| How strictly a line is read | It shows the command form (`PLACE X,Y,F`, upper case, full direction words) and one command per line. It does not say whether lower case, extra spaces or abbreviations such as `N` are accepted. | [DEC-05][dec-05] to [DEC-08][dec-08] |
| The `>` before each example command | Every example command starts with `> `. It does not say whether this is a prompt or part of the input. | [DEC-09][dec-09], assumption [A1](12-assumptions.md) |
| How a session ends | Nothing. | [DEC-10][dec-10] |
| A PLACE that is off the table | The robot must not fall off, and this "also includes the initial placement". Commands are discarded until a valid PLACE. It does not say what happens to an off-table PLACE once the robot is already on the table, and it gives no default position. | [DEC-15][dec-15] |
| Whether the user is told why a command was ignored | Commands are "ignored" or "discarded". It does not say whether the user is told. | [DEC-19][dec-19], [DEC-22][dec-22] |
| A line that is not a valid command | Nothing. | [DEC-20][dec-20] |
| The REPORT format | REPORT "can be in any form, but standard output is sufficient". The examples show `X,Y,F` (for example `0,1,NORTH`) below `Output:`. It is not clear whether `Output:` is printed by the application or is a heading in the document. | [DEC-21][dec-21], [DEC-23][dec-23], assumption [A1](12-assumptions.md) |
| How a reviewer will check the solution | Nothing. | Assumption [A1](12-assumptions.md) |

---

Back to the [solution design overview](README.md).

[dec-01]: ../design-analysis/design-decisions.md#dec-01-table-size-and-valid-positions
[dec-04]: ../design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-05]: ../design-analysis/design-decisions.md#dec-05-one-command-per-line
[dec-08]: ../design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-09]: ../design-analysis/design-decisions.md#dec-09-a-leading--is-not-part-of-a-command
[dec-10]: ../design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-15]: ../design-analysis/design-decisions.md#dec-15-place
[dec-19]: ../design-analysis/design-decisions.md#dec-19-robot-commands-before-the-robot-is-on-the-table
[dec-20]: ../design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../design-analysis/design-decisions.md#dec-23-output-channels
[dec-37]: ../design-analysis/design-decisions.md#dec-37-deliverables
