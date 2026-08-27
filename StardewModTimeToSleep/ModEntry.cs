using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewModTimeToSleep.World;
using StardewModTimeToSleep.PathFinding;
using Microsoft.Xna.Framework;
using StardewModTimeToSleep.services;
using StardewModTimeToSleep.ui;

namespace StardewModTimeToSleep
{
  /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {

        /*********
        ** Fields
        *********/
        private WorldGraphService worldGraphService = null!;
        private AStar aStar = null!;
        private PathFindingService pathFindingService = null!;
        private TimeToSleepWidget? timeToSleepWidget;

        /*********
        ** Public methods
        *********/
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {

            this.worldGraphService = new WorldGraphService(this.Monitor);
            this.aStar = new AStar(this.Monitor);
            this.pathFindingService = new PathFindingService(this.Monitor, 
                this.worldGraphService);
            this.timeToSleepWidget = new TimeToSleepWidget(this.Monitor);

            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Display.Rendered += this.OnRendered;
            helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
        }

        //Todos:
        //- skip for events
        //- handle mines
        //- error handling

        /*********
        ** Private methods
        *********/

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.Monitor.Log("Building world graph...", LogLevel.Debug);
            this.worldGraphService.InitLocationData();
            this.Monitor.Log("World graph built.", LogLevel.Debug);
        }

        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.paused)
                return;

            this.updateTimeToHome();
        }

        private void OnRendered(object? sender, RenderedEventArgs e)
        {
            if (Game1.activeClickableMenu == null)
                this.timeToSleepWidget?.Draw();
        }

        /// <summary>Updates the timeToHome</summary>
        private void updateTimeToHome()
        {
            GameLocation location = Game1.player.currentLocation;
            Point start = Game1.player.TilePoint;

            Monitor.Log(
                $"Location: {Game1.currentLocation.Name}, " +
                $"Player: {Game1.player.TilePoint}",
                LogLevel.Info
            );
            
            PathFindingResult? result = this.pathFindingService.FindDistanceToHome(
                location,
                start
            );
            if (result == null)
                return;

            this.timeToSleepWidget?.SetResult(result);

            Monitor.Log(
                $"Distance: {result.Distance} tiles, " +
                $"Travel time: {result.TravelTime:F1} minutes, " +
                $"Arrival: {result.ArrivalTime}",
                LogLevel.Info);
        }

    }
  
}