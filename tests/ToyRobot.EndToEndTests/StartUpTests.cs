namespace ToyRobot.EndToEndTests;

// E2E-09: PG-01; FR-08, FR-01 (spec 001); DEC-03, DEC-26.
public class StartUpTests
{
    [Fact]
    public void Redirected_REPORT_before_PLACE_writes_nothing_to_console_output()
    {
        var result = AppProcess.Run(AppProcess.Utf8("REPORT"));

        Assert.Empty(LineComparer.Lines(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void Application_table_is_5_by_5()
    {
        var result = AppProcess.Run(AppProcess.Utf8(
            "PLACE 4,4,NORTH", "REPORT", "PLACE 5,0,NORTH", "REPORT", "PLACE 0,5,NORTH", "REPORT"));

        Assert.Equal(
            new[] { "Output:", "4,4,NORTH", "Output:", "4,4,NORTH", "Output:", "4,4,NORTH" },
            LineComparer.Lines(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }
}
