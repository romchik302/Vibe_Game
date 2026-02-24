using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Engine;

namespace Vibe_Game.Scenes
{
    // Базовый абстрактный класс сцен от которого наследуются все остальные сцены
    // по типу игровой сцены меню паузы загрузки и тд
    public abstract class BaseScene
    {
        protected Game GameInstance { get; }

        protected BaseScene(Game game)
        {
            GameInstance = game ?? throw new ArgumentNullException(nameof(game));
        }

        // Общий метод для любых сервисов через MonoGame Game.Services
        protected T GetService<T>() where T : class
        {
            return GameInstance.Services.GetService<T>();
        }

        // Удобные хелперы для часто используемых сервисов
        protected SpriteBatch GetSpriteBatch() => GetService<SpriteBatch>();

        protected Camera GetCamera() => GetService<Camera>();

        protected Texture2D GetPixelTexture() => GetService<Texture2D>();

        public virtual void Initialize() { }
        public virtual void LoadContent() { }
        public virtual void UnloadContent() { }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(GameTime gameTime) { }
    }
}
