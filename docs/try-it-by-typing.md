# Try it by typing

How to run the application by hand and what to expect. Every warning and output line on this page was produced by running the same commands through the built application.

## Start a typed session
Run a script from the repository root with no file name:

| Shell | Command |
|---|---|
| cmd | `run` |
| PowerShell | `.\run.cmd` |
| bash, zsh (Linux, macOS) | `./run.sh` |

After the build the app shows a `> ` prompt. Type one command per line and press Enter. End with `EXIT`, or with Ctrl+Z then Enter (Windows) or Ctrl+D (Linux, macOS).

## Commands

| Command | What it does |
|---|---|
| `PLACE X,Y,F` | Puts the robot at X,Y (each 0 to 4) facing F: `NORTH`, `SOUTH`, `EAST` or `WEST`. Other commands are ignored until a valid PLACE; PLACE can be given again at any time. |
| `MOVE` | Moves one step forward, unless that would take the robot off the table. |
| `LEFT`, `RIGHT` | Turns 90 degrees without moving. |
| `REPORT` | Prints `Output:` and then the position, for example `3,3,NORTH`. |
| `EXIT` | Ends the session. |

Commands are accepted in any letter case.

## A normal session
Challenge example 3:
```
> PLACE 1,2,EAST
> MOVE
> MOVE
> LEFT
> MOVE
> REPORT
Output:
3,3,NORTH
> EXIT
```

## Commands that are ignored
Type these in a new session, in this order. Each prints its warning and the session carries on.

| Type | Warning |
|---|---|
| `MOVE` (before any PLACE) | `Warning: MOVE ignored - robot is not on the table` |
| `left` (before any PLACE) | `Warning: LEFT ignored - robot is not on the table` |
| `REPORT` (before any PLACE) | `Warning: REPORT ignored - robot is not on the table` |
| `PLACE 9,9,NORTH` | `Warning: PLACE 9,9,NORTH ignored - position is off the table` |
| `HELLO` | `Warning: 'HELLO' ignored - unknown command` |
| an empty line | `Warning: blank line ignored` |
| `PLACE 1,2` | `Warning: 'PLACE 1,2' ignored - expected PLACE X,Y,F` |
| `PLACE A,0,NORTH` | `Warning: 'PLACE A,0,NORTH' ignored - X must be a whole number` |
| `PLACE 1,2,UP` | `Warning: 'PLACE 1,2,UP' ignored - F must be NORTH, SOUTH, EAST or WEST` |
| `PLACE 1,2,N` | `Warning: 'PLACE 1,2,N' ignored - F must be NORTH, SOUTH, EAST or WEST` |
| `move now` | `Warning: 'move now' ignored - MOVE takes no values` |
| `> MOVE` | `Warning: '> MOVE' ignored - unknown command` |

## Blocked at the edge
In the same session. The blocked MOVE and the off-table PLACE are ignored; the commands after them still work:
```
> PLACE 4,2,EAST
> MOVE
Warning: MOVE ignored - next position is off the table so robot stays at 4,2,EAST
> REPORT
Output:
4,2,EAST
> LEFT
> MOVE
> REPORT
Output:
4,3,NORTH
> PLACE 7,7,NORTH
Warning: PLACE 7,7,NORTH ignored - position is off the table
> REPORT
Output:
4,3,NORTH
> EXIT
```

The rules behind each warning are in [design-decisions.md](design-analysis/design-decisions.md).

---

Back to the [README](../README.md).
