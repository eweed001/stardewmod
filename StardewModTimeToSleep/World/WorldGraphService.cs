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

      foreach (Warp warp in location.warps)
      {

        Monitor.Log(
            $"Warp: {location.Name} ({warp.X},{warp.Y}) -> " +
            $"{warp.TargetName} ({warp.TargetX},{warp.TargetY})",
            LogLevel.Info
        );
        
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
      Graph.Locations.Add(location.Name, worldLocation);
    }

    AddFarmHouseConnection();
    AddMinecartConnections();

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

  private void AddFarmHouseConnection()
  {
    Farm? farm = Game1.getFarm();

    if (farm == null)
    {
        Monitor.Log("Could not find Farm.", LogLevel.Error);
        return;
    }

    WorldLocation? farmLocation = Graph.GetLocation("Farm");

    if (farmLocation == null)
    {
        Monitor.Log("Farm is not in the world graph.", LogLevel.Error);
        return;
    }

    if (Graph.GetLocation("FarmHouse") == null)
    {
        Monitor.Log(
            "FarmHouse is not in the world graph.",
            LogLevel.Error);

        return;
    }

    farmLocation.Warps.Add(new WarpConnection
    {
        SourceName = "Farm",
        TargetName = "FarmHouse",
        ExitTile = new Point(64, 15),
        ArrivalTile = new Point(5, 8)
    });

    Monitor.Log(
        "Added Farm -> FarmHouse connection: " +
        "(64,15) -> FarmHouse (5,8)",
        LogLevel.Info);
    
  }

  private Building? FindBuildingWithInterior(
    GameLocation location,
    string interiorName)
  {
      if (location is not Farm farm)
          return null;

      foreach (Building building in farm.buildings)
      {
          if (building.indoors.Value?.Name == interiorName)
              return building;
      }

      return null;
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
