using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewModTimeToSleep.World;

namespace StardewModTimeToSleep
{
  /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {

        /*********
        ** Fields
        *********/
        private WorldGraphBuilder worldGraphBuilder = null!;

        /*********
        ** Public methods
        *********/
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {

            this.worldGraphBuilder = new WorldGraphBuilder(this.Monitor);

            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
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
            this.worldGraphBuilder.InitLocationData();
            this.Monitor.Log("World graph built.", LogLevel.Debug);
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            //Every 5 seconds (and when game isn't paused), run logic loop
            if(e.IsMultipleOf(500)&&!Game1.paused)
            {
                //Updates the time to home based on player's current location
                this.updateTimeToHome();
                // this.Monitor.Log($"{Game1.player.Name} is at {Game1.player.currentLocation}, loc", LogLevel.Debug);
                // this.Monitor.Log($"{Game1.player.Name} is at {Game1.player.Tile.X}, {Game1.player.Tile.Y}, tile vector", LogLevel.Debug);
            }
        }

        /// <summary>Updates the timeToHome</summary>
        private void updateTimeToHome()
        {
            // todo 
        }

    }
  
}