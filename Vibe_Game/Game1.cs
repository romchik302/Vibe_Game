// Vibe_Game/Game1.cs (полная рабочая версия)
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Services;
using Vibe_Game.Gameplay.Entities.Player;
using Vibe_Game.Scenes;
using Vibe_Game.Core.Settings;
using DefaultInputBindings = Vibe_Game.Core.Engine.DefaultInputBindings;

namespace Vibe_Game
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SceneManager _sceneManager;
        private InputService _inputService;
        private PlayerContentLoader _contentLoader;
        private Camera _camera;
        private Texture2D _pixelTexture;
        private Texture2D _playerTexture;
        private MainMenuScene _mainMenuScene;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Простые настройки
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            IsMouseVisible = true;

            System.Diagnostics.Debug.WriteLine("Game1 constructor completed");

            _sceneManager = new SceneManager(this);
            Components.Add(_sceneManager);

            _contentLoader = new PlayerContentLoader();
        }

        protected override void Initialize()
        {
            System.Diagnostics.Debug.WriteLine("=== Initialize started ===");

            // 1. БАЗОВАЯ ИНИЦИАЛИЗАЦИЯ MonoGame (ВАЖНО!)
            base.Initialize();

            // 2. Проверяем что GraphicsDevice создан
            if (GraphicsDevice == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: GraphicsDevice is null");
                throw new InvalidOperationException("GraphicsDevice was not created");
            }

            System.Diagnostics.Debug.WriteLine($"GraphicsDevice created: {GraphicsDevice.Adapter.Description}");

            // 3. Инициализируем сервисы
            InitializeServices();

            // 4. Загружаем сцену
            LoadInitialScene();

            System.Diagnostics.Debug.WriteLine("=== Initialize completed ===");
        }

        private void InitializeServices()
        {
            System.Diagnostics.Debug.WriteLine("--- Initialize services ---");

            // 1. SpriteBatch (main)
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), _spriteBatch);
            System.Diagnostics.Debug.WriteLine("SpriteBatch created");

            // 2. Camera
            _camera = new Camera(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            Services.AddService(typeof(Camera), _camera);
            System.Diagnostics.Debug.WriteLine($"Camera created: {_graphics.PreferredBackBufferWidth}x{_graphics.PreferredBackBufferHeight}");

            // 3. InputManager
            _inputService = new InputService(new DefaultInputBindings());
            Services.AddService(typeof(InputService), _inputService);
            System.Diagnostics.Debug.WriteLine("InputManager created");

            // 4. SceneManager (LAST!)
            Services.AddService(typeof(SceneManager), _sceneManager);
            System.Diagnostics.Debug.WriteLine("SceneManager created");

            System.Diagnostics.Debug.WriteLine("--- All services initialized ---");
        }

        protected override void LoadContent()
        {
            System.Diagnostics.Debug.WriteLine("=== LoadContent started ===");

            // 1. Загрузить контент через PlayerContentLoader
            _contentLoader.LoadContent(Content);
            _playerTexture = _contentLoader.PlayerTexture;
            System.Diagnostics.Debug.WriteLine("Player texture loaded");

            // 2. Создать единый 1x1 pixel texture для UI и тайлов
            _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            Services.AddService(typeof(Texture2D), _pixelTexture);

            // Проверяем состояние
            System.Diagnostics.Debug.WriteLine($"GraphicsDevice: {GraphicsDevice != null}");
            System.Diagnostics.Debug.WriteLine($"SceneManager: {_sceneManager != null}");
            System.Diagnostics.Debug.WriteLine($"SpriteBatch: {_spriteBatch != null}");

            if (_sceneManager == null)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: SceneManager is null, attempting recovery");

                // Экстренное восстановление
                _sceneManager = Services.GetService<SceneManager>();

                if (_sceneManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("CRITICAL ERROR: Failed to recover SceneManager");
                    throw new InvalidOperationException("SceneManager was not found in services");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SceneManager recovered from services");
                }
            }

            // Включаем менеджер сцен
            _sceneManager.Enabled = true;
            _sceneManager.Visible = true;

            System.Diagnostics.Debug.WriteLine("SceneManager enabled");

            System.Diagnostics.Debug.WriteLine("=== LoadContent completed ===");
        }

        private void LoadInitialScene()
        {
            System.Diagnostics.Debug.WriteLine("--- Loading initial scene ---");

            if (_sceneManager == null)
            {
                System.Diagnostics.Debug.WriteLine("CRITICAL ERROR: SceneManager is null");
                throw new InvalidOperationException("SceneManager is not initialized");
            }

            try
            {
                _mainMenuScene = new MainMenuScene(this, _inputService);
                _sceneManager.AddScene("main-menu", _mainMenuScene);
                _sceneManager.SwitchTo("main-menu");
                System.Diagnostics.Debug.WriteLine("Switched to MainMenuScene successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Scene loading error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }

            System.Diagnostics.Debug.WriteLine("--- Initial scene loaded ---");
        }

        public void StartNewGame()
        {
            var playerRenderer = new PlayerRenderer(_playerTexture);
            var gameScene = new GameScene(this, playerRenderer, _inputService, _contentLoader);
            _sceneManager.AddScene("game", gameScene);
            _sceneManager.SwitchTo("game");
        }

        public void ShowMainMenu()
        {
            if (_mainMenuScene == null)
            {
                _mainMenuScene = new MainMenuScene(this, _inputService);
                _sceneManager.AddScene("main-menu", _mainMenuScene);
            }

            _sceneManager.SwitchTo("main-menu");
        }

        protected override void Update(GameTime gameTime)
        {
            // Обновляем InputManager каждый кадр
            _inputService.Update();

            // Базовое обновление (включает обновление SceneManager)
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Очистка экрана
            GraphicsDevice.Clear(Color.Black);

            // Отрисовка происходит в SceneManager
            base.Draw(gameTime);
        }
    }
}
