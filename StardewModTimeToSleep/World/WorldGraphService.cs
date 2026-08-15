using StardewValley;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace StardewModTimeToSleep.World;

internal class WorldGraphService
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
  public WorldGraphService(IMonitor monitor)
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

      //Normal GameLocation warps
      foreach (Warp warp in location.warps)
      {
        
        worldLocation.Warps.Add(new WarpConnection
        {
          SourceName = location.Name,
          TargetName = warp.TargetName,
          ExitTile = new Point(warp.X, warp.Y),
          ArrivalTile = new Point(warp.TargetX, warp.TargetY)
        });

         Monitor.Log($"{location.Name} -> {warp.TargetName}",
                    LogLevel.Trace);
      }

      // Connections to buildings with their own GameLocation
      if (location.buildings != null)
        {
            foreach (Building building in location.buildings)
            {
                if (building.indoors.Value == null)
                    continue;

                Point door = building.getPointForHumanDoor();
                GameLocation indoors = building.indoors.Value;

                worldLocation.Buildings.Add(new BuildingConnection
                {
                    SourceName = location.Name,
                    TargetName = indoors.Name,
                    ExitTile = door,
                    ArrivalTile = Point.Zero // temporary
                });

                Monitor.Log(
                    $"Building connection: {location.Name} " +
                    $"({door.X},{door.Y}) -> {indoors.Name}",
                    LogLevel.Info
                );
            }
        }

      Graph.Locations.Add(location.Name, worldLocation);
    }
    AddFarmhouseConnection();
    AddMinecartConnections();

    foreach (WorldLocation worldLocation in Graph.Locations.Values)
    {
        Monitor.Log(
            $"{worldLocation.Location.Name}: " +
            $"{worldLocation.Warps.Count} warps, " +
            $"{worldLocation.Buildings.Count} building connections",
            LogLevel.Info
        );
    }

  }

  public WarpConnection? GetWarp(
    string fromLocation,
    string toLocation)
  {
      WorldLocation? location = GetLocation(fromLocation);

      if (location == null)
          return null;

      foreach (WarpConnection warp in location.Warps)
      {
          if (warp.TargetName == toLocation)
              return warp;
      }

      return null;
  }

  private void AddMinecartConnections()
  {
      // TODO
  }

  private void AddFarmhouseConnection()
  {
      Farm? farm = Game1.getFarm();

      if (farm == null || farm.mainFarmhouseEntry == null)
          return;

      WorldLocation? farmLocation = Graph.GetLocation("Farm");

      if (farmLocation == null)
          return;

      farmLocation.Buildings.Add(new BuildingConnection
      {
          SourceName = "Farm",
          TargetName = "FarmHouse",
          ExitTile = farm.mainFarmhouseEntry.Value,
          ArrivalTile = new Point(5, 8) //temporary
      });

      Monitor.Log(
          $"Building connection: Farm " +
          $"({farm.mainFarmhouseEntry.Value.X},{farm.mainFarmhouseEntry.Value.Y}) " +
          $"-> FarmHouse (5,8)",
          LogLevel.Info
      );
  }

  public WorldLocation? GetLocation(string locationName)
  {
    if(this.Graph.Locations.TryGetValue(locationName, out WorldLocation? location))
    {
      return location;
    }
    return null;
  }
  
}
