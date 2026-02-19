using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Vibe_Game.Core.Interfaces
{
    public interface IPlayerRenderer
    {
        void Draw(SpriteBatch spriteBatch, Vector2 position, Vector2 shootDirection, Color color);
    }
}
