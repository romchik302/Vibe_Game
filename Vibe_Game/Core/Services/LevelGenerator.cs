using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Vibe_Game.Core.Services
{
	public class LevelGenerator
	{
		private readonly Random _random = new Random();
		private const int GridSize = 13;
		private const int Center = 6;

		// Добавлен Treasure для исправления ошибки CS0117
		public enum RoomType { None, Normal, Start, Boss, Shop, Treasure }

		public RoomType[,] GenerateFloor(int floorIndex)
		{
			// Формула: Rooms = random(5, 6) + 2.6 * FloorIndex
			int targetRooms = (int)(_random.Next(5, 7) + 2.6 * 1);

			RoomType[,] grid = new RoomType[GridSize, GridSize];
			List<Vector2> roomPositions = new List<Vector2>();

			// Стартовая комната в центре (6,6)
			Vector2 startPos = new Vector2(Center, Center);
			grid[Center, Center] = RoomType.Start;
			roomPositions.Add(startPos);

			int roomsPlaced = 1;
			while (roomsPlaced < targetRooms)
			{
				Vector2 currentPos = roomPositions[_random.Next(roomPositions.Count)];
				Vector2 nextPos = GetRandomNeighbor(currentPos);

				if (IsInsideGrid(nextPos) && grid[(int)nextPos.X, (int)nextPos.Y] == RoomType.None)
				{
					// Правило: не более одного соседа
					if (CountNeighbors(nextPos, grid) <= 1)
					{
						grid[(int)nextPos.X, (int)nextPos.Y] = RoomType.Normal;
						roomPositions.Add(nextPos);
						roomsPlaced++;
					}
				}
			}

			return grid;
		}

		private Vector2 GetRandomNeighbor(Vector2 pos)
		{
			int dir = _random.Next(4);
			return dir switch
			{
				0 => new Vector2(pos.X, pos.Y - 1),
				1 => new Vector2(pos.X, pos.Y + 1),
				2 => new Vector2(pos.X - 1, pos.Y),
				_ => new Vector2(pos.X + 1, pos.Y),
			};
		}

		private bool IsInsideGrid(Vector2 pos)
		{
			return pos.X >= 0 && pos.X < GridSize && pos.Y >= 0 && pos.Y < GridSize;
		}

		private int CountNeighbors(Vector2 pos, RoomType[,] grid)
		{
			int count = 0;
			Vector2[] dirs = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
			foreach (var d in dirs)
			{
				Vector2 check = pos + d;
				if (IsInsideGrid(check) && grid[(int)check.X, (int)check.Y] != RoomType.None)
					count++;
			}
			return count;
		}
	}
}