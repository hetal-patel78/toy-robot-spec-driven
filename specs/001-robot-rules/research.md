# Research: Robot rules (001-robot-rules)

Phase 0 of `/speckit-plan`. The technical choices are fixed by the sources (Spec Kit brief rule 7; solution design pages 5 and 9; [DEC-37][dec-37]), so nothing here compares technologies. Each item below settles a detail that the plan, the data model or the tasks rely on. Items marked **Decided** are decisions taken under rule 10 of the brief, where the sources are silent; they are also listed in the spec's Assumptions.

## R1. Solution and project files

- **Decision**: `ToyRobot.sln` at the repository root, created with `dotnet new sln -n ToyRobot --format sln`, holding three projects whose project files are written by hand (not from the `dotnet new console` or `dotnet new xunit` templates):
  - `src/ToyRobot/ToyRobot.csproj`: `OutputType` `Exe`, `TargetFramework` `net8.0`, `RollForward` `LatestMajor`, `Nullable` `enable`, `ImplicitUsings` `enable`, `RootNamespace` and `AssemblyName` `ToyRobot`. No package references.
  - `tests/ToyRobot.Tests/ToyRobot.Tests.csproj` and `tests/ToyRobot.ConsoleEndToEndTests/ToyRobot.ConsoleEndToEndTests.csproj`: `TargetFramework` `net8.0`, `RollForward` `LatestMajor`, `Nullable` `enable`, `ImplicitUsings` `enable`, `IsPackable` `false`, `IsTestProject` `true`, a global `using Xunit;`, package references `Microsoft.NET.Test.Sdk` 17.8.0, `xunit` 2.5.3 and `xunit.runner.visualstudio` 2.5.3, and a project reference to `src/ToyRobot/ToyRobot.csproj`.
- **Rationale**: page 5 names `ToyRobot.sln`; on a machine whose newest SDK is .NET 9 or later, `dotnet new sln` creates `ToyRobot.slnx` unless `--format sln` is given (checked on this machine with SDK 10.0.302). Writing the project files by hand keeps out what the templates add and the sources do not ask for: the `coverlet.collector` package (brief rule 7: no other libraries), a `UnitTest1.cs` file that would have to be deleted (brief rule 12), and a "Hello, World!" program that writes to console output (stdout) ([DEC-23][dec-23]). The package versions are the ones the .NET SDK's own xUnit template uses on this machine, so they are known to restore and run. An xUnit project with no tests passes `dotnet test` with exit code 0 (checked on this machine), so the console end-to-end project can exist empty until spec 002 adds its tests.
- **Alternatives considered**: the templates followed by edits (adds and then removes files); newer package versions (not needed; nothing in the sources depends on them).

## R2. Placeholder entry point (Decided)

- **Decision**: `src/ToyRobot/Program.cs` holds a placeholder entry point that reads nothing, writes nothing and returns exit code 0, with a comment that spec 002 replaces it.
- **Rationale**: an `Exe` project does not build without an entry point, and Program is owned by spec 002 (brief table). The placeholder lets this spec build and test the solution on its own.
- **Alternatives considered**: a class library changed to `Exe` in spec 002 (changes the project type later); the template's "Hello, World!" (writes to stdout).

## R3. `.gitignore` for build output (Decided)

- **Decision**: add a root `.gitignore` listing `bin/`, `obj/`, `TestResults/` and `.vs/`, if the repository has none.
- **Rationale**: the repository has no `.gitignore`; without one, `dotnet build` output would show as new files and could be committed by mistake. It changes no behaviour and adds no library.
- **Alternatives considered**: none that keep build output out of the repository with less.

## R4. Code names that page 6 does not give (Decided)

- **Decision**: names taken from page 6's own wording (spec Clarifications):

  | Page 6 term | Code name |
  |---|---|
  | Parse result: a command object, EXIT, a failure with its reason | `ParseResult` with `ParseResult.CommandObject(IRobotCommand Command)`, `ParseResult.Exit()`, `ParseResult.Failure(string Reason)` |
  | Outcome: the position text, a warning, done with nothing to show | `Outcome` with `Outcome.PositionText(string Text)`, `Outcome.Warning(string Text)`, `Outcome.Done()` |
  | CommandParser turns one line into a parse result ("pass the line", 7.1) | `CommandParser.Parse(string line)` |
  | RobotOperator runs a command object ("run the PlaceCommand", 7.1) | `RobotOperator.Run(IRobotCommand command)` |
  | Robot's placement and its single replace method ("replace placement", 7.1) | `Robot.Placement`, `Robot.Replace(RobotState state)` |
  | Table answers "is this position on the table?" | `Table.IsOnTable(int x, int y)`, constructor `Table(int width, int height)`, read-only `Table.Width` and `Table.Height` |
  | RobotState: X, Y and the facing Direction | `RobotState(int X, int Y, Direction Facing)` |
  | `PlaceCommand` holds X, Y and F | `PlaceCommand(int X, int Y, Direction F)` |
  | Direction: four values, turning left and right, name in upper case | `Direction.North`, `Direction.East`, `Direction.South`, `Direction.West`, `Direction.All`, `Name`, `TurnLeft()`, `TurnRight()` |
  | Page 5 folder layout: the application folders and their namespaces (spec Assumptions, "source folders", added by T060) | `Domain`, `Commands`, `Parsing`, `Session`: namespaces `ToyRobot.Domain`, `ToyRobot.Commands`, `ToyRobot.Parsing`, `ToyRobot.Session` |

  The members page 6 names itself are used exactly: `IRobotCommand.Name`, `NeedsRobotOnTable`, `Execute(IRobotOperator robot)`; `IRobotOperator.CurrentState`, `TryMoveTo(next)` (parameter name `next`).
