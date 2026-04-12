using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Services;
using Vibe_Game.Gameplay.Entities.Player;
using Vibe_Game.Gameplay.Projectiles;

namespace Vibe_Game.Scenes
{
    internal sealed class GameSceneState
    {
        public Player Player { get; set; }
        public Room[,] FloorMap { get; set; }
        public Point CurrentRoomGrid { get; set; }
        public Point LastRoomGrid { get; set; } = new Point(-1, -1);
        public Vector2 CameraPosition { get; set; }
        public List<Projectile> Projectiles { get; } = new();
    }
}
