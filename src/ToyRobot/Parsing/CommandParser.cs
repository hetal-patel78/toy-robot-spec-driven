using System.Globalization;
using ToyRobot.Commands;
using ToyRobot.Domain;

namespace ToyRobot.Parsing;

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

    // Splits a line into the command word and the rest.
    // Example: "PLACE 1,2,NORTH" → word = "PLACE", rest = "1,2,NORTH"
    //          "MOVE"            → word = "MOVE",  rest = ""
    private static (string Word, string Remainder) SplitCommandWord(string trimmed)
    {
        var end = 0;
        while (end < trimmed.Length && !char.IsWhiteSpace(trimmed[end])) // IsWhiteSpace is true for a space, a tab, or a non-breaking space
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
            return new ParseResult.Failure($"'{trimmed}' ignored - X {WholeNumberProblem(parts[0])}");
        }

        if (!TryReadWholeNumber(parts[1], out var y))
        {
            return new ParseResult.Failure($"'{trimmed}' ignored - Y {WholeNumberProblem(parts[1])}");
        }

        var name = parts[2].Trim();
        var facing = Direction.All.FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
        if (facing is null)
        {
            return new ParseResult.Failure($"'{trimmed}' ignored - F must be NORTH, SOUTH, EAST or WEST");
        }

        return new ParseResult.CommandObject(new PlaceCommand(x, y, facing));
    }

    // A part that cannot be read but is only an optional sign and digits is a whole number that
    // does not fit in an int, in either direction (DEC-20, DEC-22).
    private static string WholeNumberProblem(string part)
    {
        var text = part.Trim();
        var digits = text.Length > 0 && (text[0] == '-' || text[0] == '+') ? text[1..] : text;
        var isWholeNumber = digits.Length > 0 && digits.All(c => c is >= '0' and <= '9');
        return isWholeNumber ? "is too large" : "must be a whole number";
    }

    private static bool TryReadWholeNumber(string part, out int value) =>
        int.TryParse(part.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
}
