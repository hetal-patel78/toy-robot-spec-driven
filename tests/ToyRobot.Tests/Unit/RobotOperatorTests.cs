using ToyRobot.Tests.Unit.Support;

namespace ToyRobot.Tests.Unit;

// FR-01, FR-02, FR-06; DEC-11, DEC-13, DEC-15, DEC-28; NFR-07h
public class RobotOperatorTests
{
    private static RobotOperator NewOperator() => new(new Table(5, 5));

    [Fact]
    public void A_new_RobotOperator_has_no_current_state()
    {
        Assert.Null(NewOperator().CurrentState);
    }

    [Fact]
    public void TryMoveTo_a_position_on_the_table_returns_true_and_sets_the_state()
    {
        var robot = NewOperator();
        var next = new RobotState(2, 2, Direction.North);

        Assert.True(robot.TryMoveTo(next));
        Assert.Equal(next, robot.CurrentState);
    }

    [Fact]
    public void TryMoveTo_a_position_off_the_table_returns_false_and_leaves_a_new_robot_unplaced()
    {
        var robot = NewOperator();

        Assert.False(robot.TryMoveTo(new RobotState(5, 0, Direction.North)));
        Assert.Null(robot.CurrentState);
    }

    [Fact]
    public void TryMoveTo_a_position_off_the_table_returns_false_and_leaves_a_placed_robot_unchanged()
    {
        var robot = NewOperator();
        var placed = new RobotState(1, 1, Direction.East);
        robot.TryMoveTo(placed);

        Assert.False(robot.TryMoveTo(new RobotState(5, 0, Direction.North)));
        Assert.Equal(placed, robot.CurrentState);
    }

    [Fact]
    public void Run_PLACE_on_the_table_returns_Done_and_places_the_robot()
    {
        var robot = NewOperator();

        var outcome = robot.Run(new PlaceCommand(0, 0, Direction.North));

        Assert.Equal(new Outcome.Done(), outcome);
        Assert.Equal(new RobotState(0, 0, Direction.North), robot.CurrentState);
    }

    [Fact]
    public void Run_PLACE_then_REPORT_returns_the_position_text()
    {
        var robot = NewOperator();
        robot.Run(new PlaceCommand(4, 4, Direction.North));

        var outcome = robot.Run(new ReportCommand());

        Assert.Equal(new Outcome.PositionText("4,4,NORTH"), outcome);
    }

    [Fact]
    public void Place_off_the_table_is_ignored_when_not_on_table()
    {
        var robot = NewOperator();

        var outcome = robot.Run(new PlaceCommand(9, 9, Direction.North));

        Assert.Equal(new Outcome.Warning("PLACE 9,9,NORTH ignored - position is off the table"), outcome);
        Assert.Null(robot.CurrentState);
    }

    [Fact]
    public void Place_off_the_table_is_ignored_when_on_table()
    {
        var robot = NewOperator();
        robot.Run(new PlaceCommand(4, 2, Direction.North));

        var outcome = robot.Run(new PlaceCommand(9, 9, Direction.North));

        Assert.Equal(new Outcome.Warning("PLACE 9,9,NORTH ignored - position is off the table"), outcome);
        Assert.Equal(new RobotState(4, 2, Direction.North), robot.CurrentState);
    }

    [Fact]
    public void A_second_valid_PLACE_moves_the_robot()
    {
        var robot = NewOperator();
        robot.Run(new PlaceCommand(4, 2, Direction.North));

        robot.Run(new PlaceCommand(1, 1, Direction.East));

        Assert.Equal(new RobotState(1, 1, Direction.East), robot.CurrentState);
    }

    [Fact]
    public void Place_with_a_negative_position_is_ignored()
    {
        var robot = NewOperator();

        var outcome = robot.Run(new PlaceCommand(-1, 0, Direction.North));

        Assert.Equal(new Outcome.Warning("PLACE -1,0,NORTH ignored - position is off the table"), outcome);
        Assert.Null(robot.CurrentState);
    }

