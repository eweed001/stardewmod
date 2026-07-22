using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace StardewModTimeToSleep.World;

internal class WorldGraphBuilder
{

  /*********
  ** Fields
  *********/
  /// <summary>The monitor with which to log.</summary>
  private readonly IMonitor Monitor;

  public WorldGraph Graph { get; } = new();
  
  // public Dictionary<string, LocData> locationData { get; } = [];


  /*********
  ** Public methods
  *********/
  
  /// <summary>Constructor.</summary>
  /// <param name="monitor">The monitor with which to log.</param>
  public WorldGraphBuilder(IMonitor monitor)
  {
    this.Monitor = monitor;
  }

  //<summary>Initialize the locationData by iterating through all GameLocations
  // and building the WorldGraph. </summary>
  public void InitLocationData()
  {
    Monitor.Log("in init location data", LogLevel.Debug);

    Graph.Locations.Clear();

    foreach (GameLocation location in Game1.locations)
    {
      var worldLocation = new WorldLocation
      {
        Location = location
      };

      foreach (Warp warp in location.warps)
      {
        worldLocation.Warps.Add(new WarpConnection
        {
          TargetName = warp.TargetName,
          ExitTile = new Point(warp.X, warp.Y),
          ArrivalTile = new Point(warp.TargetX, warp.TargetY)
        });
         Monitor.Log($"{location.Name} -> {warp.TargetName}",
                    LogLevel.Trace);
      }
      Graph.Locations.Add(location.Name, worldLocation);
    }
    AddMinecartConnections();

  }

  private void AddMinecartConnections()
  {
      // TODO
  }
  
}
