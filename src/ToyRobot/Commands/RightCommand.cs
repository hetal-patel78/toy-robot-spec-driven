using ToyRobot.Domain;

namespace ToyRobot.Commands;

/// <summary>RIGHT: turns the robot 90 degrees to the right, staying in place (DEC-17).</summary>
public sealed record RightCommand : IRobotCommand
{
    public string Name => "RIGHT";

    public bool NeedsRobotOnTable => true;

    public Outcome Execute(IRobotOperator robot)
    {
        var current = robot.CurrentState
            ?? throw new InvalidOperationException("RIGHT needs the robot to be on the table.");

        robot.TryMoveTo(current with { Facing = current.Facing.TurnRight() });
        return new Outcome.Done();
    }
}
