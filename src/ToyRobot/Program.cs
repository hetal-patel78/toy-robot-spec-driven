using System.Text;

namespace ToyRobot;

/// <summary>
/// The entry point: builds the 5 x 5 Table, the RobotOperator and the Runner, and gives back the exit code
/// (PG-01, PG-02; FR-17, NFR-01; DEC-03, DEC-04, DEC-24, DEC-26, DEC-32). It does not read or parse commands.
/// </summary>
internal static class Program
{
    private const string HowToRunMessage = "How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)";
    private const string UnexpectedProblemPrefix = "Error: unexpected problem - ";

    /// <summary>The thin wiring of the real console: chooses the input reader (reading nothing) and calls <see cref="Run"/>.</summary>
    private static int Main(string[] args)
    {
        var interactive = !Console.IsInputRedirected;
        TextReader reader = interactive
            ? Console.In
            : new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, detectEncodingFromByteOrderMarks: false);

        return Run(args, reader, Console.Out, Console.Error, interactive);
    }

    /// <summary>
    /// Does the argument check, creates the parts and runs the Runner inside the last safeguard (DEC-32).
    /// Returns 0 at a normal end, 1 for any argument and 2 for an unexpected problem.
    /// </summary>
    internal static int Run(string[] args, TextReader input, TextWriter output, TextWriter error, bool interactive)
    {
        try
        {
            if (args.Length > 0)
            {
                error.WriteLine(HowToRunMessage);
                return 1;
            }

            var table = new Table(5, 5);
            var robotOperator = new RobotOperator(table);
            new Runner(input, output, error, robotOperator, interactive).Run();
            return 0;
        }
        catch (Exception exception)
        {
            // Not caused by any input: a fault outside the application (for example a closed pipe) or a defect.
            try
            {
                error.WriteLine(UnexpectedProblemPrefix + exception.Message);
            }
            catch (Exception)
            {
                // The error line cannot be written either; the exit code still says what happened.
            }

            return 2;
        }
    }
}
