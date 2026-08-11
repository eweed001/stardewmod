using System.Drawing;

internal sealed record Pathkey(
  string LocationName,
  Point Start,
  Point End
);