namespace ToyRobot;

/// <summary>What reading one input line gives: a command object, EXIT, or a failure with its reason (DEC-26, DEC-30).</summary>
public abstract record ParseResult
{
    private ParseResult()
    {
    }

    public sealed record CommandObject(IRobotCommand Command) : ParseResult;

    public sealed record Exit : ParseResult;

    public sealed record Failure(string Reason) : ParseResult;
}
