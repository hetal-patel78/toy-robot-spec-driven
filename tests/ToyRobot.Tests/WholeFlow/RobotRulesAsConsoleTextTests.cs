using static ToyRobot.Tests.WholeFlow.SessionHarness;

namespace ToyRobot.Tests.WholeFlow;

// WF-01: every acceptance scenario of spec 001 as console text, on a 5 x 5 table with redirected input
// (NFR-05, NFR-06; FR-01 to FR-07, FR-09, FR-10; DEC-31). Scenario numbers follow specs/001-robot-rules/spec.md.
public class RobotRulesAsConsoleTextTests
{
    private static void Expect(string[] input, string[] stdout, string[] stderr)
    {
        var result = Run(Lines(input));

        Assert.Equal(Lines(stdout), result.Stdout);
        Assert.Equal(Lines(stderr), result.Stderr);
    }

    private static string[] Echo(params string[] lines) => lines.Select(line => "> " + line).ToArray();

    private static string[] Text(params string[] lines) => lines;

    // ---- User Story 1: place the robot and read its position ----

    [Fact]
    public void US1_1_Place_then_report()
    {
        Expect(
            Text("PLACE 4,4,NORTH", "REPORT"),
            Text("Output:", "4,4,NORTH"),
            Text("> PLACE 4,4,NORTH", "> REPORT"));
    }

    [Fact]
    public void US1_2_Place_off_the_table_is_ignored_and_robot_stays()
    {
        Expect(
            Text("PLACE 4,2,NORTH", "PLACE 9,9,NORTH", "REPORT"),
            Text("Output:", "4,2,NORTH"),
            Text(
                "> PLACE 4,2,NORTH",
                "> PLACE 9,9,NORTH",
                "Warning: PLACE 9,9,NORTH ignored - position is off the table",
                "> REPORT"));
    }

    [Fact]
    public void US1_3_Lower_case_input_is_reported_in_upper_case()
    {
        Expect(
            Text("place 1,2,north", "report"),
            Text("Output:", "1,2,NORTH"),
            Text("> place 1,2,north", "> report"));
    }

    [Fact]
    public void US1_4_Invalid_first_PLACE_then_REPORT_robot_not_on_table()
    {
        Expect(
            Text("PLACE 9,9,NORTH", "REPORT"),
            Text(),
            Text(
                "> PLACE 9,9,NORTH",
                "Warning: PLACE 9,9,NORTH ignored - position is off the table",
                "> REPORT",
                "Warning: REPORT ignored - robot is not on the table"));
    }

    [Fact]
    public void US1_5_Second_valid_PLACE_moves_the_robot()
    {
        Expect(
            Text("PLACE 4,2,NORTH", "PLACE 1,1,EAST", "REPORT"),
            Text("Output:", "1,1,EAST"),
            Text("> PLACE 4,2,NORTH", "> PLACE 1,1,EAST", "> REPORT"));
    }

    [Fact]
    public void US1_6_Negative_position_is_ignored()
    {
        Expect(
            Text("PLACE -1,0,NORTH"),
            Text(),
            Text("> PLACE -1,0,NORTH", "Warning: PLACE -1,0,NORTH ignored - position is off the table"));
    }

    [Fact]
    public void US1_7_Off_table_warning_shows_the_parsed_values()
    {
        Expect(
            Text("place 09 , +9 , north"),
            Text(),
            Text("> place 09 , +9 , north", "Warning: PLACE 9,9,NORTH ignored - position is off the table"));
    }

    [Fact]
    public void US1_8_Several_REPORTs_in_one_run()
    {
        Expect(
            Text("PLACE 0,0,NORTH", "REPORT", "MOVE", "REPORT"),
            Text("Output:", "0,0,NORTH", "Output:", "0,1,NORTH"),
            Text("> PLACE 0,0,NORTH", "> REPORT", "> MOVE", "> REPORT"));
    }

    // ---- User Story 2: move and turn without falling off ----

    [Fact]
    public void US2_1_Challenge_Example_1()
    {
        Expect(
            Text("PLACE 0,0,NORTH", "MOVE", "REPORT"),
            Text("Output:", "0,1,NORTH"),
            Text("> PLACE 0,0,NORTH", "> MOVE", "> REPORT"));
    }

    [Fact]
    public void US2_2_Move_off_the_south_edge_is_refused()
    {
        Expect(
            Text("PLACE 0,0,SOUTH", "MOVE", "REPORT"),
            Text("Output:", "0,0,SOUTH"),
            Text(
                "> PLACE 0,0,SOUTH",
                "> MOVE",
                "Warning: MOVE ignored - next position is off the table so robot stays at 0,0,SOUTH",
                "> REPORT"));
    }

    [Fact]
    public void US2_3_Challenge_Example_2()
    {
        Expect(
            Text("PLACE 0,0,NORTH", "LEFT", "REPORT"),
            Text("Output:", "0,0,WEST"),
            Text("> PLACE 0,0,NORTH", "> LEFT", "> REPORT"));
    }

