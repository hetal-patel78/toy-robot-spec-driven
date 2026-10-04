namespace ToyRobot.EndToEndTests;

// E2E-03, E2E-04: FR-15, FR-16, NFR-01; DEC-10, DEC-32.
public class SessionEndTests
{
    [Fact]
    public void Empty_input_gives_no_output_and_exit_code_0()
    {
        var result = AppProcess.Run(Array.Empty<byte>());

        Assert.Equal("", result.Stdout);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void Exit_code_0_at_end_of_input()
    {
        var result = AppProcess.Run(AppProcess.Utf8("PLACE 0,0,NORTH", "MOVE", "REPORT"));

        Assert.Equal(new[] { "Output:", "0,1,NORTH" }, LineComparer.Lines(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void Exit_code_0_after_EXIT()
    {
        var result = AppProcess.Run(AppProcess.Utf8("EXIT", "PLACE 0,0,NORTH", "REPORT"));

        Assert.Empty(LineComparer.Lines(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void EXIT_mid_file_later_lines_not_run()
    {
        var result = AppProcess.Run(AppProcess.Utf8("PLACE 0,0,NORTH", "REPORT", "EXIT", "MOVE", "REPORT"));

        Assert.Equal(new[] { "Output:", "0,0,NORTH" }, LineComparer.Lines(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }
}
