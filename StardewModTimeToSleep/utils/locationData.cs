using Microsoft.Xna.Framework;

namespace StardewModTimeToSleep.utils;

internal class LocData
{
  //Name of this location (GameLocation.name)
  public string Name { get;}

  //Outdoor root location which contains this location
  public string? Root { get; set;}

  //Parent location that contains this location
  public string? Parent { get; set;}

  //List of Neighboring locations
  public Dictionary<string, Vector2> Neighbors { get; } = [];

  //List of locations contained within this location
  public List<string> Children { get; } = [];

  //Integer representing the path length getting through this location
  public int? pathToExit { get; set; }

  //Map of warp target names to the length of the path to that target (from current location?)
  public Dictionary<string, int> pathLengths { get; } = [];

  //Constructor
  public LocData(String name)
  {
    this.Name = name;
  }

}