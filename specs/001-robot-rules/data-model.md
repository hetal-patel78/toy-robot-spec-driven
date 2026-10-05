# Data model: Robot rules (001-robot-rules)

Phase 1 of `/speckit-plan`. The entities are the components of page 6 that hold data or results. Names and members come from page 6 and `research.md` R4. All types are public, in `src/ToyRobot/`, in namespaces that follow its folders (`ToyRobot.Domain`, `ToyRobot.Commands`, `ToyRobot.Parsing`; see plan.md, Project Structure). Nothing is stored outside memory.

## Table (DEC-01, DEC-03, DEC-28; FR-01)

| Member | Type | Rule |
|---|---|---|
| `Table(int width, int height)` | constructor | Stores the size. No check of the values (spec Assumptions). |
| `Width`, `Height` | `int`, read-only | As given. |
| `IsOnTable(int x, int y)` | `bool` | True exactly when `0 <= x < Width` and `0 <= y < Height`. |

Does not hold the robot, know commands or read configuration. The application's table is `Table(5, 5)`, created by Program (spec 002).

## Direction (DEC-02, DEC-07, DEC-17; FR-05; NFR-07b)

| Member | Type | Rule |
|---|---|---|
| `North`, `East`, `South`, `West` | `static Direction`, read-only | The only four instances (private constructor). |
| `All` | `static IReadOnlyList<Direction>` | `North`, `East`, `South`, `West`, in that order. |
| `Name` | `string` | `NORTH`, `EAST`, `SOUTH`, `WEST`. |
| `TurnLeft()` | `Direction` | NORTH → WEST → SOUTH → EAST → NORTH. |
| `TurnRight()` | `Direction` | NORTH → EAST → SOUTH → WEST → NORTH. |
| `ToString()` | `string` | `Name`. |

Does not know positions, the table or the robot.

## RobotState (DEC-12, DEC-13; NFR-07c)

`sealed record RobotState(int X, int Y, Direction Facing)`.

| Member | Rule |
|---|---|
| `X`, `Y`, `Facing` | Set once when created; a changed state is a new `RobotState` ([DEC-13][dec-13]). |
| `ToString()` | `X,Y,F` with the numbers written with the invariant culture and F = `Facing.Name`, for example `0,1,NORTH` or `-1,0,NORTH` (research R8). |

Does not check whether it is on the table.

## Robot (DEC-11, DEC-14; NFR-07d)

| Member | Type | Rule |
|---|---|---|
| `Robot()` | constructor | Starts with `Placement` = `null` (not on the table). |
| `Placement` | `RobotState?`, read-only | `null` or a valid state. |
| `Replace(RobotState state)` | `void` | Sets `Placement` to `state`. A `null` argument throws `ArgumentNullException` (research R12). No member sets `Placement` back to `null`. |

State transitions:

```text
not on the table (Placement = null) --Replace(s)--> on the table (Placement = s)
on the table (Placement = s1)       --Replace(s2)-> on the table (Placement = s2)
(no transition back to "not on the table")
```

Only RobotOperator creates and holds the Robot; it is never handed out ([DEC-14][dec-14]).

## IRobotOperator (DEC-13, DEC-14; NFR-07g)

| Member | Type | Rule |
|---|---|---|
| `CurrentState` | `RobotState?`, read-only | The robot's placement. |
| `TryMoveTo(RobotState next)` | `bool` | If the Table says `next` is on the table: replace the Robot's state with `next`, answer `true`. Otherwise leave the robot unchanged, answer `false`. |

## RobotOperator (DEC-13, DEC-14, DEC-19, DEC-26, DEC-27, DEC-30; NFR-07h)

`sealed class RobotOperator : IRobotOperator`.

