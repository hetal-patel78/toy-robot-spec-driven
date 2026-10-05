# Implementation Plan: Robot rules

**Branch**: `001-robot-rules` | **Date**: 2026-10-05 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-robot-rules/spec.md`

## Summary

This spec builds the solution skeleton and the robot rules: the Table, Direction, RobotState and Robot; `IRobotCommand` with `PlaceCommand`, `MoveCommand`, `LeftCommand`, `RightCommand` and `ReportCommand`; `IRobotOperator` and RobotOperator; and the CommandParser, with the parse result (`ParseResult`) and the outcome (`Outcome`) that the Runner of spec 002 writes as console text. It implements [FR-01][fr-01] to [FR-07][fr-07], [FR-09][fr-09], [FR-10][fr-10] (with [FR-10a][fr-10a]) and [NFR-07][nfr-07], and plans the unit tests ([DEC-31][dec-31] level 1). The approach is the design on pages 5 and 6: the parser checks format only ([DEC-28][dec-28]); each command class holds only its own logic ([DEC-27][dec-27]); RobotOperator holds the shared rules, "robot must be on the table" and "check with the Table, then replace" ([DEC-13][dec-13], [DEC-19][dec-19]); the robot's state is an immutable value that is replaced, never changed ([DEC-12][dec-12] to [DEC-14][dec-14]). Details settled during planning are in [research.md](research.md).

## Technical Context

**Language/Version**: C# on .NET 8 (`net8.0`), `RollForward` = `LatestMajor` ([DEC-37][dec-37]; pages 5 and 9). Nullable reference types and implicit usings on.

**Primary Dependencies**: none in the application. Tests: xUnit (`xunit` 2.5.3, `xunit.runner.visualstudio` 2.5.3, `Microsoft.NET.Test.Sdk` 17.8.0). No other library, mocking library, dependency injection container or logging framework (brief rule 7, [DEC-39][dec-39]).

**Storage**: N/A (all state in memory for one session).

**Testing**: xUnit unit tests in `tests/ToyRobot.Tests/Unit/` ([DEC-31][dec-31] level 1). Hand-written test doubles (`FakeRobotOperator`, `SpyCommand`) and a test helper (`RulesDriver`) instead of a mocking library (research R13). Runner whole-flow and console end-to-end tests belong to spec 002.

**Target Platform**: Windows, Linux and macOS ([DEC-33][dec-33]; proved by spec 003's CI). Nothing in this spec depends on the system: culture-independent reading and writing (research R8, R9), no file or console access.

**Project Type**: console application (one application project, two test projects).

**Performance Goals**: none stated by the sources; each line is handled in constant time.

**Constraints**: exact texts from [DEC-22][dec-22] character for character; no exception for any input line ([DEC-32][dec-32]); names and members exactly as page 6 (brief rule 6, [NFR-07][nfr-07]).

**Scale/Scope**: one robot, one table (5 x 5 in the application, any size in tests), six command words.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | How this plan meets it | Result |
|---|---|---|
| I. Clean Code and SOLID | One class per component with page 6's job and limits; one class per command (open/closed); commands reach the robot only through `IRobotOperator`; shared rules once in RobotOperator ([DEC-27][dec-27], [DEC-34][dec-34]). Names exactly page 6 plus the result types page 6 describes (research R4). | Pass |
| II. Three Test Levels, Tests First | This spec plans level 1 (unit). Every task phase writes its tests first and ends with all tests passing (tasks.md). Levels 2 and 3 are spec 002's. | Pass |
| III. Never Crash | `CommandParser.Parse` returns a failure, never throws, for every line; `int.TryParse` for numbers; positions checked before the robot changes ([DEC-13][dec-13], [DEC-20][dec-20]). The two argument checks (research R12) are unreachable from input. | Pass |
| IV. Cross-Platform | Invariant culture for numbers, ordinal ignore-case for words (research R8, R9), with tests under Turkish and Swedish settings; no system-specific code. | Pass |
| V. No Over-Engineering | Only page 6's components; no configuration, no extra packages; the coverage package and template files are left out (research R1). | Pass |
| VI. Standard Output for REPORT Output Only | Nothing in this spec writes to any channel; RobotOperator returns the position text without the `Output:` label and warnings without the `Warning: ` prefix ([DEC-21][dec-21], [DEC-23][dec-23]). The placeholder entry point writes nothing (research R2). | Pass |

Re-check after Phase 1 (data-model.md, contracts/, quickstart.md): no new component, package or behaviour was introduced; all six principles still pass. No entry in Complexity Tracking.

## Project Structure

### Documentation (this feature)

```text
specs/001-robot-rules/
├── spec.md
├── plan.md              # this file
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
├── contracts/
│   ├── robot-rules-api.md        # public types and members used by spec 002
│   └── command-line-grammar.md   # how CommandParser reads a line; every result
├── checklists/
│   └── requirements.md
└── tasks.md             # /speckit-tasks
```

### Source Code (repository root)

```text
ToyRobot.sln                         # classic .sln format (research R1)
.gitignore                           # bin/, obj/, TestResults/, .vs/ (research R3)
src/
└── ToyRobot/
    ├── ToyRobot.csproj              # Exe, net8.0, RollForward LatestMajor, no packages
    ├── Program.cs                   # placeholder entry point; replaced by spec 002 (research R2)
    ├── Domain/                      # namespace ToyRobot.Domain: the table, the robot and the shared rules
    │   ├── Table.cs
    │   ├── Direction.cs
    │   ├── RobotState.cs
    │   ├── Robot.cs
    │   ├── IRobotOperator.cs
    │   └── RobotOperator.cs
    ├── Commands/                    # namespace ToyRobot.Commands: one class per command and their outcome
    │   ├── IRobotCommand.cs
    │   ├── PlaceCommand.cs
    │   ├── MoveCommand.cs
    │   ├── LeftCommand.cs
    │   ├── RightCommand.cs
    │   ├── ReportCommand.cs
    │   └── Outcome.cs
    └── Parsing/                     # namespace ToyRobot.Parsing: from a line to a command
        ├── ParseResult.cs
        └── CommandParser.cs
