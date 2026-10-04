namespace ToyRobot.Tests.Unit;

// FR-09, FR-10, FR-10a; DEC-06 to DEC-10, DEC-15, DEC-20, DEC-22, DEC-28
public class CommandParserTests
{
    private static readonly CommandParser Parser = new();

    [Theory]
    [InlineData("PLACE 4,4,NORTH", 4, 4, "NORTH")]
    [InlineData("place 1,2,north", 1, 2, "NORTH")]
    [InlineData("PLACE 4,2,NORTH", 4, 2, "NORTH")]
    [InlineData("PLACE 1,1,EAST", 1, 1, "EAST")]
    [InlineData("PLACE 0,0,SOUTH", 0, 0, "SOUTH")]
    [InlineData("PLACE 3,4,WEST", 3, 4, "WEST")]
    [InlineData("PLACE -1,0,NORTH", -1, 0, "NORTH")]
    [InlineData("place 09 , +9 , north", 9, 9, "NORTH")]
    [InlineData("PLACE 9,9,NORTH", 9, 9, "NORTH")]
    [InlineData("PLACE 2147483647,0,NORTH", 2147483647, 0, "NORTH")]
    public void Parse_returns_a_PlaceCommand_for_a_valid_PLACE_line(string line, int x, int y, string facingName)
    {
        var facing = Direction.All.Single(d => d.Name == facingName);

        var result = Parser.Parse(line);

        Assert.Equal(new ParseResult.CommandObject(new PlaceCommand(x, y, facing)), result);
        var placed = Assert.IsType<PlaceCommand>(Assert.IsType<ParseResult.CommandObject>(result).Command);
        Assert.Same(facing, placed.F);
    }

    [Theory]
    [InlineData("REPORT")]
    [InlineData("report")]
    public void Parse_returns_a_ReportCommand_for_REPORT(string line)
    {
        Assert.Equal(new ParseResult.CommandObject(new ReportCommand()), Parser.Parse(line));
    }

    // ---- User Story 2 (DEC-06, DEC-07, DEC-08) ----

    [Theory]
    [InlineData("MOVE")]
    [InlineData("move")]
    [InlineData("mOvE")]
    [InlineData("   MOVE   ")]
    public void Parse_returns_a_MoveCommand_for_MOVE(string line)
    {
        Assert.Equal(new ParseResult.CommandObject(new MoveCommand()), Parser.Parse(line));
    }

    [Theory]
    [InlineData("LEFT")]
    [InlineData("left")]
    public void Parse_returns_a_LeftCommand_for_LEFT(string line)
    {
        Assert.Equal(new ParseResult.CommandObject(new LeftCommand()), Parser.Parse(line));
    }

    [Theory]
    [InlineData("RIGHT")]
    [InlineData("right")]
    public void Parse_returns_a_RightCommand_for_RIGHT(string line)
    {
        Assert.Equal(new ParseResult.CommandObject(new RightCommand()), Parser.Parse(line));
    }

    // ---- User Story 4 (FR-09, FR-10, FR-10a; DEC-06 to DEC-10, DEC-20, DEC-22) ----

    private const string Tab = "\t";
    private const string Nbsp = " ";

    public static TheoryData<string, string> FailureRows => new()
    {
        // unknown command
        { "HELLO", "'HELLO' ignored - unknown command" },
        { "> MOVE", "'> MOVE' ignored - unknown command" },
        { "EXITNOW", "'EXITNOW' ignored - unknown command" },
        { "EXITING", "'EXITING' ignored - unknown command" },
        { "PLACE0,0,NORTH", "'PLACE0,0,NORTH' ignored - unknown command" },
        { "PLACEHOLDER", "'PLACEHOLDER' ignored - unknown command" },
        { "PLACE,1,2", "'PLACE,1,2' ignored - unknown command" },

        // blank
        { "", "blank line ignored" },
        { "   ", "blank line ignored" },
        { Tab + Tab, "blank line ignored" },
        { " " + Tab + " ", "blank line ignored" },
        { Nbsp, "blank line ignored" },

        // number of values
        { "PLACE 1,2", "'PLACE 1,2' ignored - expected PLACE X,Y,F" },
        { "PLACE 0 0 NORTH", "'PLACE 0 0 NORTH' ignored - expected PLACE X,Y,F" },
        { "PLACE A,B", "'PLACE A,B' ignored - expected PLACE X,Y,F" },
        { "PLACE 1,2,3,4", "'PLACE 1,2,3,4' ignored - expected PLACE X,Y,F" },
        { "PLACE", "'PLACE' ignored - expected PLACE X,Y,F" },

        // X
        { "PLACE A,0,NORTH", "'PLACE A,0,NORTH' ignored - X must be a whole number" },
        { "PLACE 99999999999,0,NORTH", "'PLACE 99999999999,0,NORTH' ignored - X must be a whole number" },
        { "PLACE 2147483648,0,NORTH", "'PLACE 2147483648,0,NORTH' ignored - X must be a whole number" },
        { "PLACE 1 0,0,NORTH", "'PLACE 1 0,0,NORTH' ignored - X must be a whole number" },
        { "PLACE 1.5,0,NORTH", "'PLACE 1.5,0,NORTH' ignored - X must be a whole number" },
        { "PLACE ,,", "'PLACE ,,' ignored - X must be a whole number" },
        { "PLACE A,B,UP", "'PLACE A,B,UP' ignored - X must be a whole number" },

        // Y
        { "PLACE 0,B,NORTH", "'PLACE 0,B,NORTH' ignored - Y must be a whole number" },
        { "PLACE 0,99999999999,NORTH", "'PLACE 0,99999999999,NORTH' ignored - Y must be a whole number" },
        { "PLACE 0,B,UP", "'PLACE 0,B,UP' ignored - Y must be a whole number" },

        // F
        { "PLACE 1,2,UP", "'PLACE 1,2,UP' ignored - F must be NORTH, SOUTH, EAST or WEST" },
        { "PLACE 0,0,N", "'PLACE 0,0,N' ignored - F must be NORTH, SOUTH, EAST or WEST" },
        { "PLACE 1,2,N", "'PLACE 1,2,N' ignored - F must be NORTH, SOUTH, EAST or WEST" },
        { "PLACE 1,2,", "'PLACE 1,2,' ignored - F must be NORTH, SOUTH, EAST or WEST" },
        { "PLACE 1,2,NORTH EXTRA", "'PLACE 1,2,NORTH EXTRA' ignored - F must be NORTH, SOUTH, EAST or WEST" },
        { "PLACE 1,2,1", "'PLACE 1,2,1' ignored - F must be NORTH, SOUTH, EAST or WEST" },

        // takes no values
        { "move now", "'move now' ignored - MOVE takes no values" },
        { "MOVE now", "'MOVE now' ignored - MOVE takes no values" },
        { "MOVE" + Nbsp + "now", "'MOVE" + Nbsp + "now' ignored - MOVE takes no values" },
        { "LEFT now", "'LEFT now' ignored - LEFT takes no values" },
        { "RIGHT now", "'RIGHT now' ignored - RIGHT takes no values" },
        { "REPORT now", "'REPORT now' ignored - REPORT takes no values" },

        // the quote is the line with whitespace removed from both ends only
        { "  HELLO  ", "'HELLO' ignored - unknown command" },
        { Tab + "PLACE 1,2" + Nbsp, "'PLACE 1,2' ignored - expected PLACE X,Y,F" },
    };

