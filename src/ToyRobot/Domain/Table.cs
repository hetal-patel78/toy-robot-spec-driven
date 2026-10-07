namespace ToyRobot.Domain;

/// <summary>The rectangular surface. Answers whether a position is on it (DEC-01, DEC-03, DEC-28).</summary>
public sealed class Table
{
    public Table(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
}
