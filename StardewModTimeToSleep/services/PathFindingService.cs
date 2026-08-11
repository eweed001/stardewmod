using StardewValley;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModTimeToSleep.PathFinding;
using StardewModTimeToSleep.World;

namespace StardewModTimeToSleep.services;

internal class PathFindingService
{
  
  private readonly IMonitor Monitor;

  private readonly AStar aStar;

  private readonly WorldGraphService worldGraph;

  private const string HomeLocation = "FarmHouse";

  public PathFindingService(IMonitor monitor, WorldGraphService worldGraph)
  {
    this.Monitor = monitor;
    this.aStar = new AStar(monitor);
    this.worldGraph = worldGraph;
  }

  public int FindDistanceToHome(GameLocation currentLocation, Point playerTile)
  {

    if (currentLocation is Farm farmm)
    {
        Point entrance = GetFarmhouseEntrance(farmm);

        return aStar.FindDistance(
            farmm,
            playerTile,
            entrance);
    }

    
    if(currentLocation.Name == "FarmHouse")
    {
      Point bedTile = GetBedTile(currentLocation);
      this.Monitor.Log(
          $"Finding path home from {currentLocation.Name}",
          LogLevel.Debug);
      return aStar.FindDistance(currentLocation, playerTile, bedTile);
    }

    return int.MaxValue;

  }

  private WarpConnection? FindWarpTo(string fromLocation, string toLocation)
  {
    WorldLocation? location = this.worldGraph.GetLocation(fromLocation);

    if(location == null)
      return null;

    foreach(WarpConnection warp in location.Warps)
    {
      if(warp.TargetName == toLocation)
        return warp;
    }

    return null;
  }

  private Point GetFarmhouseEntrance(GameLocation location)
  {
    //todo - find the farm house entrance dynamically 
    return new Point(65, 15);
  }

  private Point GetBedTile(GameLocation location)
  {
      // TODO: Find the actual bed dynamically.
      return new Point(5, 8);
  }

}