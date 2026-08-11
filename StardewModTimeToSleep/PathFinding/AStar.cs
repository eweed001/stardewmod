using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewModdingAPI;

namespace StardewModTimeToSleep.PathFinding;

internal sealed class AStar
{

  private readonly IMonitor Monitor;

   public AStar(IMonitor monitor)
  {
      this.Monitor = monitor;
  }
  
  //Determines if an in game tile is considered walkable
  private bool IsWalkable(
    GameLocation location,
    Point tile
  )
  {
    return location.isTilePassable(tile.ToVector2());
  }

  private static bool IsInsideMap(GameLocation location, Point tile)
  {
    return tile.X >= 0
        && tile.Y >= 0
        && tile.X < location.Map.Layers[0].LayerWidth
        && tile.Y < location.Map.Layers[0].LayerHeight;
  }

  private static int Heuristic(Point a, Point b)
  {
    return Math.Abs(a.X-b.X) + Math.Abs(a.Y-b.Y);
  }

  private static readonly Point[] Directions =
  {
    new(0,-1),
    new(0,1),
    new(-1,0),
    new(1,0)
  };

  public int FindDistance(
    GameLocation location,
    Point start,
    Point goal
  )
  {

    int exploredNodes = 0;
    if (start == goal)
      return 0;
    
    var open = new PriorityQueue<PathNode, int>();

    var nodes = new Dictionary<Point, PathNode>();

    var closed = new HashSet<Point>();

    var startNode = new PathNode(start)
    {
      CostFromStart = 0,
      EstimatedCostToGoal = Heuristic(start, goal)
    };

    nodes[start] = startNode;
    open.Enqueue(startNode, startNode.TotalEstimatedCost);

    while (open.Count > 0)
    {
      var current = open.Dequeue();
      exploredNodes++;

      if (exploredNodes % 1000 == 0)
      {
          Monitor.Log($"A* explored {exploredNodes} nodes", LogLevel.Debug);
      }
      if (exploredNodes > 50000)
      {
          this.Monitor.Log(
              $"A* aborted after {exploredNodes} nodes. Goal unreachable?",
              LogLevel.Warn);

          return int.MaxValue;
      }

      if (current.Position == goal)
        return current.CostFromStart;
      
      if (!closed.Add(current.Position))
        continue;

      foreach (var direction in Directions)
      {
        Point next = new(
          current.Position.X + direction.X,
          current.Position.Y + direction.Y
        );

        if (!IsInsideMap(location, next))
          continue;

        if (closed.Contains(next))
          continue;
        
        // The goal can be a blocked tile (i.e. a doorway)
        if (next == goal)
        {
            return current.CostFromStart + 1;
        }

        if(!IsWalkable(location, next))
          continue;

        int newCost = current.CostFromStart + 1;

        if (!nodes.TryGetValue(next, out var node))
        {
          node = new PathNode(next);
          nodes[next] = node;
        }

        if (node.Parent == null || newCost < node.CostFromStart)
        {
          node.Parent = current;
          node.CostFromStart = newCost;
          node.EstimatedCostToGoal = Heuristic(next, goal);
          open.Enqueue(node, node.TotalEstimatedCost);
        }
      }
    }
    return int.MaxValue;
  }
}