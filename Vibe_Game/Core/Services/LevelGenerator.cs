using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Services
{
    public class LevelGenerator
    {
        private readonly Random _random = new Random();
        public enum RoomType { None, Normal, Start, Boss, Shop, Treasure }

        public Room[,] GenerateFloor(int floorIndex)
        {
            Room[,] grid = new Room[WorldConfig.GridSize, WorldConfig.GridSize];

            // ВАЖНО: Вот эта строка отсутствовала! Мы создаем список позиций.
            List<Point> positions = new List<Point>();

            Point center = new Point(WorldConfig.CenterGrid, WorldConfig.CenterGrid);

            // Создаем стартовую комнату один раз, используя константы
            grid[center.X, center.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Start);
            grid[center.X, center.Y].IsLocked = false; // Стартовая всегда открыта

            positions.Add(center); // Добавляем стартовую позицию в список

            int target = (int)(_random.Next(5, 7) + 2.6 * floorIndex);

            while (positions.Count < target)
            {
                Point curr = positions[_random.Next(positions.Count)];
                Point next = _random.Next(4) switch
                {
                    0 => new Point(curr.X, curr.Y - 1),
                    1 => new Point(curr.X, curr.Y + 1),
                    2 => new Point(curr.X - 1, curr.Y),
                    _ => new Point(curr.X + 1, curr.Y)
                };

                // Заменили магические числа 13 на WorldConfig.GridSize
                if (next.X >= 0 && next.X < WorldConfig.GridSize &&
                    next.Y >= 0 && next.Y < WorldConfig.GridSize &&
                    grid[next.X, next.Y] == null)
                {
                    // Заменили 20 и 11 на настройки размеров
                    grid[next.X, next.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Normal);
                    positions.Add(next);
                }
            }
            return grid;
        }
    }
}