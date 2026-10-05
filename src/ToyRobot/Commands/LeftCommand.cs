using ToyRobot.Domain;

namespace ToyRobot.Commands;

/// <summary>LEFT: turns the robot 90 degrees to the left, staying in place (DEC-17).</summary>
public sealed record LeftCommand : IRobotCommand
{
    public string Name => "LEFT";

    public bool NeedsRobotOnTable => true;

    public Outcome Execute(IRobotOperator robot)
    {
        var current = robot.CurrentState
            ?? throw new InvalidOperationException("LEFT needs the robot to be on the table.");

        robot.TryMoveTo(current with { Facing = current.Facing.TurnLeft() });
        return new Outcome.Done();
    }
}
