namespace ToyRobot;

/// <summary>What a command may use of the robot: read its state, or ask to move it (DEC-13, DEC-14).</summary>
public interface IRobotOperator
{
    RobotState? CurrentState { get; }

    bool TryMoveTo(RobotState next);
}
