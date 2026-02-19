using Microsoft.Xna.Framework;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Interfaces;


namespace Vibe_Game.Gameplay.Entities.Player
{
    public class PlayerController
    {
        private readonly Player _player;
        public IInputService _inputService;

        // Настройки движения
        public float MoveSpeed { get; set; } = 200f;
        public Vector2 CurrentVelocity { get; private set; }
        public Vector2 ShootDirection { get; private set; }
        

        // Таймеры для стрельбы (как в Isaac - с задержкой)
        private float _shootCooldown;
        private const float ShootDelay = 0.3f; // Задержка между выстрелами

        public PlayerController(Player player, IInputService inputService)
        {
            _player = player;
            _inputService = inputService;
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем таймер стрельбы
            if (_shootCooldown > 0)
                _shootCooldown -= deltaTime;

            // Обработка движения
            HandleMovement(deltaTime);
        }

        private void HandleMovement(float deltaTime)
        {
            Vector2 inputDirection = Vector2.Zero;

            // Движение по WASD
            if (_inputService.IsActionDown(InputAction.MoveUp))
                inputDirection.Y -= 1;
            if (_inputService.IsActionDown(InputAction.MoveDown))
                inputDirection.Y += 1;
            if (_inputService.IsActionDown(InputAction.MoveLeft))
                inputDirection.X -= 1;
            if (_inputService.IsActionDown(InputAction.MoveRight))
                inputDirection.X += 1;

            // Нормализуем диагональное движение
            if (inputDirection != Vector2.Zero)
                inputDirection.Normalize();

            // Устанавливаем скорость
            CurrentVelocity = inputDirection * MoveSpeed;
        }
    }
}
