using System;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Locations;
using StardewModTimeToSleep.utils;

namespace StardewModTimeToSleep
{
  /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {

        /*********
        ** Fields
        *********/
        private LocationUtils locationUtils = null!;
        private int timeToHome;

        /*********
        ** Public methods
        *********/
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {

            this.locationUtils = new(this.Monitor);

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
            //Initializing the location data (currently only outdoor locations)
            this.locationUtils.initLocationData();
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
            GameLocation gameLocation = Game1.player.currentLocation;
            //Using the data initialized in the onSaveLoaded function, calculate 
            //the distance and corresponding time to home based on player's 
            //current location
            Vector2 tileLocation = Game1.player.Tile;
            var distToHome = this.calculateDistanceToHome(gameLocation, tileLocation);
            var updatedTimeToHome = this.calculateTimeToHome(distToHome, Game1.timeOfDay);
            this.updateTimeArriveHomeWidget(timeToHome, updatedTimeToHome);
            // this.Monitor.Log($"{Game1.player.Name} is at {xPos}, {yPos}", LogLevel.Debug); 
        }

        /// <summary>Calculates the distance to home, in tiles</summary>
        /// <param name="gameLocation">The Player's current GameLocation.</param>
        /// <param name="currentTileLocation">The Player's current Tile Location.</param>
        private int calculateDistanceToHome(GameLocation gameLocation, Vector2 currentTileLocation)
        {
            //Return a number representing the total number of tiles you have to
            //traverse to make it home based on currentTileLocation

            //dummy return value for now
            return 5;
        }
        
        /// <summary>Calculates the time it will be when the player arrives home, 
        /// based on the number of tiles to home</summary>
        /// <param name="distToHome">Distance to home from Player's current position, in tiles.</param>
        /// <param name="time">The current time.</param>
        private int calculateTimeToHome(int distToHome, int time)
        {
            //take in the total number of tiles to traverse to get home
            //and the current time

            //Convert the total number of tiles need to traverse into time to
            //travel, using the average speed

            //return the current time + time required to traverse

            //dummy return value for now
            return 4;
            
        }

        /// <summary>Updates the arrival time widget.</summary>
        /// <param name="time">Existing time displayed in widget.</param>
        /// <param name="updatedTime">Newly calculated arrival time.</param>
        private void updateTimeArriveHomeWidget(int time, int updatedTime)
        {
            //render a widget displaying the time you'd arrive home if you 
            //left right now, skip any changes if time and updatedTime match
            
        }
    }
  
}