using static ToyRobot.Tests.RunnerWholeFlow.SessionHarness;

namespace ToyRobot.Tests.RunnerWholeFlow;

// No input stops the session: NFR-01; DEC-19, DEC-20, DEC-22, DEC-32.
public class NeverCrashTests
{
    [Fact]
    public void Bad_lines_never_stop_the_session()
    {
        var (stdout, stderr) = Run(Lines("PLACE 99999999999,0,NORTH", "HELLO", "", "PLACE 0,0,NORTH", "REPORT"));

        Assert.Equal(Lines("Output:", "0,0,NORTH"), stdout);
        Assert.Equal(
            Lines(
                "> PLACE 99999999999,0,NORTH",
                "Warning: 'PLACE 99999999999,0,NORTH' ignored - X is too large",
                "> HELLO",
                "Warning: 'HELLO' ignored - unknown command",
                "> ",
                "Warning: blank line ignored",
                "> PLACE 0,0,NORTH",
                "> REPORT"),
            stderr);
    }

    [Fact]
    public void Every_kind_of_ignored_line_then_valid_commands_still_work()
    {
        var input = Lines(
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
            "REPORT");

        var (stdout, stderr) = Run(input);

        Assert.Equal(Lines("Output:", "0,0,SOUTH"), stdout);
        Assert.Equal(
            Lines(
                "> HELLO",
                "Warning: 'HELLO' ignored - unknown command",
                "> ",
                "Warning: blank line ignored",
                "> PLACE 1,2",
                "Warning: 'PLACE 1,2' ignored - expected PLACE X,Y,F",
                "> PLACE A,0,NORTH",
                "Warning: 'PLACE A,0,NORTH' ignored - X must be a whole number",
                "> PLACE 0,B,NORTH",
                "Warning: 'PLACE 0,B,NORTH' ignored - Y must be a whole number",
                "> PLACE 1,2,UP",
                "Warning: 'PLACE 1,2,UP' ignored - F must be NORTH, SOUTH, EAST or WEST",
                "> move now",
                "Warning: 'move now' ignored - MOVE takes no values",
                "> PLACE 99999999999,0,NORTH",
                "Warning: 'PLACE 99999999999,0,NORTH' ignored - X is too large",
                "> > MOVE",
                "Warning: '> MOVE' ignored - unknown command",
                "> MOVE",
                "Warning: MOVE ignored - robot is not on the table",
                "> PLACE 9,9,NORTH",
                "Warning: PLACE 9,9,NORTH ignored - position is off the table",
                "> PLACE 0,0,SOUTH",
                "> MOVE",
                "Warning: MOVE ignored - next position is off the table so robot stays at 0,0,SOUTH",
                "> REPORT"),
            stderr);
    }
}
