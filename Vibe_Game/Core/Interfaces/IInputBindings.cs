using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibe_Game.Core.IExample;
using Microsoft.Xna.Framework.Input;

namespace Vibe_Game.Core.Interfaces
{
    public interface IInputBindings
    {
        IEnumerable<Keys> GetKeysForAction(InputAction action);

        void SetBinding(InputAction action, Keys[] keys);

        void LoadFromFile(string path);

        void SaveToFile(string path);
    }
}
