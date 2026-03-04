using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Utilities;
using Vibe_Game.Gameplay.Entities.Player;
using Vibe_Game.Core.Services;

namespace Vibe_Game.Scenes
{
    public class GameScene : BaseScene
    {
        private Player _player;
        private readonly IPlayerRenderer _playerRenderer;
        private readonly IInputService _inputService;
        private readonly IPlayerContentLoader _contentLoader;

        private LevelGenerator _levelGenerator;
        private LevelGenerator.RoomType[,] _floorMap;

        // --- ГЛОБАЛЬНЫЕ ПАРАМЕТРЫ КОМНАТ И СТЕН ---
        private const int RoomWidth = 640;
        private const int RoomHeight = 360;
        private const int WallThickness = 9; // Базовая толщина (линия стены будет равна WallThickness * 2)

        // --- ГЛОБАЛЬНЫЕ ПАРАМЕТРЫ ДВЕРЕЙ ---
        // Физическая ширина прохода (для игрока)
        private const int PhysicalDoorWidth = 40;
        // Визуальная ширина двери (чуть больше физической для красоты, но всегда меньше стены)
        private readonly int VisualDoorWidth = Math.Min(50, Math.Min(RoomWidth, RoomHeight) - WallThickness * 2);
        private const int DoorProtrusion = 4; // Насколько дверь одинаково выпирает из стены внутрь и наружу

        // --- ЦВЕТА (ПАРАМЕТРЫ ОТРИСОВКИ) ---
        private readonly Color FloorColor = new Color(35, 25, 40);
        private readonly Color WallColor = new Color(70, 60, 80);

        // Навигация по комнатам и мир
        private Point _currentRoomGrid;
        private Vector2 _cameraPosition;

        // Отрисовка мини-карты
        private const int RoomDrawSize = 30;
        private Vector2 _mapOffset = new Vector2(10, 10);

        public GameScene(Game game, IPlayerRenderer playerRenderer, IInputService inputService, IPlayerContentLoader contentLoader)
            : base(game)
        {
            _playerRenderer = playerRenderer;
            _inputService = inputService;
            _contentLoader = contentLoader;
        }

        public override void Initialize()
        {
            _levelGenerator = new LevelGenerator();
            _floorMap = _levelGenerator.GenerateFloor(1);

            float startX = 6 * RoomWidth + (RoomWidth / 2f);
            float startY = 6 * RoomHeight + (RoomHeight / 2f);

            _player = new Player(new Vector2(startX, startY), _playerRenderer, _inputService, _contentLoader);
            _cameraPosition = new Vector2(startX, startY);

            base.Initialize();
        }

        public override void LoadContent()
        {
            _player.LoadContent(GameInstance.Content);
        }

        public override void Update(GameTime gameTime)
        {
            _player.Update(gameTime);

            HandleRoomTransitions();

            var camera = GetCamera();
            if (camera != null)
            {
                Vector2 targetCameraPos = new Vector2(
                    _currentRoomGrid.X * RoomWidth + (RoomWidth / 2f),
                    _currentRoomGrid.Y * RoomHeight + (RoomHeight / 2f)
                );

                _cameraPosition = Vector2.Lerp(_cameraPosition, targetCameraPos, 0.1f);
                camera.Follow(_cameraPosition);
            }

            base.Update(gameTime);
        }

