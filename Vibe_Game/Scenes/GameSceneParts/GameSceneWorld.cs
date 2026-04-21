using System;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Tiles;
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

        public bool IsDoorwayOpen(Point roomGrid, int deltaX, int deltaY)
        {
            Room room = GetRoomAtGrid(roomGrid);
            Point neighborGrid = new Point(roomGrid.X + deltaX, roomGrid.Y + deltaY);
            Room neighbor = GetRoomAtGridOrNull(neighborGrid);

            if (room == null || neighbor == null)
                return false;

            return !room.IsLocked && !neighbor.IsLocked;
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

            Tile tile = room.GetTile(tx, ty);
            return tile == null || !tile.IsWalkable;
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

            Tile tile = room.GetTile(tx, ty);
            return tile == null || !tile.IsWalkable;
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
            TryUpdateCurrentRoomGrid();
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

        public void InitializeDoorStates()
        {
            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                    RefreshDoorTilesForRoom(new Point(x, y));
            }
        }

        public void OnRoomEntered(Point roomGrid, Point previousRoomGrid)
        {
            Room room = GetRoomAtGrid(roomGrid);
            if (room == null)
                return;

            if (room.Type == LevelGenerator.RoomType.Start || room.IsCleared)
            {
                room.IsLocked = false;
                RefreshDoorStatesAround(roomGrid);
                return;
            }

            if (ShouldLockRoom(room))
            {
                MovePlayerInsideRoom(roomGrid, previousRoomGrid);
                room.IsLocked = true;
                RefreshDoorStatesAround(roomGrid);
            }
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

        public void UpdateCurrentRoomState()
        {
            Room room = _state.FloorMap[_state.CurrentRoomGrid.X, _state.CurrentRoomGrid.Y];
            if (room == null)
                return;

            EnsureBossFloorExit(room);

            if (room.Type == LevelGenerator.RoomType.Start)
            {
                room.IsLocked = false;
                RefreshDoorStatesAround(_state.CurrentRoomGrid);
                return;
            }

            TryPressRoomButton(room);

            if (AreUnlockRequirementsMet(room))
            {
                room.IsLocked = false;
                room.IsCleared = true;
                EnsureBossFloorExit(room);
                RefreshDoorStatesAround(_state.CurrentRoomGrid);
                return;
            }

            if (!room.IsCleared && ShouldLockRoom(room))
            {
                room.IsLocked = true;
                RefreshDoorStatesAround(_state.CurrentRoomGrid);
            }
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

                if (room.GetTile(tx, ty)?.CanHostEnemy == true)
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

        public bool TryGetFloorExitTarget(out int targetFloorIndex)
        {
            targetFloorIndex = 0;

            Room room = GetRoomAtGrid(_state.CurrentRoomGrid);
            TrapdoorTile floorExitTile = room?.FloorExitTile;
            if (floorExitTile == null)
                return false;

            float localX = _state.Player.Position.X - _state.CurrentRoomGrid.X * WorldConfig.RoomWidthPx;
            float localY = _state.Player.Position.Y - _state.CurrentRoomGrid.Y * WorldConfig.RoomHeightPx;
            Rectangle playerRect = new Rectangle(
                (int)localX - PlayerConfig.Radius,
                (int)localY - PlayerConfig.Radius,
                PlayerConfig.Size,
                PlayerConfig.Size
            );

            Rectangle trapdoorRect = new Rectangle(
                floorExitTile.GridPosition.X * WorldConfig.TileSize,
                floorExitTile.GridPosition.Y * WorldConfig.TileSize,
                WorldConfig.TileSize,
                WorldConfig.TileSize
            );

            if (!playerRect.Intersects(trapdoorRect))
                return false;

            targetFloorIndex = floorExitTile.TargetFloorIndex;
            return true;
        }

        private bool HasCollision(Vector2 position)
        {
            float offset = PlayerConfig.CollisionOffset;

            return IsWorldPointBlocked(new Vector2(position.X - offset, position.Y - offset)) ||
                   IsWorldPointBlocked(new Vector2(position.X + offset, position.Y - offset)) ||
                   IsWorldPointBlocked(new Vector2(position.X - offset, position.Y + offset)) ||
                   IsWorldPointBlocked(new Vector2(position.X + offset, position.Y + offset));
        }

        private bool IsPointWall(Room room, float lx, float ly)
        {
            int tx = (int)Math.Floor(lx / WorldConfig.TileSize);
            int ty = (int)Math.Floor(ly / WorldConfig.TileSize);

            if (tx < 0 || tx >= WorldConfig.RoomWidthTiles || ty < 0 || ty >= WorldConfig.RoomHeightTiles)
                return true;

            Tile tile = room.GetTile(tx, ty);
            return tile == null || !tile.IsWalkable;
        }

        public void RefreshEnemyOccupancy()
        {
            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    Room room = _state.FloorMap[x, y];
                    if (room == null)
                        continue;

                    room.ClearEnemyOccupancy();

                    foreach (Enemy enemy in room.enemies)
                    {
                        if (!enemy.IsAlive)
                            continue;

                        Vector2 localPosition = enemy.Position - new Vector2(x * WorldConfig.RoomWidthPx, y * WorldConfig.RoomHeightPx);
                        int tileX = (int)Math.Floor(localPosition.X / WorldConfig.TileSize);
                        int tileY = (int)Math.Floor(localPosition.Y / WorldConfig.TileSize);
                        room.MarkEnemyOccupancy(tileX, tileY);
                    }
                }
            }
        }

        private void TryUpdateCurrentRoomGrid()
        {
            float offset = PlayerConfig.CollisionOffset;

            Point topLeft = GetRoomGridAtWorldPosition(new Vector2(_state.Player.Position.X - offset, _state.Player.Position.Y - offset));
            Point topRight = GetRoomGridAtWorldPosition(new Vector2(_state.Player.Position.X + offset, _state.Player.Position.Y - offset));
            Point bottomLeft = GetRoomGridAtWorldPosition(new Vector2(_state.Player.Position.X - offset, _state.Player.Position.Y + offset));
            Point bottomRight = GetRoomGridAtWorldPosition(new Vector2(_state.Player.Position.X + offset, _state.Player.Position.Y + offset));

            bool isInsideSingleRoom =
                topLeft == topRight &&
                topLeft == bottomLeft &&
                topLeft == bottomRight;

            if (isInsideSingleRoom)
                _state.CurrentRoomGrid = topLeft;
        }

        private Room GetRoomAtGridOrNull(Point roomGrid)
        {
            if (roomGrid.X < 0 || roomGrid.X >= WorldConfig.GridSize || roomGrid.Y < 0 || roomGrid.Y >= WorldConfig.GridSize)
                return null;

            return _state.FloorMap[roomGrid.X, roomGrid.Y];
        }

        private void RefreshDoorStatesAround(Point roomGrid)
        {
            RefreshDoorTilesForRoom(roomGrid);
            RefreshDoorTilesForRoom(new Point(roomGrid.X - 1, roomGrid.Y));
            RefreshDoorTilesForRoom(new Point(roomGrid.X + 1, roomGrid.Y));
            RefreshDoorTilesForRoom(new Point(roomGrid.X, roomGrid.Y - 1));
            RefreshDoorTilesForRoom(new Point(roomGrid.X, roomGrid.Y + 1));
        }

        private void RefreshDoorTilesForRoom(Point roomGrid)
        {
            Room room = GetRoomAtGridOrNull(roomGrid);
            if (room == null)
                return;

            UpdateDoorTile(room, 0, WorldConfig.RoomHeightTiles / 2, IsDoorwayOpen(roomGrid, -1, 0));
            UpdateDoorTile(room, 0, WorldConfig.RoomHeightTiles / 2 + 1, IsDoorwayOpen(roomGrid, -1, 0));
            UpdateDoorTile(room, WorldConfig.RoomWidthTiles - 1, WorldConfig.RoomHeightTiles / 2, IsDoorwayOpen(roomGrid, 1, 0));
            UpdateDoorTile(room, WorldConfig.RoomWidthTiles - 1, WorldConfig.RoomHeightTiles / 2 + 1, IsDoorwayOpen(roomGrid, 1, 0));
            UpdateDoorTile(room, WorldConfig.RoomWidthTiles / 2 - 1, 0, IsDoorwayOpen(roomGrid, 0, -1));
            UpdateDoorTile(room, WorldConfig.RoomWidthTiles / 2, 0, IsDoorwayOpen(roomGrid, 0, -1));
            UpdateDoorTile(room, WorldConfig.RoomWidthTiles / 2 - 1, WorldConfig.RoomHeightTiles - 1, IsDoorwayOpen(roomGrid, 0, 1));
            UpdateDoorTile(room, WorldConfig.RoomWidthTiles / 2, WorldConfig.RoomHeightTiles - 1, IsDoorwayOpen(roomGrid, 0, 1));
        }

        private static void UpdateDoorTile(Room room, int tileX, int tileY, bool isOpen)
        {
            if (room.GetTile(tileX, tileY) is DoorTile doorTile)
                doorTile.SetOpen(isOpen);
        }

        private void MovePlayerInsideRoom(Point roomGrid, Point previousRoomGrid)
        {
            int deltaX = roomGrid.X - previousRoomGrid.X;
            int deltaY = roomGrid.Y - previousRoomGrid.Y;

            if (Math.Abs(deltaX) + Math.Abs(deltaY) != 1)
                return;

            float localX = _state.Player.Position.X - roomGrid.X * WorldConfig.RoomWidthPx;
            float localY = _state.Player.Position.Y - roomGrid.Y * WorldConfig.RoomHeightPx;
            float safeInset = WorldConfig.TileSize + PlayerConfig.CollisionOffset + 0.01f;

            if (deltaX == 1)
                localX = Math.Max(localX, safeInset);
            else if (deltaX == -1)
                localX = Math.Min(localX, WorldConfig.RoomWidthPx - safeInset);
            else if (deltaY == 1)
                localY = Math.Max(localY, safeInset);
            else if (deltaY == -1)
                localY = Math.Min(localY, WorldConfig.RoomHeightPx - safeInset);

            _state.Player.Position = new Vector2(
                roomGrid.X * WorldConfig.RoomWidthPx + localX,
                roomGrid.Y * WorldConfig.RoomHeightPx + localY
            );
        }

        private void TryPressRoomButton(Room room)
        {
            ButtonTile buttonTile = room.ButtonTile;
            if (buttonTile == null || buttonTile.IsPressed)
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
                buttonTile.GridPosition.X * WorldConfig.TileSize,
                buttonTile.GridPosition.Y * WorldConfig.TileSize,
                WorldConfig.TileSize,
                WorldConfig.TileSize
            );

            if (playerRect.Intersects(buttonRect))
                room.PressButton();
        }

        private static bool ShouldLockRoom(Room room)
        {
            return room.HasButton || HasAliveEnemies(room);
        }

        private static bool AreUnlockRequirementsMet(Room room)
        {
            bool buttonSatisfied = !room.HasButton || room.IsButtonPressed;
            return buttonSatisfied && !HasAliveEnemies(room);
        }

        private void EnsureBossFloorExit(Room room)
        {
            if (room.Type != LevelGenerator.RoomType.Boss)
                return;

            if (!room.IsCleared)
                return;

            if (_state.CurrentFloorIndex >= _state.MaxFloorIndex)
                return;

            room.CreateFloorExit(_state.CurrentFloorIndex + 1);
        }

        private static bool HasAliveEnemies(Room room)
        {
            foreach (Enemy enemy in room.enemies)
            {
                if (enemy.IsAlive)
                    return true;
            }

            return false;
        }

        private static float GetEnemyRadius(Enemy enemy)
        {
            if (enemy is FlyingEnemy) return EnemyConfig.DefaultFlyingRadius;
            if (enemy is ChasingEnemy) return EnemyConfig.DefaultChasingRadius;
            if (enemy is AdaptiveChasingEnemy) return EnemyConfig.DefaultChasingRadius;
            return 10f;
        }

        public void UpdatePlayerFrictionByGround()
        {
            if (_state.Player == null) return;

            Point roomGrid = GetRoomGridAtWorldPosition(_state.Player.Position);
            Room room = GetRoomAtGridOrNull(roomGrid);
            if (room == null) return;

            float localX = _state.Player.Position.X - roomGrid.X * WorldConfig.RoomWidthPx;
            float localY = _state.Player.Position.Y - roomGrid.Y * WorldConfig.RoomHeightPx;
            int tileX = (int)(localX / WorldConfig.TileSize);
            int tileY = (int)(localY / WorldConfig.TileSize);

            if (tileX < 0 || tileX >= WorldConfig.RoomWidthTiles || tileY < 0 || tileY >= WorldConfig.RoomHeightTiles)
                return;

            Tile tile = room.GetTile(tileX, tileY);
            float frictionMultiplier = 1f;

            // Предположим, у вас есть способ определить лужу (например, tile.Type == TileType.Water)
            // или через дополнительный флаг tile.ReducesFriction
            if (tile != null && tile.ReducesFriction) 
            {
                frictionMultiplier = 0.3f;   // лужа уменьшает трение → игрок дольше скользит
            }

            _state.Player.SetMovementFrictionMultiplier(frictionMultiplier);
        }
    }
}
