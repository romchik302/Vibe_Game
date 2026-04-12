using Microsoft.Xna.Framework;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Scenes
{
    internal sealed class SceneFlyingCollisionChecker : IFlyingCollisionChecker
    {
        private readonly GameSceneWorld _world;

        public SceneFlyingCollisionChecker(GameSceneWorld world)
        {
            _world = world;
        }

        public bool IsFlyingBlocked(Vector2 worldPosition)
        {
            return _world.IsFlyingPointBlocked(worldPosition);
        }
    }
}