- **Rationale**: specs 001 and 002 must use the same names (brief rule 6); spec 001 owns these components. `ParseResult` and `Outcome` are result types, not new components: page 6 defines both under CommandParser and RobotOperator.
- **Alternatives considered**: leave names to implementation (risk of mismatch with spec 002).

## R5. Shape of `ParseResult` and `Outcome` (Decided)

- **Decision**: each is an abstract C# `record` with a private constructor and exactly three nested `sealed record` cases (listed in R4). No other case can be added outside the type. Records give value equality, so a test compares a whole result in one assertion, for example `Assert.Equal(new Outcome.Warning("MOVE ignored - robot is not on the table"), outcome)`.
- **Rationale**: matches page 6's "exactly one of these" for both; the Runner (spec 002) picks the case with a C# `switch` on the type.
- **Alternatives considered**: one class with a kind enum and nullable fields (allows invalid combinations); exceptions for failures ([DEC-20][dec-20] and page 6: the parser never throws for bad input).

## R6. Command classes as records

- **Decision**: `PlaceCommand` is a `sealed record` with X, Y and F; `MoveCommand`, `LeftCommand`, `RightCommand` and `ReportCommand` are `sealed record`s with no values. Each implements `IRobotCommand`.
- **Rationale**: a record is a class (page 6 says "one class per robot command") with value equality, so a parser test can compare `new ParseResult.CommandObject(new PlaceCommand(1, 1, Direction.East))` with the result directly. They hold no state that changes.
- **Alternatives considered**: plain classes (tests would need to compare each property).

## R7. Direction as a class with four fixed instances (Decided)

- **Decision**: `Direction` is a `sealed class` with a private constructor and four static read-only instances (`North`, `East`, `South`, `West`), a `Name` property (`NORTH`, `EAST`, `SOUTH`, `WEST`), `TurnLeft()`, `TurnRight()`, and `All` (the four instances, in the order NORTH, EAST, SOUTH, WEST). `ToString()` returns `Name`.
- **Rationale**: page 6 gives Direction turning and its name in upper case. A C# `enum` cannot hold methods (turning would need a second, helper type) and its member names would have to be written in upper case to give the upper-case name, which goes against the usual C# naming. Direction does not know positions (page 6), so the one-step offsets stay in `MoveCommand`. Comparing directions by reference is safe because only four instances exist.
- **Alternatives considered**: `enum` plus a static helper class (an extra type not on page 6); `enum` with upper-case members.

## R8. Text form of a position (Decided)

- **Decision**: `RobotState.ToString()` returns `X,Y,F` (for example `0,1,NORTH`), with the numbers written with the invariant culture and F as `Facing.Name`. `ReportCommand` returns it as the position text; `MoveCommand` uses it in the blocked MOVE warning; `PlaceCommand` uses it for its proposed state in the off-table warning (`PLACE ` + text + ` ignored - position is off the table`).
- **Rationale**: [DEC-22][dec-22] requires the blocked MOVE warning to use "the same `X,Y,F` form as REPORT", and [DEC-22][dec-22]'s off-table PLACE warning uses the same form with the parsed values; one place for the form means the three texts cannot drift apart ([DEC-34][dec-34]). The invariant culture keeps `-1` written with the ASCII hyphen-minus under every regional setting (spec Clarifications).
- **Alternatives considered**: format in each command (three copies of one rule); a separate formatter type (a component not on page 6).

## R9. Culture-independent reading (DEC-08, DEC-07)

- **Decision**: X and Y are read with `int.TryParse(part, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value)`. Command words and direction names are compared with `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)`. Direction names are matched only against `Direction.All` by name; `Enum.TryParse` is not used.
- **Rationale**: [DEC-08][dec-08] step 4 names the number rule. Ordinal comparison ignoring case gives the same result under every regional setting (`right` under Turkish rules would otherwise upper-case to `RİGHT`). `Enum.TryParse` would also accept a number such as `1` as a direction, which [DEC-06][dec-06] forbids (spec Clarifications).
- **Alternatives considered**: `ToUpper()` then compare (depends on the current culture unless the invariant culture is passed; easy to get wrong).

