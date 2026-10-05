namespace ToyRobot.Domain;

/// <summary>One of the four facing directions. Turns left and right and knows its upper-case name (DEC-07, DEC-17).</summary>
public sealed class Direction
{
    public static readonly Direction North = new("NORTH");
    public static readonly Direction East = new("EAST");
    public static readonly Direction South = new("SOUTH");
    public static readonly Direction West = new("WEST");

    public static IReadOnlyList<Direction> All { get; } = new[] { North, East, South, West };

    private Direction(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public Direction TurnLeft() => All[(IndexOf(this) + All.Count - 1) % All.Count];

    public Direction TurnRight() => All[(IndexOf(this) + 1) % All.Count];

    public override string ToString() => Name;

    private static int IndexOf(Direction direction)
    {
        for (var i = 0; i < All.Count; i++)
        {
            if (ReferenceEquals(All[i], direction))
            {
                return i;
            }
        }

        throw new InvalidOperationException("Unknown direction.");
    }
}
