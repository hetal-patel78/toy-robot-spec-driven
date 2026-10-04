# Contract: how CommandParser reads one line

The grammar of one input line and the exact parse result for each case ([DEC-06][dec-06] to [DEC-10][dec-10], [DEC-20][dec-20], [DEC-22][dec-22]; [FR-09][fr-09], [FR-10][fr-10], [FR-10a][fr-10a]). Every reason is the warning text without `Warning: `; the Runner (spec 002) adds the prefix. Strings here are copied character for character from [DEC-22][dec-22].

## Reading steps (DEC-08)

1. `trimmed` = the line with whitespace of any kind removed from both ends (`string.Trim()`).
2. Command word = `trimmed` up to the first whitespace character (`char.IsWhiteSpace`), or all of `trimmed`. `rest` = what follows that whitespace character.
3. The command word is compared with `PLACE`, `MOVE`, `LEFT`, `RIGHT`, `REPORT` and `EXIT` with an ordinal comparison that ignores case.
4. For PLACE, `rest` is split on `,` into parts, and each part is trimmed (`string.Trim()`).
5. X and Y: `int.TryParse(part, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value)`.
6. F: equal, ignoring case (ordinal), to the `Name` of one of `Direction.All` (`NORTH`, `EAST`, `SOUTH`, `WEST`).

## Results, in the order they are checked (DEC-22: command word, number of values, X, Y, F)

`<line>` = `trimmed` exactly as typed. `<COMMAND>` = the command name in upper case.

| # | Condition | Result |
|---|---|---|
| 1 | `trimmed` is empty | `Failure("blank line ignored")` |
| 2 | Command word is EXIT | `Exit()` (anything after it is ignored) |
| 3 | Command word is MOVE, LEFT, RIGHT or REPORT and nothing follows | `CommandObject(new MoveCommand())` / `LeftCommand` / `RightCommand` / `ReportCommand` |
| 4 | Command word is MOVE, LEFT, RIGHT or REPORT and something follows | `Failure("'<line>' ignored - <COMMAND> takes no values")` |
| 5 | Command word is PLACE and `rest` does not split into exactly 3 parts | `Failure("'<line>' ignored - expected PLACE X,Y,F")` |
| 6 | PLACE, X part not readable | `Failure("'<line>' ignored - X must be a whole number")` |
| 7 | PLACE, Y part not readable | `Failure("'<line>' ignored - Y must be a whole number")` |
| 8 | PLACE, F part not a direction name | `Failure("'<line>' ignored - F must be NORTH, SOUTH, EAST or WEST")` |
| 9 | PLACE, all three parts valid | `CommandObject(new PlaceCommand(x, y, f))` (no table check, [DEC-28][dec-28]) |
| 10 | Any other command word | `Failure("'<line>' ignored - unknown command")` |

`Parse` never throws for any string ([DEC-20][dec-20], [DEC-32][dec-32]).

## Examples (each is a planned unit test)

