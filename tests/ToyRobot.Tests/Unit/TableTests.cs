namespace ToyRobot.Tests.Unit;

// FR-01; DEC-01, DEC-03, DEC-28
public class TableTests
{
    [Fact]
    public void Width_and_Height_return_the_constructor_values()
    {
        var table = new Table(5, 3);

        Assert.Equal(5, table.Width);
        Assert.Equal(3, table.Height);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 0)]
    [InlineData(0, 4)]
    [InlineData(4, 4)]
    [InlineData(2, 2)]
    public void Positions_on_a_5_by_5_table_are_on_the_table(int x, int y)
    {
        Assert.True(new Table(5, 5).IsOnTable(x, y));
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(0, 5)]
    [InlineData(5, 5)]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(2147483647, 0)]
    [InlineData(-2147483648, 0)]
    public void Positions_outside_a_5_by_5_table_are_not_on_the_table(int x, int y)
    {
        Assert.False(new Table(5, 5).IsOnTable(x, y));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    public void Positions_on_a_2_by_2_table_are_on_the_table(int x, int y)
    {
        Assert.True(new Table(2, 2).IsOnTable(x, y));
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(0, 2)]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    public void Positions_outside_a_2_by_2_table_are_not_on_the_table(int x, int y)
    {
        Assert.False(new Table(2, 2).IsOnTable(x, y));
    }
}
