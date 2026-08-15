using Microsoft.Xna.Framework;

namespace StardewModTimeToSleep.World;

public sealed class BuildingConnection
{
    public string SourceName { get; init; } = "";

    public string TargetName { get; init; } = "";

    // Tile on the outside map where the player enters the building.
    public Point ExitTile { get; init; }

    // Tile inside the building where the player arrives.
    public Point ArrivalTile { get; init; }

    public override string ToString()
    {
        return $"{ExitTile} -> {TargetName} ({ArrivalTile})";
    }
}