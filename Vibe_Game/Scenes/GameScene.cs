// Vibe_Game/Scenes/GameScene.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Utilities;
using Vibe_Game.Gameplay.Entities.Player;

namespace Vibe_Game.Scenes
{
    public class GameScene : BaseScene
    {
        private Player _player;

        public GameScene(Microsoft.Xna.Framework.Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            // Создаём игрока
            _player = new Player(new Vector2(400, 300));
        }

        public override void LoadContent()
        {
            // Получаем текстуру из сервисов Game, а не из GameManager
            var pixelTexture = GetPixelTexture();

            if (pixelTexture == null)
            {
                // Если нет в сервисах, пробуем из GameManager
                pixelTexture = GameManager.GetService<Texture2D>();
            }

            // Передаём контент игроку
            _player.LoadContent(GameInstance.Content);
        }

        public override void Update(GameTime gameTime)
        {
            _player.Update(gameTime);

            // Камера из сервисов Game
            var camera = GetCamera();
            if (camera != null)
            {
                camera.Follow(_player.Position);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // Очистка экрана
            GameInstance.GraphicsDevice.Clear(new Color(20, 12, 28));

            // Получаем сервисы
            var spriteBatch = GetSpriteBatch();
            var camera = GetCamera();
            var pixelTexture = GetPixelTexture();

            if (spriteBatch == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: SpriteBatch is null!");
                return;
            }

            // Начинаем рисование
            if (camera != null)
            {
                spriteBatch.Begin(
                    transformMatrix: camera.GetShakenMatrix(),
                    samplerState: SamplerState.PointClamp
                );
            }
            else
            {
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            }

            // Рисуем простую комнату
            if (pixelTexture != null)
            {
                var room = new Rectangle(0, 0, 1280, 720);
                spriteBatch.DrawRectangle(pixelTexture, room, Color.Gray * 0.5f, 2);
            }

            // Рисуем игрока
            _player.Draw(spriteBatch);

            spriteBatch.End();
        }
    }
}