## R10. Finding the command word (DEC-08 steps 1 and 2)

- **Decision**: trim the line with `string.Trim()`; the command word ends at the first character for which `char.IsWhiteSpace` is true; the rest of the line starts after that character. For PLACE the rest is split with `Split(',')` and each part is trimmed with `Trim()`.
- **Rationale**: `string.Trim()` removes exactly the characters for which `char.IsWhiteSpace` is true, so "whitespace" means the same in steps 1, 2 and 3 (spec Clarifications).

## R11. Order of checks in `CommandParser.Parse` (DEC-22)

1. Trimmed line empty → `Failure("blank line ignored")`.
2. Command word `EXIT` (any case) → `Exit()`, whatever follows.
3. Command word `MOVE`, `LEFT`, `RIGHT` or `REPORT` (any case): nothing after it → the command object; anything after it → `Failure("'<line>' ignored - <COMMAND> takes no values")`.
4. Command word `PLACE` (any case): rest split on commas; not exactly three parts → `Failure("'<line>' ignored - expected PLACE X,Y,F")`; then X not readable → `... - X is too large` if it is written as a whole number (optional sign and digits) and `... - X must be a whole number` otherwise; then Y the same way (`... - Y is too large`, `... - Y must be a whole number`); then F not one of the four names → `... - F must be NORTH, SOUTH, EAST or WEST`; otherwise `CommandObject(new PlaceCommand(x, y, f))`.
5. Any other command word → `Failure("'<line>' ignored - unknown command")`.

`<line>` is the trimmed line exactly as typed; `<COMMAND>` is the command name in upper case. `PLACE` with nothing after it has one (empty) part, so it gives "expected PLACE X,Y,F". This is [DEC-22][dec-22]'s order: command word, number of values, X, Y, F.

## R12. Preconditions inside command classes (Decided)

- **Decision**: `MoveCommand`, `LeftCommand`, `RightCommand` and `ReportCommand` read `robot.CurrentState`; if it is `null` they throw `InvalidOperationException`. RobotOperator never runs them in that case ([FR-07][fr-07]), so no input line can reach the throw. `Robot.Replace` throws `ArgumentNullException` for a `null` state ([DEC-14][dec-14]), which also no input can reach. Each throw has a unit test.
- **Rationale**: page 6 says the command classes do not check whether the robot is on the table; RobotOperator does it once. The throw states the precondition instead of failing with a null-reference error, and no warning text is invented for a path no input can take. [DEC-32][dec-32] (never crash) concerns input; it holds because RobotOperator's check comes first, which [FR-07][fr-07]'s tests prove.
- **Alternatives considered**: return a warning from the command (duplicates RobotOperator's rule, against [DEC-27][dec-27]); the `!` operator (would fail with a null-reference error, less clear).

## R13. Test doubles without a mocking library

- **Decision**: command-class unit tests use a hand-written `FakeRobotOperator` (implements `IRobotOperator`; holds a settable `CurrentState`, records each `TryMoveTo` argument, and answers with a configured yes or no). RobotOperator tests use a hand-written `SpyCommand` (implements `IRobotCommand`; records whether `Execute` was called). Acceptance scenarios at component level use a small test helper, `RulesDriver`, that passes each line to `CommandParser.Parse`, runs each command object with `RobotOperator.Run`, and collects the position texts and the warning texts (failure reasons and warning outcomes) in order, stopping at `Exit`.
- **Rationale**: brief rule 7 allows no other library. `RulesDriver` is test code, not a Runner: it writes no console text and the scenarios' console form is checked by spec 002's Runner whole-flow tests.

## R14. Culture in tests

- **Decision**: tests that need another regional setting set `CultureInfo.CurrentCulture` (and `CurrentUICulture`) to `tr-TR` or `sv-SE` inside the test and restore it in a `finally` block.
- **Rationale**: the current culture is held per thread and flows with the test's async context, so parallel xUnit tests are not affected. With correct code the tests pass whatever the machine's culture data; with culture-dependent code they fail where the culture's rules differ (Turkish `i`, Swedish minus sign `−` U+2212).

[dec-06]: ../../docs/design-analysis/design-decisions.md#dec-06-commands
[dec-08]: ../../docs/design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-14]: ../../docs/design-analysis/design-decisions.md#dec-14-only-one-way-to-change-the-robot
[dec-20]: ../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-22]: ../../docs/design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-27]: ../../docs/design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-32]: ../../docs/design-analysis/design-decisions.md#dec-32-never-crash
[dec-34]: ../../docs/design-analysis/design-decisions.md#dec-34-readable-code-solid
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[fr-07]: spec.md#fr-07
