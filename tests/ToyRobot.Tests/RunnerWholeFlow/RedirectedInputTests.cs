using static ToyRobot.Tests.RunnerWholeFlow.SessionHarness;

namespace ToyRobot.Tests.RunnerWholeFlow;

// Redirected console input: FR-08, FR-11, FR-12, FR-14, FR-16; WF-02, WF-03.
public class RedirectedInputTests
{
    [Fact]
    public void Line_with_two_commands_is_one_bad_line()
    {
        var (stdout, stderr) = Run(Lines("PLACE 0,0,NORTH MOVE"));

        Assert.Equal("", stdout);
        Assert.Equal(
            Lines("> PLACE 0,0,NORTH MOVE", "Warning: 'PLACE 0,0,NORTH MOVE' ignored - F must be NORTH, SOUTH, EAST or WEST"),
            stderr);
    }

    [Fact]
    public void Only_REPORT_result_goes_to_console_output()
    {
        var (stdout, stderr) = Run(Lines("MOVE", "PLACE 0,0,NORTH", "REPORT"));

        Assert.Equal(Lines("Output:", "0,0,NORTH"), stdout);
        Assert.Equal(
            Lines("> MOVE", "Warning: MOVE ignored - robot is not on the table", "> PLACE 0,0,NORTH", "> REPORT"),
            stderr);
    }

    [Fact]
    public void Byte_order_mark_at_start_of_input_is_ignored()
    {
        var (stdout, stderr) = Run(Lines("\uFEFFPLACE 0,0,NORTH", "MOVE", "REPORT"));

        Assert.Equal(Lines("Output:", "0,1,NORTH"), stdout);
        Assert.Equal(Lines("> PLACE 0,0,NORTH", "> MOVE", "> REPORT"), stderr);
        Assert.DoesNotContain('\uFEFF', stderr);
    }

    [Fact]
    public void Byte_order_mark_is_removed_only_from_the_first_line()
    {
        var (stdout, stderr) = Run(Lines("PLACE 0,0,NORTH", "\uFEFFREPORT"));

        Assert.Equal("", stdout);
        Assert.Equal(
            Lines("> PLACE 0,0,NORTH", "> \uFEFFREPORT", "Warning: '\uFEFFREPORT' ignored - unknown command"),
            stderr);
    }

    [Fact]
    public void Echo_removes_whitespace_of_any_kind_from_both_ends_only()
    {
        var (stdout, stderr) = Run(Lines("  \tPLACE 0 , 0 , NORTH ", " HELLO there\t", " \t ", "REPORT"));

        Assert.Equal(Lines("Output:", "0,0,NORTH"), stdout);
        Assert.Equal(
            Lines(
                "> PLACE 0 , 0 , NORTH",
                "> HELLO there",
                "Warning: 'HELLO there' ignored - unknown command",
                "> ",
                "Warning: blank line ignored",
                "> REPORT"),
            stderr);
    }

    [Fact]
    public void CRLF_and_LF_input_give_identical_output()
    {
        var lines = new[] { "MOVE", "PLACE 0,0,NORTH", "HELLO", "REPORT" };
        var crlf = Run(string.Join("\r\n", lines) + "\r\n");
        var lf = Run(string.Join("\n", lines) + "\n");

        var expectedStdout = Lines("Output:", "0,0,NORTH");
        var expectedStderr = Lines(
            "> MOVE",
            "Warning: MOVE ignored - robot is not on the table",
            "> PLACE 0,0,NORTH",
            "> HELLO",
            "Warning: 'HELLO' ignored - unknown command",
            "> REPORT");

        Assert.Equal(expectedStdout, crlf.Stdout);
        Assert.Equal(expectedStderr, crlf.Stderr);
        Assert.Equal(expectedStdout, lf.Stdout);
        Assert.Equal(expectedStderr, lf.Stderr);
    }

    [Fact]
    public void Empty_input_writes_nothing_and_session_ends()
    {
        var (stdout, stderr) = Run("");

        Assert.Equal("", stdout);
        Assert.Equal("", stderr);
    }
}
