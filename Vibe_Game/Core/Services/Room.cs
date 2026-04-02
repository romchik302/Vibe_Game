using Microsoft.Xna.Framework;
using System;

namespace Vibe_Game.Core.Services
{
    public enum TileType { Floor, Wall }

    public class Room
    {
        public const int TileSize = 32;
        public TileType[,] Tiles { get; private set; }
        public int WidthInTiles { get; private set; }
        public int HeightInTiles { get; private set; }
        public LevelGenerator.RoomType Type { get; set; }

        public bool IsLocked { get; set; } = false; // По умолчанию открыта
        public Point ButtonPos { get; set; }
        private Random _rnd = new Random();

        public Room(int widthInTiles, int heightInTiles, LevelGenerator.RoomType type)
        {
            WidthInTiles = widthInTiles;
            HeightInTiles = heightInTiles;
            Type = type;
            Tiles = new TileType[WidthInTiles, HeightInTiles];

            // Кнопка в случайном месте (не у стены)
            ButtonPos = new Point(_rnd.Next(2, WidthInTiles - 2), _rnd.Next(2, HeightInTiles - 2));

            for (int x = 0; x < WidthInTiles; x++)
            {
                for (int y = 0; y < HeightInTiles; y++)
                {
                    if (x == 0 || x == WidthInTiles - 1 || y == 0 || y == HeightInTiles - 1)
                        Tiles[x, y] = TileType.Wall;
                    else
                        Tiles[x, y] = (_rnd.Next(100) < 5) ? TileType.Wall : TileType.Floor;
                }
            }
            Tiles[ButtonPos.X, ButtonPos.Y] = TileType.Floor;
        }
    }
}