using StardewValley;
using Microsoft.Xna.Framework;

namespace StardewModTimeToSleep.PathFinding;

internal sealed class PathNode
{
  public Point Position { get; }

  //Distance traveled from start
  public int CostFromStart { get; set; }

  //Estimate remaining distance to the goal
  public int EstimatedCostToGoal { get; set; }

  //Total estimated cost
  public int TotalEstimatedCost => CostFromStart + EstimatedCostToGoal;

  public PathNode? Parent { get; set; }

  public PathNode(Point position)
  {
    Position = position;
  }
}