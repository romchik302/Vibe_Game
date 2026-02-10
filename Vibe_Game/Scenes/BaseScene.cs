using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Engine;

namespace Vibe_Game.Scenes
{
    // Базовый абстрактный класс сцен от которого наследуются все остальные сцены
    // по типу игровой сцены меню паузы загрузки и тд
    public abstract class BaseScene
    {
        protected Microsoft.Xna.Framework.Game GameInstance { get; }

        public BaseScene(Microsoft.Xna.Framework.Game game)
        {
            GameInstance = game;
        }

        // Методы для получения сервисов из Game.Services
        protected SpriteBatch GetSpriteBatch()
        {
            return GameInstance.Services.GetService<SpriteBatch>();
        }

        protected Camera GetCamera()
        {
            return GameInstance.Services.GetService<Camera>();
        }

        protected Texture2D GetPixelTexture()
        {
            return GameInstance.Services.GetService<Texture2D>();
        }

        // Общий метод для любых сервисов
        protected T GetService<T>() where T : class
        {
            return GameInstance.Services.GetService<T>();
        }

        // Метод для доступа к GameManager (статический, не через сервисы)
        protected T GetFromGameManager<T>()
        {
            return GameManager.GetService<T>();
        }

        public virtual void Initialize() { }
        public virtual void LoadContent() { }
        public virtual void UnloadContent() { }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(GameTime gameTime) { }
    }
}
