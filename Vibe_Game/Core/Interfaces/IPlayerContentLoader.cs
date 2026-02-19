using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibe_Game.Core.Interfaces
{
    public interface IPlayerContentLoader
    {
        bool IsContentLoaded { get; }
        void LoadContent(ContentManager content);
    }
}
