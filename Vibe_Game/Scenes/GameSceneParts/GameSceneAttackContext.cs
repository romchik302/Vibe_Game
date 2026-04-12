using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Scenes
{
    internal sealed class GameSceneAttackContext : IAttackContext
    {
        private readonly GameSceneState _state;
        private readonly GameSceneWorld _world;
        private readonly GameSceneProjectileController _projectiles;
        private readonly GameSceneEnemyController _enemies;
        private GameTime _gameTime;

        public GameSceneAttackContext(
            GameSceneState state,
            GameSceneWorld world,
            GameSceneProjectileController projectiles,
            GameSceneEnemyController enemies)
        {
            _state = state;
            _world = world;
            _projectiles = projectiles;
            _enemies = enemies;
        }

        public GameTime GameTime => _gameTime;
        public SpriteBatch SpriteBatch => null;

        public void Sync(GameTime gameTime)
        {
            _gameTime = gameTime;
        }

        public void SpawnProjectile(ProjectileSpawnArgs args)
        {
            _projectiles.Spawn(args);
        }

        public bool WouldCollideAtWorld(Vector2 worldPosition, float collisionRadius)
        {
            return _world.IsWorldPointBlocked(worldPosition);
        }

        public void DamageEnemiesInArea(Vector2 center, float radius, int damage)
        {
            _enemies.DamageEnemiesInArea(center, radius, damage);
        }

        public object GetEnemyAtPoint(Vector2 point, float radius)
        {
            return _enemies.GetEnemyAtPoint(point, radius);
        }

        public void DamageEnemy(object enemy, int damage)
        {
            _enemies.DamageEnemy(enemy, damage);
        }

        public void ApplyRecoilToEnemy(object enemy, Vector2 recoilDirection, float recoilForce)
        {
            _enemies.ApplyRecoilToEnemy(enemy, recoilDirection, recoilForce);
        }

        public Vector2 GetPlayerPosition()
        {
            return _state.Player.Position;
        }

        public Vector2 GetCameraPosition()
        {
            return _state.CameraPosition;
        }

        public List<object> GetEnemiesInArea(Rectangle bounds)
        {
            return _enemies.GetEnemiesInArea(bounds);
        }
    }
}