    [Fact]
    public void US2_4_Challenge_Example_3()
    {
        Expect(
            Text("PLACE 1,2,EAST", "MOVE", "MOVE", "LEFT", "MOVE", "REPORT"),
            Text("Output:", "3,3,NORTH"),
            Text("> PLACE 1,2,EAST", "> MOVE", "> MOVE", "> LEFT", "> MOVE", "> REPORT"));
    }

    [Fact]
    public void US2_5_Brainstorm_trace_ending_at_2_1_NORTH()
    {
        Expect(
            Text("PLACE 0,0,EAST", "MOVE", "MOVE", "LEFT", "MOVE", "REPORT"),
            Text("Output:", "2,1,NORTH"),
            Text("> PLACE 0,0,EAST", "> MOVE", "> MOVE", "> LEFT", "> MOVE", "> REPORT"));
    }

    [Fact]
    public void US2_6_Blocked_move_then_turn_and_move()
    {
        Expect(
            Text("PLACE 4,2,SOUTH", "MOVE", "LEFT", "MOVE", "REPORT"),
            Text("Output:", "4,1,EAST"),
            Text(
                "> PLACE 4,2,SOUTH",
                "> MOVE",
                "> LEFT",
                "> MOVE",
                "Warning: MOVE ignored - next position is off the table so robot stays at 4,1,EAST",
                "> REPORT"));
    }

