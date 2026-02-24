using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Vibe_Game.Core.Interfaces
{
    public interface IRandomService
    {
        int Next(int max);
        int Next(int min, int max);
        float NextFloat();
        float NextFloat(float min, float max);
        bool Chance(float probability);
        T RandomItem<T>(List<T> list);
        Vector2 RandomDirection4Way();
    }
}