    [Fact]
    public void Place_with_the_largest_whole_number_is_ignored()
    {
        var robot = NewOperator();

        var outcome = robot.Run(new PlaceCommand(2147483647, 0, Direction.North));

        Assert.Equal(
            new Outcome.Warning("PLACE 2147483647,0,NORTH ignored - position is off the table"),
            outcome);
    }

    [Fact]
    public void Place_on_a_2_by_2_table_follows_the_table_size()
    {
        var robot = new RobotOperator(new Table(2, 2));

        var off = robot.Run(new PlaceCommand(2, 2, Direction.North));
        var on = robot.Run(new PlaceCommand(1, 1, Direction.North));

        Assert.Equal(new Outcome.Warning("PLACE 2,2,NORTH ignored - position is off the table"), off);
        Assert.Equal(new Outcome.Done(), on);
    }

    // ---- User Story 2 (FR-03, FR-04, FR-05; DEC-02, DEC-13, DEC-16, DEC-17, DEC-22) ----

    private static RobotOperator PlacedAt(int x, int y, Direction facing, Table? table = null)
    {
        var robot = new RobotOperator(table ?? new Table(5, 5));
        robot.Run(new PlaceCommand(x, y, facing));
        return robot;
    }

    [Theory]
    [InlineData("NORTH", 2, 3)]
    [InlineData("SOUTH", 2, 1)]
    [InlineData("EAST", 3, 2)]
    [InlineData("WEST", 1, 2)]
    public void Move_goes_one_step_in_the_facing_direction(string facingName, int x, int y)
    {
        var facing = Direction.All.Single(d => d.Name == facingName);
        var robot = PlacedAt(2, 2, facing);

        var outcome = robot.Run(new MoveCommand());

        Assert.Equal(new Outcome.Done(), outcome);
        Assert.Equal(new RobotState(x, y, facing), robot.CurrentState);
    }

    [Theory]
    [InlineData("2,4,NORTH")]
    [InlineData("2,0,SOUTH")]
    [InlineData("4,2,EAST")]
    [InlineData("0,2,WEST")]
    [InlineData("0,0,SOUTH")]
    [InlineData("0,0,WEST")]
    [InlineData("0,4,NORTH")]
    [InlineData("0,4,WEST")]
    [InlineData("4,4,NORTH")]
    [InlineData("4,4,EAST")]
    [InlineData("4,0,SOUTH")]
    [InlineData("4,0,EAST")]
    public void Move_that_would_fall_off_the_table_is_ignored(string start)
    {
        var parts = start.Split(',');
        var facing = Direction.All.Single(d => d.Name == parts[2]);
        var startState = new RobotState(int.Parse(parts[0]), int.Parse(parts[1]), facing);
        var robot = PlacedAt(startState.X, startState.Y, facing);

        var outcome = robot.Run(new MoveCommand());

        Assert.Equal(
            new Outcome.Warning($"MOVE ignored - next position is off the table so robot stays at {start}"),
            outcome);
        Assert.Equal(startState, robot.CurrentState);
    }

