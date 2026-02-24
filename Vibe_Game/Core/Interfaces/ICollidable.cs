using Microsoft.Xna.Framework;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Объект, который может участвовать в коллизиях
    /// </summary>
    public interface ICollidable
    {
        Rectangle GetBounds();
        bool IsAlive { get; }
        void OnCollision(ICollidable other);
    }
}
