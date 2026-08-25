using Microsoft.Xna.Framework;

namespace StardewModTimeToSleep.World;

public sealed class WarpConnection
{

    //The location this warp leads to
    public string TargetName { get; init; } = "";

    //The location of this warp
    public string SourceName { get; init; } = "";

    // tile you stand on to leave this map
    public Point ExitTile { get; init; }

    // tile you appear on
    public Point ArrivalTile { get; init; }

    public override string ToString()
    {
        return $"{ExitTile} -> {TargetName} ({ArrivalTile})";
    }

    public bool IsMinecart { get; init; } = false;
    
}