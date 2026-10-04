namespace ToyRobot.EndToEndTests;

// E2E-02, E2E-08: FR-17; DEC-04, DEC-31 "the how-to-run message and exit codes (end-to-end)".
public class HowToRunMessageTests
{
    private const string HowToRunMessage = "How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)";

    [Theory]
    [InlineData("--help")]
    [InlineData("commands.txt")]
    [InlineData("a", "b")]
    public void How_to_run_message_and_exit_code_1_when_started_with_an_argument(params string[] args)
    {
        var result = AppProcess.Run(Array.Empty<byte>(), args);

        Assert.Equal("", result.Stdout);
        Assert.Equal(new[] { HowToRunMessage }, LineComparer.Lines(result.Stderr));
        Assert.Equal(1, result.ExitCode);
    }

    [Fact]
    public void Input_is_not_read_when_started_with_an_argument()
    {
        var result = AppProcess.Run(AppProcess.Utf8("PLACE 0,0,NORTH", "REPORT"), "--help");

        Assert.Equal("", result.Stdout);
        Assert.Equal(new[] { HowToRunMessage }, LineComparer.Lines(result.Stderr));
        Assert.Equal(1, result.ExitCode);
    }
}
