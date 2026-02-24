using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibe_Game.Core.Interfaces
{
    public interface IInputService
    {
        bool IsActionPressed(InputAction action);
        bool IsActionDown(InputAction action);
        bool IsActionUp(InputAction action);

        void Update();
    }

    public enum InputAction
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Shoot,
        Pause,
        Interact
    }
}
