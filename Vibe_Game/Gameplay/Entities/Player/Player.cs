using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Utilities;

namespace Vibe_Game.Gameplay.Entities.Player
{
    public class Player : Entity
    {
        public PlayerController Controller { get; private set; }
        public PlayerStats Stats { get; private set; }

        // Текстура для отрисовки - ИНИЦИАЛИЗИРУЕМ В КОНСТРУКТОРЕ
        private Texture2D _pixelTexture;

        // Для отрисовки направления
        private Vector2 _lastShootDirection;

        public Player(Vector2 position) : base()
        {
            Position = position;
            Controller = new PlayerController(this);
            Stats = new PlayerStats();

            Color = Color.White;

            // ВРЕМЕННО: создаём текстуру программно, если не загрузится
            // В LoadContent мы попробуем загрузить из файла
        }

        public override void LoadContent(ContentManager content)
        {
            try
            {
                // Пытаемся загрузить текстуру из файла
                _pixelTexture = content.Load<Texture2D>("Textures/Debug/pixel");
                System.Diagnostics.Debug.WriteLine("Player: pixel texture loaded from file");
            }
            catch
            {
                // Если файла нет - ошибка
                System.Diagnostics.Debug.WriteLine("Player: creating pixel texture programmatically");
            }

            // Устанавливаем базовую текстуру
            Texture = _pixelTexture;
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
            if (_pixelTexture == null)
            {
                // Если текстура всё ещё null, рисуем простой прямоугольник
                DrawDebugPlayer(spriteBatch);
                return;
            }

            // Рисуем тело игрока
            spriteBatch.DrawFilledCircle(_pixelTexture, Position, 8, Color);

            // Рисуем глаза
            spriteBatch.DrawFilledCircle(_pixelTexture, Position + new Vector2(-3, -2), 2, Color.Black);
            spriteBatch.DrawFilledCircle(_pixelTexture, Position + new Vector2(3, -2), 2, Color.Black);

            // Рисуем направление стрельбы
            if (_lastShootDirection != Vector2.Zero)
            {
                var endPos = Position + _lastShootDirection * 20;
                spriteBatch.DrawLine(_pixelTexture, Position, endPos, Color.Red, 2f);
            }
        }

        private void DrawDebugPlayer(SpriteBatch spriteBatch)
        {
            // Резервный метод рисования без текстуры
            var rect = new Rectangle((int)Position.X - 8, (int)Position.Y - 8, 16, 16);

            // Рисуем через SpriteBatch.Draw (используем белую текстуру 1x1 из GameManager)
            var pixel = GameManager.GetService<Texture2D>();
            if (pixel != null)
            {
                spriteBatch.Draw(pixel, rect, Color.White);
            }
            else
            {
                // Если совсем нет текстур, хотя бы выводим сообщение
                System.Diagnostics.Debug.WriteLine("DEBUG: Drawing player without texture");
            }
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle((int)Position.X - 8, (int)Position.Y - 8, 16, 16);
        }

        // ... остальные методы без изменений
    }
}