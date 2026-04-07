using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Gameplay.Entities.Player
{
	internal class PlayerContentLoader : IPlayerContentLoader
	{
		public bool IsContentLoaded { get; private set; } = false;
		public Texture2D PlayerTexture { get; private set; }

		public PlayerContentLoader() { }

		public void LoadContent(ContentManager content)
		{
			PlayerTexture = content.Load<Texture2D>("player_sheet");
			IsContentLoaded = true;
		}
	}
}
