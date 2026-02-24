// Vibe_Game/Scenes/GameScene.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Utilities;
using Vibe_Game.Gameplay.Entities.Player;

namespace Vibe_Game.Scenes
{
    public class GameScene : BaseScene
    {
        private Player _player;
        private readonly IPlayerRenderer _playerRenderer;
        private readonly IInputService _inputService;
        private readonly IPlayerContentLoader _contentLoader;


		public GameScene(Game game, IPlayerRenderer playerRenderer, IInputService inputService, IPlayerContentLoader contentLoader)
            : base(game)
        {
            _playerRenderer = playerRenderer;
            _inputService = inputService;
            _contentLoader = contentLoader;
        }

        public override void Initialize()
        {
            // Создаём игрока
            _player = new Player(new Vector2(400, 300), _playerRenderer, _inputService, _contentLoader);
        }

        public override void LoadContent()
        {
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