using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace StardewModTimeToSleep.utils;

internal class LocationUtils
{

  /*********
  ** Fields
  *********/
  /// <summary>The monitor with which to log.</summary>
  private readonly IMonitor Monitor;
  
  public Dictionary<string, LocData> locationData { get; } = [];


  /*********
  ** Public methods
  *********/
  
  /// <summary>Constructor.</summary>
  /// <param name="monitor">The monitor with which to log.</param>
  public LocationUtils(IMonitor monitor)
  {
    this.Monitor = monitor;
  }

  //<summary>Initialize the locationData by iterating through all GameLocations
  // and building the Neighbors. </summary>
  public Dictionary<string, LocData> initLocationData()
  {
    Monitor.Log("in init location data", LogLevel.Debug);
    foreach(var loc in Game1.locations) {
      if (loc.IsOutdoors)
      {
        LocData locData = new LocData(loc.Name) { Root = loc.Name };
        Monitor.Log("location name: " + loc.Name, LogLevel.Debug);

        //Initializing the Neighbors
        foreach (var warpLoc in loc.warps)
        {
          String targetName = warpLoc.TargetName;
          Monitor.Log("warp target name: " + loc.Name, LogLevel.Debug);
          if(!locData.Neighbors.ContainsKey(targetName))
          {
            locData.Neighbors.Add(targetName, new Vector2(warpLoc.X, warpLoc.Y));
          }
        }
        locationData.Add(loc.Name, locData);
      }
    }
    return locationData;
  }

  //todos:
  // - add ability to recursively scan interior locations
  // - add ability to calculate the path between warps within each location
  // - add ability to identify the path to home 
  
}
