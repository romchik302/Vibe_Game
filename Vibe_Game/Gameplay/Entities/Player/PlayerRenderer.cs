using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Player
{
    internal class PlayerRenderer : IPlayerRenderer
    {
        private Texture2D _pixel;

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Vector2 shootDirection, Color color)
        {
            if (spriteBatch == null)
                return;

            // Ленивая инициализация простой белой текстуры 1x1
            if (_pixel == null)
            {
                _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }

            // Используем константы из PlayerConfig для отрисовки
            var rect = new Rectangle(
                (int)position.X - PlayerConfig.Radius,
                (int)position.Y - PlayerConfig.Radius,
                PlayerConfig.Size,
                PlayerConfig.Size
            );

            spriteBatch.Draw(_pixel, rect, color);
        }
    }
}