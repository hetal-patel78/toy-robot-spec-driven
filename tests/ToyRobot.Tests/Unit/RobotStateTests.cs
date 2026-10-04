using ToyRobot.Tests.Unit.Support;

namespace ToyRobot.Tests.Unit;

// NFR-07c; DEC-12, DEC-13, DEC-21; research R8
public class RobotStateTests
{
    [Fact]
    public void Holds_X_Y_and_Facing_as_given()
    {
        var state = new RobotState(3, 4, Direction.South);

        Assert.Equal(3, state.X);
        Assert.Equal(4, state.Y);
        Assert.Same(Direction.South, state.Facing);
    }

    [Fact]
    public void Two_states_with_the_same_values_are_equal()
    {
        Assert.Equal(new RobotState(1, 2, Direction.East), new RobotState(1, 2, Direction.East));
    }

    [Fact]
    public void With_gives_a_new_state_and_leaves_the_original_unchanged()
    {
        var original = new RobotState(1, 2, Direction.North);

        var changed = original with { Facing = Direction.West };

        Assert.NotSame(original, changed);
        Assert.Same(Direction.North, original.Facing);
        Assert.Same(Direction.West, changed.Facing);
    }

    [Theory]
    [InlineData(0, 1, "NORTH", "0,1,NORTH")]
    [InlineData(4, 2, "EAST", "4,2,EAST")]
    [InlineData(-1, 0, "NORTH", "-1,0,NORTH")]
    [InlineData(2147483647, 0, "WEST", "2147483647,0,WEST")]
    public void ToString_gives_X_Y_F(int x, int y, string facingName, string expected)
    {
        var facing = Direction.All.Single(d => d.Name == facingName);

        Assert.Equal(expected, new RobotState(x, y, facing).ToString());
    }

    [Fact]
    public void ToString_writes_a_negative_number_with_an_ASCII_hyphen_minus_in_any_culture()
    {
        using var scope = new CultureScope("sv-SE");

        Assert.Equal("-1,0,NORTH", new RobotState(-1, 0, Direction.North).ToString());
    }
}
