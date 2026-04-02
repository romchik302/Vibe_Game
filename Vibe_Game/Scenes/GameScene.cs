using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Services;
using Vibe_Game.Gameplay.Entities.Player;
using Vibe_Game.Gameplay.Projectiles;
using Vibe_Game.Gameplay.Weapons;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Utilities;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Scenes
{
    public class GameScene : BaseScene
    {
        private Player _player;
        private Room[,] _floorMap;
        private Point _currentRoomGrid;
        private Point _roomGridLastFrame = new Point(-1, -1);
        private Vector2 _cameraPosition;

        private SceneFlyingCollision _flyingCollision;

        private readonly List<Projectile> _projectiles = new();
        private SceneAttackContext _attackContext;

        private readonly IPlayerRenderer _playerRenderer;
        private readonly IInputService _inputService;
        private readonly IPlayerContentLoader _contentLoader;

        // Константы для отрисовки мини-карты оставляем здесь, так как они касаются только UI сцены
        private const int MinimapRoomSize = 20;
        private const int MinimapSpacing = 22;
        private const int MinimapOffset = 10;

        public GameScene(Game game, IPlayerRenderer pr, IInputService isrv, IPlayerContentLoader pcl)
            : base(game)
        {
            _playerRenderer = pr;
            _inputService = isrv;
            _contentLoader = pcl;
        }

        private LevelGenerator _levelGenerator;

        public override void Initialize()
        {
            _levelGenerator = new LevelGenerator();
            _floorMap = _levelGenerator.GenerateFloor(1);

            _flyingCollision = new SceneFlyingCollision(this);
            SpawnFlyingEnemiesInRooms(floorIndex: 1);

            _currentRoomGrid = new Point(WorldConfig.CenterGrid, WorldConfig.CenterGrid);

            Vector2 startPos = new Vector2(
                WorldConfig.CenterGrid * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                WorldConfig.CenterGrid * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f
            );

            _attackContext = new SceneAttackContext(this);
            _player = new Player(startPos, _playerRenderer, _inputService, _contentLoader, _attackContext);
            _player.EquippedWeapon = new ForwardProjectileWeapon(
                cooldownSeconds: 0.3f,
                projectileSpeed: 250f,
                damage: 1,
                spawnOffsetPixels: 20f,
                lifetimeSeconds: 1f);
            _cameraPosition = startPos;

            base.Initialize();
        }

        public override void LoadContent() => _player.LoadContent(GameInstance.Content);

        public override void Update(GameTime gameTime)
        {
            _attackContext.Sync(gameTime);

            Vector2 oldPos = _player.Position;
            _player.Update(gameTime);

            CheckTileCollision(oldPos);
            CheckButton();
            UpdateProjectiles(gameTime);

            if (_currentRoomGrid != _roomGridLastFrame)
            {
                TryActivateEnemies(_currentRoomGrid);
                _roomGridLastFrame = _currentRoomGrid;
            }

            UpdateEnemies(gameTime);
            UpdateCamera();

            base.Update(gameTime);
        }

        private void TryActivateEnemies(Point grid)
        {
            Room room = _floorMap[grid.X, grid.Y];
            room?.enemies?.ForEach(e => e.Activate());
        }

        private void SpawnFlyingEnemiesInRooms(int floorIndex)
        {
            var rng = new Random(unchecked(floorIndex * 397 ^ 0x5EED));

            for (int gx = 0; gx < WorldConfig.GridSize; gx++)
            {
                for (int gy = 0; gy < WorldConfig.GridSize; gy++)
                {
                    Room room = _floorMap[gx, gy];
                    if (room == null)
                        continue;

                    if (room.Type == LevelGenerator.RoomType.Start)
                        continue;

                    if (rng.NextDouble() > EnemyConfig.FlyingSpawnChancePerRoom)
                        continue;

                    var spawnWorld = new Vector2(
                        gx * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                        gy * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f);

                    room.enemies.Add(new FlyingEnemy(spawnWorld, _flyingCollision));
                }
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    Room room = _floorMap[x, y];
                    if (room?.enemies == null) continue;

                    for (int i = room.enemies.Count - 1; i >= 0; i--)
                    {
                        Enemy enemy = room.enemies[i];

                        if (enemy is FlyingEnemy flying)
                            flying.ChaseTarget = _player.Position;

                        enemy.Update(gameTime);

                        if (!enemy.IsAlive)
                            room.enemies.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Коллизия для летающих: внутренние клетки комнаты (не наружное кольцо тайлов) никогда не блокируют;
        /// периметр и двери — как у <see cref="IsPointWall"/>.
        /// </summary>
        private bool IsFlyingPointBlocked(Vector2 worldPosition)
        {
            int rx = (int)Math.Floor(worldPosition.X / WorldConfig.RoomWidthPx);
            int ry = (int)Math.Floor(worldPosition.Y / WorldConfig.RoomHeightPx);
            Point roomGrid = new Point(
                Math.Clamp(rx, 0, WorldConfig.GridSize - 1),
                Math.Clamp(ry, 0, WorldConfig.GridSize - 1));

            Room room = _floorMap[roomGrid.X, roomGrid.Y];
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

        private void CheckTileCollision(Vector2 oldPos)
        {
            Vector2 targetPos = _player.Position;
            Vector2 finalPos = targetPos;

            // 1. Проверяем движение по оси X
            if (targetPos.X != oldPos.X)
            {
                if (HasCollision(new Vector2(targetPos.X, oldPos.Y)))
                {
                    float offset = PlayerConfig.CollisionOffset;
                    if (targetPos.X > oldPos.X) // Герой идет вправо
                    {
                        // Находим правый край героя и "примагничиваем" его к левому краю стены
                        float rightEdge = targetPos.X + offset;
                        int tileX = (int)(rightEdge / WorldConfig.TileSize);
                        finalPos.X = tileX * WorldConfig.TileSize - offset - 0.01f;
                    }
                    else // Герой идет влево
                    {
                        // Находим левый край героя и "примагничиваем" к правому краю стены
                        float leftEdge = targetPos.X - offset;
                        int tileX = (int)Math.Floor(leftEdge / WorldConfig.TileSize);
                        finalPos.X = (tileX + 1) * WorldConfig.TileSize + offset + 0.01f;
                    }
                }
            }

            // 2. Проверяем движение по оси Y (используя уже исправленный X)
            if (targetPos.Y != oldPos.Y)
            {
                if (HasCollision(new Vector2(finalPos.X, targetPos.Y)))
                {
                    float offset = PlayerConfig.CollisionOffset;
                    if (targetPos.Y > oldPos.Y) // Герой идет вниз
                    {
                        float bottomEdge = targetPos.Y + offset;
                        int tileY = (int)(bottomEdge / WorldConfig.TileSize);
                        finalPos.Y = tileY * WorldConfig.TileSize - offset - 0.01f;
                    }
                    else // Герой идет вверх
                    {
                        float topEdge = targetPos.Y - offset;
                        int tileY = (int)Math.Floor(topEdge / WorldConfig.TileSize);
                        finalPos.Y = (tileY + 1) * WorldConfig.TileSize + offset + 0.01f;
                    }
                }
            }

            // Применяем идеальную позицию без отступов
            _player.Position = finalPos;

            // Обновляем информацию о том, в какой комнате мы сейчас
            int rx = (int)Math.Floor(_player.Position.X / WorldConfig.RoomWidthPx);
            int ry = (int)Math.Floor(_player.Position.Y / WorldConfig.RoomHeightPx);
            _currentRoomGrid = new Point(
                Math.Clamp(rx, 0, WorldConfig.GridSize - 1),
                Math.Clamp(ry, 0, WorldConfig.GridSize - 1)
            );
        }

        // Вспомогательный метод, чтобы не писать проверку углов дважды
        private bool HasCollision(Vector2 position)
        {
            // Узнаем комнату, в которую хочет наступить игрок
            int rx = (int)Math.Floor(position.X / WorldConfig.RoomWidthPx);
            int ry = (int)Math.Floor(position.Y / WorldConfig.RoomHeightPx);
            Point roomGrid = new Point(
                Math.Clamp(rx, 0, WorldConfig.GridSize - 1),
                Math.Clamp(ry, 0, WorldConfig.GridSize - 1)
            );

            Room room = _floorMap[roomGrid.X, roomGrid.Y];
            if (room == null) return true;

            // Переводим глобальные координаты в локальные (внутри комнаты)
            float lx = position.X % WorldConfig.RoomWidthPx;
            float ly = position.Y % WorldConfig.RoomHeightPx;

            // Фикс математики C# для отрицательных координат (переходы влево/вверх)
            if (lx < 0) lx += WorldConfig.RoomWidthPx;
            if (ly < 0) ly += WorldConfig.RoomHeightPx;

            float offset = PlayerConfig.CollisionOffset;

            // Проверяем 4 угла героя
            return IsPointWall(room, lx - offset, ly - offset) ||
                   IsPointWall(room, lx + offset, ly - offset) ||
                   IsPointWall(room, lx - offset, ly + offset) ||
                   IsPointWall(room, lx + offset, ly + offset);
        }

        private bool IsWorldPointBlocked(Vector2 worldPosition)
        {
            int rx = (int)Math.Floor(worldPosition.X / WorldConfig.RoomWidthPx);
            int ry = (int)Math.Floor(worldPosition.Y / WorldConfig.RoomHeightPx);
            Point roomGrid = new Point(
                Math.Clamp(rx, 0, WorldConfig.GridSize - 1),
                Math.Clamp(ry, 0, WorldConfig.GridSize - 1));

            Room room = _floorMap[roomGrid.X, roomGrid.Y];
            if (room == null) return true;

            float lx = worldPosition.X % WorldConfig.RoomWidthPx;
            float ly = worldPosition.Y % WorldConfig.RoomHeightPx;
            if (lx < 0) lx += WorldConfig.RoomWidthPx;
            if (ly < 0) ly += WorldConfig.RoomHeightPx;

            return IsPointWall(room, lx, ly);
        }

        private void SpawnProjectileFromArgs(ProjectileSpawnArgs args)
        {
            _projectiles.Add(new Projectile(
                args.Position,
                args.Direction,
                args.Speed,
                args.Damage,
                args.LifetimeSeconds));
        }

        private void UpdateProjectiles(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                Projectile p = _projectiles[i];
                if (!p.IsAlive)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }

                Vector2 next = p.Position + p.Velocity * dt;
                if (IsWorldPointBlocked(next))
                {
                    p.IsAlive = false;
                    _projectiles.RemoveAt(i);
                    continue;
                }

                p.Update(gameTime);
                if (!p.IsAlive)
                    _projectiles.RemoveAt(i);
            }
        }

        private bool IsPointWall(Room room, float lx, float ly)
        {
            int tx = (int)Math.Floor(lx / WorldConfig.TileSize);
            int ty = (int)Math.Floor(ly / WorldConfig.TileSize);

            // Вычисляем координаты дверей (чтобы они всегда были по центру, даже если изменить размер комнаты)
            int doorY1 = WorldConfig.RoomHeightTiles / 2;     // Для высоты 11 это 5
            int doorY2 = doorY1 + 1;                          // 6
            int doorX2 = WorldConfig.RoomWidthTiles / 2;      // Для ширины 20 это 10
            int doorX1 = doorX2 - 1;                          // 9

            bool isDoorY = (ty == doorY1 || ty == doorY2);
            bool isDoorX = (tx == doorX1 || tx == doorX2);

            // Границы комнаты
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

            // Двери внутри самой комнаты
            if (!room.IsLocked)
            {
                if (tx == 0 && isDoorY) return false;
                if (tx == WorldConfig.RoomWidthTiles - 1 && isDoorY) return false;
                if (ty == 0 && isDoorX) return false;
                if (ty == WorldConfig.RoomHeightTiles - 1 && isDoorX) return false;
            }

            return room.Tiles[tx, ty] == TileType.Wall;
        }

        private void CheckButton()
        {
            Room room = _floorMap[_currentRoomGrid.X, _currentRoomGrid.Y];
            if (room == null) return;

            float lx = _player.Position.X % WorldConfig.RoomWidthPx;
            float ly = _player.Position.Y % WorldConfig.RoomHeightPx;

            // Физическое тело игрока
            Rectangle playerRect = new Rectangle(
                (int)lx - PlayerConfig.Radius,
                (int)ly - PlayerConfig.Radius,
                PlayerConfig.Size,
                PlayerConfig.Size
            );

            // Физическое тело кнопки
            Rectangle buttonRect = new Rectangle(
                room.ButtonPos.X * WorldConfig.TileSize,
                room.ButtonPos.Y * WorldConfig.TileSize,
                WorldConfig.TileSize,
                WorldConfig.TileSize
            );

            if (playerRect.Intersects(buttonRect))
                room.IsLocked = false;
        }

        private void UpdateCamera()
        {
            Vector2 target = new Vector2(
                _currentRoomGrid.X * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                _currentRoomGrid.Y * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f
            );
            _cameraPosition = Vector2.Lerp(_cameraPosition, target, 0.1f);
            GetCamera()?.Follow(_cameraPosition);
        }

        public override void Draw(GameTime gameTime)
        {
            GameInstance.GraphicsDevice.Clear(GameColors.Background);
            var sb = GetSpriteBatch();
            var pixel = GetPixelTexture();

            if (sb == null || pixel == null) return;

            sb.Begin(transformMatrix: GetCamera()?.GetShakenMatrix(), samplerState: SamplerState.PointClamp);

            for (int x = 0; x < WorldConfig.GridSize; x++)
                for (int y = 0; y < WorldConfig.GridSize; y++)
                    if (_floorMap[x, y] != null)
                        DrawSingleRoom(sb, pixel, _floorMap[x, y], x, y);

            foreach (Projectile p in _projectiles)
            {
                if (p.IsAlive)
                    sb.Draw(pixel,
                        new Rectangle((int)p.Position.X - 2, (int)p.Position.Y - 2, 4, 4),
                        Color.SkyBlue);
            }

            for (int ex = 0; ex < WorldConfig.GridSize; ex++)
            {
                for (int ey = 0; ey < WorldConfig.GridSize; ey++)
                {
                    Room room = _floorMap[ex, ey];
                    if (room?.enemies == null) continue;

                    foreach (var enemy in room.enemies)
                    {
                        if (enemy.IsAlive)
                            enemy.Draw(sb);
                    }
                }
            }

            _player.Draw(sb);
            sb.End();

            sb.Begin(samplerState: SamplerState.PointClamp);
            DrawMinimap(sb, pixel);
            sb.End();
        }

        private Vector2 GetRandomFreeTilePosition(Room room, int gx, int gy, Random rng)
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

            // fallback — центр комнаты
            return new Vector2(
                gx * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                gy * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f);
        }

        private void DrawSingleRoom(SpriteBatch sb, Texture2D pixel, Room room, int gx, int gy)
        {
            int wx = gx * WorldConfig.RoomWidthPx;
            int wy = gy * WorldConfig.RoomHeightPx;

            for (int tx = 0; tx < WorldConfig.RoomWidthTiles; tx++)
            {
                for (int ty = 0; ty < WorldConfig.RoomHeightTiles; ty++)
                {
                    Color c = room.Tiles[tx, ty] == TileType.Wall ? GameColors.Wall : GameColors.Floor;
                    sb.Draw(pixel, new Rectangle(wx + tx * WorldConfig.TileSize, wy + ty * WorldConfig.TileSize, WorldConfig.TileSize, WorldConfig.TileSize), c);
                }
            }

            sb.Draw(pixel, new Rectangle(wx + room.ButtonPos.X * WorldConfig.TileSize + 8, wy + room.ButtonPos.Y * WorldConfig.TileSize + 8, 16, 16),
                   room.IsLocked ? GameColors.ButtonLocked : GameColors.ButtonUnlocked);

            if (!room.IsLocked)
            {
                // Вычисляем координаты для отрисовки дверей
                int doorYPos = wy + (WorldConfig.RoomHeightTiles / 2) * WorldConfig.TileSize;
                int doorXPos = wx + (WorldConfig.RoomWidthTiles / 2 - 1) * WorldConfig.TileSize;

                if (gx > 0 && _floorMap[gx - 1, gy] != null)
                    sb.Draw(pixel, new Rectangle(wx, doorYPos, WorldConfig.TileSize, WorldConfig.TileSize * 2), GameColors.Floor);

                if (gx < WorldConfig.GridSize - 1 && _floorMap[gx + 1, gy] != null)
                    sb.Draw(pixel, new Rectangle(wx + WorldConfig.RoomWidthPx - WorldConfig.TileSize, doorYPos, WorldConfig.TileSize, WorldConfig.TileSize * 2), GameColors.Floor);

                if (gy > 0 && _floorMap[gx, gy - 1] != null)
                    sb.Draw(pixel, new Rectangle(doorXPos, wy, WorldConfig.TileSize * 2, WorldConfig.TileSize), GameColors.Floor);

                if (gy < WorldConfig.GridSize - 1 && _floorMap[gx, gy + 1] != null)
                    sb.Draw(pixel, new Rectangle(doorXPos, wy + WorldConfig.RoomHeightPx - WorldConfig.TileSize, WorldConfig.TileSize * 2, WorldConfig.TileSize), GameColors.Floor);
            }
        }

        private void DrawMinimap(SpriteBatch sb, Texture2D pixel)
        {
            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    if (_floorMap[x, y] == null) continue;

                    Rectangle r = new Rectangle(MinimapOffset + x * MinimapSpacing, MinimapOffset + y * MinimapSpacing, MinimapRoomSize, MinimapRoomSize);

                    Color roomColor = _floorMap[x, y].Type switch
                    {
                        LevelGenerator.RoomType.Start => GameColors.MinimapStart,
                        LevelGenerator.RoomType.Boss => GameColors.MinimapBoss,
                        _ => GameColors.MinimapDefault
                    };

                    sb.Draw(pixel, r, roomColor * 0.5f);

                    if (x == _currentRoomGrid.X && y == _currentRoomGrid.Y)
                        sb.DrawRectangle(pixel, r, GameColors.MinimapCurrent, 1);
                }
            }
        }

        private sealed class SceneAttackContext : IAttackContext
        {
            private readonly GameScene _owner;
            private GameTime _gameTime;

            public SceneAttackContext(GameScene owner)
            {
                _owner = owner;
            }

            public void Sync(GameTime gameTime) => _gameTime = gameTime;

            public GameTime GameTime => _gameTime;

            public SpriteBatch SpriteBatch => null;

            public void SpawnProjectile(ProjectileSpawnArgs args) => _owner.SpawnProjectileFromArgs(args);

            public bool WouldCollideAtWorld(Vector2 worldPosition, float collisionRadius)
                => _owner.IsWorldPointBlocked(worldPosition);
        }

        private sealed class SceneFlyingCollision : IFlyingCollisionChecker
        {
            private readonly GameScene _owner;

            public SceneFlyingCollision(GameScene owner) => _owner = owner;

            public bool IsFlyingBlocked(Vector2 worldPosition)
                => _owner.IsFlyingPointBlocked(worldPosition);
        }
    }
}