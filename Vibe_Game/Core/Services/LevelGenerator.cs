using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Tiles;

namespace Vibe_Game.Core.Services
{
    public class LevelGenerator
    {
        private static readonly Point[] Directions =
        {
            new Point(0, -1),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(1, 0)
        };

        private readonly Random _random = new Random();

        public enum RoomType
        {
            None,
            Normal,
            Start,
            Boss,
            Shop,
            Treasure,
            Secret,
            SuperSecret,
            Challenge,
            Sacrifice
        }

        public Room[,] GenerateFloor(int floorIndex)
        {
            Room[,] grid = new Room[WorldConfig.GridSize, WorldConfig.GridSize];
            List<Point> occupiedRooms = new List<Point>();

            Point start = new Point(WorldConfig.CenterGrid, WorldConfig.CenterGrid);
            grid[start.X, start.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Start)
            {
                IsLocked = false
            };
            occupiedRooms.Add(start);

            int targetRoomCount = Math.Clamp(8 + floorIndex * 2 + _random.Next(-1, 2), 8, 14);
            GrowMainLayout(grid, occupiedRooms, start, targetRoomCount);
            AssignSpecialRooms(grid, occupiedRooms, start, floorIndex);
            TryAddSecretRooms(grid, occupiedRooms, start);
            CreateDoorways(grid);

            return grid;
        }

        private void GrowMainLayout(Room[,] grid, List<Point> occupiedRooms, Point start, int targetRoomCount)
        {
            int attempts = 0;
            int maxAttempts = targetRoomCount * 80;

            while (occupiedRooms.Count < targetRoomCount && attempts < maxAttempts)
            {
                attempts++;

                Point anchor = occupiedRooms[_random.Next(occupiedRooms.Count)];
                Point candidate = anchor + Directions[_random.Next(Directions.Length)];

                if (!IsInsideGrid(candidate) || grid[candidate.X, candidate.Y] != null)
                    continue;

                if (PointManhattanDistance(start, candidate) > 4)
                    continue;

                int neighborCount = CountOccupiedNeighbors(grid, candidate);
                if (neighborCount == 0)
                    continue;

                if (neighborCount > 2 && _random.NextDouble() < 0.85)
                    continue;

                if (anchor == start && CountOccupiedNeighbors(grid, anchor) >= 2 && _random.NextDouble() < 0.75)
                    continue;

                grid[candidate.X, candidate.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Normal);
                occupiedRooms.Add(candidate);
            }

            while (occupiedRooms.Count < Math.Max(6, targetRoomCount - 1))
            {
                Point anchor = occupiedRooms[_random.Next(occupiedRooms.Count)];
                foreach (Point direction in Directions.OrderBy(_ => _random.Next()))
                {
                    Point candidate = anchor + direction;
                    if (!IsInsideGrid(candidate) || grid[candidate.X, candidate.Y] != null)
                        continue;

                    grid[candidate.X, candidate.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Normal);
                    occupiedRooms.Add(candidate);
                    break;
                }
            }
        }

        private void AssignSpecialRooms(Room[,] grid, List<Point> occupiedRooms, Point start, int floorIndex)
        {
            Dictionary<Point, int> distances = CalculateDistances(grid, start);
            List<Point> deadEnds = occupiedRooms
                .Where(point => point != start && CountOccupiedNeighbors(grid, point) == 1)
                .OrderByDescending(point => distances.GetValueOrDefault(point))
                .ToList();

            HashSet<Point> assigned = new HashSet<Point> { start };

            Point bossRoom = deadEnds.FirstOrDefault();
            if (bossRoom == Point.Zero && !deadEnds.Contains(Point.Zero))
            {
                bossRoom = occupiedRooms
                    .Where(point => point != start)
                    .OrderByDescending(point => distances.GetValueOrDefault(point))
                    .First();
            }

            AssignRoomType(grid, bossRoom, RoomType.Boss, assigned);

            Point treasureRoom = deadEnds.FirstOrDefault(point => !assigned.Contains(point));
            if (treasureRoom != Point.Zero || deadEnds.Contains(Point.Zero))
                AssignRoomType(grid, treasureRoom, RoomType.Treasure, assigned);

            Point shopRoom = occupiedRooms
                .Where(point => !assigned.Contains(point) && distances.GetValueOrDefault(point) >= 2)
                .OrderBy(point => CountOccupiedNeighbors(grid, point))
                .ThenByDescending(point => distances.GetValueOrDefault(point))
                .FirstOrDefault();
            if (shopRoom != Point.Zero || occupiedRooms.Contains(Point.Zero))
                AssignRoomType(grid, shopRoom, RoomType.Shop, assigned);

            if (floorIndex >= 1)
            {
                Point challengeRoom = occupiedRooms
                    .Where(point => !assigned.Contains(point) && CountOccupiedNeighbors(grid, point) <= 2)
                    .OrderByDescending(point => distances.GetValueOrDefault(point))
                    .FirstOrDefault();
                if (challengeRoom != Point.Zero || occupiedRooms.Contains(Point.Zero))
                    AssignRoomType(grid, challengeRoom, RoomType.Challenge, assigned);
            }

            if (floorIndex >= 2)
            {
                Point sacrificeRoom = occupiedRooms
                    .Where(point => !assigned.Contains(point) && distances.GetValueOrDefault(point) >= 2)
                    .OrderBy(point => CountOccupiedNeighbors(grid, point))
                    .ThenBy(point => _random.Next())
                    .FirstOrDefault();
                if (sacrificeRoom != Point.Zero || occupiedRooms.Contains(Point.Zero))
                    AssignRoomType(grid, sacrificeRoom, RoomType.Sacrifice, assigned);
            }
        }

