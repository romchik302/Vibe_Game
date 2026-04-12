using System;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Scenes
{
    internal sealed class GameSceneWorld
    {
        private readonly GameSceneState _state;

        public GameSceneWorld(GameSceneState state)
        {
            _state = state;
        }

        public Point GetRoomGridAtWorldPosition(Vector2 worldPosition)
        {
            int rx = (int)Math.Floor(worldPosition.X / WorldConfig.RoomWidthPx);
            int ry = (int)Math.Floor(worldPosition.Y / WorldConfig.RoomHeightPx);

            return new Point(
                Math.Clamp(rx, 0, WorldConfig.GridSize - 1),
                Math.Clamp(ry, 0, WorldConfig.GridSize - 1)
            );
        }

        public Room GetRoomAtGrid(Point roomGrid)
        {
            return _state.FloorMap[roomGrid.X, roomGrid.Y];
        }

        public bool IsFlyingPointBlocked(Vector2 worldPosition)
        {
            Point roomGrid = GetRoomGridAtWorldPosition(worldPosition);
            Room room = _state.FloorMap[roomGrid.X, roomGrid.Y];
            if (room == null)
                return true;

            float lx = worldPosition.X % WorldConfig.RoomWidthPx;
            float ly = worldPosition.Y % WorldConfig.RoomHeightPx;
            if (lx < 0) lx += WorldConfig.RoomWidthPx;
            if (ly < 0) ly += WorldConfig.RoomHeightPx;

            int tx = (int)Math.Floor(lx / WorldConfig.TileSize);
            int ty = (int)Math.Floor(ly / WorldConfig.TileSize);

            bool interior = tx >= 1 && tx < WorldConfig.RoomWidthTiles - 1
                && ty >= 1 && ty < WorldConfig.RoomHeightTiles - 1;
            if (interior)
                return false;

            return IsPointWall(room, lx, ly);
        }

        public bool IsPointBlockedByAllWalls(Vector2 worldPosition)
        {
            Point roomGrid = GetRoomGridAtWorldPosition(worldPosition);
            Room room = _state.FloorMap[roomGrid.X, roomGrid.Y];
            if (room == null)
                return true;

            float lx = worldPosition.X % WorldConfig.RoomWidthPx;
            float ly = worldPosition.Y % WorldConfig.RoomHeightPx;
            if (lx < 0) lx += WorldConfig.RoomWidthPx;
            if (ly < 0) ly += WorldConfig.RoomHeightPx;

            int tx = (int)Math.Floor(lx / WorldConfig.TileSize);
            int ty = (int)Math.Floor(ly / WorldConfig.TileSize);

            if (tx < 0 || tx >= WorldConfig.RoomWidthTiles || ty < 0 || ty >= WorldConfig.RoomHeightTiles)
                return true;

            return room.Tiles[tx, ty] == TileType.Wall;
        }

        public void CheckTileCollision(Vector2 oldPos)
        {
            Vector2 targetPos = _state.Player.Position;
            Vector2 finalPos = targetPos;

            if (targetPos.X != oldPos.X)
            {
                if (HasCollision(new Vector2(targetPos.X, oldPos.Y)))
                {
                    float offset = PlayerConfig.CollisionOffset;
                    if (targetPos.X > oldPos.X)
                    {
                        float rightEdge = targetPos.X + offset;
                        int tileX = (int)(rightEdge / WorldConfig.TileSize);
                        finalPos.X = tileX * WorldConfig.TileSize - offset - 0.01f;
                    }
                    else
                    {
                        float leftEdge = targetPos.X - offset;
                        int tileX = (int)Math.Floor(leftEdge / WorldConfig.TileSize);
                        finalPos.X = (tileX + 1) * WorldConfig.TileSize + offset + 0.01f;
                    }
                }
            }

            if (targetPos.Y != oldPos.Y)
            {
                if (HasCollision(new Vector2(finalPos.X, targetPos.Y)))
                {
                    float offset = PlayerConfig.CollisionOffset;
                    if (targetPos.Y > oldPos.Y)
                    {
                        float bottomEdge = targetPos.Y + offset;
                        int tileY = (int)(bottomEdge / WorldConfig.TileSize);
                        finalPos.Y = tileY * WorldConfig.TileSize - offset - 0.01f;
                    }
                    else
                    {
                        float topEdge = targetPos.Y - offset;
                        int tileY = (int)Math.Floor(topEdge / WorldConfig.TileSize);
                        finalPos.Y = (tileY + 1) * WorldConfig.TileSize + offset + 0.01f;
                    }
                }
            }

            _state.Player.Position = finalPos;

            int rx = (int)Math.Floor(_state.Player.Position.X / WorldConfig.RoomWidthPx);
            int ry = (int)Math.Floor(_state.Player.Position.Y / WorldConfig.RoomHeightPx);
            _state.CurrentRoomGrid = new Point(
                Math.Clamp(rx, 0, WorldConfig.GridSize - 1),
                Math.Clamp(ry, 0, WorldConfig.GridSize - 1)
            );
        }

        public bool IsWorldPointBlocked(Vector2 worldPosition)
        {
            Point roomGrid = GetRoomGridAtWorldPosition(worldPosition);
            Room room = _state.FloorMap[roomGrid.X, roomGrid.Y];
            if (room == null)
                return true;

            float lx = worldPosition.X % WorldConfig.RoomWidthPx;
            float ly = worldPosition.Y % WorldConfig.RoomHeightPx;
            if (lx < 0) lx += WorldConfig.RoomWidthPx;
            if (ly < 0) ly += WorldConfig.RoomHeightPx;

            return IsPointWall(room, lx, ly);
        }

        public Vector2 ResolveEnemyWallCollision(Enemy enemy, Vector2 oldPos, Vector2 newPos)
        {
            Vector2 delta = newPos - oldPos;
            Vector2 finalPos = newPos;

            float radius = GetEnemyRadius(enemy);

            if (delta.X != 0f)
            {
                bool isBlocked;
                if (enemy is FlyingEnemy)
                {
                    isBlocked = IsFlyingPointBlocked(new Vector2(newPos.X, oldPos.Y)) ||
                                IsFlyingPointBlocked(new Vector2(newPos.X + radius, oldPos.Y)) ||
                                IsFlyingPointBlocked(new Vector2(newPos.X - radius, oldPos.Y));
                }
                else
                {
                    isBlocked = IsPointBlockedByAllWalls(new Vector2(newPos.X, oldPos.Y)) ||
                                IsPointBlockedByAllWalls(new Vector2(newPos.X + radius, oldPos.Y)) ||
                                IsPointBlockedByAllWalls(new Vector2(newPos.X - radius, oldPos.Y));
                }

                if (isBlocked)
                    finalPos.X = oldPos.X;
            }

            if (delta.Y != 0f)
            {
                bool isBlocked;
                if (enemy is FlyingEnemy)
                {
                    isBlocked = IsFlyingPointBlocked(new Vector2(finalPos.X, newPos.Y)) ||
                                IsFlyingPointBlocked(new Vector2(finalPos.X, newPos.Y + radius)) ||
                                IsFlyingPointBlocked(new Vector2(finalPos.X, newPos.Y - radius));
                }
                else
                {
                    isBlocked = IsPointBlockedByAllWalls(new Vector2(finalPos.X, newPos.Y)) ||
                                IsPointBlockedByAllWalls(new Vector2(finalPos.X, newPos.Y + radius)) ||
                                IsPointBlockedByAllWalls(new Vector2(finalPos.X, newPos.Y - radius));
                }

                if (isBlocked)
                    finalPos.Y = oldPos.Y;
            }

            return finalPos;
        }

        public void TryUnlockButton()
        {
            Room room = _state.FloorMap[_state.CurrentRoomGrid.X, _state.CurrentRoomGrid.Y];
            if (room == null)
                return;

            float lx = _state.Player.Position.X % WorldConfig.RoomWidthPx;
            float ly = _state.Player.Position.Y % WorldConfig.RoomHeightPx;

            Rectangle playerRect = new Rectangle(
                (int)lx - PlayerConfig.Radius,
                (int)ly - PlayerConfig.Radius,
                PlayerConfig.Size,
                PlayerConfig.Size
            );

            Rectangle buttonRect = new Rectangle(
                room.ButtonPos.X * WorldConfig.TileSize,
                room.ButtonPos.Y * WorldConfig.TileSize,
                WorldConfig.TileSize,
                WorldConfig.TileSize
            );

            if (playerRect.Intersects(buttonRect))
                room.IsLocked = false;
        }

        public void UpdateCamera(Camera camera)
        {
            Vector2 target = new Vector2(
                _state.CurrentRoomGrid.X * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                _state.CurrentRoomGrid.Y * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f
            );

            _state.CameraPosition = Vector2.Lerp(_state.CameraPosition, target, 0.1f);
            camera?.Follow(_state.CameraPosition);
        }

        public Vector2 GetRandomFreeTilePosition(Room room, int gx, int gy, Random rng)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                int tx = rng.Next(1, WorldConfig.RoomWidthTiles - 1);
                int ty = rng.Next(1, WorldConfig.RoomHeightTiles - 1);

                if (room.Tiles[tx, ty] == TileType.Floor)
                {
                    float wx = gx * WorldConfig.RoomWidthPx + tx * WorldConfig.TileSize + WorldConfig.TileSize / 2f;
                    float wy = gy * WorldConfig.RoomHeightPx + ty * WorldConfig.TileSize + WorldConfig.TileSize / 2f;
                    return new Vector2(wx, wy);
                }
            }

            return new Vector2(
                gx * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                gy * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f
            );
        }

        private bool HasCollision(Vector2 position)
        {
            Point roomGrid = GetRoomGridAtWorldPosition(position);
            Room room = _state.FloorMap[roomGrid.X, roomGrid.Y];
            if (room == null)
                return true;

            float lx = position.X % WorldConfig.RoomWidthPx;
            float ly = position.Y % WorldConfig.RoomHeightPx;
            if (lx < 0) lx += WorldConfig.RoomWidthPx;
            if (ly < 0) ly += WorldConfig.RoomHeightPx;

            float offset = PlayerConfig.CollisionOffset;

            return IsPointWall(room, lx - offset, ly - offset) ||
                   IsPointWall(room, lx + offset, ly - offset) ||
                   IsPointWall(room, lx - offset, ly + offset) ||
                   IsPointWall(room, lx + offset, ly + offset);
        }

        private bool IsPointWall(Room room, float lx, float ly)
        {
            int tx = (int)Math.Floor(lx / WorldConfig.TileSize);
            int ty = (int)Math.Floor(ly / WorldConfig.TileSize);

            int doorY1 = WorldConfig.RoomHeightTiles / 2;
            int doorY2 = doorY1 + 1;
            int doorX2 = WorldConfig.RoomWidthTiles / 2;
            int doorX1 = doorX2 - 1;

            bool isDoorY = ty == doorY1 || ty == doorY2;
            bool isDoorX = tx == doorX1 || tx == doorX2;

            if (tx < 0 || tx >= WorldConfig.RoomWidthTiles || ty < 0 || ty >= WorldConfig.RoomHeightTiles)
            {
                if (!room.IsLocked)
                {
                    if (tx < 0 && isDoorY) return false;
                    if (tx >= WorldConfig.RoomWidthTiles && isDoorY) return false;
                    if (ty < 0 && isDoorX) return false;
                    if (ty >= WorldConfig.RoomHeightTiles && isDoorX) return false;
                }

                return true;
            }

            if (!room.IsLocked)
            {
                if (tx == 0 && isDoorY) return false;
                if (tx == WorldConfig.RoomWidthTiles - 1 && isDoorY) return false;
                if (ty == 0 && isDoorX) return false;
                if (ty == WorldConfig.RoomHeightTiles - 1 && isDoorX) return false;
            }

            return room.Tiles[tx, ty] == TileType.Wall;
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
