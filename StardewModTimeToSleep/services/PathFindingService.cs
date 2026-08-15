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

     List<string>? path = FindLocationPath(
        currentLocation.Name,
        HomeLocation);

      if (path == null)
      {
          Monitor.Log(
              $"Could not find route from " +
              $"{currentLocation.Name} to {HomeLocation}.",
              LogLevel.Warn);

          return int.MaxValue;
      }

      Monitor.Log(
          $"Location route: {string.Join(" -> ", path)}",
          LogLevel.Info);
      
      int totalDistance = 0;

      GameLocation location = currentLocation;
      Point position = playerTile;

      for (int i = 0; i < path.Count - 1; i++)
      {
          string from = path[i];
          string to = path[i + 1];

          WorldLocation? worldLocation =
              worldGraph.GetLocation(from);

          if (worldLocation == null)
              return int.MaxValue;

          // First check normal warps.
          WarpConnection? warp =
              worldLocation.Warps
                  .FirstOrDefault(w => w.TargetName == to);

          if (warp != null)
          {

            Monitor.Log(
                $"A*: {from} ({position}) -> {to} warp exit ({warp.ExitTile})",
                LogLevel.Info);
              Point exitTile = GetWarpExitTile(location, warp);

              Monitor.Log(
                  $"A*: {from} ({position}) -> {to} warp exit ({exitTile})",
                  LogLevel.Info);

              int distance = aStar.FindDistance(
                  location,
                  position,
                  exitTile);

              if (distance == int.MaxValue)
                  return int.MaxValue;

              totalDistance += distance;

              location = Game1.getLocationFromName(to);
              position = warp.ArrivalTile;

              continue;
          }

          // Then check building connections.
          BuildingConnection? building =
              worldLocation.Buildings
                  .FirstOrDefault(b => b.TargetName == to);

          if (building != null)
          {
            Monitor.Log(
                $"A*: {from} ({position}) -> {to} building exit ({building.ExitTile})",
                LogLevel.Info);

              int distance = aStar.FindDistance(
                  location,
                  position,
                  building.ExitTile);

              if (distance == int.MaxValue)
                  return int.MaxValue;

              totalDistance += distance;

              location = Game1.getLocationFromName(to);
              position = building.ArrivalTile;

              continue;
          }

          return int.MaxValue;
      }

      // Finally, walk from the last arrival point to the bed.
      if (location.Name == HomeLocation)
      {
          Point bedTile = GetBedTile(location);

          int distanceToBed =
              aStar.FindDistance(
                  location,
                  position,
                  bedTile);

          if (distanceToBed == int.MaxValue)
              return int.MaxValue;

          totalDistance += distanceToBed;
      }

      return totalDistance;
  }

  //temp bfs solution 
  private List<string>? FindLocationPath(
    string startLocation,
    string targetLocation)
  {
      var queue = new Queue<string>();
      var previous = new Dictionary<string, string>();

      queue.Enqueue(startLocation);
      previous[startLocation] = "";

      while (queue.Count > 0)
      {
          string current = queue.Dequeue();

          if (current == targetLocation)
              break;

          WorldLocation? location =
              this.worldGraph.GetLocation(current);

          if (location == null)
              continue;

          // Normal warps
          foreach (WarpConnection warp in location.Warps)
          {
              string next = warp.TargetName;

              if (previous.ContainsKey(next))
                  continue;

              previous[next] = current;
              queue.Enqueue(next);
          }

          // Building entrances
          foreach (BuildingConnection building in location.Buildings)
          {
              string next = building.TargetName;

              if (previous.ContainsKey(next))
                  continue;

              previous[next] = current;
              queue.Enqueue(next);
          }
      }

      if (!previous.ContainsKey(targetLocation))
          return null;

      var path = new List<string>();
      string locationName = targetLocation;

      while (locationName != "")
      {
          path.Add(locationName);
          locationName = previous[locationName];
      }

      path.Reverse();

      return path;
  }

  private Point GetWarpExitTile(
    GameLocation location,
    WarpConnection warp)
  {
      Point tile = warp.ExitTile;

      if (tile.X < 0)
          tile.X = 0;

      if (tile.Y < 0)
          tile.Y = 0;

      int width = location.Map.Layers[0].LayerWidth;
      int height = location.Map.Layers[0].LayerHeight;

      if (tile.X >= width)
          tile.X = width - 1;

      if (tile.Y >= height)
          tile.Y = height - 1;

      return tile;
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
  private Point GetBedTile(GameLocation location)
  {
      // TODO: Find the actual bed dynamically.
      return new Point(5, 8);
  }

}