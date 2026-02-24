using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Core.Engine
{
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

        IEnumerable<Keys> IInputBindings.GetKeysForAction(InputAction action)
        {
            throw new NotImplementedException();
        }
    }
}
