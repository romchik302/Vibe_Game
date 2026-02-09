using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Vibe_Game.Core.Engine
{
    // Статический доступ ко всем системам
    public static class GameManager
    {
        // Текущее состояние игры
        public enum GameState { Menu, Playing, Paused }
        public static GameState CurrentState { get; private set; }

        // Службы (Service Locator паттерн)
        private static readonly Dictionary<Type, object> _services = new();

        // Регистрация служб
        public static void RegisterService<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        // Получение служб
        public static T GetService<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;
            throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
        }

        // Изменение состояния
        public static void ChangeState(GameState newState)
        {
            CurrentState = newState;
            // Можно добавить события OnStateChanged
        }
    }
}