        private void TryAddSecretRooms(Room[,] grid, List<Point> occupiedRooms, Point start)
        {
            Point? secretRoom = FindEmptySpecialRoomCandidate(grid, minimumNeighbors: 3, maximumNeighbors: 4);
            if (secretRoom.HasValue)
            {
                grid[secretRoom.Value.X, secretRoom.Value.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.Secret);
                occupiedRooms.Add(secretRoom.Value);
            }

            Point? superSecretRoom = FindEmptySuperSecretCandidate(grid, start);
            if (superSecretRoom.HasValue)
            {
                grid[superSecretRoom.Value.X, superSecretRoom.Value.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, RoomType.SuperSecret);
                occupiedRooms.Add(superSecretRoom.Value);
            }
        }

        private Point? FindEmptySpecialRoomCandidate(Room[,] grid, int minimumNeighbors, int maximumNeighbors)
        {
            List<Point> candidates = new List<Point>();

            for (int x = 1; x < WorldConfig.GridSize - 1; x++)
            {
                for (int y = 1; y < WorldConfig.GridSize - 1; y++)
                {
                    Point point = new Point(x, y);
                    if (grid[x, y] != null || point == new Point(WorldConfig.CenterGrid, WorldConfig.CenterGrid))
                        continue;

                    int occupiedNeighbors = CountOccupiedNeighbors(grid, point);
                    if (occupiedNeighbors >= minimumNeighbors && occupiedNeighbors <= maximumNeighbors)
                        candidates.Add(point);
                }
            }

            if (candidates.Count == 0 && minimumNeighbors > 2)
                return FindEmptySpecialRoomCandidate(grid, minimumNeighbors - 1, maximumNeighbors);

            return candidates.Count == 0
                ? null
                : candidates[_random.Next(candidates.Count)];
        }

        private Point? FindEmptySuperSecretCandidate(Room[,] grid, Point start)
        {
            List<Point> candidates = new List<Point>();

            for (int x = 1; x < WorldConfig.GridSize - 1; x++)
            {
                for (int y = 1; y < WorldConfig.GridSize - 1; y++)
                {
                    Point point = new Point(x, y);
                    if (grid[x, y] != null)
                        continue;

                    if (CountOccupiedNeighbors(grid, point) != 1)
                        continue;

                    if (PointManhattanDistance(start, point) < 3)
                        continue;

                    candidates.Add(point);
                }
            }

            return candidates.Count == 0
                ? null
                : candidates.OrderByDescending(point => PointManhattanDistance(start, point)).First();
        }

        private static void AssignRoomType(Room[,] grid, Point point, RoomType roomType, HashSet<Point> assigned)
        {
            Room room = grid[point.X, point.Y];
            if (room == null || assigned.Contains(point))
                return;

            grid[point.X, point.Y] = new Room(WorldConfig.RoomWidthTiles, WorldConfig.RoomHeightTiles, roomType);
            assigned.Add(point);
        }

        private static Dictionary<Point, int> CalculateDistances(Room[,] grid, Point start)
        {
            Dictionary<Point, int> distances = new Dictionary<Point, int> { [start] = 0 };
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();
                int nextDistance = distances[current] + 1;

                foreach (Point neighbor in GetOccupiedNeighbors(grid, current))
                {
                    if (distances.ContainsKey(neighbor))
                        continue;

                    distances[neighbor] = nextDistance;
                    queue.Enqueue(neighbor);
                }
            }

            return distances;
        }

        private static IEnumerable<Point> GetOccupiedNeighbors(Room[,] grid, Point point)
        {
            foreach (Point direction in Directions)
            {
                Point neighbor = point + direction;
                if (IsInsideGrid(neighbor) && grid[neighbor.X, neighbor.Y] != null)
                    yield return neighbor;
            }
        }

        private static int CountOccupiedNeighbors(Room[,] grid, Point point)
        {
            return GetOccupiedNeighbors(grid, point).Count();
        }

        private static bool IsInsideGrid(Point point)
        {
            return point.X >= 0 &&
                   point.X < WorldConfig.GridSize &&
                   point.Y >= 0 &&
                   point.Y < WorldConfig.GridSize;
        }

        private static int PointManhattanDistance(Point left, Point right)
        {
            return Math.Abs(left.X - right.X) + Math.Abs(left.Y - right.Y);
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
