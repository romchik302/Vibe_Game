using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    public sealed class DoorTile : Tile
    {
        public DoorTile(Point gridPosition) : base(gridPosition)
        {
        }

        public bool IsOpen { get; private set; }
        public override bool IsWalkable => IsOpen;
        public override bool CanHostEnemy => false;
        public override Color Tint => IsOpen ? GameColors.Floor : GameColors.Wall;

        public void SetOpen(bool isOpen)
        {
            IsOpen = isOpen;
        }
    }
}
