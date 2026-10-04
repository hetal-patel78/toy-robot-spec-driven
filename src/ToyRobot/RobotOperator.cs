namespace ToyRobot;

/// <summary>
/// Holds the only Robot and the Table. Applies the rule "check with the Table, then replace" (DEC-13, DEC-14)
/// and runs command objects (DEC-26, DEC-27).
/// </summary>
public sealed class RobotOperator : IRobotOperator
{
    private readonly Table _table;
    private readonly Robot _robot = new();

    public RobotOperator(Table table)
    {
        _table = table;
    }

    public RobotState? CurrentState => _robot.Placement;

    public bool TryMoveTo(RobotState next)
    {
        if (!_table.IsOnTable(next.X, next.Y))
        {
            return false;
        }

        _robot.Replace(next);
        return true;
    }

    public Outcome Run(IRobotCommand command)
    {
        if (command.NeedsRobotOnTable && _robot.Placement is null)
        {
            return new Outcome.Warning($"{command.Name} ignored - robot is not on the table");
        }

        return command.Execute(this);
    }
}
