
using Vibe_Game.Gameplay.Entities.Player;

namespace Vibe_Game.Core.Interfaces
{
    public interface IItemEffect
    {
        string Name { get; }
        string Description { get; }
        void Apply(PlayerStats stats);
        void Remove(PlayerStats stats);
    }
}
