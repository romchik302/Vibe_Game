// Пример рефакторинга Player с разделением ответственностей
// Показывает применение SRP и DIP

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Gameplay.Entities;

namespace Vibe_Game.Gameplay.Entities.PlayerExample
{
    /// <summary>
    /// Рефакторинг Player с применением SOLID принципов
    /// </summary>
    public class Player : Entity
    {
        // Зависимости через конструктор (Dependency Injection)
        private readonly IPlayerRenderer _renderer;
        private readonly IInputService _inputService;
        private readonly IPlayerContentLoader _contentLoader;

        // Компоненты игрока
        public PlayerController Controller { get; private set; }
        public PlayerStats Stats { get; private set; }

        // Для отрисовки направления стрельбы
        private Vector2 _lastShootDirection;

        /// <summary>
        /// Конструктор с инъекцией зависимостей
        /// </summary>
        public Player(
            Vector2 position,
            IPlayerRenderer renderer,
            IInputService inputService,
            IPlayerContentLoader contentLoader)
            : base()
        {
            Position = position;
            _renderer = renderer ?? throw new System.ArgumentNullException(nameof(renderer));
            _inputService = inputService ?? throw new System.ArgumentNullException(nameof(inputService));
            _contentLoader = contentLoader ?? throw new System.ArgumentNullException(nameof(contentLoader));

            // Создаём контроллер с зависимостью от IInputService
            Controller = new PlayerController(this, _inputService);
            Stats = new PlayerStats();

            Color = Color.White;
        }

        public override void LoadContent(ContentManager content)
        {
            // Делегируем загрузку контента специальному классу
            _contentLoader.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            // Обновляем контроллер
            Controller.Update(gameTime);

            // Сохраняем направление стрельбы
            _lastShootDirection = Controller.ShootDirection;

            // Применяем скорость
            Velocity = Controller.CurrentVelocity;
            
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Делегируем отрисовку специальному классу
            _renderer.Draw(spriteBatch, Position, _lastShootDirection, Color);
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X - 8, (int)Position.Y - 8, 16, 16);
        }
    }

    /// <summary>
    /// Реализация рендерера игрока (отдельная ответственность)
    /// </summary>
    public class PlayerRenderer : IPlayerRenderer
    {
        private readonly Texture2D _pixelTexture;

        public PlayerRenderer(Texture2D pixelTexture)
        {
            _pixelTexture = pixelTexture ?? throw new System.ArgumentNullException(nameof(pixelTexture));
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Vector2 shootDirection, Color color)
        {
            if (_pixelTexture == null)
                return;

            // Рисуем тело игрока
            spriteBatch.DrawFilledCircle(_pixelTexture, position, 8, color);

            // Рисуем глаза
            spriteBatch.DrawFilledCircle(_pixelTexture, position + new Vector2(-3, -2), 2, Color.Black);
            spriteBatch.DrawFilledCircle(_pixelTexture, position + new Vector2(3, -2), 2, Color.Black);

            // Рисуем направление стрельбы
            if (shootDirection != Vector2.Zero)
            {
                var endPos = position + shootDirection * 20;
                spriteBatch.DrawLine(_pixelTexture, position, endPos, Color.Red, 2f);
            }
        }
    }

    /// <summary>
    /// Реализация загрузчика контента игрока (отдельная ответственность)
    /// </summary>
    public class PlayerContentLoader : IPlayerContentLoader
    {
        private Texture2D _texture;
        public bool IsContentLoaded => _texture != null;

        public void LoadContent(ContentManager content)
        {
            try
            {
                _texture = content.Load<Texture2D>("Textures/Debug/pixel");
                System.Diagnostics.Debug.WriteLine("Player: pixel texture loaded from file");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Player: failed to load texture from file");
                // Можно создать текстуру программно или использовать fallback
            }
        }

        public Texture2D GetTexture() => _texture;
    }

    /// <summary>
    /// Рефакторинг PlayerController с использованием IInputService
    /// </summary>
    public class PlayerController
    {
        private readonly Player _player;
        private readonly IInputService _inputService;

        // Настройки движения
        public float MoveSpeed { get; set; } = 200f;
        public Vector2 CurrentVelocity { get; private set; }
        public Vector2 ShootDirection { get; private set; }

        // Таймеры для стрельбы
        private float _shootCooldown;
        private const float ShootDelay = 0.3f;

        public PlayerController(Player player, IInputService inputService)
        {
            _player = player ?? throw new System.ArgumentNullException(nameof(player));
            _inputService = inputService ?? throw new System.ArgumentNullException(nameof(inputService));
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Обновляем таймер стрельбы
            if (_shootCooldown > 0)
                _shootCooldown -= deltaTime;

            // Обработка движения
            HandleMovement(deltaTime);

            // Обработка стрельбы
            HandleShooting();
        }

        private void HandleMovement(float deltaTime)
        {
            Vector2 inputDirection = Vector2.Zero;

            // Используем IInputService вместо статического InputManager
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

        private void HandleShooting()
        {
            // TODO: Реализовать стрельбу через IInputService
            // if (_inputService.IsActionDown(InputAction.Shoot) && _shootCooldown <= 0)
            // {
            //     // Создать пулю
            //     _shootCooldown = ShootDelay;
            // }
        }
    }
}
