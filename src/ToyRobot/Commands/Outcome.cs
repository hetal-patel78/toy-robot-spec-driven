namespace ToyRobot.Commands;

/// <summary>What running a command gives back: a position text, a warning, or nothing to show (DEC-21, DEC-26).</summary>
public abstract record Outcome
{
    private Outcome()
    {
    }

    public sealed record PositionText(string Text) : Outcome;

    public sealed record Warning(string Text) : Outcome;

    public sealed record Done : Outcome;
}