    [Theory]
    [MemberData(nameof(FailureRows))]
    public void Parse_refuses_a_bad_line_with_one_reason(string line, string reason)
    {
        Assert.Equal(new ParseResult.Failure(reason), Parser.Parse(line));
    }

    [Theory]
    [InlineData("PLACE     0,0,NORTH", 0, 0, "NORTH")]
    [InlineData("place 0 , 0 , north", 0, 0, "NORTH")]
    [InlineData("PLACE 01,+1,EAST", 1, 1, "EAST")]
    [InlineData("\tplace 0, 0\t,north ", 0, 0, "NORTH")]
    [InlineData("PLACE\t1,2,EAST", 1, 2, "EAST")]
    [InlineData("Place 1,1,East", 1, 1, "EAST")]
    public void Parse_reads_valid_PLACE_lines_with_any_whitespace_and_case(string line, int x, int y, string facingName)
    {
        var facing = Direction.All.Single(d => d.Name == facingName);

        Assert.Equal(new ParseResult.CommandObject(new PlaceCommand(x, y, facing)), Parser.Parse(line));
    }

    [Fact]
    public void Parse_reads_REPORT_in_mixed_case()
    {
        Assert.Equal(new ParseResult.CommandObject(new ReportCommand()), Parser.Parse("rEpOrT"));
    }

    [Theory]
    [InlineData("EXIT")]
    [InlineData("exit")]
    [InlineData("Exit")]
    [InlineData("EXIT now")]
    [InlineData("exit now")]
    [InlineData("\tEXIT\t")]
    public void Parse_returns_Exit_for_the_EXIT_word_whatever_follows(string line)
    {
        Assert.Equal(new ParseResult.Exit(), Parser.Parse(line));
    }

    [Fact]
    public void Parse_reads_words_the_same_under_the_Turkish_culture()
    {
        using var scope = new Support.CultureScope("tr-TR");

        Assert.Equal(new ParseResult.Exit(), Parser.Parse("exit"));
        Assert.Equal(new ParseResult.CommandObject(new RightCommand()), Parser.Parse("right"));
        Assert.Equal(new ParseResult.CommandObject(new LeftCommand()), Parser.Parse("left"));
        Assert.Equal(
            new ParseResult.CommandObject(new PlaceCommand(1, 2, Direction.East)),
            Parser.Parse("place 1,2,east"));
        Assert.Equal(
            new ParseResult.CommandObject(new PlaceCommand(-1, 0, Direction.North)),
            Parser.Parse("PLACE -1,0,NORTH"));
    }

    [Fact]
    public void Parse_reads_a_negative_number_the_same_under_the_Swedish_culture()
    {
        using var scope = new Support.CultureScope("sv-SE");

        Assert.Equal(
            new ParseResult.CommandObject(new PlaceCommand(-1, 0, Direction.North)),
            Parser.Parse("PLACE -1,0,NORTH"));
    }

    public static TheoryData<string> NeverThrowsInputs()
    {
        var lines = new TheoryData<string>();
        foreach (var row in FailureRows)
        {
            lines.Add((string)row[0]);
        }

        foreach (var line in new[]
                 {
                     "PLACE 1,2,NORTH,",
                     "PLACE --1,0,NORTH",
                     "PLACE 0x1,0,NORTH",
                     "PLACE -2147483649,0,NORTH",
                     new string('A', 10000),
                     "\0",
                     "PLACE 0,0,NORTH",
                     "MOVE",
                     "EXIT",
                 })
        {
            lines.Add(line);
        }

        return lines;
    }

    [Theory]
    [MemberData(nameof(NeverThrowsInputs))]
    public void Parse_never_throws_and_always_returns_one_of_the_three_cases(string line)
    {
        ParseResult? result = null;

        var exception = Record.Exception(() => result = Parser.Parse(line));

        Assert.Null(exception);
        Assert.True(
            result is ParseResult.CommandObject or ParseResult.Exit or ParseResult.Failure,
            "Parse must return one of the three ParseResult cases.");
    }
}
