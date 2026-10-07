namespace ToyRobot.Domain;

/// <summary>Holds the robot's placement: nothing until the first PLACE, then a state (DEC-11, DEC-14).</summary>
public sealed class Robot
{
    public RobotState? Placement { get; private set; }

    public void MoveTo(RobotState nextState)
    {
        ArgumentNullException.ThrowIfNull(nextState);
        Placement = nextState;
    }
}
