using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Vibe_Game.Core.Tiles;
using Vibe_Game.Gameplay.Entities.Enemies;

namespace Vibe_Game.Core.Services
{
    public class Room
    {
        public const int TileSize = 32;
        public Tile[,] Tiles { get; private set; }
        public int WidthInTiles { get; private set; }
        public int HeightInTiles { get; private set; }
        public LevelGenerator.RoomType Type { get; set; }

        public bool IsLocked { get; set; } = false;
        public bool HasButton { get; set; }
        public bool IsButtonPressed => ButtonTile?.IsPressed ?? false;
        public bool IsCleared { get; set; }
        public Point ButtonPos => ButtonTile?.GridPosition ?? Point.Zero;
        public ButtonTile ButtonTile { get; private set; }

        /// <summary>����, ����������� � ���� ������� (������� B). �������� ������ ����� ��������� �����.</summary>
        public List<Enemy> enemies { get; private set; }
        private Random _rnd = new Random();

        public Room(int widthInTiles, int heightInTiles, LevelGenerator.RoomType type)
        {
            WidthInTiles = widthInTiles;
            HeightInTiles = heightInTiles;
            Type = type;
            HasButton = type != LevelGenerator.RoomType.Start;
            IsCleared = type == LevelGenerator.RoomType.Start;
            Tiles = new Tile[WidthInTiles, HeightInTiles];
            enemies = new List<Enemy>();

            for (int x = 0; x < WidthInTiles; x++)
            {
                for (int y = 0; y < HeightInTiles; y++)
                {
                    Point tilePosition = new Point(x, y);
                    if (x == 0 || x == WidthInTiles - 1 || y == 0 || y == HeightInTiles - 1)
                        Tiles[x, y] = new WallTile(tilePosition);
                    else
                        Tiles[x, y] = (_rnd.Next(100) < 5) ? new WallTile(tilePosition) : new FloorTile(tilePosition);
                }
            }

            if (HasButton)
            {
                Point buttonPos = new Point(_rnd.Next(2, WidthInTiles - 2), _rnd.Next(2, HeightInTiles - 2));
                ButtonTile = new ButtonTile(buttonPos);
                Tiles[buttonPos.X, buttonPos.Y] = ButtonTile;
            }
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

            Tiles[x, y] = tile;

            if (tile is ButtonTile buttonTile)
                ButtonTile = buttonTile;
        }

        public void PressButton()
        {
            ButtonTile?.Press();
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
    }
}
