using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Utilities;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;

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
            // 1. Обновляем контроллер (вычисляет желаемую скорость)
            Controller.Update(gameTime);

            // 2. Сохраняем направление стрельбы
            _lastShootDirection = Controller.ShootDirection;

            // 3. Применяем скорость (физика из базового класса Entity)
            Velocity = Controller.CurrentVelocity;

            // Здесь происходит Position += Velocity * deltaTime
            base.Update(gameTime);

            // ВАЖНО: Мы больше не ограничиваем позицию здесь (ClampToWindow удален),
            // так как игрок теперь перемещается по глобальному миру с множеством комнат,
            // а столкновениями со стенами занимается HandleRoomTransitions в GameScene.cs.
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Делегируем отрисовку специальному классу
            _renderer.Draw(spriteBatch, Position, _lastShootDirection, Color);
        }

        // Теперь мы берем размеры напрямую из единого файла настроек
        public override Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X - PlayerConfig.Radius,
                (int)Position.Y - PlayerConfig.Radius,
                PlayerConfig.Size,
                PlayerConfig.Size
            );
        }
    }
}