// Vibe_Game/Game1.cs (полная рабочая версия)
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Core.Engine;
using Vibe_Game.Scenes;

namespace Vibe_Game
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SceneManager _sceneManager;
        private InputManager _inputManager;
        private Camera _camera;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Простые настройки
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            IsMouseVisible = true;

            System.Diagnostics.Debug.WriteLine("Конструктор Game1 выполнен");

            _sceneManager = new SceneManager(this);
            Components.Add(_sceneManager);
        }

        protected override void Initialize()
        {
            System.Diagnostics.Debug.WriteLine("=== Initialize начат ===");

            // 1. БАЗОВАЯ ИНИЦИАЛИЗАЦИЯ MonoGame (ВАЖНО!)
            base.Initialize();

            // 2. Проверяем что GraphicsDevice создан
            if (GraphicsDevice == null)
            {
                System.Diagnostics.Debug.WriteLine("ОШИБКА: GraphicsDevice is null!");
                throw new InvalidOperationException("GraphicsDevice не создан");
            }

            System.Diagnostics.Debug.WriteLine($"GraphicsDevice создан: {GraphicsDevice.Adapter.Description}");

            // 3. Инициализируем сервисы
            InitializeServices();

            // 4. Загружаем сцену
            LoadInitialScene();

            System.Diagnostics.Debug.WriteLine("=== Initialize завершён ===");
        }

        private void InitializeServices()
        {
            System.Diagnostics.Debug.WriteLine("--- Инициализация сервисов ---");

            // 1. SpriteBatch (основной)
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), _spriteBatch);
            System.Diagnostics.Debug.WriteLine("SpriteBatch создан");

            // 2. Пиксельная текстура (для рисования)
            var pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            Services.AddService(typeof(Texture2D), pixelTexture);
            System.Diagnostics.Debug.WriteLine("Пиксельная текстура создана");

            // 3. Камера
            _camera = new Camera(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            Services.AddService(typeof(Camera), _camera);
            System.Diagnostics.Debug.WriteLine($"Камера создана: {_graphics.PreferredBackBufferWidth}x{_graphics.PreferredBackBufferHeight}");

            // 4. InputManager
            _inputManager = new InputManager();
            Services.AddService(typeof(InputManager), _inputManager);
            System.Diagnostics.Debug.WriteLine("InputManager создан");

            // 5. SceneManager (ПОСЛЕДНИМ!)
            Services.AddService(typeof(SceneManager), _sceneManager);
            System.Diagnostics.Debug.WriteLine("SceneManager создан");

            // Регистрируем в GameManager
            GameManager.RegisterService(_spriteBatch);
            GameManager.RegisterService(pixelTexture);
            GameManager.RegisterService(_camera);
            GameManager.RegisterService(_inputManager);
            GameManager.RegisterService(_sceneManager);

            System.Diagnostics.Debug.WriteLine("--- Все сервисы инициализированы ---");
        }

        protected override void LoadContent()
        {
            System.Diagnostics.Debug.WriteLine("=== LoadContent начат ===");

            // Проверяем состояние
            System.Diagnostics.Debug.WriteLine($"GraphicsDevice: {GraphicsDevice != null}");
            System.Diagnostics.Debug.WriteLine($"SceneManager: {_sceneManager != null}");
            System.Diagnostics.Debug.WriteLine($"SpriteBatch: {_spriteBatch != null}");

            if (_sceneManager == null)
            {
                System.Diagnostics.Debug.WriteLine("ПРЕДУПРЕЖДЕНИЕ: SceneManager is null, пробуем восстановить...");

                // Экстренное восстановление
                _sceneManager = Services.GetService<SceneManager>();

                if (_sceneManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("КРИТИЧЕСКАЯ ОШИБКА: Не могу восстановить SceneManager!");
                    throw new InvalidOperationException("SceneManager не найден в сервисах");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SceneManager восстановлен из сервисов");
                }
            }

            // Включаем менеджер сцен
            _sceneManager.Enabled = true;
            _sceneManager.Visible = true;

            System.Diagnostics.Debug.WriteLine("SceneManager включен");

            System.Diagnostics.Debug.WriteLine("=== LoadContent завершён ===");
        }

        private void LoadInitialScene()
        {
            System.Diagnostics.Debug.WriteLine("--- Загрузка начальной сцены ---");

            if (_sceneManager == null)
            {
                System.Diagnostics.Debug.WriteLine("КРИТИЧЕСКАЯ ОШИБКА: SceneManager is null!");
                throw new InvalidOperationException("SceneManager не инициализирован");
            }

            try
            {
                // Создаём игровую сцену
                var gameScene = new GameScene(this);
                _sceneManager.AddScene("game", gameScene);
                System.Diagnostics.Debug.WriteLine("GameScene создана");

                // Переключаемся на неё
                _sceneManager.SwitchTo("game");
                System.Diagnostics.Debug.WriteLine("Переключение на GameScene успешно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА загрузки сцены: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }

            System.Diagnostics.Debug.WriteLine("--- Начальная сцена загружена ---");
        }

        protected override void Update(GameTime gameTime)
        {
            // Обновляем InputManager каждый кадр
            InputManager.Update();

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