| Member | Type | Rule |
|---|---|---|
| `RobotOperator(Table table)` | constructor | Keeps the Table; creates the only Robot. |
| `CurrentState`, `TryMoveTo(next)` | from `IRobotOperator` | As above. |
| `Run(IRobotCommand command)` | `Outcome` | 1. If `command.NeedsRobotOnTable` and the robot is not on the table: return `Outcome.Warning("<Name> ignored - robot is not on the table")` without running the command. 2. Otherwise return `command.Execute(this)`. |

Never receives EXIT or a failure (the Runner, spec 002, filters them).

## IRobotCommand and the command classes (DEC-13, DEC-15 to DEC-19, DEC-27; NFR-07e, NFR-07f)

`IRobotCommand`: `string Name { get; }`, `bool NeedsRobotOnTable { get; }`, `Outcome Execute(IRobotOperator robot)`.

| Class | Values | `Name` | `NeedsRobotOnTable` | `Execute(robot)` |
|---|---|---|---|---|
| `PlaceCommand` (`sealed record PlaceCommand(int X, int Y, Direction F)`) | X, Y, F | `PLACE` | `false` | `proposed = new RobotState(X, Y, F)`; `TryMoveTo(proposed)` true → `Outcome.Done()`; false → `Outcome.Warning("PLACE " + proposed + " ignored - position is off the table")` |
| `MoveCommand` (`sealed record`) | none | `MOVE` | `true` | `current = CurrentState` (null → `InvalidOperationException`); next = one step: NORTH Y + 1, SOUTH Y − 1, EAST X + 1, WEST X − 1, same facing; `TryMoveTo(next)` true → `Outcome.Done()`; false → `Outcome.Warning("MOVE ignored - next position is off the table so robot stays at " + current)` |
| `LeftCommand` (`sealed record`) | none | `LEFT` | `true` | `current` (null → `InvalidOperationException`); `TryMoveTo(current with { Facing = current.Facing.TurnLeft() })`; `Outcome.Done()` (no warning, page 6) |
| `RightCommand` (`sealed record`) | none | `RIGHT` | `true` | As LEFT with `TurnRight()` |
| `ReportCommand` (`sealed record`) | none | `REPORT` | `true` | `current` (null → `InvalidOperationException`); `Outcome.PositionText(current.ToString())`; does not call `TryMoveTo` |

## Outcome (DEC-21, DEC-22, DEC-26; page 6 "Outcome")

Abstract record with a private constructor and exactly three cases:

| Case | Holds | When |
|---|---|---|
| `Outcome.PositionText(string Text)` | `X,Y,F`, no label | REPORT with the robot on the table |
| `Outcome.Warning(string Text)` | warning text, no `Warning: ` prefix | Command ignored (off-table PLACE, blocked MOVE, not on the table) |
| `Outcome.Done()` | nothing | PLACE, MOVE, LEFT or RIGHT carried out |

## ParseResult (DEC-08, DEC-10, DEC-20, DEC-22, DEC-30; page 6 "Parse result")

Abstract record with a private constructor and exactly three cases:

| Case | Holds | When |
|---|---|---|
| `ParseResult.CommandObject(IRobotCommand Command)` | a new command object | Valid PLACE, MOVE, LEFT, RIGHT or REPORT |
| `ParseResult.Exit()` | nothing | Command word EXIT, any case, anything after it |
| `ParseResult.Failure(string Reason)` | the warning text without `Warning: ` | Any bad line (reasons in `contracts/command-line-grammar.md`) |

## CommandParser (DEC-06 to DEC-10, DEC-20, DEC-22, DEC-28; FR-09, FR-10, FR-10a; NFR-07i)

`sealed class CommandParser` with no fields and `ParseResult Parse(string line)`. Rules: `contracts/command-line-grammar.md` and `research.md` R9 to R11.

[dec-13]: ../../docs/design-analysis/design-decisions.md#dec-13-state-is-replaced-never-changed-in-place
[dec-14]: ../../docs/design-analysis/design-decisions.md#dec-14-only-one-way-to-change-the-robot
