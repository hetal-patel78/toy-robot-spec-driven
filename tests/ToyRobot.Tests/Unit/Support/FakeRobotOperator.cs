namespace ToyRobot.Tests.Unit.Support;

/// <summary>Test double for <see cref="IRobotOperator"/>: records each request and answers yes or no.</summary>
public sealed class FakeRobotOperator : IRobotOperator
{
    public RobotState? CurrentState { get; set; }

    public bool Answer { get; set; } = true;

    public List<RobotState> Requests { get; } = new();

    public bool TryMoveTo(RobotState nextState)
    {
        Requests.Add(nextState);
        if (Answer)
        {
            CurrentState = nextState;
        }

        return Answer;
    }
}
