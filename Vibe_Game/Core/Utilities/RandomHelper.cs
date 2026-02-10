using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibe_Game.Core.Utilities
{
    // Еще один вспомогательный непонятный класс написанный нейросетью
    // В этот раз это функции которые помогают создать иллюзию случайных событий в игре
    public static class RandomHelper
    {
        private static readonly Random _random = new Random();

        public static int Next(int max) => _random.Next(max);
        public static int Next(int min, int max) => _random.Next(min, max);
        public static float NextFloat() => (float)_random.NextDouble();
        public static float NextFloat(float min, float max) => min + (max - min) * NextFloat();
        public static bool Chance(float probability) => NextFloat() < probability;

        public static T RandomItem<T>(List<T> list) => list[Next(list.Count)];

        public static Vector2 RandomDirection4Way()
        {
            var dirs = new[] { new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0) };
            return dirs[Next(4)];
        }
    }
}
