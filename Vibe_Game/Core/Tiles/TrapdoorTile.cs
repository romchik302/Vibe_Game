using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    public sealed class TrapdoorTile : Tile
    {
        public TrapdoorTile(Point gridPosition, int targetFloorIndex) : base(gridPosition)
        {
            TargetFloorIndex = targetFloorIndex;
        }

        public int TargetFloorIndex { get; }
        public override bool CanHostEnemy => false;
        public override Color Tint => GameColors.Trapdoor;
    }
}
