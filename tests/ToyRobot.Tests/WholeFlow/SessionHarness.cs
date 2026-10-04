namespace ToyRobot.Tests.WholeFlow;

/// <summary>
/// Runs a <see cref="Runner"/> over a string reader and two string writers and gives back what was written
/// to console output (stdout) and console messages (stderr). Both writers use "\n" so exact comparison
/// is the same on every system (NFR-04, NFR-05, DEC-29).
/// </summary>
public static class SessionHarness
{
    public static (string Stdout, string Stderr) Run(string input, bool interactive = false, int width = 5, int height = 5)
    {
        var table = new Table(width, height);
        var robotOperator = new RobotOperator(table);
        var reader = new StringReader(input);
        var outWriter = new StringWriter { NewLine = "\n" };
        var errWriter = new StringWriter { NewLine = "\n" };

        new Runner(reader, outWriter, errWriter, robotOperator, interactive).Run();

        return (outWriter.ToString(), errWriter.ToString());
    }

    public static string Lines(params string[] lines)
    {
        return string.Concat(lines.Select(line => line + "\n"));
    }
}
