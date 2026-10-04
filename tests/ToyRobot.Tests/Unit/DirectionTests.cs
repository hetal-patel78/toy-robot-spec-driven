namespace ToyRobot.Tests.Unit;

// FR-05; DEC-02, DEC-07, DEC-17; NFR-07b
public class DirectionTests
{
    [Fact]
    public void Name_is_the_upper_case_word()
    {
        Assert.Equal("NORTH", Direction.North.Name);
        Assert.Equal("EAST", Direction.East.Name);
        Assert.Equal("SOUTH", Direction.South.Name);
        Assert.Equal("WEST", Direction.West.Name);
    }

    [Fact]
    public void ToString_equals_Name()
    {
        foreach (var direction in Direction.All)
        {
            Assert.Equal(direction.Name, direction.ToString());
        }
    }

    [Fact]
    public void All_holds_the_four_directions_in_order()
    {
        Assert.Equal(
            new[] { Direction.North, Direction.East, Direction.South, Direction.West },
            Direction.All);
    }

    [Fact]
    public void TurnLeft_goes_NORTH_WEST_SOUTH_EAST_NORTH()
    {
        Assert.Same(Direction.West, Direction.North.TurnLeft());
        Assert.Same(Direction.South, Direction.West.TurnLeft());
        Assert.Same(Direction.East, Direction.South.TurnLeft());
        Assert.Same(Direction.North, Direction.East.TurnLeft());
    }

    [Fact]
    public void TurnRight_goes_NORTH_EAST_SOUTH_WEST_NORTH()
    {
        Assert.Same(Direction.East, Direction.North.TurnRight());
        Assert.Same(Direction.South, Direction.East.TurnRight());
        Assert.Same(Direction.West, Direction.South.TurnRight());
        Assert.Same(Direction.North, Direction.West.TurnRight());
    }

    [Fact]
    public void Four_left_turns_return_to_the_start()
    {
        foreach (var start in Direction.All)
        {
            Assert.Same(start, start.TurnLeft().TurnLeft().TurnLeft().TurnLeft());
        }
    }

    [Fact]
    public void Four_right_turns_return_to_the_start()
    {
        foreach (var start in Direction.All)
        {
            Assert.Same(start, start.TurnRight().TurnRight().TurnRight().TurnRight());
        }
    }

    [Fact]
    public void Left_then_right_returns_to_the_start()
    {
        foreach (var start in Direction.All)
        {
            Assert.Same(start, start.TurnLeft().TurnRight());
        }
    }
}
