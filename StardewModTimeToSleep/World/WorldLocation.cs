using StardewValley;

namespace StardewModTimeToSleep.World;

public sealed class WorldLocation
{
    public GameLocation Location { get; init; } = null!;

    public List<WarpConnection> Warps { get; } = [];

    public List<BuildingConnection> Buildings { get; } = [];
}
