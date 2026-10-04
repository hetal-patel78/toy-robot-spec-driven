namespace ToyRobot.Tests.Unit.Support;

/// <summary>Test double for <see cref="IRobotOperator"/>: records each request and answers yes or no.</summary>
public sealed class FakeRobotOperator : IRobotOperator
{
    public RobotState? CurrentState { get; set; }

    public bool Answer { get; set; } = true;

    public List<RobotState> Requests { get; } = new();

    public bool TryMoveTo(RobotState next)
    {
        Requests.Add(next);
        if (Answer)
        {
            CurrentState = next;
        }

        return Answer;
    }
}
