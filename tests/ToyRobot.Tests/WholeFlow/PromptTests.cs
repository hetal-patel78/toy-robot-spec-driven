using static ToyRobot.Tests.WholeFlow.SessionHarness;

namespace ToyRobot.Tests.WholeFlow;

// The prompt when a person is typing: FR-13, FR-14, FR-16; DEC-10, DEC-24, DEC-25.
public class PromptTests
{
    [Fact]
    public void Prompt_before_each_read_and_typed_lines_not_echoed()
    {
        var (stdout, stderr) = Run("MOVE\nEXIT\n", interactive: true);

        Assert.Equal("", stdout);
        Assert.Equal("> Warning: MOVE ignored - robot is not on the table\n> ", stderr);
    }

    [Fact]
    public void Prompt_also_shown_before_the_read_that_finds_end_of_input()
    {
        var (stdout, stderr) = Run("PLACE 0,0,NORTH\nREPORT\n", interactive: true);

        Assert.Equal("Output:\n0,0,NORTH\n", stdout);
        Assert.Equal("> > > ", stderr);
    }

    [Fact]
    public void Empty_typed_input_shows_one_prompt()
    {
        var (stdout, stderr) = Run("", interactive: true);

        Assert.Equal("", stdout);
        Assert.Equal("> ", stderr);
    }

    [Fact]
    public void No_prompt_when_input_is_redirected()
    {
        var (stdout, stderr) = Run("MOVE\nEXIT\n", interactive: false);

        Assert.Equal("", stdout);
        Assert.Equal("> MOVE\nWarning: MOVE ignored - robot is not on the table\n> EXIT\n", stderr);
    }
}
