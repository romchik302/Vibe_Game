using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Utilities;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Gameplay.Entities.Player
{
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
}