using ToyRobot.Tests.Unit.Support;

namespace ToyRobot.Tests.Unit;

// FR-05; DEC-13, DEC-17; NFR-07f; research R12
public class RightCommandTests
{
    [Fact]
    public void Name_is_RIGHT()
    {
        Assert.Equal("RIGHT", new RightCommand().Name);
    }

    [Fact]
    public void NeedsRobotOnTable_is_true()
    {
        Assert.True(new RightCommand().NeedsRobotOnTable);
    }

    [Theory]
    [InlineData("NORTH", "EAST")]
    [InlineData("EAST", "SOUTH")]
    [InlineData("SOUTH", "WEST")]
    [InlineData("WEST", "NORTH")]
    public void Execute_asks_for_the_same_position_turned_right_and_returns_Done(string from, string to)
    {
        var start = Direction.All.Single(d => d.Name == from);
        var expected = Direction.All.Single(d => d.Name == to);
        var robot = new FakeRobotOperator { CurrentState = new RobotState(2, 3, start) };

        var outcome = new RightCommand().Execute(robot);

        Assert.Equal(new[] { new RobotState(2, 3, expected) }, robot.Requests);
        Assert.Equal(new Outcome.Done(), outcome);
    }

    [Fact]
    public void Execute_returns_Done_also_when_the_answer_is_no()
    {
        var robot = new FakeRobotOperator { CurrentState = new RobotState(2, 3, Direction.North), Answer = false };

        Assert.Equal(new Outcome.Done(), new RightCommand().Execute(robot));
    }

    [Fact]
    public void Execute_throws_when_the_robot_is_not_on_the_table()
    {
        var robot = new FakeRobotOperator { CurrentState = null };

        Assert.Throws<InvalidOperationException>(() => new RightCommand().Execute(robot));
    }
}
