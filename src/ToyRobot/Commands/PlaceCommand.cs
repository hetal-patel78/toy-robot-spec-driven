using ToyRobot.Domain;

namespace ToyRobot.Commands;

/// <summary>PLACE X,Y,F: puts the robot at the given position and facing (DEC-15).</summary>
public sealed record PlaceCommand(int X, int Y, Direction F) : IRobotCommand
{
    public string Name => "PLACE";

    public bool NeedsRobotOnTable => false;

    public Outcome Execute(IRobotOperator robot)
    {
        var proposed = new RobotState(X, Y, F);

        return robot.TryMoveTo(proposed)
            ? new Outcome.Done()
            : new Outcome.Warning($"PLACE {proposed} ignored - position is off the table");
    }
}
