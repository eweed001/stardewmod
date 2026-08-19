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

  private class RouteState
{
    public string LocationName { get; init; } = "";
    public Point Position { get; init; }
    public int Distance { get; init; }
    public List<string> Path { get; init; } = new();
}

  public int FindDistanceToHome(GameLocation currentLocation, Point playerTile)
  {

     List<string>? path = FindLocationPath(
        currentLocation,
        playerTile);

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

      return CalculateRouteDistance(path, 
        currentLocation, playerTile);
  }

  //temp bfs solution 
  private List<string>? FindLocationPath(
    GameLocation currentLocation,
    Point playerTile)
  {
      var queue = new PriorityQueue<RouteState, int>();

      var start = new RouteState
      {
          LocationName = currentLocation.Name,
          Position = playerTile,
          Distance = 0,
          Path = new List<string> { currentLocation.Name }
      };

      queue.Enqueue(start, 0);

      var bestDistance = new Dictionary<string, int>
      {
          [currentLocation.Name] = 0
      };

      while (queue.Count > 0)
      {
          RouteState state = queue.Dequeue();

          if (state.LocationName == HomeLocation)
              return state.Path;

          WorldLocation? worldLocation =
              worldGraph.GetLocation(state.LocationName);

          if (worldLocation == null)
              continue;

          GameLocation location =
              Game1.getLocationFromName(state.LocationName);

          // Normal warps
          foreach (WarpConnection warp in worldLocation.Warps)
          {
              GameLocation target =
                  Game1.getLocationFromName(warp.TargetName);

              Point exitTile =
                  GetWarpExitTile(location, warp);

              int distance =
                  aStar.FindDistance(
                      location,
                      state.Position,
                      exitTile);

              if (distance == int.MaxValue)
                  continue;

              int newDistance =
                  state.Distance + distance;

              if (bestDistance.TryGetValue(
                      warp.TargetName,
                      out int oldDistance)
                  && oldDistance <= newDistance)
              {
                  continue;
              }

              bestDistance[warp.TargetName] = newDistance;

              queue.Enqueue(
                  new RouteState
                  {
                      LocationName = warp.TargetName,
                      Position = warp.ArrivalTile,
                      Distance = newDistance,
                      Path = new List<string>(state.Path)
                      {
                          warp.TargetName
                      }
                  },
                  newDistance);
          }

          // Building connections
          foreach (BuildingConnection building in worldLocation.Buildings)
          {
              GameLocation target =
                  Game1.getLocationFromName(building.TargetName);

              int distance =
                  aStar.FindDistance(
                      location,
                      state.Position,
                      building.ExitTile);

              if (distance == int.MaxValue)
                  continue;

              int newDistance =
                  state.Distance + distance;

              if (bestDistance.TryGetValue(
                      building.TargetName,
                      out int oldDistance)
                  && oldDistance <= newDistance)
              {
                  continue;
              }

              bestDistance[building.TargetName] = newDistance;

              queue.Enqueue(
                  new RouteState
                  {
                      LocationName = building.TargetName,
                      Position = building.ArrivalTile,
                      Distance = newDistance,
                      Path = new List<string>(state.Path)
                      {
                          building.TargetName
                      }
                  },
                  newDistance);
          }
      }

      return null;
  }

  private int CalculateRouteDistance(
    List<string> path,
    GameLocation currentLocation,
    Point playerTile)
  {
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

          // Normal warp
          WarpConnection? warp =
              worldLocation.Warps
                  .FirstOrDefault(w => w.TargetName == to);

          if (warp != null)
          {
              Point exitTile =
                  GetWarpExitTile(location, warp);

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

          // Building connection
          BuildingConnection? building =
              worldLocation.Buildings
                  .FirstOrDefault(b => b.TargetName == to);

          if (building != null)
          {
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

      // Final path from arrival point to bed.
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