using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Vibe_Game.Gameplay.Entities
{
    public abstract class Entity
    {
        public Vector2 Position { get; set; } // Позиция в данный момент времени
        public Vector2 Velocity { get; set; } // Направление движения
        public bool IsAlive { get; set; } = true; // Состояние жизни, по умолчанию живой

        protected Texture2D Texture { get; set; } // Текстура
        protected Color Color { get; set; } = Color.White; // Цвет для удобства
        protected Vector2 Origin = Vector2.Zero; // Центр по умолчанию в нулях

        public virtual void Update(GameTime gameTime)
        {
            // Базовая физика: позиция += скорость * время
            if (Velocity != Vector2.Zero)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                Position += Velocity * deltaTime;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null && IsAlive)
            {
                spriteBatch.Draw(Texture, Position, null, Color, 0f, Origin, 1f, SpriteEffects.None, 0);
            }
        }

        // Загрузка контента (переопределяется в наследниках)
        public virtual void LoadContent(ContentManager content) { }

        // Хитбокс для столкновений
        public virtual Rectangle GetBounds()
        {
            if (Texture == null)
                return new Rectangle((int)Position.X, (int)Position.Y, 16, 16);

            return new Rectangle(
                (int)(Position.X - Origin.X),
                (int)(Position.Y - Origin.Y),
                Texture.Width,
                Texture.Height
            );
        }

    }
}
