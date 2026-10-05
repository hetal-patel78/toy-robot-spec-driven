# Quickstart: validating spec 001-robot-rules

How to check that this spec's part of the application is built and working. Details of the types are in [data-model.md](data-model.md) and [contracts/](contracts/); the test list is in [tasks.md](tasks.md).

## Prerequisites

- A .NET SDK, version 8 or later (`dotnet --list-sdks`). A newer SDK alone is enough to build and test, because every project sets `RollForward` = `LatestMajor` ([SK-02][sk-02], [DEC-37][dec-37]).
- A clone of the repository; all commands run from its root.

## Build and test

```text
dotnet build ToyRobot.sln
dotnet test ToyRobot.sln
```

Expected:
- The build succeeds for the three projects: `src/ToyRobot`, `tests/ToyRobot.Tests`, `tests/ToyRobot.ConsoleEndToEndTests` ([SK-01][sk-01]).
- Every unit test in `tests/ToyRobot.Tests/Unit/` passes. `tests/ToyRobot.ConsoleEndToEndTests` holds no tests until spec 002 adds them; `dotnet test` reports no test for it and still exits with code 0.
- `dotnet test` exits with code 0.

To run one group of tests, for example the parser tests:

```text
dotnet test tests/ToyRobot.Tests --filter "FullyQualifiedName~CommandParserTests"
```

## What to look for

| Check | Where | Expected |
|---|---|---|
| Solution file | repository root | `ToyRobot.sln` (classic format, not `.slnx`) |
| .NET settings | `src/ToyRobot/ToyRobot.csproj` | `net8.0`, `RollForward` = `LatestMajor`, no package references |
| Test packages | both test project files | only `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio` |
| Component names | `src/ToyRobot/` | exactly the files and members in `contracts/robot-rules-api.md`; nothing else except the placeholder `Program.cs` |
| Acceptance scenarios | `tests/ToyRobot.Tests/Unit/RobotRulesScenarioTests.cs` | one test per scenario in spec.md, all passing |
| Exact texts | parser, command and operator tests | every reason and warning text equal, character for character, to [DEC-22][dec-22] |

## Running the application

Not part of this spec. The placeholder `Program.cs` reads nothing, writes nothing and exits with code 0. Spec 002 replaces it with Program and adds the Runner, after which the scenarios in spec.md can be run as console text.

[dec-22]: ../../docs/design-analysis/design-decisions.md#dec-22-warning-wording
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[sk-01]: spec.md#sk-01
[sk-02]: spec.md#sk-02
