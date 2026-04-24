// Vibe_Game/Game1.cs (полная рабочая версия)
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        private KeyboardState _previousKeyboardState;
        private RenderTarget2D _sceneRenderTarget;
        private const int VirtualWidth = 1280;
        private const int VirtualHeight = 720;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Простые настройки
            _graphics.PreferredBackBufferWidth = VirtualWidth;
            _graphics.PreferredBackBufferHeight = VirtualHeight;
            _graphics.HardwareModeSwitch = false; // Borderless fullscreen: быстрый toggle и масштабирование содержимого
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
            EnsureSceneRenderTarget();

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
            HandleFullscreenToggle();

            // Базовое обновление (включает обновление SceneManager)
            base.Update(gameTime);
        }

        private void HandleFullscreenToggle()
        {
            KeyboardState keyboard = Keyboard.GetState();
            bool fPressedNow = keyboard.IsKeyDown(Keys.F);
            bool fPressedPrev = _previousKeyboardState.IsKeyDown(Keys.F);

            if (fPressedNow && !fPressedPrev)
            {
                _graphics.IsFullScreen = !_graphics.IsFullScreen;
                _graphics.ApplyChanges();
            }

            _previousKeyboardState = keyboard;
        }

        protected override void Draw(GameTime gameTime)
        {
            EnsureSceneRenderTarget();

            // 1) Рисуем всю сцену в фиксированное "виртуальное" окно 1280x720.
            // Это сохраняет один и тот же обзор комнаты в окне и в fullscreen.
            GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);

            // 2) Растягиваем готовый кадр на весь экран.
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(
                _sceneRenderTarget,
                destinationRectangle: new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                color: Color.White
            );
            _spriteBatch.End();
        }

        private void EnsureSceneRenderTarget()
        {
            if (_sceneRenderTarget != null &&
                !_sceneRenderTarget.IsDisposed &&
                _sceneRenderTarget.Width == VirtualWidth &&
                _sceneRenderTarget.Height == VirtualHeight)
                return;

            _sceneRenderTarget?.Dispose();
            _sceneRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                VirtualWidth,
                VirtualHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.DiscardContents
            );
        }
    }
}
