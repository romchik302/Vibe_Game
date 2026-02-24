using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Gameplay.Entities.Player
{
	internal class PlayerContentLoader : IPlayerContentLoader
	{
		public bool IsContentLoaded { get; private set; } = false;

		public PlayerContentLoader() { }

		public void LoadContent(ContentManager content)
		{
			IsContentLoaded = true;
		}
	}
}
