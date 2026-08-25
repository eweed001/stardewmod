using StardewValley;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModTimeToSleep.PathFinding;
using StardewModTimeToSleep.World;
using StardewValley.Locations;

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
    List<List<string>> paths = FindLocationPaths(
      currentLocation.Name,
      HomeLocation);

    if (paths.Count == 0)
    {
      Monitor.Log(
          $"Could not find any route from " +
          $"{currentLocation.Name} to {HomeLocation}.",
          LogLevel.Warn);

      return int.MaxValue;
    }

    int shortestDistance = int.MaxValue;
    List<string>? shortestPath = null;

    foreach (List<string> alternativePath in paths)
    {
      Monitor.Log(
        $"Possible route: " +
        $"{string.Join(" -> ", alternativePath)}",
        LogLevel.Info);

      int distance = CalculateRouteDistance(
        alternativePath,
        currentLocation,
        playerTile);

      Monitor.Log(
        $"Route distance: {distance} tiles",
        LogLevel.Info);

      if (distance < shortestDistance)
      {
        shortestDistance = distance;
        shortestPath = alternativePath;
      }
    }

    if (shortestPath == null ||
          shortestDistance == int.MaxValue)
    {
      Monitor.Log(
        $"Could not find a walkable route from " +
        $"{currentLocation.Name} to {HomeLocation}.",
        LogLevel.Warn);

      return int.MaxValue;
    }

    Monitor.Log(
      $"Shortest route: " +
      $"{string.Join(" -> ", shortestPath)} " +
      $"({shortestDistance} tiles)",
      LogLevel.Info);

    return shortestDistance;
  }

  private List<List<string>> FindLocationPaths(
    string startLocation,
    string targetLocation)
{
    var paths = new List<List<string>>();

    var currentPath = new List<string>
    {
        startLocation
    };

    var visited = new HashSet<string>
    {
        startLocation
    };

    FindPathsRecursive(
        startLocation,
        targetLocation,
        currentPath,
        visited,
        paths);

    return paths;
}


private void FindPathsRecursive(
    string currentLocation,
    string targetLocation,
    List<string> currentPath,
    HashSet<string> visited,
    List<List<string>> paths)
{
    if (currentLocation == targetLocation)
    {
        paths.Add(new List<string>(currentPath));
        return;
    }

    WorldLocation? location =
        this.worldGraph.GetLocation(currentLocation);

    if (location == null)
        return;

    HashSet<string> warpTargets = new();

    // Normal warps
    foreach (WarpConnection warp in location.Warps)
    {
      string next = warp.TargetName;

      if (!warpTargets.Add(next))
        continue;

      if (visited.Contains(next))
          continue;

      visited.Add(next);
      currentPath.Add(next);

      FindPathsRecursive(
          next,
          targetLocation,
          currentPath,
          visited,
          paths);

      currentPath.RemoveAt(currentPath.Count - 1);
      visited.Remove(next);
    }

    HashSet<string> buildingTargets = new();

    // Building entrances
    foreach (BuildingConnection building in location.Buildings)
    {
      string next = building.TargetName;

      if (!buildingTargets.Add(next))
        continue;

      if (visited.Contains(next))
          continue;

      visited.Add(next);
      currentPath.Add(next);

      FindPathsRecursive(
          next,
          targetLocation,
          currentPath,
          visited,
          paths);

      currentPath.RemoveAt(currentPath.Count - 1);
      visited.Remove(next);
    }
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

            Monitor.Log(
              $"{from} -> {to}: {distance} tiles",
              LogLevel.Debug);

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
        Point? bedTile = GetBedTile(location);

        if (bedTile == null)
          return int.MaxValue;

        int distanceToBed =
            aStar.FindDistance(
                location,
                position,
                bedTile.Value);

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

  private Point? GetBedTile(GameLocation location)
  {
    if (location is FarmHouse farmhouse)
    {
      return farmhouse.GetPlayerBedSpot();
    }
    Monitor.Log(
      $"Could not find bed in {location.Name}.",
      LogLevel.Warn);

    return null;
  }

}