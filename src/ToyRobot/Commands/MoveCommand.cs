using ToyRobot.Domain;

namespace ToyRobot.Commands;

/// <summary>MOVE: asks to move the robot one step in the direction it faces (DEC-16).</summary>
public sealed record MoveCommand : IRobotCommand
{
    public string Name => "MOVE";

    public bool NeedsRobotOnTable => true;

    public Outcome Execute(IRobotOperator robot)
    {
        var current = robot.CurrentState
            ?? throw new InvalidOperationException("MOVE needs the robot to be on the table.");

        var next = Step(current);

        return robot.TryMoveTo(next)
            ? new Outcome.Done()
            : new Outcome.Warning($"MOVE ignored - next position is off the table so robot stays at {current}");
    }

    private static RobotState Step(RobotState current)
    {
        if (current.Facing == Direction.North)
        {
            return current with { Y = current.Y + 1 };
        }

        if (current.Facing == Direction.South)
        {
            return current with { Y = current.Y - 1 };
        }

        if (current.Facing == Direction.East)
        {
            return current with { X = current.X + 1 };
        }

        return current with { X = current.X - 1 };
    }
}
