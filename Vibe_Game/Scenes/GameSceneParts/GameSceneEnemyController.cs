using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Scenes
{
    internal sealed class GameSceneEnemyController
    {
        private readonly GameSceneState _state;
        private readonly GameSceneWorld _world;
        private readonly IFlyingCollisionChecker _flyingCollision;
        private readonly IWallCollisionChecker _wallCollision;

        public GameSceneEnemyController(GameSceneState state, GameSceneWorld world)
        {
            _state = state;
            _world = world;
            _flyingCollision = new SceneFlyingCollisionChecker(world);
            _wallCollision = new SceneWallCollisionChecker(world);
        }

        public void SpawnEnemies(int floorIndex)
        {
            SpawnFlyingEnemiesInRooms(floorIndex);
            SpawnChasingEnemiesInRooms(floorIndex);
            SpawnAdaptiveChasingEnemiesInRooms(floorIndex);
        }

        public void ActivateEnemies(Point grid)
        {
            Room room = _state.FloorMap[grid.X, grid.Y];
            room?.enemies?.ForEach(e => e.Activate());
        }

        public void Update(GameTime gameTime)
        {
            Rectangle playerBounds = _state.Player.GetBounds();

            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    Room room = _state.FloorMap[x, y];
                    if (room?.enemies == null)
                        continue;

                    for (int i = room.enemies.Count - 1; i >= 0; i--)
                    {
                        Enemy enemy = room.enemies[i];

                        if (enemy is FlyingEnemy flying)
                            flying.ChaseTarget = _state.Player.Position;
                        else if (enemy is ChasingEnemy chasing)
                            chasing.ChaseTarget = _state.Player.Position;
                        else if (enemy is AdaptiveChasingEnemy adaptive)
                            adaptive.ChaseTarget = _state.Player.Position;

                        if (enemy.GetBounds().Intersects(playerBounds))
                        {
                            _state.Player.TakeDamage(1.0f);

                            Vector2 toEnemy = enemy.Position - _state.Player.Position;
                            float distance = toEnemy.Length();
                            if (distance > 0.001f)
                            {
                                toEnemy.Normalize();

                                float playerRadius = PlayerConfig.Radius;
                                float enemyRadius = GetEnemyRadius(enemy);
                                float minDistance = playerRadius + enemyRadius - enemy.PenetrationRadius;

                                if (distance < minDistance)
                                {
                                    Vector2 pushDir = toEnemy;
                                    float pushAmount = minDistance - distance;

                                    Vector2 oldEnemyPos = enemy.Position;
                                    Vector2 oldPlayerPos = _state.Player.Position;

                                    const float enemyPushCoefficient = 0.2f;
                                    enemy.Position += pushDir * pushAmount * enemyPushCoefficient;
                                    enemy.Position = _world.ResolveEnemyWallCollision(enemy, oldEnemyPos, enemy.Position);

                                    const float playerResistanceCoefficient = 0.1f;
                                    _state.Player.Position -= pushDir * pushAmount * playerResistanceCoefficient;
                                    _world.CheckTileCollision(oldPlayerPos);
                                }
                            }
                        }

                        enemy.Update(gameTime);

                        if (!enemy.IsAlive)
                            room.enemies.RemoveAt(i);
                    }

                    for (int j = 0; j < room.enemies.Count; j++)
                    {
                        for (int k = j + 1; k < room.enemies.Count; k++)
                        {
                            Enemy enemy1 = room.enemies[j];
                            Enemy enemy2 = room.enemies[k];

                            if (!enemy1.IsAlive || !enemy2.IsAlive)
                                continue;

                            Rectangle bounds1 = enemy1.GetBounds();
                            Rectangle bounds2 = enemy2.GetBounds();

                            if (bounds1.Intersects(bounds2))
                            {
                                Vector2 toEnemy2 = enemy2.Position - enemy1.Position;
                                float distance = toEnemy2.Length();
                                if (distance > 0.001f)
                                {
                                    toEnemy2.Normalize();

                                    float radius1 = GetEnemyRadius(enemy1);
                                    float radius2 = GetEnemyRadius(enemy2);
                                    float minDistance = radius1 + radius2 - enemy1.PenetrationRadius - enemy2.PenetrationRadius;

                                    if (distance < minDistance)
                                    {
                                        Vector2 pushDir = toEnemy2;
                                        float pushAmount = (minDistance - distance) * 0.5f;

                                        Vector2 oldPos1 = enemy1.Position;
                                        Vector2 oldPos2 = enemy2.Position;

                                        enemy1.Position -= pushDir * pushAmount;
                                        enemy2.Position += pushDir * pushAmount;

                                        enemy1.Position = _world.ResolveEnemyWallCollision(enemy1, oldPos1, enemy1.Position);
                                        enemy2.Position = _world.ResolveEnemyWallCollision(enemy2, oldPos2, enemy2.Position);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int ex = 0; ex < WorldConfig.GridSize; ex++)
            {
                for (int ey = 0; ey < WorldConfig.GridSize; ey++)
                {
                    Room room = _state.FloorMap[ex, ey];
                    if (room?.enemies == null)
                        continue;

                    foreach (var enemy in room.enemies)
                    {
                        if (enemy.IsAlive)
                            enemy.Draw(spriteBatch);
                    }
                }
            }
        }

        public void DamageEnemiesInArea(Vector2 center, float radius, int damage)
        {
            int rx = (int)(center.X / WorldConfig.RoomWidthPx);
            int ry = (int)(center.Y / WorldConfig.RoomHeightPx);

            rx = Math.Clamp(rx, 0, WorldConfig.GridSize - 1);
            ry = Math.Clamp(ry, 0, WorldConfig.GridSize - 1);

            Room room = _state.FloorMap[rx, ry];
            if (room?.enemies == null)
                return;

            for (int i = room.enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = room.enemies[i];
                if (!enemy.IsAlive)
                    continue;

                float distance = Vector2.Distance(center, enemy.Position);
                if (distance <= radius)
                    enemy.TakeDamage(damage);
            }
        }

        public object GetEnemyAtPoint(Vector2 point, float radius)
        {
            int rx = (int)(point.X / WorldConfig.RoomWidthPx);
            int ry = (int)(point.Y / WorldConfig.RoomHeightPx);

            rx = Math.Clamp(rx, 0, WorldConfig.GridSize - 1);
            ry = Math.Clamp(ry, 0, WorldConfig.GridSize - 1);

            Room room = _state.FloorMap[rx, ry];
            if (room?.enemies == null)
                return null;

            foreach (var enemy in room.enemies)
            {
                if (!enemy.IsAlive)
                    continue;

                float distance = Vector2.Distance(point, enemy.Position);
                if (distance <= radius)
                    return enemy;
            }

            return null;
        }

        public void DamageEnemy(object enemy, int damage)
        {
            if (enemy is Enemy castedEnemy && castedEnemy.IsAlive)
                castedEnemy.TakeDamage(damage);
        }

        public void ApplyRecoilToEnemy(object enemy, Vector2 recoilDirection, float recoilForce)
        {
            if (enemy is Enemy castedEnemy && castedEnemy.IsAlive)
                castedEnemy.ApplyRecoil(recoilDirection, recoilForce);
        }

        public List<object> GetEnemiesInArea(Rectangle bounds)
        {
            List<object> result = new List<object>();

            int startX = Math.Max(0, (int)(bounds.Left / WorldConfig.RoomWidthPx));
            int endX = Math.Min(WorldConfig.GridSize - 1, (int)(bounds.Right / WorldConfig.RoomWidthPx));
            int startY = Math.Max(0, (int)(bounds.Top / WorldConfig.RoomHeightPx));
            int endY = Math.Min(WorldConfig.GridSize - 1, (int)(bounds.Bottom / WorldConfig.RoomHeightPx));

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    Room room = _state.FloorMap[x, y];
                    if (room?.enemies == null)
                        continue;

                    foreach (var enemy in room.enemies)
                    {
                        if (!enemy.IsAlive)
                            continue;

                        Rectangle enemyBounds = enemy.GetBounds();
                        if (bounds.Intersects(enemyBounds))
                            result.Add(enemy);
                    }
                }
            }

            return result;
        }

        private void SpawnFlyingEnemiesInRooms(int floorIndex)
        {
            var rng = new Random(unchecked(floorIndex * 397 ^ 0x5EED));

            for (int gx = 0; gx < WorldConfig.GridSize; gx++)
            {
                for (int gy = 0; gy < WorldConfig.GridSize; gy++)
                {
                    Room room = _state.FloorMap[gx, gy];
                    if (room == null)
                        continue;

                    if (room.Type == LevelGenerator.RoomType.Start)
                        continue;

                    if (rng.NextDouble() > EnemyConfig.FlyingSpawnChancePerRoom)
                        continue;

                    int enemyCount = rng.Next(2, 6);

                    for (int i = 0; i < enemyCount; i++)
                    {
                        Vector2 spawnWorld = _world.GetRandomFreeTilePosition(room, gx, gy, rng);
                        room.enemies.Add(new FlyingEnemy(spawnWorld, _flyingCollision));
                    }
                }
            }
        }

        private void SpawnChasingEnemiesInRooms(int floorIndex)
        {
            var rng = new Random(unchecked(floorIndex * 397 ^ 0x5EED));

            for (int gx = 0; gx < WorldConfig.GridSize; gx++)
            {
                for (int gy = 0; gy < WorldConfig.GridSize; gy++)
                {
                    Room room = _state.FloorMap[gx, gy];
                    if (room == null || room.Type == LevelGenerator.RoomType.Start)
                        continue;

                    if (rng.NextDouble() > EnemyConfig.ChasingSpawnChancePerRoom)
                        continue;

                    int enemyCount = rng.Next(1, 4);

                    for (int i = 0; i < enemyCount; i++)
                    {
                        Vector2 spawnWorld = _world.GetRandomFreeTilePosition(room, gx, gy, rng);
                        room.enemies.Add(new ChasingEnemy(spawnWorld, _wallCollision));
                    }
                }
            }
        }

        private void SpawnAdaptiveChasingEnemiesInRooms(int floorIndex)
        {
            var rng = new Random(unchecked(floorIndex * 397 ^ 0x5EED));

            for (int gx = 0; gx < WorldConfig.GridSize; gx++)
            {
                for (int gy = 0; gy < WorldConfig.GridSize; gy++)
                {
                    Room room = _state.FloorMap[gx, gy];
                    if (room == null || room.Type == LevelGenerator.RoomType.Start)
                        continue;

                    if (rng.NextDouble() > EnemyConfig.AdaptiveChasingSpawnChance)
                        continue;

                    int enemyCount = rng.Next(1, 3);

                    for (int i = 0; i < enemyCount; i++)
                    {
                        Vector2 spawnWorld = _world.GetRandomFreeTilePosition(room, gx, gy, rng);
                        room.enemies.Add(new AdaptiveChasingEnemy(spawnWorld, _wallCollision));
                    }
                }
            }
        }

        private static float GetEnemyRadius(Enemy enemy)
        {
            if (enemy is FlyingEnemy) return EnemyConfig.DefaultFlyingRadius;
            if (enemy is ChasingEnemy) return EnemyConfig.DefaultChasingRadius;
            if (enemy is AdaptiveChasingEnemy) return EnemyConfig.DefaultChasingRadius;
            return 10f;
        }
    }
}
