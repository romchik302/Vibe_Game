using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Vibe_Game.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Vibe_Game.Core.Services
{
    /// <summary>
    /// Реализация сервиса ввода с поддержкой настраиваемых привязок
    /// </summary>
    public class InputService : IInputService
    {
        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private readonly IInputBindings _bindings;

        public InputService(IInputBindings bindings)
        {
            _bindings = bindings ?? throw new System.ArgumentNullException(nameof(bindings));
        }

        public void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
        }

        public bool IsActionPressed(InputAction action)
        {
            var keys = _bindings.GetKeysForAction(action);
            foreach (var key in keys)
            {
                if (_currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key))
                    return true;
            }
            return false;
        }

        public bool IsActionDown(InputAction action)
        {
            var keys = _bindings.GetKeysForAction(action);
            foreach (var key in keys)
            {
                if (_currentKeyState.IsKeyDown(key))
                    return true;
            }
            return false;
        }

        // Дополнительные методы для работы с мышью (если нужно)
        public bool IsMouseButtonPressed()
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed &&
                   _previousMouseState.LeftButton == ButtonState.Released;
        }

        public Vector2 GetMousePosition()
        {
            return new Vector2(_currentMouseState.X, _currentMouseState.Y);
        }

        public bool IsActionUp(InputAction action)
        {
            var keys = _bindings.GetKeysForAction(action);
            foreach (var key in keys)
            {
                if (_currentKeyState.IsKeyUp(key) && _previousKeyState.IsKeyDown(key))
                    return true;
            }
            return false;
        }
    }
}
