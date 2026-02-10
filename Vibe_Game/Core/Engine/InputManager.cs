using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibe_Game.Core.Engine
{
    public class InputManager
    {
        private static KeyboardState _currentKeyState;
        private static KeyboardState _previousKeyState;
        private static MouseState _currentMouseState;
        private static MouseState _previousMouseState;

        // Поддерживаемые действия
        public enum Action { MoveUp, MoveDown, MoveLeft, MoveRight, Pause}

        // Настройки управления
        private static readonly Dictionary<Action, Keys[]> _keyBindings = new()
        {
            [Action.MoveUp] = new[] { Keys.W, Keys.Up },
            [Action.MoveDown] = new[] { Keys.S, Keys.Down },
            [Action.MoveLeft] = new[] { Keys.A, Keys.Left },
            [Action.MoveRight] = new[] { Keys.D, Keys.Right },
            [Action.Pause] = new[] { Keys.Escape },
        };

        public static void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
        }

        // Проверка нажатия (однократное)
        public static bool IsActionPressed(Action action)
        {
            foreach (var key in _keyBindings[action])
            {
                if (_currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key))
                    return true;
            }
            return false;
        }

        // Проверка удержания
        public static bool IsActionDown(Action action)
        {
            foreach (var key in _keyBindings[action])
            {
                if (_currentKeyState.IsKeyDown(key))
                    return true;
            }
            return false;
        }

    }
}
