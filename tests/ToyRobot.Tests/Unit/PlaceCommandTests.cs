using ToyRobot.Tests.Unit.Support;

namespace ToyRobot.Tests.Unit;

// FR-02; DEC-13, DEC-15, DEC-22, DEC-27; NFR-07f
public class PlaceCommandTests
{
    [Fact]
    public void Name_is_PLACE()
    {
        Assert.Equal("PLACE", new PlaceCommand(0, 0, Direction.North).Name);
    }

    [Fact]
    public void NeedsRobotOnTable_is_false()
    {
        Assert.False(new PlaceCommand(0, 0, Direction.North).NeedsRobotOnTable);
    }

    [Fact]
    public void Holds_X_Y_and_F()
    {
        var command = new PlaceCommand(1, 1, Direction.East);

        Assert.Equal(1, command.X);
        Assert.Equal(1, command.Y);
        Assert.Same(Direction.East, command.F);
    }

    [Fact]
    public void Execute_makes_one_request_with_the_proposed_state_and_returns_Done_when_accepted()
    {
        var robot = new FakeRobotOperator { Answer = true };

        var outcome = new PlaceCommand(1, 1, Direction.East).Execute(robot);

        Assert.Equal(new Outcome.Done(), outcome);
        Assert.Equal(new[] { new RobotState(1, 1, Direction.East) }, robot.Requests);
    }

    [Fact]
    public void Execute_returns_a_warning_when_the_position_is_refused()
    {
        var robot = new FakeRobotOperator { Answer = false };

        var outcome = new PlaceCommand(9, 9, Direction.North).Execute(robot);

        Assert.Equal(new Outcome.Warning("PLACE 9,9,NORTH ignored - position is off the table"), outcome);
    }

    [Fact]
    public void Execute_writes_a_negative_X_in_the_warning()
    {
        var robot = new FakeRobotOperator { Answer = false };

        var outcome = new PlaceCommand(-1, 0, Direction.North).Execute(robot);

        Assert.Equal(new Outcome.Warning("PLACE -1,0,NORTH ignored - position is off the table"), outcome);
    }

    [Fact]
    public void Execute_writes_a_negative_X_with_an_ASCII_hyphen_minus_in_any_culture()
    {
        using var scope = new CultureScope("sv-SE");
        var robot = new FakeRobotOperator { Answer = false };

        var outcome = new PlaceCommand(-1, 0, Direction.North).Execute(robot);

        Assert.Equal(new Outcome.Warning("PLACE -1,0,NORTH ignored - position is off the table"), outcome);
    }

    [Fact]
    public void Execute_writes_the_largest_whole_number_in_the_warning()
    {
        var robot = new FakeRobotOperator { Answer = false };

        var outcome = new PlaceCommand(2147483647, 0, Direction.North).Execute(robot);

        Assert.Equal(
            new Outcome.Warning("PLACE 2147483647,0,NORTH ignored - position is off the table"),
            outcome);
    }

    [Fact]
    public void Execute_works_when_the_robot_is_not_on_the_table()
    {
        var robot = new FakeRobotOperator { CurrentState = null };

        var outcome = new PlaceCommand(2, 2, Direction.South).Execute(robot);

        Assert.Equal(new Outcome.Done(), outcome);
    }
}
