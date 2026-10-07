using System.Text;
using ToyRobot.Domain;
using ToyRobot.Session;

namespace ToyRobot;

/// <summary>
/// The entry point: shows the help for --help, builds the 5 x 5 Table, the RobotOperator and the Runner, and gives
/// back the exit code (PG-01, PG-02; FR-17, FR-18, NFR-01; DEC-03, DEC-04, DEC-24, DEC-26, DEC-32). It does not read
/// or parse commands.
/// </summary>
internal static class Program
{
    private const string HowToRunMessage = "How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)";
    private const string UnexpectedProblemPrefix = "Error: unexpected problem - ";

    /// <summary>The help text of DEC-04, line for line (FR-18). Plain ASCII, like all the application's output.</summary>
    private static readonly string[] HelpLines =
    {
        "Toy Robot Simulator - help",
        "",
        "HOW TO START",
        "  Type commands:        ToyRobot",
        "  Commands from a file: ToyRobot < commands.txt",
        "                        run commands.txt         (cmd)",
        "                        .\\run.cmd commands.txt   (PowerShell)",
        "                        ./run.sh commands.txt    (bash, zsh)",
        "",
        "THE TABLE",
        "  5 x 5 units. Positions are X = 0..4 and Y = 0..4.",
        "  0,0 is the SOUTH WEST corner; NORTH is up (Y + 1), EAST is right (X + 1).",
        "",
        "COMMANDS (one per line, upper or lower case)",
        "  PLACE X,Y,F  Put the robot on the table at X,Y facing F.",
        "               F is NORTH, SOUTH, EAST or WEST (full word).",
        "               Example: PLACE 0,0,NORTH",
        "  MOVE         Move the robot one unit forward in the direction it faces.",
        "  LEFT         Turn the robot 90 degrees left, without moving it.",
        "  RIGHT        Turn the robot 90 degrees right, without moving it.",
        "  REPORT       Show the robot's position and direction, for example:",
        "                 Output:",
        "                 0,1,NORTH",
        "  EXIT         End the session.",
        "",
        "RULES",
        "  - The first valid PLACE puts the robot on the table. Until then",
        "    MOVE, LEFT, RIGHT and REPORT are ignored.",
        "  - A PLACE or MOVE that would put the robot off the table is ignored;",
        "    the robot stays where it was and later commands still work.",
        "  - An ignored command or a line that is not a valid command shows a",
        "    warning, and the next line is read.",
        "",
        "ENDING A SESSION",
        "  EXIT                     Ends the session.",
        "  Ctrl+Z then Enter        End of input on Windows: ends the session.",
        "  Ctrl+D                   End of input on Linux and macOS: ends the session.",
        "  Ctrl+C                   Stops the app at once.",
    };

    /// <summary>The thin wiring of the real console: chooses the input reader (reading nothing) and calls <see cref="Run"/>.</summary>
    private static int Main(string[] args)
    {
        // True if a person is typing on the console; false if the data comes from a file or pipe.
        var interactive = !Console.IsInputRedirected;

        // For a file or pipe:
        // Console.OpenStandardInput()      - the raw bytes coming in from the file or pipe.
        // Encoding.UTF8                    - turns those bytes into text using UTF-8.
        // detectEncodingFromByteOrderMarks - false: don't let the file's first bytes switch to another encoding.
        TextReader reader = interactive
            ? Console.In
            : new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, detectEncodingFromByteOrderMarks: false);

        return Run(args, reader, Console.Out, Console.Error, interactive);
    }

    /// <summary>
    /// Does the argument check, creates the parts and runs the Runner inside the last safeguard (DEC-32).
    /// Returns 0 at a normal end or after the help text, 1 for any other argument and 2 for an unexpected problem.
    /// </summary>
    internal static int Run(string[] args, TextReader input, TextWriter output, TextWriter error, bool interactive)
    {
        try
        {
            if (args.Length == 1 && args[0] == "--help")
            {
                foreach (var line in HelpLines)
                {
                    output.WriteLine(line);
                }

                return 0;
            }

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
