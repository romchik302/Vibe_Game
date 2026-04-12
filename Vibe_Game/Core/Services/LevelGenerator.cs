using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Tiles;

namespace Vibe_Game.Core.Services
{
    public class LevelGenerator
    {
        private readonly Random _random = new Random();

        public enum RoomType
        {
            None,
            Normal,
            Start,
            Boss,
            Shop,
            Treasure
        }

        public Room[,] GenerateFloor(int floorIndex)
        {
            Room[,] grid = new Room[WorldConfig.GridSize, WorldConfig.GridSize];
            List<Point> positions = new List<Point>();

            Point center = new Point(WorldConfig.CenterGrid, WorldConfig.CenterGrid);
            grid[center.X, center.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Start);
            grid[center.X, center.Y].IsLocked = false;

            positions.Add(center);

            int target = (int)(_random.Next(5, 7) + 2.6 * floorIndex);

            while (positions.Count < target)
            {
                Point current = positions[_random.Next(positions.Count)];
                Point next = _random.Next(4) switch
                {
                    0 => new Point(current.X, current.Y - 1),
                    1 => new Point(current.X, current.Y + 1),
                    2 => new Point(current.X - 1, current.Y),
                    _ => new Point(current.X + 1, current.Y)
                };

                if (next.X >= 0 && next.X < WorldConfig.GridSize &&
                    next.Y >= 0 && next.Y < WorldConfig.GridSize &&
                    grid[next.X, next.Y] == null)
                {
                    grid[next.X, next.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Normal);
                    positions.Add(next);
                }
            }

            CreateDoorways(grid);
            return grid;
        }

        private static void CreateDoorways(Room[,] grid)
        {
            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    Room room = grid[x, y];
                    if (room == null)
                        continue;

                    if (x > 0 && grid[x - 1, y] != null)
                        CreateHorizontalDoorway(room, isLeftSide: true);

                    if (x < WorldConfig.GridSize - 1 && grid[x + 1, y] != null)
                        CreateHorizontalDoorway(room, isLeftSide: false);

                    if (y > 0 && grid[x, y - 1] != null)
                        CreateVerticalDoorway(room, isTopSide: true);

                    if (y < WorldConfig.GridSize - 1 && grid[x, y + 1] != null)
                        CreateVerticalDoorway(room, isTopSide: false);
                }
            }
        }

        private static void CreateHorizontalDoorway(Room room, bool isLeftSide)
        {
            int x = isLeftSide ? 0 : WorldConfig.RoomWidthTiles - 1;
            int innerX = isLeftSide ? 1 : WorldConfig.RoomWidthTiles - 2;
            int doorY1 = WorldConfig.RoomHeightTiles / 2;
            int doorY2 = doorY1 + 1;

            room.SetTile(x, doorY1, new DoorTile(new Point(x, doorY1)));
            room.SetTile(x, doorY2, new DoorTile(new Point(x, doorY2)));
            EnsureFloorTile(room, innerX, doorY1);
            EnsureFloorTile(room, innerX, doorY2);
        }

        private static void CreateVerticalDoorway(Room room, bool isTopSide)
        {
            int y = isTopSide ? 0 : WorldConfig.RoomHeightTiles - 1;
            int innerY = isTopSide ? 1 : WorldConfig.RoomHeightTiles - 2;
            int doorX2 = WorldConfig.RoomWidthTiles / 2;
            int doorX1 = doorX2 - 1;

            room.SetTile(doorX1, y, new DoorTile(new Point(doorX1, y)));
            room.SetTile(doorX2, y, new DoorTile(new Point(doorX2, y)));
            EnsureFloorTile(room, doorX1, innerY);
            EnsureFloorTile(room, doorX2, innerY);
        }

        private static void EnsureFloorTile(Room room, int tileX, int tileY)
        {
            if (room.GetTile(tileX, tileY) is ButtonTile)
                return;

            room.SetTile(tileX, tileY, new FloorTile(new Point(tileX, tileY)));
        }
    }
}
