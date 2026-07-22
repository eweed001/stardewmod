namespace StardewModTimeToSleep.World;

public sealed class WorldGraph
{
    public Dictionary<string, WorldLocation> Locations { get; } = [];

    public WorldLocation? GetLocation(string name)
    {
        Locations.TryGetValue(name, out var location);
        return location;
    }
}
