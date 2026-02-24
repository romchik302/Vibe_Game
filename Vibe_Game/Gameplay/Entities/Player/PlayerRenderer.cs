using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Gameplay.Entities.Player
{
	internal class PlayerRenderer : IPlayerRenderer
	{
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

			var rect = new Rectangle((int)position.X - 8, (int)position.Y - 8, 16, 16);
			spriteBatch.Draw(_pixel, rect, color);
		}

		private Texture2D _pixel;
	}
}