| Input line | Result |
|---|---|
| `PLACE 4,4,NORTH` | `CommandObject(PlaceCommand(4, 4, North))` |
| `   MOVE   ` | `CommandObject(MoveCommand)` |
| `PLACE     0,0,NORTH` | `CommandObject(PlaceCommand(0, 0, North))` |
| `place 0 , 0 , north` | `CommandObject(PlaceCommand(0, 0, North))` |
| `PLACE 01,+1,EAST` | `CommandObject(PlaceCommand(1, 1, East))` |
| `place 09 , +9 , north` | `CommandObject(PlaceCommand(9, 9, North))` |
| `PLACE -1,0,NORTH` | `CommandObject(PlaceCommand(-1, 0, North))` |
| `PLACE 2147483647,0,NORTH` | `CommandObject(PlaceCommand(2147483647, 0, North))` |
| `[TAB]place[NBSP]0,[NBSP]0[TAB],north[NBSP]` | `CommandObject(PlaceCommand(0, 0, North))` |
| `PLACE[TAB]1,2,EAST` | `CommandObject(PlaceCommand(1, 2, East))` |
| `Place 1,1,East`, `mOvE`, `rEpOrT` | `PlaceCommand(1, 1, East)`, `MoveCommand`, `ReportCommand` |
| `EXIT`, `exit`, `Exit`, `EXIT now`, `exit now` | `Exit()` |
| (empty), spaces and tabs only | `Failure("blank line ignored")` |
| `HELLO` | `Failure("'HELLO' ignored - unknown command")` |
| `> MOVE` | `Failure("'> MOVE' ignored - unknown command")` |
| `EXITNOW` | `Failure("'EXITNOW' ignored - unknown command")` |
| `EXITING` | `Failure("'EXITING' ignored - unknown command")` |
| `PLACE0,0,NORTH` | `Failure("'PLACE0,0,NORTH' ignored - unknown command")` |
| `PLACEHOLDER` | `Failure("'PLACEHOLDER' ignored - unknown command")` |
| `PLACE,1,2` | `Failure("'PLACE,1,2' ignored - unknown command")` |
| `PLACE 1,2` | `Failure("'PLACE 1,2' ignored - expected PLACE X,Y,F")` |
| `PLACE 0 0 NORTH` | `Failure("'PLACE 0 0 NORTH' ignored - expected PLACE X,Y,F")` |
| `PLACE A,B` | `Failure("'PLACE A,B' ignored - expected PLACE X,Y,F")` |
| `PLACE 1,2,3,4` | `Failure("'PLACE 1,2,3,4' ignored - expected PLACE X,Y,F")` |
| `PLACE` | `Failure("'PLACE' ignored - expected PLACE X,Y,F")` |
| `PLACE A,0,NORTH` | `Failure("'PLACE A,0,NORTH' ignored - X must be a whole number")` |
| `PLACE 99999999999,0,NORTH` | `Failure("'PLACE 99999999999,0,NORTH' ignored - X must be a whole number")` |
| `PLACE 2147483648,0,NORTH` | `Failure("'PLACE 2147483648,0,NORTH' ignored - X must be a whole number")` |
| `PLACE 1 0,0,NORTH` | `Failure("'PLACE 1 0,0,NORTH' ignored - X must be a whole number")` |
| `PLACE ,,` | `Failure("'PLACE ,,' ignored - X must be a whole number")` |
| `PLACE A,B,UP` | `Failure("'PLACE A,B,UP' ignored - X must be a whole number")` |
| `PLACE 0,B,NORTH` | `Failure("'PLACE 0,B,NORTH' ignored - Y must be a whole number")` |
| `PLACE 0,99999999999,NORTH` | `Failure("'PLACE 0,99999999999,NORTH' ignored - Y must be a whole number")` |
| `PLACE 0,B,UP` | `Failure("'PLACE 0,B,UP' ignored - Y must be a whole number")` |
| `PLACE 1,2,UP` | `Failure("'PLACE 1,2,UP' ignored - F must be NORTH, SOUTH, EAST or WEST")` |
| `PLACE 0,0,N` | `Failure("'PLACE 0,0,N' ignored - F must be NORTH, SOUTH, EAST or WEST")` |
| `PLACE 1,2,N` | `Failure("'PLACE 1,2,N' ignored - F must be NORTH, SOUTH, EAST or WEST")` |
| `PLACE 1,2,` | `Failure("'PLACE 1,2,' ignored - F must be NORTH, SOUTH, EAST or WEST")` |
| `PLACE 1,2,NORTH EXTRA` | `Failure("'PLACE 1,2,NORTH EXTRA' ignored - F must be NORTH, SOUTH, EAST or WEST")` |
| `PLACE 1,2,1` | `Failure("'PLACE 1,2,1' ignored - F must be NORTH, SOUTH, EAST or WEST")` |
| `move now` | `Failure("'move now' ignored - MOVE takes no values")` |
| `MOVE now` | `Failure("'MOVE now' ignored - MOVE takes no values")` |
| `MOVE[NBSP]now` | `Failure("'MOVE[NBSP]now' ignored - MOVE takes no values")` (the quote keeps the non-breaking space) |
| `LEFT now` | `Failure("'LEFT now' ignored - LEFT takes no values")` |
| `RIGHT now` | `Failure("'RIGHT now' ignored - RIGHT takes no values")` |
| `REPORT now` | `Failure("'REPORT now' ignored - REPORT takes no values")` |
| `exit`, `right`, `place 1,2,east` under the `tr-TR` culture | `Exit()`, `CommandObject(RightCommand)`, `CommandObject(PlaceCommand(1, 2, East))` |

`[TAB]` is one tab character (U+0009); `[NBSP]` is one non-breaking space (U+00A0).

[dec-06]: ../../../docs/design-analysis/design-decisions.md#dec-06-commands
[dec-10]: ../../../docs/design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-20]: ../../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-22]: ../../../docs/design-analysis/design-decisions.md#dec-22-warning-wording
[dec-28]: ../../../docs/design-analysis/design-decisions.md#dec-28-the-parser-checks-format-the-table-checks-position
[dec-32]: ../../../docs/design-analysis/design-decisions.md#dec-32-never-crash
[fr-09]: ../spec.md#fr-09
[fr-10]: ../spec.md#fr-10
[fr-10a]: ../spec.md#fr-10a
