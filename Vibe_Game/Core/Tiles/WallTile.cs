using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    public sealed class WallTile : Tile
    {
        public WallTile(Point gridPosition) : base(gridPosition)
        {
        }

        public override bool IsWalkable => false;
        public override bool CanHostEnemy => false;
        public override Color Tint => GameColors.Wall;
    }
}
