using System.Text;

namespace ToyRobot.EndToEndTests;

// E2E-07: NFR-04; DEC-31, DEC-33.
public class LineEndingTests
{
    [Fact]
    public void CRLF_input_gives_same_console_output_as_LF()
    {
        var crlf = new UTF8Encoding(false).GetBytes("PLACE 0,0,NORTH\r\nMOVE\r\nREPORT\r\n");
        var lf = AppProcess.Utf8("PLACE 0,0,NORTH", "MOVE", "REPORT");

        var crlfResult = AppProcess.Run(crlf);
        var lfResult = AppProcess.Run(lf);

        var expected = new[] { "Output:", "0,1,NORTH" };
        Assert.Equal(expected, LineComparer.Lines(crlfResult.Stdout));
        Assert.Equal(0, crlfResult.ExitCode);
        Assert.Equal(expected, LineComparer.Lines(lfResult.Stdout));
        Assert.Equal(0, lfResult.ExitCode);
    }
}
