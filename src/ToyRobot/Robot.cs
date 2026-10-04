namespace ToyRobot;

/// <summary>Holds the robot's placement: nothing until the first PLACE, then a state (DEC-11, DEC-14).</summary>
public sealed class Robot
{
    public RobotState? Placement { get; private set; }

    public void Replace(RobotState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        Placement = state;
    }
}
