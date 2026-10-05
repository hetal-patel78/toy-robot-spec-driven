# Quickstart: validating the delivery

How to show that this spec works. Run from the repository root of a fresh clone, with a .NET SDK 8 or later installed. Expected results are in [contracts/run-scripts.md](contracts/run-scripts.md), [contracts/testdata.md](contracts/testdata.md) and [contracts/ci-workflow.md](contracts/ci-workflow.md).

## 1. Build and all tests (NFR-02; specs 001 and 002)

```text
dotnet build
dotnet test
```

All tests pass, including spec 002's [E2E-01][e2e-01] over the seven test-data pairs, [TD-02][td-02] and [BS-01][bs-01].

## 2. Run scripts with a challenge example (NFR-08)

| Shell | Command | Console output (stdout) | Exit code |
|---|---|---|---|
| cmd | `run testdata\challenge-example-1.txt` | `Output:` then `0,1,NORTH` | 0 |
| PowerShell | `.\run.cmd testdata\challenge-example-1.txt` | same | 0 (`$LASTEXITCODE`) |
| bash | `./run.sh testdata/challenge-example-1.txt` | same | 0 (`$?`) |

Console messages (stderr) show the progress lines `Checking for a .NET SDK, version 8 or later...`, `Building the application (Release)...` and `Running the application with commands from <file>` (the file as typed), then `> PLACE 0,0,NORTH`, `> MOVE`, `> REPORT`. Saving stdout only: `run testdata\challenge-example-1.txt > out.txt` (cmd) or `./run.sh testdata/challenge-example-1.txt > out.txt` (bash); `out.txt` holds the two lines.

## 3. Script error cases (NFR-08)

| Case | cmd | bash | Exit code | stdout |
|---|---|---|---|---|
| Missing file | `run testdata\no-such-file.txt` | `./run.sh testdata/no-such-file.txt` | 1 | empty |
| A folder | `run testdata` | `./run.sh testdata` | 1 | empty |
| Two arguments | `run a.txt b.txt` | `./run.sh a.txt b.txt` | 1 | empty |
| `--HELP` (help is case-sensitive) | `run --HELP` | `./run.sh --HELP` | 1 | empty |
| Help | `run --help` | `./run.sh --help` | 0 | the help text; stderr only the first two progress lines |
| Menu choice 4 | `run`, then `4`, `3`, `0`, `0` | `./run.sh`, then `4`, `3`, `0`, `0` | 0 | the menu, the help menu, the COMMANDS topic, the help menu, the menu |
| No `dotnet` | `cmd /c "set PATH=%SystemRoot%\System32&& run testdata\challenge-example-1.txt"` | `PATH=/nonexistent /bin/bash ./run.sh testdata/challenge-example-1.txt` | 1 | empty |
| Typed session | `run`, then type `EXIT` and Enter | `./run.sh`, then `EXIT` | 0 | empty; stderr shows the progress lines, then `> ` |
| No argument, redirected | `run < testdata\challenge-example-1.txt` | `./run.sh < testdata/challenge-example-1.txt` | 0 | `Output:`, `0,1,NORTH` |

The older-SDK and failed-build cases are run as in [CI-04][ci-04] (b) and [CI-05][ci-05] ([contracts/ci-workflow.md](contracts/ci-workflow.md)); by hand, the failed-build case is run in a temporary copy of the repository, never in the working copy.

## 4. CI (NFR-02)

Push the branch; the `CI` workflow passes on `windows-latest`, `ubuntu-latest` and `macos-latest` (`gh run watch`).

## 5. README check (NFR-09)

The README is the root `README.md`. Check that it holds, in about one screen, these parts ([DEC-37][dec-37]):

1. What it is.
2. Requirements: a .NET SDK 8 or later (link `https://dotnet.microsoft.com/download`); it also runs where only a later major version is installed ([NFR-03][nfr-03]).
3. How to run it with the scripts: `run commands.txt` (cmd), `.\run.cmd commands.txt` (PowerShell), `./run.sh commands.txt` (bash or zsh); with no file, type commands and end with `EXIT`.
4. Ready-to-copy commands for the three challenge examples in each shell form (`testdata/challenge-example-1.txt` to `-3.txt`) with their expected console output (stdout): `Output:` `0,1,NORTH`; `Output:` `0,0,WEST`; `Output:` `3,3,NORTH`.
5. Build, test and direct run: `dotnet build -c Release`; `dotnet test`; cmd `src\ToyRobot\bin\Release\net8.0\ToyRobot.exe < testdata\challenge-example-1.txt`; bash `./src/ToyRobot/bin/Release/net8.0/ToyRobot < testdata/challenge-example-1.txt`; PowerShell `cmd /c "src\ToyRobot\bin\Release\net8.0\ToyRobot.exe < testdata\challenge-example-1.txt"`.
6. The output format with an example: `Output:` then `X,Y,F` on console output (stdout); the prompt, echo, warnings and how-to-run message on console messages (stderr) ([DEC-21][dec-21], [DEC-23][dec-23]).
7. A few key assumptions (5 x 5 table, (0,0) SOUTH WEST; commands from console input; any letter case, full direction words; ignored commands give a warning; EXIT ends a session) with a link to `docs/design-analysis/design-decisions.md`.
8. Tested on Windows, Linux and macOS via GitHub Actions: the CI badge `https://github.com/hetal-patel78/toy-robot-spec-driven/actions/workflows/ci.yml/badge.svg`, linking to `https://github.com/hetal-patel78/toy-robot-spec-driven/actions/workflows/ci.yml`.
9. The test-data list (the seven pairs in `testdata/`).
10. "How this would evolve": multiple robots, a configurable table and obstacles; none is built ([DEC-38][dec-38], [DEC-39][dec-39]).
11. Where the development-process material and AI artefacts are: `CLAUDE.md`; `docs/design-analysis/prompts-design-phase.md`; `docs/design-analysis/prompts-implementation-phase.md`; the Spec Kit files (`.specify/memory/constitution.md`, `specs/`); the handwritten analysis (`docs/design-analysis/handwritten/`); the clarifications (`docs/design-analysis/clarifications/`); the design analysis (`docs/design-analysis/README.md`); the solution design (`docs/solution-design/README.md`). The existing "Where to look" links stay.

Then follow it word for word on a fresh clone (`git clone <repository> <new folder>`) in cmd, in PowerShell and in bash: every command works as written and every output it shows matches what is printed.

[bs-01]: spec.md#bs-01
[ci-04]: spec.md#ci-04
[ci-05]: spec.md#ci-05
[dec-21]: ../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[dec-38]: ../../docs/design-analysis/design-decisions.md#dec-38-one-robot-now-room-for-more-later
[dec-39]: ../../docs/design-analysis/design-decisions.md#dec-39-not-included
[e2e-01]: ../002-console-session/spec.md#e2e-01
[nfr-03]: spec.md#nfr-03
[td-02]: spec.md#td-02
