namespace ToyRobot.EndToEndTests;

// E2E-06: FR-12, FR-08; DEC-04, DEC-05, DEC-23.
public class OutputChannelTests
{
    [Fact]
    public void Only_REPORT_result_on_console_output()
    {
        var result = AppProcess.Run(AppProcess.Utf8("MOVE", "PLACE 0,0,NORTH", "REPORT"));

        Assert.Equal(new[] { "Output:", "0,0,NORTH" }, LineComparer.Lines(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void Line_with_two_commands_gives_warning_and_exit_code_0()
    {
        var result = AppProcess.Run(AppProcess.Utf8("PLACE 0,0,NORTH MOVE"));

        Assert.Empty(LineComparer.Lines(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }

    // E2E-06, NFR-01 part: NFR-01; DEC-32.
    [Fact]
    public void Bad_lines_never_stop_the_application()
    {
        var result = AppProcess.Run(AppProcess.Utf8("PLACE 99999999999,0,NORTH", "HELLO", "", "PLACE 0,0,NORTH", "REPORT"));

        Assert.Equal(new[] { "Output:", "0,0,NORTH" }, LineComparer.Lines(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void Every_kind_of_ignored_line_then_valid_commands_end_to_end()
    {
        var result = AppProcess.Run(AppProcess.Utf8(
            "HELLO",
            "",
            "PLACE 1,2",
            "PLACE A,0,NORTH",
            "PLACE 0,B,NORTH",
            "PLACE 1,2,UP",
            "move now",
            "PLACE 99999999999,0,NORTH",
            "> MOVE",
            "MOVE",
            "PLACE 9,9,NORTH",
            "PLACE 0,0,SOUTH",
            "MOVE",
            "REPORT"));

        Assert.Equal(new[] { "Output:", "0,0,SOUTH" }, LineComparer.Lines(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }
}
