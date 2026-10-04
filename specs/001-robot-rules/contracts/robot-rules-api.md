# Contract: the robot-rules types used by spec 002

The public surface that spec 002's Runner and Program build on. Names and members are exactly as on page 6, with the code names settled in `research.md` R4. Everything is in the namespace `ToyRobot`, in the project `src/ToyRobot/`. Spec 002 uses these types and does not restate their rules.

```csharp
public sealed class Table
{
    public Table(int width, int height);
    public int Width { get; }
    public int Height { get; }
    public bool IsOnTable(int x, int y);           // 0 <= x < Width && 0 <= y < Height
}

public sealed class Direction
{
    public static readonly Direction North, East, South, West;
    public static IReadOnlyList<Direction> All { get; }   // North, East, South, West
    public string Name { get; }                    // "NORTH", "EAST", "SOUTH", "WEST"
    public Direction TurnLeft();
    public Direction TurnRight();
    public override string ToString();             // Name
}

public sealed record RobotState(int X, int Y, Direction Facing)
{
    public override string ToString();             // "X,Y,F", invariant culture, e.g. "0,1,NORTH"
}

public sealed class Robot
{
    public RobotState? Placement { get; }          // null until the first Replace
    public void Replace(RobotState state);         // null -> ArgumentNullException
}

public interface IRobotOperator
{
    RobotState? CurrentState { get; }
    bool TryMoveTo(RobotState next);
}

public sealed class RobotOperator : IRobotOperator
{
    public RobotOperator(Table table);
    public RobotState? CurrentState { get; }
    public bool TryMoveTo(RobotState next);
    public Outcome Run(IRobotCommand command);
}

public interface IRobotCommand
{
    string Name { get; }
    bool NeedsRobotOnTable { get; }
    Outcome Execute(IRobotOperator robot);
}

public sealed record PlaceCommand(int X, int Y, Direction F) : IRobotCommand;
public sealed record MoveCommand : IRobotCommand;
public sealed record LeftCommand : IRobotCommand;
public sealed record RightCommand : IRobotCommand;
public sealed record ReportCommand : IRobotCommand;

public abstract record Outcome
{
    public sealed record PositionText(string Text) : Outcome;   // "0,1,NORTH", no "Output:" label
    public sealed record Warning(string Text) : Outcome;        // no "Warning: " prefix
    public sealed record Done : Outcome;
}

public abstract record ParseResult
{
    public sealed record CommandObject(IRobotCommand Command) : ParseResult;
    public sealed record Exit : ParseResult;
    public sealed record Failure(string Reason) : ParseResult;  // no "Warning: " prefix
}

public sealed class CommandParser
{
    public ParseResult Parse(string line);         // never throws; grammar in command-line-grammar.md
}
```

## How spec 002 uses it (for reference; the rules are spec 002's)

- Program creates `new Table(5, 5)` and `new RobotOperator(table)` ([DEC-03][dec-03], [DEC-26][dec-26]).
- The Runner creates its own `new CommandParser()` and, for each line, switches on the `ParseResult` case: `Failure` → write `Warning: ` + `Reason`; `Exit` → stop; `CommandObject` → `robotOperator.Run(Command)` and switch on the `Outcome` case: `PositionText` → write `Output:` then `Text` to console output (stdout); `Warning` → write `Warning: ` + `Text` to console messages (stderr); `Done` → write nothing ([DEC-21][dec-21] to [DEC-23][dec-23], [DEC-27][dec-27], [DEC-30][dec-30]).

## Warning texts produced here (DEC-22, without `Warning: `)

| Producer | Text |
|---|---|
| `PlaceCommand` | `PLACE X,Y,F ignored - position is off the table` (parsed values, F in upper case), e.g. `PLACE 9,9,NORTH ignored - position is off the table` |
| `MoveCommand` | `MOVE ignored - next position is off the table so robot stays at X,Y,F` (current position), e.g. `MOVE ignored - next position is off the table so robot stays at 4,2,EAST` |
| RobotOperator | `MOVE ignored - robot is not on the table`, `LEFT ignored - robot is not on the table`, `RIGHT ignored - robot is not on the table`, `REPORT ignored - robot is not on the table` |
| CommandParser | The seven failure reasons in `command-line-grammar.md` |

[dec-03]: ../../../docs/design-analysis/design-decisions.md#dec-03-table-size-is-a-constructor-parameter
[dec-21]: ../../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-23]: ../../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-26]: ../../../docs/design-analysis/design-decisions.md#dec-26-responsibilities
[dec-27]: ../../../docs/design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-30]: ../../../docs/design-analysis/design-decisions.md#dec-30-exit-is-handled-by-the-runner
