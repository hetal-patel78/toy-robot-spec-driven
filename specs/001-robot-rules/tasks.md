---

description: "Task list for spec 001-robot-rules"
---

# Tasks: Robot rules

**Input**: Design documents from `/specs/001-robot-rules/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/ (robot-rules-api.md, command-line-grammar.md), quickstart.md

**Tests**: Required. The spec plans unit tests ([UT-01][ut-01] to [UT-04][ut-04], [DEC-31][dec-31] level 1) and the constitution (principle II) and brief rule 8 require tests first. In every phase the test tasks come before the code that makes them pass; a test file that does not compile yet because its type does not exist is in its failing state. Every phase ends with a checkpoint at which `dotnet build ToyRobot.sln` and `dotnet test ToyRobot.sln` from the repository root pass with every test green.

**Depends on**: nothing. This spec is implemented first. Spec 002-console-session starts only after the last checkpoint of this file (it builds on the types in `contracts/robot-rules-api.md` and replaces the placeholder `src/ToyRobot/Program.cs`); spec 003-delivery comes after spec 002.

**Organization**: Setup and Foundational phases, then one phase per user story in the spec's priority order, then Polish.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: can run in parallel (different files, no dependency on an unfinished task).
- **[Story]**: the user story in spec.md (US1 to US4).
- Every task cites its requirement (FR, NFR, SK, UT) and DEC IDs. Expected texts in test tasks are copied character for character from [DEC-22][dec-22]; write them exactly as shown, between the backticks.
- `[TAB]` means one tab character (U+0009) and `[NBSP]` one non-breaking space (U+00A0); in C# write them as `\t` and `\u00A0`.
- "Driver" means the test helper `RulesDriver` (T028): it passes each line to `CommandParser.Parse`, runs each command object with `RobotOperator.Run`, and gives back, in order, the position texts (from `Outcome.PositionText`) and the warning texts (from `Outcome.Warning` and from `ParseResult.Failure` reasons), plus the final `CurrentState`. Spec 002's Runner whole-flow tests check the same scenarios as console text.

## Path conventions

Application: `src/ToyRobot/`, grouped in the folders `Domain/`, `Commands/` and `Parsing/` (namespaces `ToyRobot.Domain`, `ToyRobot.Commands`, `ToyRobot.Parsing`; Program in `ToyRobot`). The files were first created directly in `src/ToyRobot/` and moved into these folders by T060. Unit tests: `tests/ToyRobot.Tests/Unit/` (namespace `ToyRobot.Tests.Unit`); test helpers: `tests/ToyRobot.Tests/Unit/Support/` (namespace `ToyRobot.Tests.Unit.Support`).

---

## Phase 1: Setup (solution skeleton)

**Purpose**: the solution and the three projects ([SK-01][sk-01] to [SK-03][sk-03]; [DEC-31][dec-31], [DEC-37][dec-37], [DEC-39][dec-39]; pages 5 and 9; research R1 to R3).

- [x] T001 Create `.gitignore` at the repository root with the lines `bin/`, `obj/`, `TestResults/` and `.vs/` ([NFR-06][nfr-06]; [DEC-31][dec-31], [DEC-37][dec-37]; research R3; spec Assumptions "Decided: `.gitignore`").
- [x] T002 Create `src/ToyRobot/ToyRobot.csproj` by hand (not from a template): `Sdk="Microsoft.NET.Sdk"`, `OutputType` `Exe`, `TargetFramework` `net8.0`, `RollForward` `LatestMajor`, `Nullable` `enable`, `ImplicitUsings` `enable`, `RootNamespace` `ToyRobot`, `AssemblyName` `ToyRobot`, no package references ([NFR-03][nfr-03]; [SK-02][sk-02], [SK-03][sk-03]; [DEC-37][dec-37], [DEC-39][dec-39]; research R1).
- [x] T003 Create `src/ToyRobot/Program.cs` as a placeholder entry point: a top-level `return 0;` with a comment saying it is a placeholder from spec 001 that spec 002 replaces with Program as page 6 describes. It reads nothing and writes nothing ([NFR-06][nfr-06]; [DEC-23][dec-23], [DEC-26][dec-26]; research R2).
- [x] T004 [P] Create `tests/ToyRobot.Tests/ToyRobot.Tests.csproj` by hand: `TargetFramework` `net8.0`, `RollForward` `LatestMajor`, `Nullable` `enable`, `ImplicitUsings` `enable`, `IsPackable` `false`, `IsTestProject` `true`; package references exactly `Microsoft.NET.Test.Sdk` 17.8.0, `xunit` 2.5.3, `xunit.runner.visualstudio` 2.5.3; `<Using Include="Xunit" />`; a project reference to `..\..\src\ToyRobot\ToyRobot.csproj`. No `coverlet.collector`, no `UnitTest1.cs` ([NFR-03][nfr-03], [NFR-06][nfr-06]; [SK-01][sk-01], [SK-03][sk-03]; [DEC-31][dec-31]; research R1).
- [x] T005 [P] Create `tests/ToyRobot.ConsoleEndToEndTests/ToyRobot.ConsoleEndToEndTests.csproj` with the same content as T004 (same settings, packages and project reference). It holds no test file; spec 002 adds its tests ([NFR-03][nfr-03], [NFR-06][nfr-06]; [SK-01][sk-01]; [DEC-31][dec-31], [DEC-37][dec-37]; spec Assumptions "the console end-to-end test project in the skeleton").
- [x] T006 Create `ToyRobot.sln` at the repository root with `dotnet new sln -n ToyRobot --format sln` (the `--format sln` option is needed with an SDK of version 9 or later, which otherwise creates `.slnx`), then add the three projects with `dotnet sln ToyRobot.sln add src/ToyRobot/ToyRobot.csproj tests/ToyRobot.Tests/ToyRobot.Tests.csproj tests/ToyRobot.ConsoleEndToEndTests/ToyRobot.ConsoleEndToEndTests.csproj` ([NFR-06][nfr-06]; [SK-01][sk-01]; [DEC-31][dec-31], [DEC-37][dec-37]; page 5).
- [x] T007 Run `dotnet build ToyRobot.sln` and `dotnet test ToyRobot.sln` from the repository root: both exit with code 0 (no tests yet). Run `git status` and confirm no `bin/` or `obj/` folder is listed ([NFR-06][nfr-06]; [SK-01][sk-01], [SC-007][sc-007]; [DEC-31][dec-31], [DEC-37][dec-37]).

**Checkpoint**: the skeleton builds and `dotnet test` passes. Commit.

---

## Phase 2: Foundational (types every story uses)

**Purpose**: Table, Direction, RobotState, Robot, the two interfaces and the two result types, with their unit tests ([FR-01][fr-01], [FR-05][fr-05], [NFR-07a][nfr-07a] to [NFR-07e][nfr-07e], [NFR-07g][nfr-07g]; [DEC-01][dec-01] to [DEC-03][dec-03], [DEC-11][dec-11] to [DEC-14][dec-14], [DEC-17][dec-17]; research R4, R5, R7, R8, R12).

**⚠️ CRITICAL**: no user story can start before this phase is complete.

### Test helpers and tests (write first)

- [x] T008 [P] Create the test helper `CultureScope` in `tests/ToyRobot.Tests/Unit/Support/CultureScope.cs`: an `IDisposable` whose constructor takes a culture name, keeps the current `CultureInfo.CurrentCulture` and `CurrentUICulture`, sets both to the named culture, and restores both in `Dispose` ([NFR-04][nfr-04], [FR-09][fr-09]; [DEC-07][dec-07], [DEC-08][dec-08]; research R14).
- [x] T009 [P] Create the test helper `FakeRobotOperator` in `tests/ToyRobot.Tests/Unit/Support/FakeRobotOperator.cs`: implements `IRobotOperator`; settable `CurrentState`; a settable `Answer` (`bool`, default `true`); `TryMoveTo(nextState)` adds `nextState` to a public list `Requests`, sets `CurrentState` to `nextState` when `Answer` is `true`, and returns `Answer` ([NFR-07g][nfr-07g], [NFR-06][nfr-06]; research R13; [DEC-14][dec-14]).
- [x] T010 [P] Create the test helper `SpyCommand` in `tests/ToyRobot.Tests/Unit/Support/SpyCommand.cs`: implements `IRobotCommand`; constructor `(string name, bool needsRobotOnTable, Outcome outcome)`; `Execute` adds 1 to a public `ExecuteCount` and returns `outcome` ([NFR-07e][nfr-07e], [NFR-06][nfr-06]; research R13; [DEC-27][dec-27]).
- [x] T011 [P] Write `TableTests` in `tests/ToyRobot.Tests/Unit/TableTests.cs` ([FR-01][fr-01]; [DEC-01][dec-01], [DEC-03][dec-03], [DEC-28][dec-28]): `Width` and `Height` return the constructor values; on a 5 x 5 table the four corners `0,0`, `4,0`, `0,4`, `4,4` and the middle `2,2` are on the table; `5,0`, `0,5`, `5,5`, `-1,0`, `0,-1`, `2147483647,0` and `-2147483648,0` are not; on a 2 x 2 table `0,0`, `1,0`, `0,1`, `1,1` are on the table and `2,0`, `0,2`, `-1,0`, `0,-1` are not (spec US1 scenario 9).
- [x] T012 [P] Write `DirectionTests` in `tests/ToyRobot.Tests/Unit/DirectionTests.cs` ([FR-05][fr-05]; [DEC-02][dec-02], [DEC-07][dec-07], [DEC-17][dec-17]; [NFR-07b][nfr-07b]): `Name` is `NORTH`, `EAST`, `SOUTH`, `WEST` for `North`, `East`, `South`, `West`; `ToString()` equals `Name`; `All` holds exactly `North`, `East`, `South`, `West` in that order; `TurnLeft()`: NORTH → WEST, WEST → SOUTH, SOUTH → EAST, EAST → NORTH; `TurnRight()`: NORTH → EAST, EAST → SOUTH, SOUTH → WEST, WEST → NORTH; four `TurnLeft()` calls and four `TurnRight()` calls each return the starting direction, for every direction; `TurnLeft().TurnRight()` returns the starting direction.
- [x] T013 [P] Write `RobotStateTests` in `tests/ToyRobot.Tests/Unit/RobotStateTests.cs` ([NFR-07c][nfr-07c]; [DEC-12][dec-12], [DEC-13][dec-13], [DEC-21][dec-21]; research R8): it holds `X`, `Y` and `Facing` as given; two states with the same values are equal; `state with { Facing = Direction.West }` gives a new state and leaves the original unchanged; `ToString()` gives `0,1,NORTH` for `(0, 1, North)`, `4,2,EAST` for `(4, 2, East)`, `-1,0,NORTH` for `(-1, 0, North)` and `2147483647,0,WEST` for `(2147483647, 0, West)`; inside `new CultureScope("sv-SE")`, `(-1, 0, North).ToString()` is exactly `-1,0,NORTH` (ASCII hyphen-minus, spec Clarifications).
- [x] T014 [P] Write `RobotTests` in `tests/ToyRobot.Tests/Unit/RobotTests.cs` ([NFR-07d][nfr-07d]; [DEC-11][dec-11], [DEC-14][dec-14]; research R12): a new Robot has `Placement` `null`; `MoveTo(new RobotState(0, 0, North))` sets `Placement` to that state; a second `MoveTo` replaces it; `MoveTo(null!)` throws `ArgumentNullException` and leaves `Placement` unchanged, both for a new robot (still `null`) and for a placed robot (still its state); the `Placement` property has no public setter (checked with reflection).

### Implementation

- [x] T015 [P] Implement `Table` in `src/ToyRobot/Domain/Table.cs` per data-model.md: constructor `Table(int width, int height)`, `Width`, `Height`, `Contains(int x, int y)` true exactly when `0 <= x < Width` and `0 <= y < Height`; no check of the size; no reference to the robot or commands ([FR-01][fr-01]; [DEC-01][dec-01], [DEC-03][dec-03], [DEC-28][dec-28]; [NFR-07a][nfr-07a]).
- [x] T016 [P] Implement `Direction` in `src/ToyRobot/Domain/Direction.cs` per research R7: `sealed class`, private constructor, `public static readonly` `North`, `East`, `South`, `West` (declared before `All`), `All` (`IReadOnlyList<Direction>`: North, East, South, West), `Name`, `TurnLeft()`, `TurnRight()`, `ToString()` returning `Name`; no position offsets ([FR-05][fr-05]; [DEC-07][dec-07], [DEC-17][dec-17]; [NFR-07b][nfr-07b]).
- [x] T017 Implement `RobotState` in `src/ToyRobot/Domain/RobotState.cs`: `public sealed record RobotState(int X, int Y, Direction Facing)` with `ToString()` overridden to return `X,Y,F` using `CultureInfo.InvariantCulture` for the numbers and `Facing.Name` for F; no table check ([NFR-07c][nfr-07c]; [DEC-12][dec-12], [DEC-21][dec-21], [DEC-22][dec-22]; research R8). Depends on T016.
- [x] T018 Implement `Robot` in `src/ToyRobot/Domain/Robot.cs`: `sealed class`; `public RobotState? Placement { get; private set; }` starting `null`; `public void MoveTo(RobotState nextState)` that calls `ArgumentNullException.ThrowIfNull(nextState)` and then sets `Placement`; no other member that changes `Placement`; no console, command or table use ([NFR-07d][nfr-07d]; [DEC-11][dec-11], [DEC-14][dec-14], [DEC-38][dec-38]). Depends on T017.
- [x] T019 [P] Implement `Outcome` in `src/ToyRobot/Commands/Outcome.cs` per research R5: `public abstract record Outcome` with a private constructor and exactly the nested cases `public sealed record PositionText(string Text) : Outcome;`, `public sealed record Warning(string Text) : Outcome;`, `public sealed record Done : Outcome;` ([FR-06][fr-06], [FR-07][fr-07], [NFR-07h][nfr-07h]; [DEC-21][dec-21], [DEC-26][dec-26]; page 6 "Outcome").
- [x] T020 [P] Implement `IRobotOperator` in `src/ToyRobot/Domain/IRobotOperator.cs`: exactly `RobotState? CurrentState { get; }` and `bool TryMoveTo(RobotState nextState);` ([NFR-07g][nfr-07g]; [DEC-13][dec-13], [DEC-14][dec-14]). Depends on T017.
- [x] T021 [P] Implement `IRobotCommand` in `src/ToyRobot/Commands/IRobotCommand.cs`: exactly `string Name { get; }`, `bool NeedsRobotOnTable { get; }` and `Outcome Execute(IRobotOperator robot);` ([NFR-07e][nfr-07e]; [DEC-27][dec-27]). Depends on T019, T020.
- [x] T022 Implement `ParseResult` in `src/ToyRobot/Parsing/ParseResult.cs` per research R5: `public abstract record ParseResult` with a private constructor and exactly the nested cases `public sealed record CommandObject(IRobotCommand Command) : ParseResult;`, `public sealed record Exit : ParseResult;`, `public sealed record Failure(string Reason) : ParseResult;` ([FR-10a][fr-10a]; [DEC-26][dec-26], [DEC-30][dec-30]; page 6 "Parse result"). Depends on T021.
- [x] T023 Run `dotnet build ToyRobot.sln` and `dotnet test ToyRobot.sln` from the repository root: T011 to T014 all pass ([NFR-06][nfr-06]; [SC-007][sc-007]; [DEC-31][dec-31]).

**Checkpoint**: foundation ready; all tests pass. Commit with the task IDs.

---

## Phase 3: User Story 1 - Place the robot and read its position (Priority: P1) 🎯 MVP

**Goal**: PLACE (valid and off the table), REPORT on a placed robot, RobotOperator's "check, then replace", and the parser for PLACE and REPORT ([FR-01][fr-01], [FR-02][fr-02], [FR-06][fr-06], [FR-09][fr-09], [FR-10a][fr-10a]; [DEC-11][dec-11], [DEC-13][dec-13], [DEC-15][dec-15], [DEC-18][dec-18], [DEC-21][dec-21], [DEC-22][dec-22], [DEC-28][dec-28]).

**Independent Test**: unit tests of `PlaceCommand`, `ReportCommand`, RobotOperator and CommandParser with PLACE and REPORT inputs only, and spec US1 scenarios 1, 2, 3, 5, 6 and 7 through the driver. (US1 scenario 4 needs [FR-07][fr-07] and is tested in Phase 5; US1 scenarios 8 and 9 need MOVE and are tested in Phase 4.)

### Tests for User Story 1 (write first)

- [x] T024 [P] [US1] Write `PlaceCommandTests` in `tests/ToyRobot.Tests/Unit/PlaceCommandTests.cs` with `FakeRobotOperator` ([FR-02][fr-02]; [DEC-13][dec-13], [DEC-15][dec-15], [DEC-22][dec-22], [DEC-27][dec-27]; [NFR-07f][nfr-07f]): `Name` is `PLACE`; `NeedsRobotOnTable` is `false`; `new PlaceCommand(1, 1, Direction.East)` holds `X` 1, `Y` 1, `F` East; `Execute` makes exactly one `TryMoveTo` request, with `new RobotState(1, 1, East)`, and returns `new Outcome.Done()` when the answer is yes; with the answer no, `PlaceCommand(9, 9, North)` returns `new Outcome.Warning("PLACE 9,9,NORTH ignored - position is off the table")`, `PlaceCommand(-1, 0, North)` returns `new Outcome.Warning("PLACE -1,0,NORTH ignored - position is off the table")` (also inside `new CultureScope("sv-SE")`), and `PlaceCommand(2147483647, 0, North)` returns `new Outcome.Warning("PLACE 2147483647,0,NORTH ignored - position is off the table")`; `Execute` works when `CurrentState` is `null` (PLACE has no precondition).
- [x] T025 [P] [US1] Write `ReportCommandTests` in `tests/ToyRobot.Tests/Unit/ReportCommandTests.cs` with `FakeRobotOperator` ([FR-06][fr-06]; [DEC-07][dec-07], [DEC-18][dec-18], [DEC-21][dec-21]; [NFR-07f][nfr-07f]; research R12): `Name` is `REPORT`; `NeedsRobotOnTable` is `true`; at `(1, 2, North)` `Execute` returns `new Outcome.PositionText("1,2,NORTH")`, at `(0, 0, North)` `new Outcome.PositionText("0,0,NORTH")`, at `(4, 4, West)` `new Outcome.PositionText("4,4,WEST")`; it makes no `TryMoveTo` request; with `CurrentState` `null`, `Execute` throws `InvalidOperationException`.
- [x] T026 [P] [US1] Write the US1 part of `RobotOperatorTests` in `tests/ToyRobot.Tests/Unit/RobotOperatorTests.cs` with a real `Table(5, 5)` unless stated ([FR-01][fr-01], [FR-02][fr-02], [FR-06][fr-06]; [DEC-11][dec-11], [DEC-13][dec-13], [DEC-15][dec-15], [DEC-28][dec-28]; [NFR-07h][nfr-07h]): a new RobotOperator has `CurrentState` `null`; `TryMoveTo((2, 2, North))` returns `true` and sets `CurrentState`; `TryMoveTo((5, 0, North))` returns `false` and leaves `CurrentState` unchanged (both `null` and placed); `Run(new PlaceCommand(0, 0, North))` returns `new Outcome.Done()` and `CurrentState` is `(0, 0, North)`; `Run(new PlaceCommand(4, 4, North))` then `Run(new ReportCommand())` returns `new Outcome.PositionText("4,4,NORTH")`; `Place_off_the_table_is_ignored_when_not_on_table`: `Run(PlaceCommand(9, 9, North))` returns `new Outcome.Warning("PLACE 9,9,NORTH ignored - position is off the table")` and `CurrentState` stays `null`; `Place_off_the_table_is_ignored_when_on_table`: after `PlaceCommand(4, 2, North)`, `PlaceCommand(9, 9, North)` gives the same warning and `CurrentState` stays `(4, 2, North)`; `A_second_valid_PLACE_moves_the_robot`: `(4, 2, North)` then `(1, 1, East)` gives `(1, 1, East)`; `PlaceCommand(-1, 0, North)` gives `new Outcome.Warning("PLACE -1,0,NORTH ignored - position is off the table")` and `null`; `PlaceCommand(2147483647, 0, North)` gives `new Outcome.Warning("PLACE 2147483647,0,NORTH ignored - position is off the table")`; on a `Table(2, 2)`, `PlaceCommand(2, 2, North)` gives `new Outcome.Warning("PLACE 2,2,NORTH ignored - position is off the table")` and `PlaceCommand(1, 1, North)` gives `new Outcome.Done()`.
- [x] T027 [P] [US1] Write the US1 part of `CommandParserTests` in `tests/ToyRobot.Tests/Unit/CommandParserTests.cs` ([FR-09][fr-09], [FR-10a][fr-10a]; [DEC-07][dec-07], [DEC-08][dec-08], [DEC-15][dec-15], [DEC-28][dec-28]): `Parse` returns `new ParseResult.CommandObject(new PlaceCommand(4, 4, Direction.North))` for `PLACE 4,4,NORTH`; `(1, 2, North)` for `place 1,2,north`; `(4, 2, North)` for `PLACE 4,2,NORTH`; `(1, 1, East)` for `PLACE 1,1,EAST`; `(0, 0, South)` for `PLACE 0,0,SOUTH`; `(3, 4, West)` for `PLACE 3,4,WEST`; `(-1, 0, North)` for `PLACE -1,0,NORTH` (format only, no table check); `(9, 9, North)` for `place 09 , +9 , north` and for `PLACE 9,9,NORTH`; `(2147483647, 0, North)` for `PLACE 2147483647,0,NORTH`; and `new ParseResult.CommandObject(new ReportCommand())` for `REPORT` and `report`. The direction in each result is the same instance as `Direction.North` (or East, South, West).
- [x] T028 [US1] Create the test helper `RulesDriver` in `tests/ToyRobot.Tests/Unit/Support/RulesDriver.cs` (research R13): constructor takes a `Table`; `Run(params string[] lines)` creates one `CommandParser` and one `RobotOperator(table)`, and for each line in order: `Failure` → add `Reason` to `Warnings`; `Exit` → stop; `CommandObject` → `Run(Command)`, then `PositionText` → add `Text` to `Positions`, `Warning` → add `Text` to `Warnings`, `Done` → nothing. Exposes `Positions`, `Warnings` (both `IReadOnlyList<string>`) and `FinalState` (`RobotState?`). It writes nothing ([NFR-06][nfr-06], [FR-10a][fr-10a]; [DEC-27][dec-27], [DEC-31][dec-31]).
- [x] T029 [US1] Write the US1 scenarios in `tests/ToyRobot.Tests/Unit/RobotRulesScenarioTests.cs`, one test each, with the driver on `Table(5, 5)`; each test checks `Positions` and `Warnings` exactly (spec.md US1; [FR-01][fr-01], [FR-02][fr-02], [FR-06][fr-06]; [DEC-01][dec-01], [DEC-02][dec-02], [DEC-07][dec-07], [DEC-11][dec-11], [DEC-15][dec-15], [DEC-21][dec-21], [DEC-22][dec-22]): scenario 1 `PLACE 4,4,NORTH`, `REPORT` → positions `4,4,NORTH`, no warnings; scenario 2 `PLACE 4,2,NORTH`, `PLACE 9,9,NORTH`, `REPORT` → positions `4,2,NORTH`, warnings `PLACE 9,9,NORTH ignored - position is off the table`; scenario 3 `place 1,2,north`, `report` → `1,2,NORTH`, none; scenario 5 `PLACE 4,2,NORTH`, `PLACE 1,1,EAST`, `REPORT` → `1,1,EAST`, none; scenario 6 `PLACE -1,0,NORTH` → no positions, warnings `PLACE -1,0,NORTH ignored - position is off the table`, final state `null`; scenario 7 `place 09 , +9 , north` → no positions, warnings `PLACE 9,9,NORTH ignored - position is off the table`.

### Implementation for User Story 1

- [x] T030 [P] [US1] Implement `PlaceCommand` in `src/ToyRobot/Commands/PlaceCommand.cs`: `public sealed record PlaceCommand(int X, int Y, Direction F) : IRobotCommand`; `Name` `PLACE`; `NeedsRobotOnTable` `false`; `Execute`: `proposed = new RobotState(X, Y, F)`; `robot.TryMoveTo(proposed)` → `new Outcome.Done()`, otherwise `new Outcome.Warning($"PLACE {proposed} ignored - position is off the table")`. No table check of its own, no output ([FR-02][fr-02]; [DEC-13][dec-13], [DEC-15][dec-15], [DEC-22][dec-22], [DEC-27][dec-27]; [NFR-07f][nfr-07f]).
- [x] T031 [P] [US1] Implement `ReportCommand` in `src/ToyRobot/Commands/ReportCommand.cs`: `public sealed record ReportCommand : IRobotCommand`; `Name` `REPORT`; `NeedsRobotOnTable` `true`; `Execute`: `CurrentState` or, when `null`, throw `InvalidOperationException`; return `new Outcome.PositionText(current.ToString())`; no `TryMoveTo` ([FR-06][fr-06]; [DEC-18][dec-18], [DEC-21][dec-21]; [NFR-07f][nfr-07f]; research R12).
- [x] T032 [US1] Implement `RobotOperator` in `src/ToyRobot/Domain/RobotOperator.cs`: `public sealed class RobotOperator : IRobotOperator`; constructor `RobotOperator(Table table)` keeps the table and creates the only `Robot` in a private read-only field; `CurrentState` returns the robot's `Placement`; `TryMoveTo(nextState)` returns `false` when `!table.Contains(nextState.X, nextState.Y)`, otherwise calls `robot.MoveTo(nextState)` and returns `true`; `Run(IRobotCommand command)` returns `command.Execute(this)` (the "not on the table" check is added in T048). No member returns the Robot; no parsing, output, labels or prefixes ([FR-02][fr-02]; [DEC-13][dec-13], [DEC-14][dec-14], [DEC-26][dec-26], [DEC-27][dec-27]; [NFR-07h][nfr-07h]). Depends on T015, T018, T020, T021.
- [x] T033 [US1] Implement the first part of `CommandParser` in `src/ToyRobot/Parsing/CommandParser.cs` per contracts/command-line-grammar.md: `public sealed class CommandParser` with no fields and `public ParseResult Parse(string line)`: trim with `Trim()`; command word up to the first `char.IsWhiteSpace` character; compare words with `StringComparison.OrdinalIgnoreCase`; `REPORT` with nothing after it → `CommandObject(new ReportCommand())`; `PLACE`: split the rest on `,`, trim each part, read X and Y with `int.TryParse(part, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value)`, match F against `Direction.All` by `Name` with `StringComparison.OrdinalIgnoreCase` (never `Enum.TryParse`), and return `CommandObject(new PlaceCommand(x, y, f))`; every other line returns `new ParseResult.Failure($"'{trimmed}' ignored - unknown command")` until T052 completes the rules. Never throws ([FR-09][fr-09], [FR-10a][fr-10a]; [DEC-07][dec-07], [DEC-08][dec-08], [DEC-28][dec-28]; [NFR-07i][nfr-07i]; research R9 to R11).
- [x] T034 [US1] Run `dotnet build ToyRobot.sln` and `dotnet test ToyRobot.sln`: all tests from T011 to T029 pass ([NFR-06][nfr-06]; [DEC-31][dec-31]).

**Checkpoint**: User Story 1 works on its own; all tests pass. Commit with the task IDs.

---

## Phase 4: User Story 2 - Move and turn without falling off (Priority: P1)

**Goal**: MOVE, LEFT and RIGHT, a refused MOVE at every edge and corner, and the parser for MOVE, LEFT and RIGHT ([FR-03][fr-03], [FR-04][fr-04], [FR-05][fr-05]; [DEC-02][dec-02], [DEC-13][dec-13], [DEC-16][dec-16], [DEC-17][dec-17], [DEC-22][dec-22]).

**Independent Test**: unit tests of `MoveCommand`, `LeftCommand`, `RightCommand` and RobotOperator with a real Table from a placed robot, and spec US2 scenarios 1 to 11 plus US1 scenarios 8 and 9 through the driver.

### Tests for User Story 2 (write first)

- [x] T035 [P] [US2] Write `MoveCommandTests` in `tests/ToyRobot.Tests/Unit/MoveCommandTests.cs` with `FakeRobotOperator` ([FR-03][fr-03], [FR-04][fr-04]; [DEC-02][dec-02], [DEC-13][dec-13], [DEC-16][dec-16], [DEC-22][dec-22]; [NFR-07f][nfr-07f]; research R12): `Name` is `MOVE`; `NeedsRobotOnTable` is `true`; from `(2, 2, North)` the one request is `(2, 3, North)`, from `(2, 2, South)` `(2, 1, South)`, from `(2, 2, East)` `(3, 2, East)`, from `(2, 2, West)` `(1, 2, West)`; with the answer yes it returns `new Outcome.Done()`; with the answer no, from `(4, 2, East)` it returns `new Outcome.Warning("MOVE ignored - next position is off the table so robot stays at 4,2,EAST")` and from `(0, 0, South)` `new Outcome.Warning("MOVE ignored - next position is off the table so robot stays at 0,0,SOUTH")`; with `CurrentState` `null`, `Execute` throws `InvalidOperationException`.
- [x] T036 [P] [US2] Write `LeftCommandTests` in `tests/ToyRobot.Tests/Unit/LeftCommandTests.cs` with `FakeRobotOperator` ([FR-05][fr-05]; [DEC-13][dec-13], [DEC-17][dec-17]; [NFR-07f][nfr-07f]; research R12): `Name` is `LEFT`; `NeedsRobotOnTable` is `true`; for each facing at `(2, 3, f)` it makes exactly one request, `(2, 3, f.TurnLeft())` (NORTH → WEST, WEST → SOUTH, SOUTH → EAST, EAST → NORTH), and returns `new Outcome.Done()`; it returns `new Outcome.Done()` also when the answer is no (page 6: no warning); with `CurrentState` `null`, `Execute` throws `InvalidOperationException`.
- [x] T037 [P] [US2] Write `RightCommandTests` in `tests/ToyRobot.Tests/Unit/RightCommandTests.cs`, as T036 with `RIGHT` and `TurnRight()` (NORTH → EAST, EAST → SOUTH, SOUTH → WEST, WEST → NORTH) ([FR-05][fr-05]; [DEC-13][dec-13], [DEC-17][dec-17]; [NFR-07f][nfr-07f]).
- [x] T038 [US2] Add the US2 part of `RobotOperatorTests` in `tests/ToyRobot.Tests/Unit/RobotOperatorTests.cs` with a real `Table(5, 5)` ([FR-03][fr-03], [FR-04][fr-04], [FR-05][fr-05]; [DEC-02][dec-02], [DEC-13][dec-13], [DEC-16][dec-16], [DEC-17][dec-17], [DEC-22][dec-22]; [NFR-07h][nfr-07h]): from `(2, 2, f)`, `Run(new MoveCommand())` returns `Done` and gives `(2, 3, North)`, `(2, 1, South)`, `(3, 2, East)`, `(1, 2, West)`; `Move_that_would_fall_off_the_table_is_ignored` as a theory over the 12 starts `2,4,NORTH`, `2,0,SOUTH`, `4,2,EAST`, `0,2,WEST`, `0,0,SOUTH`, `0,0,WEST`, `0,4,NORTH`, `0,4,WEST`, `4,4,NORTH`, `4,4,EAST`, `4,0,SOUTH`, `4,0,EAST`: the outcome is `new Outcome.Warning("MOVE ignored - next position is off the table so robot stays at <start>")` and `CurrentState` is still the start; `Further_valid_movement_is_still_allowed`: from `(4, 2, East)` MOVE (warning), LEFT, MOVE gives `(4, 3, North)`; LEFT and RIGHT keep X and Y and return `Done` at every corner; on a `Table(2, 2)` with the robot placed at `(1, 1, North)`, `Run(new MoveCommand())` returns `new Outcome.Warning("MOVE ignored - next position is off the table so robot stays at 1,1,NORTH")` and `Run(new ReportCommand())` returns `new Outcome.PositionText("1,1,NORTH")` (spec US1 scenario 9; [DEC-03][dec-03]).
- [x] T039 [US2] Add the US2 part of `CommandParserTests` in `tests/ToyRobot.Tests/Unit/CommandParserTests.cs` ([FR-09][fr-09], [FR-10a][fr-10a]; [DEC-06][dec-06], [DEC-07][dec-07], [DEC-08][dec-08]): `Parse` returns `CommandObject(new MoveCommand())` for `MOVE`, `move`, `mOvE` and `   MOVE   ` (three spaces each side); `CommandObject(new LeftCommand())` for `LEFT` and `left`; `CommandObject(new RightCommand())` for `RIGHT` and `right`.
- [x] T040 [US2] Add the US2 scenarios and US1 scenarios 8 and 9 to `tests/ToyRobot.Tests/Unit/RobotRulesScenarioTests.cs`, with the driver, checking `Positions` and `Warnings` exactly (spec.md US1, US2; [FR-01][fr-01], [FR-03][fr-03] to [FR-06][fr-06]; [DEC-02][dec-02], [DEC-03][dec-03], [DEC-16][dec-16], [DEC-17][dec-17], [DEC-21][dec-21], [DEC-22][dec-22], [DEC-31][dec-31]): US2-1 Challenge Example 1 (`PLACE 0,0,NORTH`, `MOVE`, `REPORT` → `0,1,NORTH`); US2-2 (`PLACE 0,0,SOUTH`, `MOVE`, `REPORT` → `0,0,SOUTH`, warning `MOVE ignored - next position is off the table so robot stays at 0,0,SOUTH`); US2-3 Challenge Example 2 (→ `0,0,WEST`); US2-4 Challenge Example 3 (→ `3,3,NORTH`); US2-5 (→ `2,1,NORTH`); US2-6 (→ `4,1,EAST`, warning `MOVE ignored - next position is off the table so robot stays at 4,1,EAST`); US2-7 (→ `0,3,WEST`); US2-8 as a theory over the 12 rows (positions `<start>`, warnings the blocked MOVE text with `<start>`); US2-9 (→ `4,3,NORTH`, warning `MOVE ignored - next position is off the table so robot stays at 4,2,EAST`); US2-10 full LEFT cycle (positions `2,2,WEST`, `2,2,SOUTH`, `2,2,EAST`, `2,2,NORTH`, no warnings); US2-11 full RIGHT cycle (`2,2,EAST`, `2,2,SOUTH`, `2,2,WEST`, `2,2,NORTH`); US1-8 several REPORTs (`0,0,NORTH`, `0,1,NORTH`); US1-9 with the driver on `Table(2, 2)`: `PLACE 1,1,NORTH`, `MOVE`, `REPORT`, `PLACE 2,2,NORTH` → positions `1,1,NORTH`, warnings `MOVE ignored - next position is off the table so robot stays at 1,1,NORTH` and `PLACE 2,2,NORTH ignored - position is off the table`.

### Implementation for User Story 2

- [x] T041 [P] [US2] Implement `MoveCommand` in `src/ToyRobot/Commands/MoveCommand.cs`: `public sealed record MoveCommand : IRobotCommand`; `Name` `MOVE`; `NeedsRobotOnTable` `true`; `Execute`: `current` = `CurrentState` (null → `InvalidOperationException`); `nextState` = `current` moved one step (NORTH Y + 1, SOUTH Y − 1, EAST X + 1, WEST X − 1) with the same facing; `TryMoveTo(nextState)` → `Done`, otherwise `new Outcome.Warning($"MOVE ignored - next position is off the table so robot stays at {current}")` ([FR-03][fr-03], [FR-04][fr-04]; [DEC-02][dec-02], [DEC-13][dec-13], [DEC-16][dec-16], [DEC-22][dec-22]; [NFR-07f][nfr-07f]).
- [x] T042 [P] [US2] Implement `LeftCommand` in `src/ToyRobot/Commands/LeftCommand.cs`: `public sealed record LeftCommand : IRobotCommand`; `Name` `LEFT`; `NeedsRobotOnTable` `true`; `Execute`: `current` (null → `InvalidOperationException`); `robot.TryMoveTo(current with { Facing = current.Facing.TurnLeft() })`; return `new Outcome.Done()` ([FR-05][fr-05]; [DEC-13][dec-13], [DEC-17][dec-17]; [NFR-07f][nfr-07f]).
- [x] T043 [P] [US2] Implement `RightCommand` in `src/ToyRobot/Commands/RightCommand.cs`, as T042 with `RIGHT` and `TurnRight()` ([FR-05][fr-05]; [DEC-13][dec-13], [DEC-17][dec-17]; [NFR-07f][nfr-07f]).
- [x] T044 [US2] Extend `CommandParser.Parse` in `src/ToyRobot/Parsing/CommandParser.cs`: the command words `MOVE`, `LEFT` and `RIGHT` with nothing after them return `CommandObject(new MoveCommand())`, `CommandObject(new LeftCommand())`, `CommandObject(new RightCommand())` ([FR-09][fr-09], [FR-10a][fr-10a]; [DEC-06][dec-06], [DEC-07][dec-07]).
- [x] T045 [US2] Run `dotnet build ToyRobot.sln` and `dotnet test ToyRobot.sln`: all tests from T011 to T040 pass ([NFR-06][nfr-06]; [DEC-31][dec-31]).

**Checkpoint**: User Stories 1 and 2 work; all tests pass. Commit with the task IDs.

---

## Phase 5: User Story 3 - Robot commands before the robot is on the table (Priority: P2)

**Goal**: RobotOperator ignores MOVE, LEFT, RIGHT and REPORT, with a warning naming the command, until a valid PLACE; the command is not run and the Table is not asked ([FR-07][fr-07]; [DEC-19][dec-19], [DEC-27][dec-27]; page 7.3).

**Independent Test**: unit tests of RobotOperator with a new robot and each command object, and spec US3 scenarios 1 to 3 and US1 scenario 4 through the driver.

### Tests for User Story 3 (write first)

- [x] T046 [US3] Add the US3 part of `RobotOperatorTests` in `tests/ToyRobot.Tests/Unit/RobotOperatorTests.cs` with a real `Table(5, 5)` ([FR-07][fr-07]; [DEC-19][dec-19], [DEC-27][dec-27]; [NFR-07h][nfr-07h]): `Robot_not_on_table_ignores_MOVE`, `Robot_not_on_table_ignores_LEFT`, `Robot_not_on_table_ignores_RIGHT`, `Robot_not_on_table_ignores_REPORT`: on a new RobotOperator, `Run` returns exactly `new Outcome.Warning("MOVE ignored - robot is not on the table")`, `new Outcome.Warning("LEFT ignored - robot is not on the table")`, `new Outcome.Warning("RIGHT ignored - robot is not on the table")`, `new Outcome.Warning("REPORT ignored - robot is not on the table")`, and `CurrentState` stays `null`; with `new SpyCommand("MOVE", true, new Outcome.Done())` on a new RobotOperator the outcome is `new Outcome.Warning("MOVE ignored - robot is not on the table")` and `ExecuteCount` is 0 (the command is not run, so it cannot ask the Table, page 7.3); with `new SpyCommand("PLACE", false, new Outcome.Done())` on a new RobotOperator `ExecuteCount` is 1 and its outcome is returned (PLACE is never refused by this check); with `SpyCommand("MOVE", true, ...)` after a valid PLACE, `ExecuteCount` is 1; after the four ignored commands, `PlaceCommand(0, 0, North)` returns `Done` and `ReportCommand` returns `new Outcome.PositionText("0,0,NORTH")`.
- [x] T047 [US3] Add the US3 scenarios and US1 scenario 4 to `tests/ToyRobot.Tests/Unit/RobotRulesScenarioTests.cs` with the driver (spec.md US3, US1; [FR-02][fr-02], [FR-07][fr-07], [FR-09][fr-09]; [DEC-07][dec-07], [DEC-15][dec-15], [DEC-19][dec-19], [DEC-22][dec-22], [DEC-31][dec-31]): US3-1 `MOVE`, `PLACE 0,0,NORTH`, `REPORT` → positions `0,0,NORTH`, warnings `MOVE ignored - robot is not on the table`; US3-2 `MOVE`, `LEFT`, `RIGHT`, `REPORT` → no positions, warnings `MOVE ignored - robot is not on the table`, `LEFT ignored - robot is not on the table`, `RIGHT ignored - robot is not on the table`, `REPORT ignored - robot is not on the table`; US3-3 `move` → warnings `MOVE ignored - robot is not on the table`; US1-4 (an invalid first PLACE) `PLACE 9,9,NORTH`, `REPORT` → no positions, warnings `PLACE 9,9,NORTH ignored - position is off the table`, `REPORT ignored - robot is not on the table`.

### Implementation for User Story 3

- [x] T048 [US3] Add the shared check to `RobotOperator.Run` in `src/ToyRobot/Domain/RobotOperator.cs`: if `command.NeedsRobotOnTable` and the robot's `Placement` is `null`, return `new Outcome.Warning($"{command.Name} ignored - robot is not on the table")` without calling `Execute`; otherwise return `command.Execute(this)` ([FR-07][fr-07]; [DEC-19][dec-19], [DEC-27][dec-27]).
- [x] T049 [US3] Run `dotnet build ToyRobot.sln` and `dotnet test ToyRobot.sln`: all tests from T011 to T047 pass ([NFR-06][nfr-06]; [DEC-31][dec-31]).

**Checkpoint**: User Stories 1 to 3 work; all tests pass. Commit with the task IDs.

---

## Phase 6: User Story 4 - Lines are read the same way however they are typed, and bad lines are refused (Priority: P2)

**Goal**: the complete CommandParser: blank lines, unknown words, EXIT, values after MOVE, LEFT, RIGHT or REPORT, PLACE with the wrong number of values, bad X, Y or F, in [DEC-22][dec-22]'s order, with culture-independent rules ([FR-09][fr-09], [FR-10][fr-10], [FR-10a][fr-10a]; [DEC-06][dec-06] to [DEC-10][dec-10], [DEC-20][dec-20], [DEC-22][dec-22], [DEC-28][dec-28]).

**Independent Test**: unit tests of CommandParser for every row of contracts/command-line-grammar.md (every row of the [DEC-08][dec-08] table, every bad-line reason in [DEC-22][dec-22], EXIT) and spec US4 scenarios 1 to 7 through the driver.

### Tests for User Story 4 (write first)

- [x] T050 [US4] Add the US4 part of `CommandParserTests` in `tests/ToyRobot.Tests/Unit/CommandParserTests.cs`, one theory row or test per input, each asserting the exact `ParseResult` ([FR-09][fr-09], [FR-10][fr-10], [FR-10a][fr-10a]; [DEC-06][dec-06] to [DEC-10][dec-10], [DEC-20][dec-20], [DEC-22][dec-22], [DEC-31][dec-31]; [SC-002][sc-002], [SC-003][sc-003]):
  - unknown command: `HELLO`, `> MOVE`, `EXITNOW`, `EXITING`, `PLACE0,0,NORTH`, `PLACEHOLDER`, `PLACE,1,2` → `new ParseResult.Failure("'<line>' ignored - unknown command")` with `<line>` the input, for example `'> MOVE' ignored - unknown command`;
  - blank: the empty string, `   `, `[TAB][TAB]`, ` [TAB] ` and `[NBSP]` → `new ParseResult.Failure("blank line ignored")`;
  - number of values: `PLACE 1,2`, `PLACE 0 0 NORTH`, `PLACE A,B`, `PLACE 1,2,3,4`, `PLACE` → `Failure("'<line>' ignored - expected PLACE X,Y,F")`;
  - X: `PLACE A,0,NORTH`, `PLACE 1 0,0,NORTH`, `PLACE 1.5,0,NORTH`, `PLACE ,,`, `PLACE A,B,UP` → `Failure("'<line>' ignored - X must be a whole number")`; `PLACE 99999999999,0,NORTH`, `PLACE 2147483648,0,NORTH`, `PLACE -99999999999,0,NORTH` → `Failure("'<line>' ignored - X is too large")`;
  - Y: `PLACE 0,B,NORTH`, `PLACE 0,B,UP` → `Failure("'<line>' ignored - Y must be a whole number")`; `PLACE 0,99999999999,NORTH` → `Failure("'<line>' ignored - Y is too large")`;
  - first problem only: `PLACE 9999999999999999999,111111111111111111111111111,east` → `Failure("'<line>' ignored - X is too large")`, `PLACE 1,111111111111111111111111111,east` → `Failure("'<line>' ignored - Y is too large")`;
  - F: `PLACE 1,2,UP`, `PLACE 0,0,N`, `PLACE 1,2,N`, `PLACE 1,2,`, `PLACE 1,2,NORTH EXTRA`, `PLACE 1,2,1` → `Failure("'<line>' ignored - F must be NORTH, SOUTH, EAST or WEST")`;
  - takes no values: `move now` → `Failure("'move now' ignored - MOVE takes no values")`, `MOVE now` → `Failure("'MOVE now' ignored - MOVE takes no values")`, `MOVE[NBSP]now` → `Failure("'MOVE[NBSP]now' ignored - MOVE takes no values")` (quote keeps the non-breaking space), `LEFT now` → `Failure("'LEFT now' ignored - LEFT takes no values")`, `RIGHT now` → `Failure("'RIGHT now' ignored - RIGHT takes no values")`, `REPORT now` → `Failure("'REPORT now' ignored - REPORT takes no values")`;
  - the quote is the line with whitespace removed from both ends only: `  HELLO  ` → `Failure("'HELLO' ignored - unknown command")`, `[TAB]PLACE 1,2[NBSP]` → `Failure("'PLACE 1,2' ignored - expected PLACE X,Y,F")`;
  - valid rows of the [DEC-08][dec-08] table and whitespace: `PLACE     0,0,NORTH` → `PlaceCommand(0, 0, North)`; `place 0 , 0 , north` → `(0, 0, North)`; `PLACE 01,+1,EAST` → `(1, 1, East)`; `[TAB]place[NBSP]0,[NBSP]0[TAB],north[NBSP]` → `(0, 0, North)`; `PLACE[TAB]1,2,EAST` → `(1, 2, East)`; `Place 1,1,East` → `(1, 1, East)`; `rEpOrT` → `ReportCommand`;
  - EXIT: `EXIT`, `exit`, `Exit`, `EXIT now`, `exit now`, `[TAB]EXIT[TAB]` → `new ParseResult.Exit()`;
  - culture: inside `new CultureScope("tr-TR")`, `exit` → `Exit()`, `right` → `CommandObject(new RightCommand())`, `left` → `CommandObject(new LeftCommand())`, `place 1,2,east` → `CommandObject(new PlaceCommand(1, 2, East))`, `PLACE -1,0,NORTH` → `(-1, 0, North)`; inside `new CultureScope("sv-SE")`, `PLACE -1,0,NORTH` → `(-1, 0, North)`;
  - never throws ([UT-04][ut-04], [SC-005][sc-005]; [DEC-20][dec-20], [DEC-32][dec-32]): for every input above and for `PLACE 1,2,NORTH,`, `PLACE --1,0,NORTH`, `PLACE 0x1,0,NORTH`, `PLACE -2147483649,0,NORTH`, a line of 10 000 `A` characters and the line `\0`, `Record.Exception(() => parser.Parse(line))` is `null` and the result is one of the three `ParseResult` cases.
- [x] T051 [US4] Add the US4 scenarios to `tests/ToyRobot.Tests/Unit/RobotRulesScenarioTests.cs` with the driver on `Table(5, 5)` (spec.md US4; [FR-09][fr-09], [FR-10][fr-10]; [DEC-07][dec-07], [DEC-08][dec-08], [DEC-20][dec-20], [DEC-22][dec-22], [DEC-27][dec-27], [DEC-31][dec-31]): US4-1 `place 0 , 0 , north`, `REPORT` → `0,0,NORTH`; US4-2 `PLACE 1,0,EAST`, `PLACE 1,2`, `PLACE 3,2,NORTH`, `REPORT` → positions `3,2,NORTH`, warnings `'PLACE 1,2' ignored - expected PLACE X,Y,F` (processing continues after a bad line); US4-3 a theory over its 28 rows, one line each → no positions, warnings exactly the reason in that row without `Warning: `; US4-4 the four valid rows (`PLACE 0,0,NORTH`, `   MOVE   `, `REPORT` → `0,1,NORTH`; `PLACE     0,0,NORTH`, `REPORT` → `0,0,NORTH`; `place 0 , 0 , north`, `REPORT` → `0,0,NORTH`; `PLACE 01,+1,EAST`, `REPORT` → `1,1,EAST`); US4-5 `[TAB]place[NBSP]0,[NBSP]0[TAB],north[NBSP]`, `REPORT` → `0,0,NORTH`; US4-6 `Place 1,1,East`, `mOvE`, `rEpOrT` → `2,1,EAST`; US4-7 a theory over its seven rows (`PLACE A,B`, `PLACE 1,2,3,4`, `PLACE` → expected PLACE X,Y,F; `PLACE A,B,UP` → X must be a whole number; `PLACE 0,B,UP` → Y must be a whole number; `PLACE 9999999999999999999,111111111111111111111111111,east` → X is too large; `PLACE 1,111111111111111111111111111,east` → Y is too large) with the exact reasons; edge case `REPORT now` before any PLACE → warnings only `'REPORT now' ignored - REPORT takes no values` (not the "not on the table" warning); edge case `PLACE 2147483647,0,NORTH` → warnings `PLACE 2147483647,0,NORTH ignored - position is off the table`; edge case `EXIT`, `PLACE 0,0,NORTH`, `REPORT` → the driver stops at EXIT: no positions, no warnings, final state `null` (the session behaviour itself is spec 002's [FR-15][fr-15]).

### Implementation for User Story 4

- [x] T052 [US4] Complete `CommandParser.Parse` in `src/ToyRobot/Parsing/CommandParser.cs` per contracts/command-line-grammar.md, in this order: empty trimmed line → `Failure("blank line ignored")`; command word `EXIT` → `new ParseResult.Exit()` whatever follows; `MOVE`, `LEFT`, `RIGHT`, `REPORT` with anything after the word → `Failure($"'{trimmed}' ignored - {NAME} takes no values")` with the name in upper case; `PLACE`: not exactly three comma-separated parts → `Failure($"'{trimmed}' ignored - expected PLACE X,Y,F")`, then X → `... - X is too large` (written as a whole number but too large to read) or `... - X must be a whole number`, then Y → `... - Y is too large` or `... - Y must be a whole number`, then F → `... - F must be NORTH, SOUTH, EAST or WEST`; any other word → `Failure($"'{trimmed}' ignored - unknown command")`. Keep `OrdinalIgnoreCase`, `char.IsWhiteSpace`, `Trim()` and `int.TryParse` with `NumberStyles.AllowLeadingSign` and `CultureInfo.InvariantCulture`; no exceptions, no table check, no state ([FR-09][fr-09], [FR-10][fr-10], [FR-10a][fr-10a]; [DEC-06][dec-06] to [DEC-10][dec-10], [DEC-20][dec-20], [DEC-22][dec-22], [DEC-28][dec-28]; [NFR-07i][nfr-07i]; research R9 to R11).
- [x] T053 [US4] Run `dotnet build ToyRobot.sln` and `dotnet test ToyRobot.sln`: all tests from T011 to T051 pass ([NFR-06][nfr-06]; [DEC-31][dec-31]).

**Checkpoint**: all user stories work; all tests pass. Commit with the task IDs.

---

## Phase 7: Polish and cross-cutting checks

**Purpose**: [NFR-07][nfr-07] (names, members and limits exactly as page 6), test names, and a full run.

- [x] T054 [P] Write `DesignRulesTests` in `tests/ToyRobot.Tests/Unit/DesignRulesTests.cs` using reflection on the `ToyRobot` assembly ([NFR-07][nfr-07], [NFR-07a][nfr-07a] to [NFR-07j][nfr-07j]; [DEC-14][dec-14], [DEC-26][dec-26], [DEC-27][dec-27], [DEC-34][dec-34], [DEC-38][dec-38]; [SC-006][sc-006]): the public top-level types are exactly `Table`, `Direction`, `RobotState`, `Robot`, `IRobotCommand`, `PlaceCommand`, `MoveCommand`, `LeftCommand`, `RightCommand`, `ReportCommand`, `IRobotOperator`, `RobotOperator`, `CommandParser`, `Outcome`, `ParseResult` (spec 002 adds `Runner` to this list when it adds the Runner); `IRobotCommand` declares exactly `Name`, `NeedsRobotOnTable` and `Execute`; `IRobotOperator` declares exactly `CurrentState` and `TryMoveTo`; the five command classes are sealed and implement `IRobotCommand`; `Outcome` and `ParseResult` each have exactly three nested public cases with the names in data-model.md; no public or internal member of `RobotOperator`, `Table` or any command class has the type `Robot`, other than RobotOperator's private field; no type in the assembly has a static field of type `Robot`; `CommandParser` has no instance fields; `Robot.Placement` has no public setter. These tests check code that already exists; they must pass at once and guard against later drift.
- [x] T055 Review `src/ToyRobot/` against page 6 and [NFR-07a][nfr-07a] to [NFR-07j][nfr-07j]: each class does only its job and none of its "Does not" items; no command class checks the table, the robot's presence, parses text or writes output; RobotOperator does not parse, write, add `Output:` or `Warning: `, or handle EXIT; adding a robot command would need only a new class and a CommandParser change ([NFR-07j][nfr-07j]; [DEC-27][dec-27], [DEC-34][dec-34], [DEC-38][dec-38]). Fix any finding through the task it belongs to and re-run the tests.
- [x] T056 Check exact strings: search `src/ToyRobot/` and `tests/ToyRobot.Tests/Unit/` for every warning text and reason and compare each with [DEC-22][dec-22] character for character (`ignored - position is off the table`, `ignored - next position is off the table so robot stays at `, `ignored - robot is not on the table`, `ignored - unknown command`, `blank line ignored`, `ignored - expected PLACE X,Y,F`, `ignored - X must be a whole number`, `ignored - X is too large`, `ignored - Y must be a whole number`, `ignored - Y is too large`, `ignored - F must be NORTH, SOUTH, EAST or WEST`, `takes no values`) ([FR-02][fr-02], [FR-04][fr-04], [FR-07][fr-07], [FR-10][fr-10]; [DEC-22][dec-22]; [SC-002][sc-002]).
- [x] T057 Check the test list against spec.md: every acceptance scenario (US1 1 to 9, US2 1 to 11, US3 1 to 3, US4 1 to 8) and every Edge Cases item has a test ([SC-001][sc-001], [UT-01][ut-01], [UT-02][ut-02]); test names use the challenge wording where possible, for example `Robot_not_on_table_ignores_MOVE` ([NFR-06][nfr-06]; [UT-03][ut-03]; [DEC-31][dec-31]).
- [x] T058 Run quickstart.md from the repository root: `dotnet build ToyRobot.sln` and `dotnet test ToyRobot.sln` both exit with code 0 and every test passes; `git status` shows no build output ([NFR-03][nfr-03], [NFR-06][nfr-06]; [SC-007][sc-007], [SK-01][sk-01]; [DEC-31][dec-31], [DEC-37][dec-37]).
- [x] T059 [US4] Change after the run: a number written as a whole number but too large to read gets its own reason ([FR-10][fr-10], [FR-09][fr-09]; [DEC-20][dec-20], [DEC-22][dec-22]). Tests first, and they fail before the code: in `tests/ToyRobot.Tests/Unit/CommandParserTests.cs`, change the `FailureRows` so `PLACE 99999999999,0,NORTH`, `PLACE 2147483648,0,NORTH`, `PLACE -2147483649,0,NORTH`, `PLACE -99999999999,0,NORTH`, `PLACE +99999999999,0,NORTH` and `PLACE 9999999999999999999,0,east` expect `Failure("'<line>' ignored - X is too large")`, `PLACE 0,99999999999,NORTH`, `PLACE 0,-99999999999,NORTH` and `PLACE 1,111111111111111111111111111,east` expect `... - Y is too large`, `PLACE 9999999999999999999,111111111111111111111111111,east` expects only `... - X is too large`, and `PLACE 99999999999.5,0,NORTH` and `PLACE -,0,NORTH` keep `... - X must be a whole number`; change and add the matching rows in `RobotRulesScenarioTests.cs` (US4-3, US4-7), `RobotRulesAsConsoleTextTests.cs` (US4-3, US4-7) and `NeverCrashTests.cs` (the exact console text for `PLACE 99999999999,0,NORTH`). The two new [DEC-22][dec-22] texts are checked in full: `'PLACE 9999999999,0,NORTH' ignored - X is too large` and `'PLACE 0,9999999999,NORTH' ignored - Y is too large` (the same rows are in `CommandParserTests.cs`). Then in `src/ToyRobot/Parsing/CommandParser.cs`, when X or Y cannot be read, report `... - X is too large` (or Y) if the part is an optional `-` or `+` followed by digits only, and `... - X must be a whole number` (or Y) otherwise; keep `int.TryParse` with the invariant culture, report only the first problem, and do not change the quoted line. Run `dotnet build` (0 warnings) and `dotnet test`: all tests pass.
- [x] T060 Change after the run (developer request 2026-10-05; [NFR-07][nfr-07]; [DEC-26][dec-26], [DEC-34][dec-34], [DEC-40][dec-40]): group the application files into folders with `git mv`, namespaces following the folders: `src/ToyRobot/Domain/` (Table, Direction, RobotState, Robot, IRobotOperator, RobotOperator; `ToyRobot.Domain`), `src/ToyRobot/Commands/` (IRobotCommand, the five command classes, Outcome; `ToyRobot.Commands`), `src/ToyRobot/Parsing/` (ParseResult, CommandParser; `ToyRobot.Parsing`) and, from spec 002, `src/ToyRobot/Session/` (Runner; `ToyRobot.Session`); Program stays at the project root in `ToyRobot`. Each file gets the `using` directives it needs; the unit-test project gets them as global usings in `ToyRobot.Tests.csproj`, next to `Xunit`. Test first: `DesignRulesTests.Each_public_type_is_in_the_namespace_of_its_folder`. No type, member or behaviour changes; all tests pass.
- [x] T061 Change after the run ([FR-01][fr-01]; [DEC-01][dec-01], [DEC-28][dec-28]): rename the Table's `IsOnTable(x, y)` to `Contains(x, y)`. At the call site `table.IsOnTable(x, y)` repeats "table"; `table.Contains(x, y)` reads as a sentence ("does the table contain x, y?") and follows .NET's own `Contains` (for example `Rectangle.Contains`). Tests first: `TableTests` calls `Contains` and does not build until the code changes; then `Table` and `RobotOperator.TryMoveTo` use the new name, and the spec check script lists `Contains` as a decided member of `Table`. No behaviour changes; all tests pass.
- [x] T062 Change after the run ([NFR-07d][nfr-07d], [NFR-07g][nfr-07g]; [DEC-13][dec-13], [DEC-14][dec-14]): rename the Robot's `Replace(state)` to `MoveTo(nextState)`, and the parameter `next` of `TryMoveTo` to `nextState` (in `IRobotOperator`, `RobotOperator` and `FakeRobotOperator`; the local `next` in `MoveCommand` becomes `nextState` too). Read together, the operator tries and the robot does: `TryMoveTo(nextState)` asks the Table, then `robot.MoveTo(nextState)`; turning is already a move to the same square facing a new way. `nextState` says what the value is, where `next` did not. The state is still replaced, never changed in place ([DEC-13][dec-13]). Solution design pages 5, 6 and 7.1 and the glossary use the new names. Tests first: `RobotTests` calls `MoveTo` and does not build until the code changes; then `Robot`, `IRobotOperator`, `RobotOperator` and `MoveCommand` use the new names, and the spec check script lists `MoveTo` as a decided member of `Robot`. No behaviour changes; all tests pass.

**Checkpoint**: spec 001 complete; spec 002 can start. Commit with the task IDs.

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (Phase 1)**: no dependencies.
- **Foundational (Phase 2)**: after Phase 1; blocks every user story.
- **US1 (Phase 3)**: after Phase 2.
- **US2 (Phase 4)**: after Phase 3 (uses RobotOperator, CommandParser and the driver from US1).
- **US3 (Phase 5)**: after Phase 4 (its tests run MOVE, LEFT and RIGHT command objects).
- **US4 (Phase 6)**: after Phase 3 for the parser; done after Phase 5 so that its scenario tests (for example `REPORT now` before PLACE and `move` before PLACE) run against the complete RobotOperator.
- **Polish (Phase 7)**: after all user stories.
- **Other specs**: spec 002-console-session starts after T058; spec 003-delivery after spec 002. Spec 002 adds `Runner` to the public-type list in `DesignRulesTests` (T054) in its task T020a, just before the task that creates the Runner. It keeps Program non-public (`internal static class Program`, spec 002 T022), so T054 stays green.

### Within each phase

- Test helpers and tests before the code they exercise. Tests of behaviour that the phase builds fail (or do not compile) until its implementation tasks are done. Tests of behaviour an earlier phase already built pass at once and guard it: in Phase 5, the T046 SpyCommand checks for PLACE before placement and for MOVE after a valid PLACE; in Phase 6, the T050 unknown-command rows, valid rows and never-throws checks, and the T051 scenarios US4-1, US4-4, US4-5 and US4-6.
- Within Phase 2: T016 before T017; T017 before T018 and T020; T019 and T020 before T021; T021 before T022.
- `RobotOperator.cs` (T032, T048) and `CommandParser.cs` (T033, T044, T052) are each changed by several tasks, in task order; those tasks are not parallel.
- `RobotOperatorTests.cs`, `CommandParserTests.cs` and `RobotRulesScenarioTests.cs` are extended by several tasks, in task order.

### Parallel opportunities

- Phase 1: T004 and T005.
- Phase 2: T008 to T014 together; then T015, T016 and T019 together.
- Phase 3: T024 to T027 together; T030 and T031 together.
- Phase 4: T035 to T037 together; T041 to T043 together.

## Parallel Example: User Story 2

```text
Task: "T035 Write MoveCommandTests in tests/ToyRobot.Tests/Unit/MoveCommandTests.cs"
Task: "T036 Write LeftCommandTests in tests/ToyRobot.Tests/Unit/LeftCommandTests.cs"
Task: "T037 Write RightCommandTests in tests/ToyRobot.Tests/Unit/RightCommandTests.cs"