tests/
├── ToyRobot.Tests/
│   ├── ToyRobot.Tests.csproj        # xUnit; references src/ToyRobot
│   └── Unit/
│       ├── Support/
│       │   ├── FakeRobotOperator.cs
│       │   ├── SpyCommand.cs
│       │   ├── RulesDriver.cs
│       │   └── CultureScope.cs
│       ├── TableTests.cs
│       ├── DirectionTests.cs
│       ├── RobotStateTests.cs
│       ├── RobotTests.cs
│       ├── PlaceCommandTests.cs
│       ├── MoveCommandTests.cs
│       ├── LeftCommandTests.cs
│       ├── RightCommandTests.cs
│       ├── ReportCommandTests.cs
│       ├── RobotOperatorTests.cs
│       ├── CommandParserTests.cs
│       ├── RobotRulesScenarioTests.cs
│       └── DesignRulesTests.cs
└── ToyRobot.ConsoleEndToEndTests/
    └── ToyRobot.ConsoleEndToEndTests.csproj   # xUnit; references src/ToyRobot; no tests until spec 002
```

`ToyRobot.Tests` will also hold spec 002's Runner whole-flow tests (page 9), in their own folder. The application's folders group its types, and the namespaces follow the folders: `ToyRobot.Domain` (Table, Direction, RobotState, Robot, IRobotOperator, RobotOperator), `ToyRobot.Commands` (IRobotCommand, the five command classes, Outcome) and `ToyRobot.Parsing` (ParseResult, CommandParser); spec 002 adds `ToyRobot.Session` (Runner), and Program stays in `ToyRobot` at the project root (see the spec's "Decided (developer, 2026-10-05): source folders"). These types are public, except Program, which is internal (the compiler-generated class of this spec's placeholder entry point, then `internal static class Program` in spec 002 T022); test classes are in `ToyRobot.Tests.Unit` (support types in `ToyRobot.Tests.Unit.Support`).

**Structure Decision**: the single-application layout fixed by page 5 (`src/ToyRobot/`, `tests/ToyRobot.Tests/`, `tests/ToyRobot.ConsoleEndToEndTests/`, `ToyRobot.sln` at the root). One file per type, named after the type. `testdata/`, `run.cmd`, `run.sh`, `.gitattributes`, the CI workflow and the README belong to spec 003; Runner and Program to spec 002.

## Implementation notes

- **Project files** (research R1): written by hand; the content is listed in research R1. The solution is created with `dotnet new sln -n ToyRobot --format sln` and the three projects are added with `dotnet sln ToyRobot.sln add <path>`.
- **Parser** (research R9 to R11, contracts/command-line-grammar.md): the order of checks gives [DEC-22][dec-22]'s "first problem only" rule without extra code.
- **RobotOperator.Run** (data-model.md): the "not on the table" check uses `command.Name` and `command.NeedsRobotOnTable`, so it is written once for all commands ([DEC-27][dec-27]); PLACE is never refused by it.
- **Text form** (research R8): `RobotState.ToString()` is the only place that writes `X,Y,F`.
- **Culture tests** (research R14): `CultureScope` is a small `IDisposable` test helper that sets and restores `CultureInfo.CurrentCulture` and `CurrentUICulture`.
- **Test names** use the challenge wording where possible, for example `Robot_not_on_table_ignores_MOVE`, `Move_that_would_fall_off_the_table_is_ignored` ([DEC-31][dec-31], [UT-03][ut-03]).

## Dependencies on other specs

- None before this spec: it is implemented first.
- Spec 002 builds on this spec's types (contracts/robot-rules-api.md), adds the Runner and Program (replacing the placeholder `Program.cs`), and adds Runner whole-flow tests to `tests/ToyRobot.Tests` and console end-to-end tests to `tests/ToyRobot.ConsoleEndToEndTests`.
- Spec 003 needs the built application and spec 002's console end-to-end tests.

## Complexity Tracking

No Constitution Check violations; nothing to justify.

[dec-12]: ../../docs/design-analysis/design-decisions.md#dec-12-robot-state-is-an-immutable-value
[dec-13]: ../../docs/design-analysis/design-decisions.md#dec-13-state-is-replaced-never-changed-in-place
[dec-14]: ../../docs/design-analysis/design-decisions.md#dec-14-only-one-way-to-change-the-robot
[dec-19]: ../../docs/design-analysis/design-decisions.md#dec-19-robot-commands-before-the-robot-is-on-the-table
[dec-20]: ../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../../docs/design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-27]: ../../docs/design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-28]: ../../docs/design-analysis/design-decisions.md#dec-28-the-parser-checks-format-the-table-checks-position
[dec-31]: ../../docs/design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../../docs/design-analysis/design-decisions.md#dec-32-never-crash
[dec-33]: ../../docs/design-analysis/design-decisions.md#dec-33-cross-platform
[dec-34]: ../../docs/design-analysis/design-decisions.md#dec-34-readable-code-solid
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[dec-39]: ../../docs/design-analysis/design-decisions.md#dec-39-not-included
[fr-01]: spec.md#fr-01
[fr-07]: spec.md#fr-07
[fr-09]: spec.md#fr-09
[fr-10]: spec.md#fr-10
[fr-10a]: spec.md#fr-10a
[nfr-07]: spec.md#nfr-07
[ut-03]: spec.md#ut-03
