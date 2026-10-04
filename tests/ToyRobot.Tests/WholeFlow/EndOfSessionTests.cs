using static ToyRobot.Tests.WholeFlow.SessionHarness;

namespace ToyRobot.Tests.WholeFlow;

// Ending a session with EXIT: FR-15, FR-14; DEC-10, DEC-25, DEC-30.
public class EndOfSessionTests
{
    [Fact]
    public void EXIT_with_text_after_it_ends_session()
    {
        var (stdout, stderr) = Run(Lines("exit now", "REPORT"));

        Assert.Equal("", stdout);
        Assert.Equal(Lines("> exit now"), stderr);
    }

    [Fact]
    public void EXIT_mid_file_stops_later_lines()
    {
        var (stdout, stderr) = Run(Lines("PLACE 0,0,NORTH", "REPORT", "EXIT", "MOVE", "REPORT"));

        Assert.Equal(Lines("Output:", "0,0,NORTH"), stdout);
        Assert.Equal(Lines("> PLACE 0,0,NORTH", "> REPORT", "> EXIT"), stderr);
    }

    [Theory]
    [InlineData("EXIT")]
    [InlineData("EXIT now")]
    [InlineData("Exit")]
    [InlineData("exit")]
    public void EXIT_before_PLACE_ends_session(string line)
    {
        var (stdout, stderr) = Run(Lines(line, "PLACE 0,0,NORTH", "REPORT"));

        Assert.Equal("", stdout);
        Assert.Equal(Lines("> " + line), stderr);
    }

    [Theory]
    [InlineData("EXITNOW")]
    [InlineData("EXITING")]
    public void EXITNOW_and_EXITING_are_unknown_commands_and_session_continues(string row)
    {
        var (stdout, stderr) = Run(Lines(row, "REPORT"));

        Assert.Equal("", stdout);
        Assert.Equal(
            Lines(
                "> " + row,
                $"Warning: '{row}' ignored - unknown command",
                "> REPORT",
                "Warning: REPORT ignored - robot is not on the table"),
            stderr);
    }

    [Fact]
    public void Echo_includes_trimmed_line_blank_line_and_EXIT_but_not_lines_after_EXIT()
    {
        var (stdout, stderr) = Run(Lines("  MOVE  ", "", "EXIT", "REPORT"));

        Assert.Equal("", stdout);
        Assert.Equal(
            Lines("> MOVE", "Warning: MOVE ignored - robot is not on the table", "> ", "Warning: blank line ignored", "> EXIT"),
            stderr);
    }
}
