using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibe_Game.Core.Interfaces
{
    public interface ICameraShake
    {
        Vector2 GetShakeOffset();
        void Shake(float intensity, float duration);
        void Update(GameTime gameTime);
    }
}
