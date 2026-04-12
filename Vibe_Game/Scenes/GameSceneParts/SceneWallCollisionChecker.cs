using Microsoft.Xna.Framework;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Scenes
{
    internal sealed class SceneWallCollisionChecker : IWallCollisionChecker
    {
        private readonly GameSceneWorld _world;

        public SceneWallCollisionChecker(GameSceneWorld world)
        {
            _world = world;
        }

        public bool IsPointBlockedByWall(Vector2 worldPosition)
        {
            return _world.IsPointBlockedByAllWalls(worldPosition);
        }
    }
}
