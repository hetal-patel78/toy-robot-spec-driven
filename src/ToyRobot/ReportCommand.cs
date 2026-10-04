namespace ToyRobot;

/// <summary>REPORT: gives the robot's position and facing as text (DEC-18, DEC-21).</summary>
public sealed record ReportCommand : IRobotCommand
{
    public string Name => "REPORT";

    public bool NeedsRobotOnTable => true;

    public Outcome Execute(IRobotOperator robot)
    {
        var current = robot.CurrentState
            ?? throw new InvalidOperationException("REPORT needs the robot to be on the table.");

        return new Outcome.PositionText(current.ToString());
    }
}
