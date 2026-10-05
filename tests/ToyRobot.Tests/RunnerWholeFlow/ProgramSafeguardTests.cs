using System.Text;
using static ToyRobot.Tests.RunnerWholeFlow.SessionHarness;

namespace ToyRobot.Tests.RunnerWholeFlow;

// Program's Run in memory: the last safeguard for unexpected problems and the existing exit codes
// (SG-01; PG-01, PG-02; NFR-01, FR-17; DEC-04, DEC-10, DEC-23, DEC-26, DEC-32).
public class ProgramSafeguardTests
{
    private const string HowToRunMessage = "How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)";

    // T053 (a): User Story 5 scenario 4.
    [Fact]
    public void Unexpected_problem_while_reading_gives_one_error_line_and_exit_code_2()
    {
        var output = NewWriter();
        var error = NewWriter();

        var exitCode = Program.Run(Array.Empty<string>(), new FailingReader(), output, error, interactive: false);

        Assert.Equal(2, exitCode);
        Assert.Equal("", output.ToString());
        Assert.Equal(Lines("Error: unexpected problem - boom"), error.ToString());
    }

    // T053 (b): User Story 5 scenario 5.
    [Fact]
    public void Unexpected_problem_while_writing_console_output_gives_one_error_line_and_exit_code_2()
    {
        var error = NewWriter();

        var exitCode = Program.Run(
            Array.Empty<string>(), new StringReader(Lines("PLACE 0,0,NORTH", "REPORT")), new FailingWriter(), error, interactive: false);

        Assert.Equal(2, exitCode);
        Assert.Equal(Lines("> PLACE 0,0,NORTH", "> REPORT", "Error: unexpected problem - boom"), error.ToString());
    }

    // T053 (c): User Story 5 scenario 6.
    [Fact]
    public void Unexpected_problem_when_the_error_line_cannot_be_written_still_gives_exit_code_2()
    {
        var exitCode = Program.Run(
            Array.Empty<string>(), new StringReader(Lines("PLACE 0,0,NORTH", "REPORT")), new FailingWriter(), new FailingWriter(), interactive: false);

        Assert.Equal(2, exitCode);
    }

    // T054 (a): FR-17 through Run.
    [Fact]
    public void Run_with_an_argument_gives_the_how_to_run_message_and_exit_code_1()
    {
        var input = new StringReader(Lines("PLACE 0,0,NORTH"));
        var output = NewWriter();
        var error = NewWriter();

        var exitCode = Program.Run(new[] { "commands.txt" }, input, output, error, interactive: false);

        Assert.Equal(1, exitCode);
        Assert.Equal("", output.ToString());
        Assert.Equal(Lines(HowToRunMessage), error.ToString());
        Assert.Equal("PLACE 0,0,NORTH", input.ReadLine());
    }

    // T054 (b): normal end through Run.
    [Fact]
    public void Run_with_Challenge_Example_1_gives_its_output_and_exit_code_0()
    {
        var output = NewWriter();
        var error = NewWriter();

        var exitCode = Program.Run(
            Array.Empty<string>(), new StringReader(Lines("PLACE 0,0,NORTH", "MOVE", "REPORT")), output, error, interactive: false);

        Assert.Equal(0, exitCode);
        Assert.Equal(Lines("Output:", "0,1,NORTH"), output.ToString());
        Assert.Equal(Lines("> PLACE 0,0,NORTH", "> MOVE", "> REPORT"), error.ToString());
    }

    private static StringWriter NewWriter() => new() { NewLine = "\n" };

    /// <summary>Stands for a fault outside the application while reading, for example a pipe that closes.</summary>
    private sealed class FailingReader : TextReader
    {
        public override string? ReadLine() => throw new IOException("boom");

        public override int Read() => throw new IOException("boom");
    }

    /// <summary>Stands for a fault outside the application while writing: every write throws.</summary>
    private sealed class FailingWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value) => throw new IOException("boom");

        public override void Write(string? value) => throw new IOException("boom");

        public override void WriteLine(string? value) => throw new IOException("boom");
    }
}
