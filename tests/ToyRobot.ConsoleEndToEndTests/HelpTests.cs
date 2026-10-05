namespace ToyRobot.ConsoleEndToEndTests;

// E2E-10: FR-18; DEC-04, DEC-23, DEC-31 "the help text, word for word, on standard output".
public class HelpTests
{
    // The help text of DEC-04, copied line for line; the test's own copy, so a change to one side alone fails.
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

    [Fact]
    public void Help_text_and_exit_code_0_when_started_with_help()
    {
        var result = AppProcess.Run(AppProcess.Utf8("PLACE 0,0,NORTH", "REPORT"), "--help");

        Assert.Equal(HelpLines, LineComparer.Lines(result.Stdout));
        Assert.Equal("", result.Stderr);
        Assert.Equal(0, result.ExitCode);
    }
}
