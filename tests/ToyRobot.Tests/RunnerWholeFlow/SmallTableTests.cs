using static ToyRobot.Tests.RunnerWholeFlow.SessionHarness;

namespace ToyRobot.Tests.RunnerWholeFlow;

// The whole flow on a table that is not 5 x 5: NFR-05; DEC-03, DEC-29.
public class SmallTableTests
{
    [Fact]
    public void Whole_flow_on_a_2_by_2_table()
    {
        var (stdout, stderr) = Run(
            Lines("PLACE 1,1,NORTH", "MOVE", "PLACE 2,2,NORTH", "REPORT"),
            width: 2,
            height: 2);

        Assert.Equal(Lines("Output:", "1,1,NORTH"), stdout);
        Assert.Equal(
            Lines(
                "> PLACE 1,1,NORTH",
                "> MOVE",
                "Warning: MOVE ignored - next position is off the table so robot stays at 1,1,NORTH",
                "> PLACE 2,2,NORTH",
                "Warning: PLACE 2,2,NORTH ignored - position is off the table",
                "> REPORT"),
            stderr);
    }
}
