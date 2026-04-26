using Microsoft.Xna.Framework;

namespace Vibe_Game.Core.Tiles
{
    public abstract class Tile
    {
        protected Tile(Point gridPosition)
        {
            GridPosition = gridPosition;
        }

        public Point GridPosition { get; }
        public bool HasEnemy { get; set; }
        public virtual bool IsWalkable => true;
        public virtual bool HasButton => false;
        public virtual bool CanHostEnemy => IsWalkable && !HasButton;
        public abstract Color Tint { get; }
        public bool ReducesFriction { get; set; } = false;
    }
}
