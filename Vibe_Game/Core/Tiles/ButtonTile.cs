using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    public sealed class ButtonTile : Tile
    {
        public ButtonTile(Point gridPosition) : base(gridPosition)
        {
        }

        public bool IsPressed { get; private set; }
        public override bool HasButton => true;
        public override bool CanHostEnemy => false;
        public override Color Tint => IsPressed ? GameColors.ButtonUnlocked : GameColors.ButtonLocked;

        public void Press()
        {
            IsPressed = true;
        }
    }
}
