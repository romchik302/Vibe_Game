// Пример рефакторинга InputManager в InputService с интерфейсом
// Файл: Core/Engine/InputService.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Vibe_Game.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Vibe_Game.Core.EngineExample
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

        public Microsoft.Xna.Framework.Vector2 GetMousePosition()
        {
            return new Microsoft.Xna.Framework.Vector2(_currentMouseState.X, _currentMouseState.Y);
        }
    }

    /// <summary>
    /// Реализация привязок клавиш по умолчанию
    /// </summary>
    public class DefaultInputBindings : IInputBindings
    {
        private readonly Dictionary<InputAction, Keys[]> _bindings;

        public DefaultInputBindings()
        {
            _bindings = new Dictionary<InputAction, Keys[]>
            {
                [InputAction.MoveUp] = new[] { Keys.W, Keys.Up },
                [InputAction.MoveDown] = new[] { Keys.S, Keys.Down },
                [InputAction.MoveLeft] = new[] { Keys.A, Keys.Left },
                [InputAction.MoveRight] = new[] { Keys.D, Keys.Right },
                [InputAction.Shoot] = new[] { Keys.Space },
                [InputAction.Pause] = new[] { Keys.Escape },
                [InputAction.Interact] = new[] { Keys.E }
            };
        }

        public IEnumerable<Keys> GetKeysForAction(InputAction action)
        {
            return _bindings.TryGetValue(action, out var keys) 
                ? keys 
                : Enumerable.Empty<Keys>();
        }

        public void SetBinding(InputAction action, Keys[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new System.ArgumentException("Keys array cannot be null or empty", nameof(keys));

            _bindings[action] = keys;
        }

        public void LoadFromFile(string path)
        {
            // TODO: Реализовать загрузку из JSON/XML
            // Пример структуры JSON:
            // {
            //   "MoveUp": ["W", "Up"],
            //   "MoveDown": ["S", "Down"],
            //   ...
            // }
            throw new System.NotImplementedException();
        }

        public void SaveToFile(string path)
        {
            // TODO: Реализовать сохранение в JSON/XML
            throw new System.NotImplementedException();
        }
    }
}
