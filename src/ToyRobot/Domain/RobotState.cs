using System.Globalization;

namespace ToyRobot.Domain;

/// <summary>The robot's position and facing as one immutable value (DEC-12, DEC-13).</summary>
public sealed record RobotState(int X, int Y, Direction Facing)
{
    /// <summary>The text form X,Y,F, written the same way under every culture (DEC-21, DEC-22).</summary>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{X},{Y},{Facing.Name}");
}
