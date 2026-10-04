using ToyRobot.Tests.Unit.Support;

namespace ToyRobot.Tests.Unit;

// FR-03, FR-04; DEC-02, DEC-13, DEC-16, DEC-22; NFR-07f; research R12
public class MoveCommandTests
{
    [Fact]
    public void Name_is_MOVE()
    {
        Assert.Equal("MOVE", new MoveCommand().Name);
    }

    [Fact]
    public void NeedsRobotOnTable_is_true()
    {
        Assert.True(new MoveCommand().NeedsRobotOnTable);
    }

    [Theory]
    [InlineData("NORTH", 2, 3)]
    [InlineData("SOUTH", 2, 1)]
    [InlineData("EAST", 3, 2)]
    [InlineData("WEST", 1, 2)]
    public void Execute_asks_for_one_step_in_the_facing_direction(string facingName, int nextX, int nextY)
    {
        var facing = Direction.All.Single(d => d.Name == facingName);
        var robot = new FakeRobotOperator { CurrentState = new RobotState(2, 2, facing) };

        var outcome = new MoveCommand().Execute(robot);

        Assert.Equal(new[] { new RobotState(nextX, nextY, facing) }, robot.Requests);
        Assert.Equal(new Outcome.Done(), outcome);
    }

    [Theory]
    [InlineData(4, 2, "EAST", "4,2,EAST")]
    [InlineData(0, 0, "SOUTH", "0,0,SOUTH")]
    public void Execute_returns_a_warning_naming_the_current_position_when_the_move_is_refused(
        int x, int y, string facingName, string expectedPosition)
    {
        var facing = Direction.All.Single(d => d.Name == facingName);
        var robot = new FakeRobotOperator { CurrentState = new RobotState(x, y, facing), Answer = false };

        var outcome = new MoveCommand().Execute(robot);

        Assert.Equal(
            new Outcome.Warning($"MOVE ignored - next position is off the table so robot stays at {expectedPosition}"),
            outcome);
    }

    [Fact]
    public void Execute_throws_when_the_robot_is_not_on_the_table()
    {
        var robot = new FakeRobotOperator { CurrentState = null };

        Assert.Throws<InvalidOperationException>(() => new MoveCommand().Execute(robot));
    }
}
