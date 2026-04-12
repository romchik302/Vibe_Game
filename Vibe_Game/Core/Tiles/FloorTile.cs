using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    public sealed class FloorTile : Tile
    {
        public FloorTile(Point gridPosition) : base(gridPosition)
        {
        }

        public override Color Tint => GameColors.Floor;
    }
}
