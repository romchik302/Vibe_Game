using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Tiles;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Core.Services
{
    public class Room
    {
        public Tile[,] Tiles { get; private set; }
        public int WidthInTiles { get; private set; }
        public int HeightInTiles { get; private set; }
        public LevelGenerator.RoomType Type { get; set; }

        public bool IsLocked { get; set; }
        public bool HasButton { get; set; }
        public bool IsButtonPressed => ButtonTile?.IsPressed ?? false;
        public bool IsCleared { get; set; }
        public Point ButtonPos => ButtonTile?.GridPosition ?? Point.Zero;
        public ButtonTile ButtonTile { get; private set; }
        public TrapdoorTile FloorExitTile { get; private set; }

        public List<Enemy> enemies { get; } = new();

        private readonly Random _random = new Random();

        public Room(int widthInTiles, int heightInTiles, LevelGenerator.RoomType type)
        {
            WidthInTiles = widthInTiles;
            HeightInTiles = heightInTiles;
            Type = type;
            HasButton = type is LevelGenerator.RoomType.Normal or LevelGenerator.RoomType.Challenge;
            IsCleared = type == LevelGenerator.RoomType.Start;
            Tiles = new Tile[WidthInTiles, HeightInTiles];

            InitializeTiles();
            CarveCenterArea(radius: 1);

            if (HasButton)
                PlaceButton();
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < WidthInTiles && y >= 0 && y < HeightInTiles;
        }

        public Tile GetTile(int x, int y)
        {
            return IsInside(x, y) ? Tiles[x, y] : null;
        }

        public void SetTile(int x, int y, Tile tile)
        {
            if (!IsInside(x, y))
                return;

            Tile existingTile = Tiles[x, y];
            if (existingTile == ButtonTile)
                ButtonTile = null;

            if (existingTile == FloorExitTile)
                FloorExitTile = null;

            Tiles[x, y] = tile;

            if (tile is ButtonTile buttonTile)
                ButtonTile = buttonTile;

            if (tile is TrapdoorTile trapdoorTile)
                FloorExitTile = trapdoorTile;
        }

        public void PressButton()
        {
            ButtonTile?.Press();
        }

        public void CreateFloorExit(int targetFloorIndex)
        {
            if (FloorExitTile != null)
                return;

            Point center = new Point(WidthInTiles / 2, HeightInTiles / 2);
            SetTile(center.X, center.Y, new TrapdoorTile(center, targetFloorIndex));
        }

        public void ClearEnemyOccupancy()
        {
            for (int x = 0; x < WidthInTiles; x++)
            {
                for (int y = 0; y < HeightInTiles; y++)
                    Tiles[x, y].HasEnemy = false;
            }
        }

        public void MarkEnemyOccupancy(int tileX, int tileY)
        {
            Tile tile = GetTile(tileX, tileY);
            if (tile != null)
                tile.HasEnemy = true;
        }

        private void InitializeTiles()
        {
            int obstacleChance = GetInteriorObstacleChance(Type);

            for (int x = 0; x < WidthInTiles; x++)
            {
                for (int y = 0; y < HeightInTiles; y++)
                {
                    Point tilePosition = new Point(x, y);
                    if (x == 0 || x == WidthInTiles - 1 || y == 0 || y == HeightInTiles - 1)
                    {
                        Tiles[x, y] = new WallTile(tilePosition);
                    }
                    else
                    {
                        bool shouldPlaceObstacle = obstacleChance > 0 && _random.Next(100) < obstacleChance;
                        Tiles[x, y] = shouldPlaceObstacle
                            ? new WallTile(tilePosition)
                            : new FloorTile(tilePosition);
                    }
                }
            }
        }

        private void CarveCenterArea(int radius)
        {
            Point center = new Point(WidthInTiles / 2, HeightInTiles / 2);
            for (int x = center.X - radius; x <= center.X + radius; x++)
            {
                for (int y = center.Y - radius; y <= center.Y + radius; y++)
                    SetTile(x, y, new FloorTile(new Point(x, y)));
            }
        }

        private void PlaceButton()
        {
            for (int attempt = 0; attempt < 40; attempt++)
            {
                Point buttonPos = new Point(_random.Next(2, WidthInTiles - 2), _random.Next(2, HeightInTiles - 2));
                if (buttonPos == new Point(WidthInTiles / 2, HeightInTiles / 2))
                    continue;

                ButtonTile = new ButtonTile(buttonPos);
                SetTile(buttonPos.X, buttonPos.Y, ButtonTile);
                return;
            }
        }

        private static int GetInteriorObstacleChance(LevelGenerator.RoomType roomType)
        {
            return roomType switch
            {
                LevelGenerator.RoomType.Boss => 1,
                LevelGenerator.RoomType.Shop => 0,
                LevelGenerator.RoomType.Treasure => 0,
                LevelGenerator.RoomType.Secret => 0,
                LevelGenerator.RoomType.SuperSecret => 0,
                LevelGenerator.RoomType.Sacrifice => 2,
                LevelGenerator.RoomType.Challenge => 3,
                _ => 5
            };
        }
    }
}
