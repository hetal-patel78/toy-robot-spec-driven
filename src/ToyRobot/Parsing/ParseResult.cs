using ToyRobot.Commands;

namespace ToyRobot.Parsing;

/// <summary>What reading one input line gives: a command object, EXIT, or a failure with its reason (DEC-26, DEC-30).</summary>
public abstract record ParseResult
{
    private ParseResult()
    {
    }

    // One of the three results of reading a line: "this line is a valid command".
    // It holds the command object the parser created, e.g. new MoveCommand().
    // The Runner takes it out with .Command and hands it to the RobotOperator to run.
    public sealed record CommandObject(IRobotCommand Command) : ParseResult;

    public sealed record Exit : ParseResult;

    public sealed record Failure(string Reason) : ParseResult;
}
