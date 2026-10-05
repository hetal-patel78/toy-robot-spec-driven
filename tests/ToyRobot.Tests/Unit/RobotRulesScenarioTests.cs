using ToyRobot.Tests.Unit.Support;

namespace ToyRobot.Tests.Unit;

// Acceptance scenarios of spec 001, run through the driver on a 5 x 5 table unless stated.
public class RobotRulesScenarioTests
{
    private static RulesDriver Run(params string[] lines)
    {
        var driver = new RulesDriver(new Table(5, 5));
        driver.Run(lines);
        return driver;
    }

    // ---- User Story 1 ----

    [Fact]
    public void US1_1_Place_then_report()
    {
        var driver = Run("PLACE 4,4,NORTH", "REPORT");

        Assert.Equal(new[] { "4,4,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US1_2_A_later_PLACE_off_the_table_is_ignored()
    {
        var driver = Run("PLACE 4,2,NORTH", "PLACE 9,9,NORTH", "REPORT");

        Assert.Equal(new[] { "4,2,NORTH" }, driver.Positions);
        Assert.Equal(new[] { "PLACE 9,9,NORTH ignored - position is off the table" }, driver.Warnings);
    }

    [Fact]
    public void US1_3_Lower_case_input_is_reported_in_upper_case()
    {
        var driver = Run("place 1,2,north", "report");

        Assert.Equal(new[] { "1,2,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US1_5_A_second_valid_PLACE_moves_the_robot()
    {
        var driver = Run("PLACE 4,2,NORTH", "PLACE 1,1,EAST", "REPORT");

        Assert.Equal(new[] { "1,1,EAST" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US1_6_A_negative_position_is_off_the_table()
    {
        var driver = Run("PLACE -1,0,NORTH");

        Assert.Empty(driver.Positions);
        Assert.Equal(new[] { "PLACE -1,0,NORTH ignored - position is off the table" }, driver.Warnings);
        Assert.Null(driver.FinalState);
    }

    [Fact]
    public void US1_7_Padded_values_are_read_as_whole_numbers()
    {
        var driver = Run("place 09 , +9 , north");

        Assert.Empty(driver.Positions);
        Assert.Equal(new[] { "PLACE 9,9,NORTH ignored - position is off the table" }, driver.Warnings);
    }

    // ---- User Story 2 and User Story 1 scenarios 8 and 9 ----

    private const string BlockedAt = "MOVE ignored - next position is off the table so robot stays at ";

    [Fact]
    public void US2_1_Challenge_Example_1()
    {
        var driver = Run("PLACE 0,0,NORTH", "MOVE", "REPORT");

        Assert.Equal(new[] { "0,1,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US2_2_Move_south_from_the_bottom_edge_is_ignored()
    {
        var driver = Run("PLACE 0,0,SOUTH", "MOVE", "REPORT");

        Assert.Equal(new[] { "0,0,SOUTH" }, driver.Positions);
        Assert.Equal(new[] { BlockedAt + "0,0,SOUTH" }, driver.Warnings);
    }

    [Fact]
    public void US2_3_Challenge_Example_2()
    {
        var driver = Run("PLACE 0,0,NORTH", "LEFT", "REPORT");

        Assert.Equal(new[] { "0,0,WEST" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US2_4_Challenge_Example_3()
    {
        var driver = Run("PLACE 1,2,EAST", "MOVE", "MOVE", "LEFT", "MOVE", "REPORT");

        Assert.Equal(new[] { "3,3,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US2_5_Hand_traced_example_ending_at_2_1_NORTH()
    {
        var driver = Run("PLACE 0,0,EAST", "MOVE", "MOVE", "LEFT", "MOVE", "REPORT");

        Assert.Equal(new[] { "2,1,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US2_6_Hand_traced_blocked_move_from_page_3()
    {
        var driver = Run("PLACE 4,2,SOUTH", "MOVE", "LEFT", "MOVE", "REPORT");

        Assert.Equal(new[] { "4,1,EAST" }, driver.Positions);
        Assert.Equal(new[] { BlockedAt + "4,1,EAST" }, driver.Warnings);
    }

    [Fact]
    public void US2_7_Hand_traced_example_ending_at_0_3_WEST()
    {
        var driver = Run("PLACE 3,4,WEST", "MOVE", "MOVE", "LEFT", "MOVE", "RIGHT", "MOVE", "REPORT");

        Assert.Equal(new[] { "0,3,WEST" }, driver.Positions);
        Assert.Empty(driver.Warnings);
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
    public void US2_8_Move_that_would_fall_off_the_table_is_ignored_at_every_edge_and_corner(string start)
    {
        var driver = Run($"PLACE {start}", "MOVE", "REPORT");

        Assert.Equal(new[] { start }, driver.Positions);
        Assert.Equal(new[] { BlockedAt + start }, driver.Warnings);
    }

    [Fact]
    public void US2_9_Valid_movement_still_works_after_a_refused_move()
    {
        var driver = Run("PLACE 4,2,EAST", "MOVE", "LEFT", "MOVE", "REPORT");

        Assert.Equal(new[] { "4,3,NORTH" }, driver.Positions);
        Assert.Equal(new[] { BlockedAt + "4,2,EAST" }, driver.Warnings);
    }

    [Fact]
    public void US2_10_Four_LEFT_turns_make_a_full_cycle()
    {
        var driver = Run("PLACE 2,2,NORTH", "LEFT", "REPORT", "LEFT", "REPORT", "LEFT", "REPORT", "LEFT", "REPORT");

        Assert.Equal(new[] { "2,2,WEST", "2,2,SOUTH", "2,2,EAST", "2,2,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US2_11_Four_RIGHT_turns_make_a_full_cycle()
    {
        var driver = Run("PLACE 2,2,NORTH", "RIGHT", "REPORT", "RIGHT", "REPORT", "RIGHT", "REPORT", "RIGHT", "REPORT");

        Assert.Equal(new[] { "2,2,EAST", "2,2,SOUTH", "2,2,WEST", "2,2,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US1_8_Several_REPORTs_in_one_run()
    {
        var driver = Run("PLACE 0,0,NORTH", "REPORT", "MOVE", "REPORT");

        Assert.Equal(new[] { "0,0,NORTH", "0,1,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US1_9_A_2_by_2_table_refuses_positions_beyond_its_size()
    {
        var driver = new RulesDriver(new Table(2, 2));

        driver.Run("PLACE 1,1,NORTH", "MOVE", "REPORT", "PLACE 2,2,NORTH");

        Assert.Equal(new[] { "1,1,NORTH" }, driver.Positions);
        Assert.Equal(
            new[]
            {
                BlockedAt + "1,1,NORTH",
                "PLACE 2,2,NORTH ignored - position is off the table",
            },
            driver.Warnings);
    }

    // ---- User Story 3 and User Story 1 scenario 4 ----

    [Fact]
    public void US3_1_A_command_before_PLACE_is_ignored_and_the_run_continues()
    {
        var driver = Run("MOVE", "PLACE 0,0,NORTH", "REPORT");

        Assert.Equal(new[] { "0,0,NORTH" }, driver.Positions);
        Assert.Equal(new[] { "MOVE ignored - robot is not on the table" }, driver.Warnings);
    }

    [Fact]
    public void US3_2_Robot_not_on_table_ignores_MOVE_LEFT_RIGHT_and_REPORT()
    {
        var driver = Run("MOVE", "LEFT", "RIGHT", "REPORT");

        Assert.Empty(driver.Positions);
        Assert.Equal(
            new[]
            {
                "MOVE ignored - robot is not on the table",
                "LEFT ignored - robot is not on the table",
                "RIGHT ignored - robot is not on the table",
                "REPORT ignored - robot is not on the table",
            },
            driver.Warnings);
    }

    [Fact]
    public void US3_3_A_lower_case_command_before_PLACE_is_named_in_upper_case()
    {
        var driver = Run("move");

        Assert.Empty(driver.Positions);
        Assert.Equal(new[] { "MOVE ignored - robot is not on the table" }, driver.Warnings);
    }

    [Fact]
    public void US1_4_An_invalid_first_PLACE_leaves_the_robot_off_the_table()
    {
        var driver = Run("PLACE 9,9,NORTH", "REPORT");

        Assert.Empty(driver.Positions);
        Assert.Equal(
            new[]
            {
                "PLACE 9,9,NORTH ignored - position is off the table",
                "REPORT ignored - robot is not on the table",
            },
            driver.Warnings);
    }

    // ---- User Story 4 ----

    [Fact]
    public void US4_1_Spaces_around_the_PLACE_values_are_ignored()
    {
        var driver = Run("place 0 , 0 , north", "REPORT");

        Assert.Equal(new[] { "0,0,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US4_2_Processing_continues_after_a_bad_line()
    {
        var driver = Run("PLACE 1,0,EAST", "PLACE 1,2", "PLACE 3,2,NORTH", "REPORT");

        Assert.Equal(new[] { "3,2,NORTH" }, driver.Positions);
        Assert.Equal(new[] { "'PLACE 1,2' ignored - expected PLACE X,Y,F" }, driver.Warnings);
    }

    [Theory]
    [InlineData("HELLO", "'HELLO' ignored - unknown command")]
    [InlineData("> MOVE", "'> MOVE' ignored - unknown command")]
    [InlineData("EXITNOW", "'EXITNOW' ignored - unknown command")]
    [InlineData("PLACE0,0,NORTH", "'PLACE0,0,NORTH' ignored - unknown command")]
    [InlineData("PLACEHOLDER", "'PLACEHOLDER' ignored - unknown command")]
    [InlineData("PLACE,1,2", "'PLACE,1,2' ignored - unknown command")]
    [InlineData("", "blank line ignored")]
    [InlineData("  \t  ", "blank line ignored")]
    [InlineData("PLACE 1,2", "'PLACE 1,2' ignored - expected PLACE X,Y,F")]
    [InlineData("PLACE 0 0 NORTH", "'PLACE 0 0 NORTH' ignored - expected PLACE X,Y,F")]
    [InlineData("PLACE A,0,NORTH", "'PLACE A,0,NORTH' ignored - X must be a whole number")]
    [InlineData("PLACE 9999999999,0,NORTH", "'PLACE 9999999999,0,NORTH' ignored - X is too large")]
    [InlineData("PLACE 99999999999,0,NORTH", "'PLACE 99999999999,0,NORTH' ignored - X is too large")]
    [InlineData("PLACE -99999999999,0,NORTH", "'PLACE -99999999999,0,NORTH' ignored - X is too large")]
    [InlineData("PLACE 1 0,0,NORTH", "'PLACE 1 0,0,NORTH' ignored - X must be a whole number")]
    [InlineData("PLACE 0,B,NORTH", "'PLACE 0,B,NORTH' ignored - Y must be a whole number")]
    [InlineData("PLACE 0,9999999999,NORTH", "'PLACE 0,9999999999,NORTH' ignored - Y is too large")]
    [InlineData("PLACE 0,99999999999,NORTH", "'PLACE 0,99999999999,NORTH' ignored - Y is too large")]
    [InlineData("PLACE 1,2,UP", "'PLACE 1,2,UP' ignored - F must be NORTH, SOUTH, EAST or WEST")]
    [InlineData("PLACE 0,0,N", "'PLACE 0,0,N' ignored - F must be NORTH, SOUTH, EAST or WEST")]
    [InlineData("PLACE 1,2,N", "'PLACE 1,2,N' ignored - F must be NORTH, SOUTH, EAST or WEST")]
    [InlineData("PLACE 1,2,", "'PLACE 1,2,' ignored - F must be NORTH, SOUTH, EAST or WEST")]
    [InlineData("PLACE 1,2,NORTH EXTRA", "'PLACE 1,2,NORTH EXTRA' ignored - F must be NORTH, SOUTH, EAST or WEST")]
    [InlineData("move now", "'move now' ignored - MOVE takes no values")]
    [InlineData("MOVE now", "'MOVE now' ignored - MOVE takes no values")]
    [InlineData("LEFT now", "'LEFT now' ignored - LEFT takes no values")]
    [InlineData("RIGHT now", "'RIGHT now' ignored - RIGHT takes no values")]
    [InlineData("REPORT now", "'REPORT now' ignored - REPORT takes no values")]
    public void US4_3_One_bad_line_gives_one_warning_and_no_position(string line, string reason)
    {
        var driver = Run(line);

        Assert.Empty(driver.Positions);
        Assert.Equal(new[] { reason }, driver.Warnings);
    }

    [Fact]
    public void US4_4_Valid_rows_with_extra_whitespace_around_MOVE()
    {
        var driver = Run("PLACE 0,0,NORTH", "   MOVE   ", "REPORT");

        Assert.Equal(new[] { "0,1,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US4_4_Valid_rows_with_extra_spaces_after_PLACE()
    {
        var driver = Run("PLACE     0,0,NORTH", "REPORT");

        Assert.Equal(new[] { "0,0,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US4_4_Valid_rows_with_spaces_around_the_commas_and_lower_case()
    {
        var driver = Run("place 0 , 0 , north", "REPORT");

        Assert.Equal(new[] { "0,0,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US4_4_Valid_rows_with_leading_zeros_and_a_plus_sign()
    {
        var driver = Run("PLACE 01,+1,EAST", "REPORT");

        Assert.Equal(new[] { "1,1,EAST" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US4_5_Tabs_and_non_breaking_spaces_count_as_whitespace()
    {
        var driver = Run("\tplace 0, 0\t,north ", "REPORT");

        Assert.Equal(new[] { "0,0,NORTH" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void US4_6_Mixed_case_commands_are_read_and_REPORT_is_in_upper_case()
    {
        var driver = Run("Place 1,1,East", "mOvE", "rEpOrT");

        Assert.Equal(new[] { "2,1,EAST" }, driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Theory]
    [InlineData("PLACE A,B", "'PLACE A,B' ignored - expected PLACE X,Y,F")]
    [InlineData("PLACE 1,2,3,4", "'PLACE 1,2,3,4' ignored - expected PLACE X,Y,F")]
    [InlineData("PLACE", "'PLACE' ignored - expected PLACE X,Y,F")]
    [InlineData("PLACE A,B,UP", "'PLACE A,B,UP' ignored - X must be a whole number")]
    [InlineData("PLACE 0,B,UP", "'PLACE 0,B,UP' ignored - Y must be a whole number")]
    [InlineData("PLACE 9999999999999999999,111111111111111111111111111,east", "'PLACE 9999999999999999999,111111111111111111111111111,east' ignored - X is too large")]
    [InlineData("PLACE 1,111111111111111111111111111,east", "'PLACE 1,111111111111111111111111111,east' ignored - Y is too large")]
    public void US4_7_Only_the_first_problem_is_reported(string line, string reason)
    {
        var driver = Run(line);

        Assert.Empty(driver.Positions);
        Assert.Equal(new[] { reason }, driver.Warnings);
    }

    [Fact]
    public void US4_8_EXIT_is_recognised_whatever_follows_it()
    {
        var driver = Run("exit now", "REPORT");

        Assert.Empty(driver.Positions);
        Assert.Empty(driver.Warnings);
    }

    [Fact]
    public void Edge_REPORT_with_a_value_before_PLACE_gives_the_takes_no_values_warning_only()
    {
        var driver = Run("REPORT now");

        Assert.Empty(driver.Positions);
        Assert.Equal(new[] { "'REPORT now' ignored - REPORT takes no values" }, driver.Warnings);
    }

    [Fact]
    public void Edge_PLACE_at_the_largest_whole_number_is_read_and_refused_by_the_table()
    {
        var driver = Run("PLACE 2147483647,0,NORTH");

        Assert.Empty(driver.Positions);
        Assert.Equal(
            new[] { "PLACE 2147483647,0,NORTH ignored - position is off the table" },
            driver.Warnings);
    }

    [Fact]
    public void Edge_The_driver_stops_at_EXIT()
    {
        var driver = Run("EXIT", "PLACE 0,0,NORTH", "REPORT");

        Assert.Empty(driver.Positions);
        Assert.Empty(driver.Warnings);
        Assert.Null(driver.FinalState);
    }
}