    [Fact]
    public void US2_7_Brainstorm_trace_ending_at_0_3_WEST()
    {
        Expect(
            Text("PLACE 3,4,WEST", "MOVE", "MOVE", "LEFT", "MOVE", "RIGHT", "MOVE", "REPORT"),
            Text("Output:", "0,3,WEST"),
            Text("> PLACE 3,4,WEST", "> MOVE", "> MOVE", "> LEFT", "> MOVE", "> RIGHT", "> MOVE", "> REPORT"));
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
    public void US2_8_Move_off_an_edge_or_corner_is_refused(string start)
    {
        Expect(
            Text("PLACE " + start, "MOVE", "REPORT"),
            Text("Output:", start),
            Text(
                "> PLACE " + start,
                "> MOVE",
                "Warning: MOVE ignored - next position is off the table so robot stays at " + start,
                "> REPORT"));
    }

    [Fact]
    public void US2_9_Refused_move_leaves_the_robot_able_to_turn_and_move()
    {
        Expect(
            Text("PLACE 4,2,EAST", "MOVE", "LEFT", "MOVE", "REPORT"),
            Text("Output:", "4,3,NORTH"),
            Text(
                "> PLACE 4,2,EAST",
                "> MOVE",
                "Warning: MOVE ignored - next position is off the table so robot stays at 4,2,EAST",
                "> LEFT",
                "> MOVE",
                "> REPORT"));
    }

    [Fact]
    public void US2_10_Full_LEFT_cycle()
    {
        var input = Text("PLACE 2,2,NORTH", "LEFT", "REPORT", "LEFT", "REPORT", "LEFT", "REPORT", "LEFT", "REPORT");

        Expect(
            input,
            Text("Output:", "2,2,WEST", "Output:", "2,2,SOUTH", "Output:", "2,2,EAST", "Output:", "2,2,NORTH"),
            Echo(input));
    }

    [Fact]
    public void US2_11_Full_RIGHT_cycle()
    {
        var input = Text("PLACE 2,2,NORTH", "RIGHT", "REPORT", "RIGHT", "REPORT", "RIGHT", "REPORT", "RIGHT", "REPORT");

        Expect(
            input,
            Text("Output:", "2,2,EAST", "Output:", "2,2,SOUTH", "Output:", "2,2,WEST", "Output:", "2,2,NORTH"),
            Echo(input));
    }

    // ---- User Story 3: robot commands before the robot is on the table ----

    [Fact]
    public void US3_1_Robot_not_on_table_ignores_MOVE()
    {
        Expect(
            Text("MOVE", "PLACE 0,0,NORTH", "REPORT"),
            Text("Output:", "0,0,NORTH"),
            Text("> MOVE", "Warning: MOVE ignored - robot is not on the table", "> PLACE 0,0,NORTH", "> REPORT"));
    }

    [Fact]
    public void US3_2_Robot_not_on_table_ignores_MOVE_LEFT_RIGHT_and_REPORT()
    {
        Expect(
            Text("MOVE", "LEFT", "RIGHT", "REPORT"),
            Text(),
            Text(
                "> MOVE",
                "Warning: MOVE ignored - robot is not on the table",
                "> LEFT",
                "Warning: LEFT ignored - robot is not on the table",
                "> RIGHT",
                "Warning: RIGHT ignored - robot is not on the table",
                "> REPORT",
                "Warning: REPORT ignored - robot is not on the table"));
    }

    [Fact]
    public void US3_3_Lower_case_move_names_the_command_in_upper_case()
    {
        Expect(
            Text("move"),
            Text(),
            Text("> move", "Warning: MOVE ignored - robot is not on the table"));
    }

    // ---- User Story 4: lines are read the same way however they are typed, and bad lines are refused ----

    [Fact]
    public void US4_1_Spaces_around_the_PLACE_values_and_lower_case()
    {
        Expect(
            Text("place 0 , 0 , north", "REPORT"),
            Text("Output:", "0,0,NORTH"),
            Text("> place 0 , 0 , north", "> REPORT"));
    }

    [Fact]
    public void US4_2_Bad_PLACE_does_not_change_the_robot()
    {
        Expect(
            Text("PLACE 1,0,EAST", "PLACE 1,2", "PLACE 3,2,NORTH", "REPORT"),
            Text("Output:", "3,2,NORTH"),
            Text(
                "> PLACE 1,0,EAST",
                "> PLACE 1,2",
                "Warning: 'PLACE 1,2' ignored - expected PLACE X,Y,F",
                "> PLACE 3,2,NORTH",
                "> REPORT"));
    }

    [Theory]
    [InlineData("HELLO", "'HELLO' ignored - unknown command")]
    [InlineData("> MOVE", "'> MOVE' ignored - unknown command")]
    [InlineData("EXITNOW", "'EXITNOW' ignored - unknown command")]
    [InlineData("PLACE0,0,NORTH", "'PLACE0,0,NORTH' ignored - unknown command")]
    [InlineData("PLACEHOLDER", "'PLACEHOLDER' ignored - unknown command")]
    [InlineData("PLACE,1,2", "'PLACE,1,2' ignored - unknown command")]
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
    public void US4_3_One_bad_line_gives_the_echo_and_one_warning(string line, string warning)
    {
        Expect(Text(line), Text(), Text("> " + line, "Warning: " + warning));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t ")]
    public void US4_3_A_blank_line_gives_the_echo_and_one_warning(string line)
    {
        Expect(Text(line), Text(), Text("> ", "Warning: blank line ignored"));
    }

    [Fact]
    public void US4_4_Spaces_around_MOVE_are_removed()
    {
        Expect(
            Text("PLACE 0,0,NORTH", "   MOVE   ", "REPORT"),
            Text("Output:", "0,1,NORTH"),
            Text("> PLACE 0,0,NORTH", "> MOVE", "> REPORT"));
    }

    [Fact]
    public void US4_4_Many_spaces_after_PLACE()
    {
        Expect(
            Text("PLACE     0,0,NORTH", "REPORT"),
            Text("Output:", "0,0,NORTH"),
            Text("> PLACE     0,0,NORTH", "> REPORT"));
    }

    [Fact]
    public void US4_4_Spaces_around_the_values_and_lower_case()
    {
        Expect(
            Text("place 0 , 0 , north", "REPORT"),
            Text("Output:", "0,0,NORTH"),
            Text("> place 0 , 0 , north", "> REPORT"));
    }

    [Fact]
    public void US4_4_Leading_zero_and_plus_sign_in_PLACE_values()
    {
        Expect(
            Text("PLACE 01,+1,EAST", "REPORT"),
            Text("Output:", "1,1,EAST"),
            Text("> PLACE 01,+1,EAST", "> REPORT"));
    }

    [Fact]
    public void US4_5_Tabs_and_non_breaking_spaces_count_as_whitespace()
    {
        Expect(
            Text("\tplace 0, 0\t,north ", "REPORT"),
            Text("Output:", "0,0,NORTH"),
            Text("> place 0, 0\t,north", "> REPORT"));
    }

    [Fact]
    public void US4_6_Mixed_case_commands_and_REPORT_in_upper_case()
    {
        Expect(
            Text("Place 1,1,East", "mOvE", "rEpOrT"),
            Text("Output:", "2,1,EAST"),
            Text("> Place 1,1,East", "> mOvE", "> rEpOrT"));
    }

    [Theory]
    [InlineData("PLACE A,B", "'PLACE A,B' ignored - expected PLACE X,Y,F")]
    [InlineData("PLACE 1,2,3,4", "'PLACE 1,2,3,4' ignored - expected PLACE X,Y,F")]
    [InlineData("PLACE", "'PLACE' ignored - expected PLACE X,Y,F")]
    [InlineData("PLACE A,B,UP", "'PLACE A,B,UP' ignored - X must be a whole number")]
    [InlineData("PLACE 0,B,UP", "'PLACE 0,B,UP' ignored - Y must be a whole number")]
    [InlineData("PLACE 9999999999999999999,111111111111111111111111111,east", "'PLACE 9999999999999999999,111111111111111111111111111,east' ignored - X is too large")]
    [InlineData("PLACE 1,111111111111111111111111111,east", "'PLACE 1,111111111111111111111111111,east' ignored - Y is too large")]
    public void US4_7_Only_the_first_problem_is_reported(string line, string warning)
    {
        Expect(Text(line), Text(), Text("> " + line, "Warning: " + warning));
    }
}
