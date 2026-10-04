namespace ToyRobot;

/// <summary>One robot command (DEC-27).</summary>
public interface IRobotCommand
{
    string Name { get; }

    bool NeedsRobotOnTable { get; }

    Outcome Execute(IRobotOperator robot);
}
