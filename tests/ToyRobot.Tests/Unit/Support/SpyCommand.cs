namespace ToyRobot.Tests.Unit.Support;

/// <summary>Test double for <see cref="IRobotCommand"/>: counts how often it is executed.</summary>
public sealed class SpyCommand : IRobotCommand
{
    private readonly Outcome _outcome;

    public SpyCommand(string name, bool needsRobotOnTable, Outcome outcome)
    {
        Name = name;
        NeedsRobotOnTable = needsRobotOnTable;
        _outcome = outcome;
    }

    public string Name { get; }

    public bool NeedsRobotOnTable { get; }

    public int ExecuteCount { get; private set; }

    public Outcome Execute(IRobotOperator robot)
    {
        ExecuteCount++;
        return _outcome;
    }
}
