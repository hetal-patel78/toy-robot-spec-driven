# Data model: Delivery

This spec has no classes of its own. Its "entities" are files and the steps they follow. The application's components (page 6) belong to specs 001 and 002.

## Test-data pair (TD-01)

| Field | Value |
|---|---|
| Input file | `testdata/<name>.txt`: the lines fed to console input (stdin) |
| Expected file | `testdata/<name>.expected.txt`: the exact console output (stdout) for that input |
| Encoding | UTF-8; no byte-order mark except in `bom-challenge-example-1.txt` |
| Line endings | LF in the repository and in every checkout (`.gitattributes`); compared line by line, so CRLF output also passes |
| Last line | ends with a line break; the empty pair is two zero-byte files |

Rules: every input file has exactly one expected file with the same `<name>`; spec 002's [E2E-01][e2e-01] runs every pair; [TD-02][td-02] checks all seven are present. The seven names and their content are in [contracts/testdata.md](contracts/testdata.md).

## Run script (NFR-08)

Steps, in this order; each failing step ends the script:

| # | Step | Success | Failure: console messages (stderr) | Exit code |
|---|---|---|---|---|
| 1 | .NET SDK check (`dotnet --list-sdks`, major version 8 or more) | nothing shown | where to get the .NET SDK | 1 |
| 2 | Build `src/ToyRobot/ToyRobot.csproj`, `Release`, output captured, no console input | nothing shown | the captured build output | the build's |
| 3a | No argument | start the application with the script's console input | — | — |
| 3b | One argument, an existing file | start the application with the file on its console input (stdin) | — | — |
| 3c | One argument, missing or a folder | — | file not found | 1 |
| 3d | More than one argument | — | the script's how-to-run message | 1 |
| 4 | Application ends | — | — | the application's |

State: the script has no state between runs; the build output in `src/ToyRobot/bin/Release/net8.0/` is reused by the next build.

## CI job (NFR-02)

One job per system in the matrix `windows-latest`, `ubuntu-latest`, `macos-latest`. Steps and their checks are in [contracts/ci-workflow.md](contracts/ci-workflow.md). A system passes only when every step passes; the run passes only when all three systems pass.

## Line-ending rules (NFR-02a)

| Files | Checkout line endings |
|---|---|
| `*.cmd` | CRLF |
| every other text file | LF |
| `*.pdf` and other binary files | unchanged |
| `run.sh` | LF, stored with mode `100755` |

## README (NFR-09)

The root `README.md`, changed in place. Its parts, in order, are listed in [quickstart.md](quickstart.md) ("README check"), which is also the check that the README works.

[e2e-01]: ../002-console-session/spec.md#e2e-01
[td-02]: spec.md#td-02
