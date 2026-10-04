using ToyRobot.Tests.Unit.Support;

namespace ToyRobot.Tests.Unit;

// FR-06; DEC-07, DEC-18, DEC-21; NFR-07f; research R12
public class ReportCommandTests
{
    [Fact]
    public void Name_is_REPORT()
    {
        Assert.Equal("REPORT", new ReportCommand().Name);
    }

    [Fact]
    public void NeedsRobotOnTable_is_true()
    {
        Assert.True(new ReportCommand().NeedsRobotOnTable);
    }

    [Theory]
    [InlineData(1, 2, "NORTH", "1,2,NORTH")]
    [InlineData(0, 0, "NORTH", "0,0,NORTH")]
    [InlineData(4, 4, "WEST", "4,4,WEST")]
    public void Execute_returns_the_position_text(int x, int y, string facingName, string expected)
    {
        var facing = Direction.All.Single(d => d.Name == facingName);
        var robot = new FakeRobotOperator { CurrentState = new RobotState(x, y, facing) };

        var outcome = new ReportCommand().Execute(robot);

        Assert.Equal(new Outcome.PositionText(expected), outcome);
    }

    [Fact]
    public void Execute_makes_no_move_request()
    {
        var robot = new FakeRobotOperator { CurrentState = new RobotState(1, 2, Direction.North) };

        new ReportCommand().Execute(robot);

        Assert.Empty(robot.Requests);
    }

    [Fact]
    public void Execute_throws_when_the_robot_is_not_on_the_table()
    {
        var robot = new FakeRobotOperator { CurrentState = null };

        Assert.Throws<InvalidOperationException>(() => new ReportCommand().Execute(robot));
    }
}
