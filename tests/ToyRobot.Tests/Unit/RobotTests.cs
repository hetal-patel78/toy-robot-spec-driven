namespace ToyRobot.Tests.Unit;

// NFR-07d; DEC-11, DEC-14; research R12
public class RobotTests
{
    [Fact]
    public void A_new_robot_has_no_placement()
    {
        Assert.Null(new Robot().Placement);
    }

    [Fact]
    public void MoveTo_sets_the_placement()
    {
        var robot = new Robot();
        var state = new RobotState(0, 0, Direction.North);

        robot.MoveTo(state);

        Assert.Equal(state, robot.Placement);
    }

    [Fact]
    public void A_second_MoveTo_replaces_the_placement()
    {
        var robot = new Robot();
        robot.MoveTo(new RobotState(0, 0, Direction.North));
        var second = new RobotState(3, 2, Direction.West);

        robot.MoveTo(second);

        Assert.Equal(second, robot.Placement);
    }

    [Fact]
    public void MoveTo_with_null_throws_and_leaves_a_new_robot_unplaced()
    {
        var robot = new Robot();

        Assert.Throws<ArgumentNullException>(() => robot.MoveTo(null!));
        Assert.Null(robot.Placement);
    }

    [Fact]
    public void MoveTo_with_null_throws_and_leaves_a_placed_robot_unchanged()
    {
        var robot = new Robot();
        var state = new RobotState(1, 1, Direction.East);
        robot.MoveTo(state);

        Assert.Throws<ArgumentNullException>(() => robot.MoveTo(null!));
        Assert.Equal(state, robot.Placement);
    }

    [Fact]
    public void Placement_has_no_public_setter()
    {
        var property = typeof(Robot).GetProperty(nameof(Robot.Placement));

        Assert.NotNull(property);
        Assert.True(property!.SetMethod is null || !property.SetMethod.IsPublic);
    }
}
