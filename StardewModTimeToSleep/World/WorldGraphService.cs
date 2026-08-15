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
      Monitor.Log(
          $"Checking buildings for {location.Name}: " +
          $"{location.buildings?.Count ?? 0} buildings",
          LogLevel.Info
      );

      var worldLocation = new WorldLocation
      {
        Location = location
      };

      if (location.buildings != null)
      {
          foreach (Building building in location.buildings)
          {
              if (building.indoors.Value == null)
                  continue;

              Point door = building.getPointForHumanDoor();

               GameLocation indoors = building.indoors.Value;

              if (indoors.warps.Count == 0)
              {
                  Monitor.Log(
                      $"Building {indoors.Name} has no interior warp.",
                      LogLevel.Warn
                  );

                  continue;
              }

              Warp interiorWarp = indoors.warps[0];

              Point arrivalTile = new(
                  interiorWarp.X,
                  interiorWarp.Y - 1
              );

              worldLocation.Buildings.Add(new BuildingConnection
              {
                  SourceName = location.Name,
                  TargetName = indoors.Name,
                  ExitTile = door,
                  ArrivalTile = arrivalTile
              });


              Monitor.Log(
                  $"Building connection: {location.Name} " +
                  $"({door.X},{door.Y}) -> " +
                  $"{indoors.Name} ({arrivalTile.X},{arrivalTile.Y})",
                  LogLevel.Info
              );
          }
      }

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

  public WorldLocation? GetLocation(string locationName)
  {
    if(this.Graph.Locations.TryGetValue(locationName, out WorldLocation? location))
    {
      return location;
    }
    return null;
  }
  
}
