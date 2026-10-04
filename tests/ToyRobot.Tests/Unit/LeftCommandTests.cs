using ToyRobot.Tests.Unit.Support;

namespace ToyRobot.Tests.Unit;

// FR-05; DEC-13, DEC-17; NFR-07f; research R12
public class LeftCommandTests
{
    [Fact]
    public void Name_is_LEFT()
    {
        Assert.Equal("LEFT", new LeftCommand().Name);
    }

    [Fact]
    public void NeedsRobotOnTable_is_true()
    {
        Assert.True(new LeftCommand().NeedsRobotOnTable);
    }

    [Theory]
    [InlineData("NORTH", "WEST")]
    [InlineData("WEST", "SOUTH")]
    [InlineData("SOUTH", "EAST")]
    [InlineData("EAST", "NORTH")]
    public void Execute_asks_for_the_same_position_turned_left_and_returns_Done(string from, string to)
    {
        var start = Direction.All.Single(d => d.Name == from);
        var expected = Direction.All.Single(d => d.Name == to);
        var robot = new FakeRobotOperator { CurrentState = new RobotState(2, 3, start) };

        var outcome = new LeftCommand().Execute(robot);

        Assert.Equal(new[] { new RobotState(2, 3, expected) }, robot.Requests);
        Assert.Equal(new Outcome.Done(), outcome);
    }

    [Fact]
    public void Execute_returns_Done_also_when_the_answer_is_no()
    {
        var robot = new FakeRobotOperator { CurrentState = new RobotState(2, 3, Direction.North), Answer = false };

        Assert.Equal(new Outcome.Done(), new LeftCommand().Execute(robot));
    }

    [Fact]
    public void Execute_throws_when_the_robot_is_not_on_the_table()
    {
        var robot = new FakeRobotOperator { CurrentState = null };

        Assert.Throws<InvalidOperationException>(() => new LeftCommand().Execute(robot));
    }
}
