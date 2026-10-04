namespace ToyRobot.EndToEndTests;

// E2E-05: FR-11; DEC-20, DEC-26, DEC-31 "input starting with a byte-order mark".
public class ByteOrderMarkTests
{
    [Fact]
    public void Input_starting_with_byte_order_mark_runs_challenge_example_1()
    {
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var body = AppProcess.Utf8("PLACE 0,0,NORTH", "MOVE", "REPORT");
        var stdin = bom.Concat(body).ToArray();

        var result = AppProcess.Run(stdin);

        Assert.Equal(new[] { "Output:", "0,1,NORTH" }, LineComparer.Lines(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }
}
