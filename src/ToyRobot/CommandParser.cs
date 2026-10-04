using System.Globalization;

namespace ToyRobot;

/// <summary>
/// Turns one input line into a parse result. Checks the format only, never the table (DEC-28),
/// and never throws (DEC-20). The first problem found, in the order of DEC-22, is the one reported.
/// </summary>
public sealed class CommandParser
{
    public ParseResult Parse(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0)
        {
            return new ParseResult.Failure("blank line ignored");
        }

        var (word, rest) = SplitCommandWord(trimmed);

        if (IsWord(word, "EXIT"))
        {
            return new ParseResult.Exit();
        }

        var withoutValues = CreateCommandWithoutValues(word);
        if (withoutValues is not null)
        {
            return rest.Length == 0
                ? new ParseResult.CommandObject(withoutValues)
                : new ParseResult.Failure($"'{trimmed}' ignored - {withoutValues.Name} takes no values");
        }

        if (IsWord(word, "PLACE"))
        {
            return ParsePlace(trimmed, rest);
        }

        return new ParseResult.Failure($"'{trimmed}' ignored - unknown command");
    }

    private static (string Word, string Remainder) SplitCommandWord(string trimmed)
    {
        var end = 0;
        while (end < trimmed.Length && !char.IsWhiteSpace(trimmed[end]))
        {
            end++;
        }

        var rest = end < trimmed.Length ? trimmed[(end + 1)..] : string.Empty;
        return (trimmed[..end], rest);
    }

    private static IRobotCommand? CreateCommandWithoutValues(string word)
    {
        if (IsWord(word, "MOVE"))
        {
            return new MoveCommand();
        }

        if (IsWord(word, "LEFT"))
        {
            return new LeftCommand();
        }

        if (IsWord(word, "RIGHT"))
        {
            return new RightCommand();
        }

        return IsWord(word, "REPORT") ? new ReportCommand() : null;
    }

    private static bool IsWord(string word, string expected) =>
        string.Equals(word, expected, StringComparison.OrdinalIgnoreCase);

    private static ParseResult ParsePlace(string trimmed, string rest)
    {
        var parts = rest.Split(',');
        if (parts.Length != 3)
        {
            return new ParseResult.Failure($"'{trimmed}' ignored - expected PLACE X,Y,F");
        }

        if (!TryReadWholeNumber(parts[0], out var x))
        {
            return new ParseResult.Failure($"'{trimmed}' ignored - X must be a whole number");
        }

        if (!TryReadWholeNumber(parts[1], out var y))
        {
            return new ParseResult.Failure($"'{trimmed}' ignored - Y must be a whole number");
        }

        var name = parts[2].Trim();
        var facing = Direction.All.FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
        if (facing is null)
        {
            return new ParseResult.Failure($"'{trimmed}' ignored - F must be NORTH, SOUTH, EAST or WEST");
        }

        return new ParseResult.CommandObject(new PlaceCommand(x, y, facing));
    }

    private static bool TryReadWholeNumber(string part, out int value) =>
        int.TryParse(part.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
}
