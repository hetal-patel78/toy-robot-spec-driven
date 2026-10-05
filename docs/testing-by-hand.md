# Testing by hand

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
| `PLACE 9999999999,0,NORTH` | `Warning: 'PLACE 9999999999,0,NORTH' ignored - X is too large` |
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

## Run the test data
Each file in `testdata/` can be run the same way, from the repository root:

| Shell | Command (for example) |
|---|---|
| cmd | `run testdata\long-session.txt` |
| PowerShell | `.\run.cmd testdata\long-session.txt` |
| bash, zsh (Linux, macOS) | `./run.sh testdata/long-session.txt` |

After the script's three progress lines (checking .NET, building, running), the console shows each line of the file as `> ` and the line, then any warning or report for it. Only the `Output:` lines and the positions are console output (stdout); the expected file next to each input file (`<name>.expected.txt`) holds exactly those lines. What each file shows on the console:

### challenge-example-1
Challenge example 1.

```
> PLACE 0,0,NORTH
> MOVE
> REPORT
Output:
0,1,NORTH
```

### challenge-example-2
Challenge example 2.

```
> PLACE 0,0,NORTH
> LEFT
> REPORT
Output:
0,0,WEST
```

### challenge-example-3
Challenge example 3.

```
> PLACE 1,2,EAST
> MOVE
> MOVE
> LEFT
> MOVE
> REPORT
Output:
3,3,NORTH
```

### bom-challenge-example-1
Challenge example 1 in a file that starts with a byte-order mark: the mark is removed, so the result is the same as example 1.

```
> PLACE 0,0,NORTH
> MOVE
> REPORT
Output:
0,1,NORTH
```

### exit-mid-file
`EXIT` in the middle of the file: the `MOVE` and `REPORT` after it are not read.

```
> PLACE 0,0,NORTH
> REPORT
Output:
0,0,NORTH
> EXIT
```

### empty
An empty file: nothing is shown and the exit code is 0.

(nothing)

### long-session
One long session mixing every kind of line: commands before PLACE, PLACE off the table, MOVE blocked at two corners, bad and blank lines, lower case and spaces, a full turn and four REPORTs.

```
> MOVE
Warning: MOVE ignored - robot is not on the table
> REPORT
Warning: REPORT ignored - robot is not on the table
> PLACE 5,5,NORTH
Warning: PLACE 5,5,NORTH ignored - position is off the table
> PLACE 0,0,NORTH
> LEFT
> MOVE
Warning: MOVE ignored - next position is off the table so robot stays at 0,0,WEST
> REPORT
Output:
0,0,WEST
> RIGHT
> MOVE
> MOVE
> MOVE
> MOVE
> MOVE
Warning: MOVE ignored - next position is off the table so robot stays at 0,4,NORTH
> RIGHT
> MOVE
> MOVE
> MOVE
> MOVE
> MOVE
Warning: MOVE ignored - next position is off the table so robot stays at 4,4,EAST
> REPORT
Output:
4,4,EAST
> JUMP
Warning: 'JUMP' ignored - unknown command
> PLACE 2,2
Warning: 'PLACE 2,2' ignored - expected PLACE X,Y,F
> 
Warning: blank line ignored
> place 2,2,south
> move
> MOVE
> MOVE
Warning: MOVE ignored - next position is off the table so robot stays at 2,0,SOUTH
> RIGHT
> RIGHT
> RIGHT
> RIGHT
> LEFT
> MOVE
> REPORT
Output:
3,0,EAST
> PLACE -1,0,NORTH
Warning: PLACE -1,0,NORTH ignored - position is off the table
> REPORT
Output:
3,0,EAST
```

## Start-up checks
How the application and the run scripts start, as shown on [solution design page 7.7](solution-design/07-logical-flow/07-start-up.md): the argument check and the exit codes. Run these from the repository root after a build (`dotnet build -c Release`, or any run of a script).

See the exit code straight after a command:

| Shell | Command |
|---|---|
| cmd | `echo %ERRORLEVEL%` |
| PowerShell | `$LASTEXITCODE` |
| bash, zsh | `echo $?` |

**The application with an argument.** The application takes no arguments; it reads commands from console input. Any argument gives the how-to-run message and exit code 1, and no command is read.

| Shell | Command |
|---|---|
| cmd, PowerShell | `src\ToyRobot\bin\Release\net8.0\ToyRobot.exe testdata\challenge-example-1.txt` |
| bash, zsh | `./src/ToyRobot/bin/Release/net8.0/ToyRobot testdata/challenge-example-1.txt` |

Console message (stderr), exit code 1:
```
How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)
```

**The run scripts with a wrong argument.** After their progress lines, the scripts stop with one console message (stderr) and exit code 1; console output (stdout) is empty.

| Case | cmd (PowerShell: `.\run.cmd` instead of `run`) | bash, zsh | Console message (stderr) |
|---|---|---|---|
| Two files | `run testdata\challenge-example-1.txt testdata\challenge-example-2.txt` | `./run.sh testdata/challenge-example-1.txt testdata/challenge-example-2.txt` | `How to run: run testdata\challenge-example-1.txt to run the commands in a file, or run with no file to type the commands. In PowerShell, use .\run.cmd instead of run.` (bash: `How to run: ./run.sh testdata/challenge-example-1.txt to run the commands in a file, or ./run.sh with no file to type the commands.`) |
| A file that does not exist | `run testdata\no-such-file.txt` | `./run.sh testdata/no-such-file.txt` | `File not found: "testdata\no-such-file.txt"` (bash: `File not found: testdata/no-such-file.txt`) |
| A folder | `run testdata` | `./run.sh testdata` | `File not found: "testdata"` (bash: `File not found: testdata`) |
| `*` or `?` in the name (cmd only) | `run testdata\challenge-example-1.*` | | `The file name must not contain * or ?: "testdata\challenge-example-1.*"` |

**A normal run** (any command in "Run the test data" above, or a typed session ended with `EXIT`) ends with exit code 0.

**Exit code 2** (`Error: unexpected problem - ...`) is the last safeguard for a fault outside the application, such as the console closing while it writes. No input causes it, so it cannot be shown by hand; the Runner whole-flow tests in `tests/ToyRobot.Tests/RunnerWholeFlow/ProgramSafeguardTests.cs` check it.

The rules behind each warning are in [design-decisions.md](design-analysis/design-decisions.md).

---

Back to the [README](../README.md).