        private void HandleRoomTransitions()
        {
            int currentX = (int)Math.Floor(_player.Position.X / RoomWidth);
            int currentY = (int)Math.Floor(_player.Position.Y / RoomHeight);

            currentX = Math.Clamp(currentX, 0, 12);
            currentY = Math.Clamp(currentY, 0, 12);

            _currentRoomGrid = new Point(currentX, currentY);

            float minX = currentX * RoomWidth;
            float maxX = minX + RoomWidth;
            float minY = currentY * RoomHeight;
            float maxY = minY + RoomHeight;

            float centerX = minX + RoomWidth / 2f;
            float centerY = minY + RoomHeight / 2f;

            float margin = 16; // Половина ширины игрока

            // Граница учитывает толщину стены, чтобы игрок в неё не врезался
            float limitMinX = minX + WallThickness + margin;
            float limitMaxX = maxX - WallThickness - margin;
            float limitMinY = minY + WallThickness + margin;
            float limitMaxY = maxY - WallThickness - margin;

            // Проверка ЛЕВОЙ стены
            if (_player.Position.X < limitMinX)
            {
                bool hasRoomLeft = currentX > 0 && _floorMap[currentX - 1, currentY] != LevelGenerator.RoomType.None;
                bool inDoorwayY = Math.Abs(_player.Position.Y - centerY) < (PhysicalDoorWidth / 2f);

                if (!hasRoomLeft || !inDoorwayY)
                    _player.Position = new Vector2(limitMinX, _player.Position.Y);
            }
            // Проверка ПРАВОЙ стены
            else if (_player.Position.X > limitMaxX)
            {
                bool hasRoomRight = currentX < 12 && _floorMap[currentX + 1, currentY] != LevelGenerator.RoomType.None;
                bool inDoorwayY = Math.Abs(_player.Position.Y - centerY) < (PhysicalDoorWidth / 2f);

                if (!hasRoomRight || !inDoorwayY)
                    _player.Position = new Vector2(limitMaxX, _player.Position.Y);
            }

            float clampedX = _player.Position.X;

            // Проверка ВЕРХНЕЙ стены
            if (_player.Position.Y < limitMinY)
            {
                bool hasRoomTop = currentY > 0 && _floorMap[currentX, currentY - 1] != LevelGenerator.RoomType.None;
                bool inDoorwayX = Math.Abs(clampedX - centerX) < (PhysicalDoorWidth / 2f);

                if (!hasRoomTop || !inDoorwayX)
                    _player.Position = new Vector2(clampedX, limitMinY);
            }
            // Проверка НИЖНЕЙ стены
            else if (_player.Position.Y > limitMaxY)
            {
                bool hasRoomBottom = currentY < 12 && _floorMap[currentX, currentY + 1] != LevelGenerator.RoomType.None;
                bool inDoorwayX = Math.Abs(clampedX - centerX) < (PhysicalDoorWidth / 2f);

                if (!hasRoomBottom || !inDoorwayX)
                    _player.Position = new Vector2(clampedX, limitMaxY);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GameInstance.GraphicsDevice.Clear(new Color(15, 10, 20));

            var spriteBatch = GetSpriteBatch();
            var camera = GetCamera();
            var pixelTexture = GetPixelTexture();

            if (spriteBatch == null) return;

            spriteBatch.Begin(
                transformMatrix: camera?.GetShakenMatrix(),
                samplerState: SamplerState.PointClamp
            );

            if (pixelTexture != null)
            {
                DrawRooms(spriteBatch, pixelTexture);
            }

            _player.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawMinimap(spriteBatch, pixelTexture);
            spriteBatch.End();
        }

        private void DrawRooms(SpriteBatch spriteBatch, Texture2D pixel)
        {
            int fullWallDepth = WallThickness * 2; // Полная визуальная глубина стены (т.к. DrawRectangle рисует внутрь)
            int doorTotalDepth = fullWallDepth + (DoorProtrusion * 2); // Дверь перекрывает стену + выпирает с двух сторон

            for (int x = 0; x < 13; x++)
            {
                for (int y = 0; y < 13; y++)
                {
                    if (_floorMap[x, y] == LevelGenerator.RoomType.None) continue;

                    int worldX = x * RoomWidth;
                    int worldY = y * RoomHeight;
                    var roomRect = new Rectangle(worldX, worldY, RoomWidth, RoomHeight);

                    // 1. Пол
                    spriteBatch.Draw(pixel, roomRect, FloorColor);

                    // 2. Стены (рисуются рамкой внутрь комнаты)
                    spriteBatch.DrawRectangle(pixel, roomRect, WallColor, fullWallDepth);

                    // 3. Рисуем двери/проходы только туда, где есть комната
                    bool hasLeft = x > 0 && _floorMap[x - 1, y] != LevelGenerator.RoomType.None;
                    bool hasRight = x < 12 && _floorMap[x + 1, y] != LevelGenerator.RoomType.None;
                    bool hasTop = y > 0 && _floorMap[x, y - 1] != LevelGenerator.RoomType.None;
                    bool hasBottom = y < 12 && _floorMap[x, y + 1] != LevelGenerator.RoomType.None;

                    // Центр комнаты для позиционирования дверей
                    int midY = worldY + RoomHeight / 2;
                    int midX = worldX + RoomWidth / 2;

                    // Левая сторона (вырезает левую стену, смещаясь на DoorProtrusion влево)
                    if (hasLeft)
                    {
                        spriteBatch.Draw(pixel, new Rectangle(
                            worldX - DoorProtrusion,
                            midY - VisualDoorWidth / 2,
                            doorTotalDepth,
                            VisualDoorWidth), FloorColor);
                    }

                    // Правая сторона (вырезает правую стену)
                    if (hasRight)
                    {
                        spriteBatch.Draw(pixel, new Rectangle(
                            worldX + RoomWidth - fullWallDepth - DoorProtrusion,
                            midY - VisualDoorWidth / 2,
                            doorTotalDepth,
                            VisualDoorWidth), FloorColor);
                    }

                    // Верхняя сторона (вырезает верхнюю стену)
                    if (hasTop)
                    {
                        spriteBatch.Draw(pixel, new Rectangle(
                            midX - VisualDoorWidth / 2,
                            worldY - DoorProtrusion,
                            VisualDoorWidth,
                            doorTotalDepth), FloorColor);
                    }

                    // Нижняя сторона (вырезает нижнюю стену)
                    if (hasBottom)
                    {
                        spriteBatch.Draw(pixel, new Rectangle(
                            midX - VisualDoorWidth / 2,
                            worldY + RoomHeight - fullWallDepth - DoorProtrusion,
                            VisualDoorWidth,
                            doorTotalDepth), FloorColor);
                    }
                }
            }
        }

        private void DrawMinimap(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (pixel == null || _floorMap == null) return;

            for (int x = 0; x < 13; x++)
            {
                for (int y = 0; y < 13; y++)
                {
                    var type = _floorMap[x, y];
                    if (type == LevelGenerator.RoomType.None) continue;

                    Color color = type switch
                    {
                        LevelGenerator.RoomType.Start => Color.DodgerBlue,
                        LevelGenerator.RoomType.Boss => Color.Crimson,
                        LevelGenerator.RoomType.Treasure => Color.Gold,
                        LevelGenerator.RoomType.Shop => Color.Green,
                        _ => Color.LightGray
                    };

                    Rectangle rect = new Rectangle(
                        (int)_mapOffset.X + (x * RoomDrawSize),
                        (int)_mapOffset.Y + (y * RoomDrawSize),
                        RoomDrawSize - 2,
                        RoomDrawSize - 2
                    );

                    spriteBatch.Draw(pixel, rect, color * 0.8f);

                    if (x == _currentRoomGrid.X && y == _currentRoomGrid.Y)
                    {
                        spriteBatch.DrawRectangle(pixel, rect, Color.Red, 2);
                    }
                }
            }
        }
    }
}