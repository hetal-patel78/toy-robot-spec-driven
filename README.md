# Toy Robot Simulator

[![CI](https://github.com/hetal-patel78/toy-robot-spec-driven/actions/workflows/ci.yml/badge.svg)](https://github.com/hetal-patel78/toy-robot-spec-driven/actions/workflows/ci.yml)

A C# .NET 8 console application for the classic "Toy Robot" coding challenge, built to learn spec-driven development with GitHub Spec Kit and Claude Code: a robot moving on a 5 x 5 table, driven by `PLACE`, `MOVE`, `LEFT`, `RIGHT` and `REPORT` commands.

## Status
**Finished.** The application, the run scripts, the tests and the CI workflow are in place. The file-based commands below were run on Windows (cmd and PowerShell) and through the run scripts in CI on Windows, Linux and macOS; the final word-for-word check of this README on a fresh clone is still to be done.

## Requirements
- A .NET SDK, version 8 or later: <https://dotnet.microsoft.com/download>. The app also runs where only a later major version of .NET is installed.

## Run it
Run the commands from the repository root. The scripts build the app quietly and then run it.

| Shell | With a file of commands | Typing the commands |
|---|---|---|
| cmd | `run commands.txt` | `run` |
| PowerShell | `.\run.cmd commands.txt` | `.\run.cmd` |
| bash, zsh (Linux, macOS) | `./run.sh commands.txt` | `./run.sh` |

While they work, the scripts show what they are doing on the console (checking .NET, building, then running); only the application's REPORT output goes to console output (stdout).

When typing, end the session with `EXIT`, or with Ctrl+Z then Enter (Windows) or Ctrl+D (Linux, macOS). To try it by hand, see [docs/try-it-by-typing.md](docs/try-it-by-typing.md): the commands, a normal session, the commands that are ignored with the exact warning each prints, and a session blocked at the edge.

## The three challenge examples
Copy and run; each prints the two lines shown.

| Example | cmd | PowerShell | bash, zsh | Console output (stdout) |
|---|---|---|---|---|
| 1 | `run testdata\challenge-example-1.txt` | `.\run.cmd testdata\challenge-example-1.txt` | `./run.sh testdata/challenge-example-1.txt` | `Output:` `0,1,NORTH` |
| 2 | `run testdata\challenge-example-2.txt` | `.\run.cmd testdata\challenge-example-2.txt` | `./run.sh testdata/challenge-example-2.txt` | `Output:` `0,0,WEST` |
| 3 | `run testdata\challenge-example-3.txt` | `.\run.cmd testdata\challenge-example-3.txt` | `./run.sh testdata/challenge-example-3.txt` | `Output:` `3,3,NORTH` |

## Build, test and run directly
```
dotnet build -c Release
dotnet test
```

| Shell | Direct run of example 1 |
|---|---|
| cmd | `src\ToyRobot\bin\Release\net8.0\ToyRobot.exe < testdata\challenge-example-1.txt` |
| PowerShell | `cmd /c "src\ToyRobot\bin\Release\net8.0\ToyRobot.exe < testdata\challenge-example-1.txt"` |
| bash, zsh | `./src/ToyRobot/bin/Release/net8.0/ToyRobot < testdata/challenge-example-1.txt` |

PowerShell has no `<` for input, so it uses `cmd /c` here (or the run script above).

## Output format
`REPORT` prints `Output:` and then `X,Y,F` (for example `0,1,NORTH`) on console output (stdout). Everything else (the prompt, the echo of each line read from a file, warnings and the how-to-run message) goes to console messages (stderr), so redirecting stdout to a file saves only the `Output:` and position lines. Exit codes: 0 normal end, 1 any argument (how-to-run message), 2 unexpected problem (one `Error: unexpected problem - ...` line on stderr instead of a stack trace).

```
Output:
0,1,NORTH
```

## Key assumptions
- The table is 5 x 5; (0,0) is the south-west corner.
- Commands come from console input (stdin), one per line; the app takes no arguments.
- Commands are accepted in any letter case; directions are the full words `NORTH`, `SOUTH`, `EAST` and `WEST`.
- A command that cannot be carried out is ignored with a warning on console messages (stderr); the run continues.
- `EXIT` ends a session.

All behaviour is defined in [docs/design-analysis/design-decisions.md](docs/design-analysis/design-decisions.md).

## Tested on Windows, Linux and macOS
The solution is tested on all three systems through the [CI workflow](https://github.com/hetal-patel78/toy-robot-spec-driven/actions/workflows/ci.yml), which on every push builds it, runs the unit, whole-flow and end-to-end tests, and runs each run script with the test data and checks its output.

## Test data
`testdata/` holds pairs of an input file and its expected console output (stdout):

| Pair | What it shows |
|---|---|
| `challenge-example-1`, `challenge-example-2`, `challenge-example-3` | the three challenge examples |
| `bom-challenge-example-1` | Challenge example 1 in a file that starts with a byte-order mark |
| `exit-mid-file` | `EXIT` in the middle of a file: later lines are not run |
| `empty` | an empty file: no output, exit code 0 |

## How this would evolve
Multiple robots, a configurable table size and obstacles are not built. The design keeps each of them a local change ([DEC-38][dec-38], [DEC-39][dec-39] in the design decisions).

## How it was built
The developer's brainstorm, the design analysis ([design-decisions.md](docs/design-analysis/design-decisions.md)) and the solution design ([docs/solution-design/](docs/solution-design/README.md)) came first. The GitHub Spec Kit specification (constitution, specs, plans and tasks) and the code were then generated from them with Claude Code and GitHub Spec Kit. The result is checked by tests at three levels (unit, whole-flow and end-to-end) and by CI on Windows, Linux and macOS. The prompts are in the [design-phase log](docs/design-analysis/prompts-design-phase.md) and the [implementation-phase log](docs/design-analysis/prompts-implementation-phase.md); the Spec Kit brief and inputs are in [docs/spec-kit/](docs/spec-kit/), and the decisions taken while writing the specification and code, with all check, test and CI results, are in the [Spec Kit run report](docs/spec-kit/implementation-run-report.md). Every decision taken in the run is listed there and was confirmed by the developer.

## Where to look
| What | Location |
|---|---|
| The challenge | [docs/challenge/README.md](docs/challenge/README.md) |
| Trying the application by hand: commands, a normal session, ignored commands and their warnings | [docs/try-it-by-typing.md](docs/try-it-by-typing.md) |
| Design analysis overview | [docs/design-analysis/README.md](docs/design-analysis/README.md) |
| Design decisions (the single source of truth) | [docs/design-analysis/design-decisions.md](docs/design-analysis/design-decisions.md) |
| Handwritten initial analysis | [docs/design-analysis/handwritten/images/](docs/design-analysis/handwritten/images/) (one image per page; the PDFs are in [docs/design-analysis/handwritten/](docs/design-analysis/handwritten/)) |
| Clarifications | [docs/design-analysis/clarifications/images/](docs/design-analysis/clarifications/images/) (one image per page; the PDFs are in [docs/design-analysis/clarifications/](docs/design-analysis/clarifications/)) |
| Instructions for the AI assistant | [CLAUDE.md](CLAUDE.md) |
| Prompts used in the design phase | [docs/design-analysis/prompts-design-phase.md](docs/design-analysis/prompts-design-phase.md) |
| Prompts used in the implementation phase | [docs/design-analysis/prompts-implementation-phase.md](docs/design-analysis/prompts-implementation-phase.md) |
| Prompts used while reviewing the delivered work (questions and answers) | [docs/design-analysis/prompts-review.md](docs/design-analysis/prompts-review.md) |
| Spec Kit constitution and specifications | [.specify/memory/constitution.md](.specify/memory/constitution.md), [specs/](specs/) |
| Solution design | [docs/solution-design/README.md](docs/solution-design/README.md) |

[dec-38]: docs/design-analysis/design-decisions.md#dec-38-one-robot-now-room-for-more-later
[dec-39]: docs/design-analysis/design-decisions.md#dec-39-not-included
