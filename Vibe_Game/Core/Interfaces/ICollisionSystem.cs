using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Vibe_Game.Core.Interfaces
{

    /// <summary>
    /// Система коллизий
    /// </summary>
    public interface ICollisionSystem
    {
        void Register(ICollidable collidable);
        void Unregister(ICollidable collidable);
        IEnumerable<ICollidable> GetCollisions(Rectangle bounds);
        void Update(GameTime gameTime);
    }
}
