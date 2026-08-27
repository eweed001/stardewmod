using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewModdingAPI;
using StardewModTimeToSleep.services;
using StardewValley.Menus;

namespace StardewModTimeToSleep.ui;

internal class TimeToSleepWidget
{
  private readonly IMonitor Monitor;

  private PathFindingResult? result;

  public TimeToSleepWidget(IMonitor monitor)
  {
    this.Monitor = monitor;
  }

  public void SetResult(PathFindingResult? result)
  {
    this.result = result;
  }

  public void Draw()
  {
      if (this.result == null)
          return;

      string arrivalText =
          $"Home by: {this.FormatTime(this.result.ArrivalTime)}";

      string distanceText =
          $"{this.result.Distance} tiles";

      Vector2 arrivalSize =
          Game1.smallFont.MeasureString(arrivalText);

      Vector2 distanceSize =
          Game1.smallFont.MeasureString(distanceText);

      float width = Math.Max(
          arrivalSize.X,
          distanceSize.X);

      float padding = 16f;
      float spacing = 4f;

      float height =
          arrivalSize.Y +
          distanceSize.Y +
          spacing +
          padding * 2;

      float clockLeft =
        Game1.uiViewport.Width - 300 + 32;

      float gap = 55f;

      float x =
          clockLeft -
          width -
          gap;

      float y = 20f;

      Rectangle background = new Rectangle(
        (int)(x - padding),
        (int)(y - padding),
        (int)(width + padding * 2),
        (int)height
      );

      // Vanilla-style HUD background.
      DrawHudBox(Game1.spriteBatch, background);

      Utility.drawTextWithShadow(
          Game1.spriteBatch,
          arrivalText,
          Game1.smallFont,
          new Vector2(x, y),
          Game1.textColor
      );

      Utility.drawTextWithShadow(
          Game1.spriteBatch,
          distanceText,
          Game1.smallFont,
          new Vector2(
              x,
              y + arrivalSize.Y + spacing
          ),
          Game1.textColor
      );
  }

  private string FormatTime(int time)
  {
      int hour = time / 100;
      int minute = time % 100;

      string period = hour >= 12 ? "PM" : "AM";

      if (hour == 0)
          hour = 12;
      else if (hour > 12)
          hour -= 12;

      return $"{hour}:{minute:00} {period}";
  }

  private void DrawHudBox(SpriteBatch b, Rectangle destination)
  {
      IClickableMenu.drawTextureBox(
          b,
          Game1.menuTexture,
          new Rectangle(0, 256, 60, 60),
          destination.X,
          destination.Y,
          destination.Width,
          destination.Height,
          Color.White,
          drawShadow: false
      );
  }

}