Task: "T041 Implement MoveCommand in src/ToyRobot/Commands/MoveCommand.cs"
Task: "T042 Implement LeftCommand in src/ToyRobot/Commands/LeftCommand.cs"
Task: "T043 Implement RightCommand in src/ToyRobot/Commands/RightCommand.cs"
```

## Implementation Strategy

1. Phase 1 and Phase 2: skeleton and foundation, all tests passing.
2. Phase 3 (US1, MVP): place and report; stop and check that all tests pass.
3. Phases 4 to 6 in order, each ending with all tests passing and a commit naming its task IDs.
4. Phase 7: design checks, exact strings, coverage of every scenario and edge case, final run.

## Coverage map

| Requirement | Tests | Code |
|---|---|---|
| [FR-01][fr-01] ([DEC-01][dec-01] to [DEC-03][dec-03]) | T011, T026, T029, T038, T040 | T015 |
| [FR-02][fr-02] ([DEC-11][dec-11], [DEC-13][dec-13], [DEC-15][dec-15], [DEC-22][dec-22], [DEC-28][dec-28]) | T024, T026, T027, T029, T040, T047, T051 | T030, T032, T033 |
| [FR-03][fr-03] ([DEC-02][dec-02], [DEC-16][dec-16]) | T035, T038, T040 | T041 |
| [FR-04][fr-04] ([DEC-13][dec-13], [DEC-16][dec-16], [DEC-22][dec-22]) | T035, T038, T040 | T041 |
| [FR-05][fr-05] ([DEC-13][dec-13], [DEC-17][dec-17]) | T012, T036, T037, T038, T040 | T016, T042, T043 |
| [FR-06][fr-06] ([DEC-07][dec-07], [DEC-18][dec-18], [DEC-21][dec-21]) | T013, T025, T026, T029, T040 | T017, T031 |
| [FR-07][fr-07] ([DEC-19][dec-19], [DEC-27][dec-27]) | T046, T047, T051 | T048 |
| [FR-09][fr-09] ([DEC-07][dec-07], [DEC-08][dec-08]) | T027, T039, T050, T051 | T033, T044, T052 |
| [FR-10][fr-10], [FR-10a][fr-10a] ([DEC-06][dec-06], [DEC-08][dec-08] to [DEC-10][dec-10], [DEC-20][dec-20], [DEC-22][dec-22], [DEC-28][dec-28]) | T027, T039, T050, T051, T059 | T022, T033, T044, T052, T059 |
| [NFR-07][nfr-07] ([DEC-12][dec-12] to [DEC-14][dec-14], [DEC-26][dec-26] to [DEC-28][dec-28], [DEC-34][dec-34], [DEC-38][dec-38]) | T013, T014, T046, T054, T055 | T015 to T022, T030 to T033, T041 to T044, T048, T052 |
| [SK-01][sk-01] to [SK-03][sk-03] ([DEC-31][dec-31], [DEC-37][dec-37], [DEC-39][dec-39]) | T007, T058 | T001 to T006 |
| [UT-01][ut-01] to [UT-04][ut-04] ([DEC-31][dec-31], [DEC-32][dec-32]) | T011 to T014, T024 to T029, T035 to T040, T046, T047, T050, T051, T054, T057 | T008 to T010, T028 |

### DEC-31 cases owned by this spec's unit tests

| [DEC-31][dec-31] case | Task |
|---|---|
| The three challenge examples | T040 (US2-1, US2-3, US2-4) |
| Hand-traced Brainstorm examples (2,1,NORTH; blocked moves pages 3 and 5; 0,3,WEST) | T040 (US2-5, US2-6, US2-2, US2-7) |
| Every edge and corner | T038, T040 (US2-8) |
| Full LEFT and RIGHT cycles | T012, T040 (US2-10, US2-11) |
| A small table such as 2 x 2 | T011, T026, T038, T040 (US1-9) |
| MOVE, LEFT, RIGHT and REPORT each before PLACE | T046, T047 (US3-2) |
| An invalid first PLACE; an invalid later PLACE; a second valid PLACE | T026, T047 (US1-4); T026, T029 (US1-2); T026, T029 (US1-5) |
| A negative position | T024, T026, T029 (US1-6) |
| Several REPORTs in one run | T040 (US1-8) |
| Case-insensitivity, `north`, REPORT in upper case after lower-case input | T027, T039, T050, T051 (US4-6), T029 (US1-3) |
| Every row of the [DEC-08][dec-08] table; tabs and non-breaking spaces | T050, T051 (US4-4, US4-5) |
| Every warning in [DEC-22][dec-22] with its exact text, including `> MOVE`, `MOVE now`, `PLACEHOLDER`, `PLACE 99999999999,0,NORTH`, `PLACE 0,0,N`, blank lines, off-table with parsed values | T024, T026, T035, T046, T050, T051 (US4-3), T029 (US1-7) |
| `EXIT now` exits, `EXITNOW` warns (parser level) | T050 |

The remaining [DEC-31][dec-31] cases (EXIT mid-file and before PLACE as a session, prompt and echo, empty input, byte-order mark, how-to-run message and exit codes) belong to spec 002's Runner whole-flow and console end-to-end tests.

## Notes

- [P] tasks touch different files and have no dependency on an unfinished task.
- Commit after each phase checkpoint (or smaller), naming the task IDs.
- Never weaken a test to make it pass; if a task is wrong or conflicts with a source, the sources win: fix the spec, plan or tasks first (brief rule 10 and step 9.3).

[dec-01]: ../../docs/design-analysis/design-decisions.md#dec-01-table-size-and-valid-positions
[dec-02]: ../../docs/design-analysis/design-decisions.md#dec-02-origin-and-directions
[dec-03]: ../../docs/design-analysis/design-decisions.md#dec-03-table-size-is-a-constructor-parameter
[dec-06]: ../../docs/design-analysis/design-decisions.md#dec-06-commands
[dec-07]: ../../docs/design-analysis/design-decisions.md#dec-07-case-insensitive
[dec-08]: ../../docs/design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-10]: ../../docs/design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-11]: ../../docs/design-analysis/design-decisions.md#dec-11-the-robot-exists-from-the-start-not-on-the-table
[dec-12]: ../../docs/design-analysis/design-decisions.md#dec-12-robot-state-is-an-immutable-value
[dec-13]: ../../docs/design-analysis/design-decisions.md#dec-13-state-is-replaced-never-changed-in-place
[dec-14]: ../../docs/design-analysis/design-decisions.md#dec-14-only-one-way-to-change-the-robot
[dec-15]: ../../docs/design-analysis/design-decisions.md#dec-15-place
[dec-16]: ../../docs/design-analysis/design-decisions.md#dec-16-move
[dec-17]: ../../docs/design-analysis/design-decisions.md#dec-17-left-and-right
[dec-18]: ../../docs/design-analysis/design-decisions.md#dec-18-report
[dec-19]: ../../docs/design-analysis/design-decisions.md#dec-19-robot-commands-before-the-robot-is-on-the-table
[dec-20]: ../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../../docs/design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-26]: ../../docs/design-analysis/design-decisions.md#dec-26-responsibilities
[dec-27]: ../../docs/design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-28]: ../../docs/design-analysis/design-decisions.md#dec-28-the-parser-checks-format-the-table-checks-position
[dec-30]: ../../docs/design-analysis/design-decisions.md#dec-30-exit-is-handled-by-the-runner
[dec-31]: ../../docs/design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../../docs/design-analysis/design-decisions.md#dec-32-never-crash
[dec-34]: ../../docs/design-analysis/design-decisions.md#dec-34-readable-code-solid
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[dec-38]: ../../docs/design-analysis/design-decisions.md#dec-38-one-robot-now-room-for-more-later
[dec-39]: ../../docs/design-analysis/design-decisions.md#dec-39-not-included
[dec-40]: ../../docs/design-analysis/design-decisions.md#dec-40-source-folders
[fr-01]: spec.md#fr-01
[fr-02]: spec.md#fr-02
[fr-03]: spec.md#fr-03
[fr-04]: spec.md#fr-04
[fr-05]: spec.md#fr-05
[fr-06]: spec.md#fr-06
[fr-07]: spec.md#fr-07
[fr-09]: spec.md#fr-09
[fr-10]: spec.md#fr-10
[fr-10a]: spec.md#fr-10a
[fr-15]: ../../docs/solution-design/02-functional-requirements.md#fr-15
[nfr-03]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-03
[nfr-04]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-04
[nfr-06]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-06
[nfr-07]: spec.md#nfr-07
[nfr-07a]: spec.md#nfr-07a
[nfr-07b]: spec.md#nfr-07b
[nfr-07c]: spec.md#nfr-07c
[nfr-07d]: spec.md#nfr-07d
[nfr-07e]: spec.md#nfr-07e
[nfr-07f]: spec.md#nfr-07f
[nfr-07g]: spec.md#nfr-07g
[nfr-07h]: spec.md#nfr-07h
[nfr-07i]: spec.md#nfr-07i
[nfr-07j]: spec.md#nfr-07j
[sc-001]: spec.md#sc-001
[sc-002]: spec.md#sc-002
[sc-003]: spec.md#sc-003
[sc-005]: spec.md#sc-005
[sc-006]: spec.md#sc-006
[sc-007]: spec.md#sc-007
[sk-01]: spec.md#sk-01
[sk-02]: spec.md#sk-02
[sk-03]: spec.md#sk-03
[ut-01]: spec.md#ut-01
[ut-02]: spec.md#ut-02
[ut-03]: spec.md#ut-03
[ut-04]: spec.md#ut-04