    [Fact]
    public void Further_valid_movement_is_still_allowed()
    {
        var robot = PlacedAt(4, 2, Direction.East);

        var blocked = robot.Run(new MoveCommand());
        robot.Run(new LeftCommand());
        var moved = robot.Run(new MoveCommand());

        Assert.IsType<Outcome.Warning>(blocked);
        Assert.Equal(new Outcome.Done(), moved);
        Assert.Equal(new RobotState(4, 3, Direction.North), robot.CurrentState);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 0)]
    [InlineData(0, 4)]
    [InlineData(4, 4)]
    public void Left_and_right_keep_the_position_and_return_Done_at_every_corner(int x, int y)
    {
        var robot = PlacedAt(x, y, Direction.North);

        Assert.Equal(new Outcome.Done(), robot.Run(new LeftCommand()));
        Assert.Equal(new RobotState(x, y, Direction.West), robot.CurrentState);
        Assert.Equal(new Outcome.Done(), robot.Run(new RightCommand()));
        Assert.Equal(new RobotState(x, y, Direction.North), robot.CurrentState);
        Assert.Equal(new Outcome.Done(), robot.Run(new RightCommand()));
        Assert.Equal(new RobotState(x, y, Direction.East), robot.CurrentState);
    }

    [Fact]
    public void Move_on_a_2_by_2_table_is_refused_at_the_far_corner()
    {
        var robot = PlacedAt(1, 1, Direction.North, new Table(2, 2));

        var moved = robot.Run(new MoveCommand());
        var reported = robot.Run(new ReportCommand());

        Assert.Equal(
            new Outcome.Warning("MOVE ignored - next position is off the table so robot stays at 1,1,NORTH"),
            moved);
        Assert.Equal(new Outcome.PositionText("1,1,NORTH"), reported);
    }

    // ---- User Story 3 (FR-07; DEC-19, DEC-27) ----

    [Fact]
    public void Robot_not_on_table_ignores_MOVE()
    {
        var robot = NewOperator();

        var outcome = robot.Run(new MoveCommand());

        Assert.Equal(new Outcome.Warning("MOVE ignored - robot is not on the table"), outcome);
        Assert.Null(robot.CurrentState);
    }

    [Fact]
    public void Robot_not_on_table_ignores_LEFT()
    {
        var robot = NewOperator();

        var outcome = robot.Run(new LeftCommand());

        Assert.Equal(new Outcome.Warning("LEFT ignored - robot is not on the table"), outcome);
        Assert.Null(robot.CurrentState);
    }

    [Fact]
    public void Robot_not_on_table_ignores_RIGHT()
    {
        var robot = NewOperator();

        var outcome = robot.Run(new RightCommand());

        Assert.Equal(new Outcome.Warning("RIGHT ignored - robot is not on the table"), outcome);
        Assert.Null(robot.CurrentState);
    }

    [Fact]
    public void Robot_not_on_table_ignores_REPORT()
    {
        var robot = NewOperator();

        var outcome = robot.Run(new ReportCommand());

        Assert.Equal(new Outcome.Warning("REPORT ignored - robot is not on the table"), outcome);
        Assert.Null(robot.CurrentState);
    }

    [Fact]
    public void A_command_that_needs_the_robot_on_the_table_is_not_run_when_the_robot_is_not_placed()
    {
        var robot = NewOperator();
        var spy = new SpyCommand("MOVE", true, new Outcome.Done());

        var outcome = robot.Run(spy);

        Assert.Equal(new Outcome.Warning("MOVE ignored - robot is not on the table"), outcome);
        Assert.Equal(0, spy.ExecuteCount);
    }

    [Fact]
    public void A_command_that_does_not_need_the_robot_on_the_table_is_always_run()
    {
        var robot = NewOperator();
        var spy = new SpyCommand("PLACE", false, new Outcome.Done());

        var outcome = robot.Run(spy);

        Assert.Equal(1, spy.ExecuteCount);
        Assert.Equal(new Outcome.Done(), outcome);
    }

    [Fact]
    public void A_command_that_needs_the_robot_on_the_table_is_run_after_a_valid_PLACE()
    {
        var robot = PlacedAt(0, 0, Direction.North);
        var spy = new SpyCommand("MOVE", true, new Outcome.Done());

        robot.Run(spy);

        Assert.Equal(1, spy.ExecuteCount);
    }

    [Fact]
    public void After_the_ignored_commands_a_valid_PLACE_and_REPORT_work()
    {
        var robot = NewOperator();
        robot.Run(new MoveCommand());
        robot.Run(new LeftCommand());
        robot.Run(new RightCommand());
        robot.Run(new ReportCommand());

        var placed = robot.Run(new PlaceCommand(0, 0, Direction.North));
        var reported = robot.Run(new ReportCommand());

        Assert.Equal(new Outcome.Done(), placed);
        Assert.Equal(new Outcome.PositionText("0,0,NORTH"), reported);
    }
}
