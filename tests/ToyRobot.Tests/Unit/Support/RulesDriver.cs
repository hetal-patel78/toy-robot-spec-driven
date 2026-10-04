namespace ToyRobot.Tests.Unit.Support;

/// <summary>
/// Runs input lines through <see cref="CommandParser"/> and <see cref="RobotOperator"/> and collects
/// the position texts and warning texts in order. It writes nothing; spec 002's tests check console text.
/// </summary>
public sealed class RulesDriver
{
    private readonly Table _table;
    private readonly List<string> _positions = new();
    private readonly List<string> _warnings = new();

    public RulesDriver(Table table)
    {
        _table = table;
    }

    public IReadOnlyList<string> Positions => _positions;

    public IReadOnlyList<string> Warnings => _warnings;

    public RobotState? FinalState { get; private set; }

    public void Run(params string[] lines)
    {
        var parser = new CommandParser();
        var robotOperator = new RobotOperator(_table);

        foreach (var line in lines)
        {
            var result = parser.Parse(line);
            if (result is ParseResult.Exit)
            {
                break;
            }

            if (result is ParseResult.Failure failure)
            {
                _warnings.Add(failure.Reason);
                continue;
            }

            var command = ((ParseResult.CommandObject)result).Command;
            switch (robotOperator.Run(command))
            {
                case Outcome.PositionText position:
                    _positions.Add(position.Text);
                    break;
                case Outcome.Warning warning:
                    _warnings.Add(warning.Text);
                    break;
            }
        }

        FinalState = robotOperator.CurrentState;
    }
}
