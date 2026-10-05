# Quickstart: checking the console session

How to prove this spec works once its tasks are done. Prerequisite: spec 001 is implemented and its tests pass; a .NET SDK 8 or later is installed. All commands run from the repository root.

## Build and run all tests

```text
dotnet build
dotnet test
```

Expected: the build succeeds; every unit test (spec 001), Runner whole-flow test and console end-to-end test passes. The console end-to-end tests start `src/ToyRobot/bin/Debug/net8.0/ToyRobot(.exe)`, built by the same command. To check the Release build the same way: `dotnet test -c Release`.

## Try it by hand

The application path (page 5) after `dotnet build` is `src/ToyRobot/bin/Debug/net8.0/ToyRobot.exe` on Windows and `src/ToyRobot/bin/Debug/net8.0/ToyRobot` on Linux and macOS (Release builds use `Release` in place of `Debug`). Below, `ToyRobot` stands for that path. A file `commands.txt` holding Challenge Example 1 (`PLACE 0,0,NORTH`, `MOVE`, `REPORT`) is used.

| Check | Command (cmd, bash or zsh; in PowerShell wrap as `cmd /c "..."`) | Expected |
|---|---|---|
| File run ([FR-08][fr-08], [FR-12][fr-12], [FR-14][fr-14]) | `ToyRobot < commands.txt` | stdout `Output:` / `0,1,NORTH`; stderr `> PLACE 0,0,NORTH` / `> MOVE` / `> REPORT`; exit code 0 |
| Only REPORT on stdout ([FR-12][fr-12]) | `ToyRobot < commands.txt > results.txt` | `results.txt` holds `Output:` and `0,1,NORTH`; the screen shows only the echo lines |
| Help ([FR-18][fr-18]) | `ToyRobot --help` | stdout the help text (starts `Toy Robot Simulator - help`); stderr empty; exit code 0 |
| How-to-run message ([FR-17][fr-17]) | `ToyRobot -h` (or `ToyRobot commands.txt`) | stderr `How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)`; stdout empty; exit code 1 |
| Typing ([FR-13][fr-13]) | `ToyRobot`, then type `MOVE`, Enter, `EXIT`, Enter | `> `, then `Warning: MOVE ignored - robot is not on the table`, then `> `; exit code 0 |
| End of input when typing ([FR-16][fr-16]) | `ToyRobot`, then Ctrl+Z, Enter (Windows) or Ctrl+D (Linux, macOS) | the last `> ` stays on screen; exit code 0 (on Windows the console gives Ctrl+Z as a line holding only U+001A, which the Runner treats as the end of input) |

Check the exit code with `echo %ERRORLEVEL%` (cmd), `echo $?` (bash, zsh) or `$LASTEXITCODE` (PowerShell).

## What the tests cover

The full mapping from requirement to test is in [tasks.md](tasks.md); the exact strings and exit codes are in [contracts/console-interface.md](contracts/console-interface.md); the Runner's steps are in [contracts/runner.md](contracts/runner.md).

[fr-08]: spec.md#fr-08
[fr-12]: spec.md#fr-12
[fr-13]: spec.md#fr-13
[fr-14]: spec.md#fr-14
[fr-16]: spec.md#fr-16
[fr-17]: spec.md#fr-17
[fr-18]: spec.md#fr-18
