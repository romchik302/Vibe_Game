// Пример настройки Dependency Injection в Game1.cs
// Показывает, как заменить Service Locator на DI контейнер

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Services;
using Vibe_Game.Scenes;

namespace Vibe_GameExample
{
    public partial class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private IServiceProvider _serviceProvider;
        private SceneManager _sceneManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Настраиваем DI контейнер
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Регистрируем IServiceProvider в MonoGame Services для обратной совместимости
            Services.AddService(typeof(IServiceProvider), _serviceProvider);

            // Инициализируем сервисы
            InitializeServices();

            // Загружаем начальную сцену
            LoadInitialScene();
        }

        /// <summary>
        /// Настройка DI контейнера
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            // Регистрируем Game как синглтон
            services.AddSingleton<Microsoft.Xna.Framework.Game>(this);

            // Регистрируем привязки клавиш
            services.AddSingleton<IInputBindings, DefaultInputBindings>();

            // Регистрируем сервис ввода
            services.AddSingleton<IInputService, InputService>();

            // Регистрируем камеру (будет создана после Initialize)
            services.AddSingleton<ICamera>(provider =>
            {
                return new Camera(
                    _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight,
                    provider.GetService<ICameraShake>()
                );
            });

            // Регистрируем эффект тряски камеры
            services.AddSingleton<ICameraShake, CameraShake>();

            // Регистрируем менеджер состояний игры
            services.AddSingleton<IGameStateManager, GameStateManager>();

            // Регистрируем репозиторий статистики
            services.AddSingleton<IStatsRepository, FileStatsRepository>();

            // Регистрируем сервис случайных чисел
            services.AddSingleton<IRandomService, RandomService>();

            // Регистрируем SceneManager
            services.AddSingleton<SceneManager>(provider =>
            {
                return new SceneManager(this, provider);
            });
        }

        private void InitializeServices()
        {
            // Создаём SpriteBatch
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), _spriteBatch);

            // Создаём пиксельную текстуру
            var pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
            Services.AddService(typeof(Texture2D), pixelTexture);

            // Получаем сервисы через DI
            var inputService = _serviceProvider.GetService<IInputService>();
            var camera = _serviceProvider.GetService<ICamera>();
            _sceneManager = _serviceProvider.GetService<SceneManager>();

            // Регистрируем в MonoGame Services для обратной совместимости
            Services.AddService(typeof(IInputService), inputService);
            Services.AddService(typeof(ICamera), camera);
            Services.AddService(typeof(SceneManager), _sceneManager);

            // Добавляем SceneManager в компоненты
            Components.Add(_sceneManager);
        }

        private void LoadInitialScene()
        {
            // Создаём сцену через DI
            var gameScene = new GameScene(this, _serviceProvider);
            _sceneManager.AddScene("game", gameScene);
            _sceneManager.SwitchTo("game");
        }

        protected override void Update(GameTime gameTime)
        {
            // Обновляем сервис ввода
            var inputService = _serviceProvider.GetService<IInputService>();
            inputService?.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }
    }
}

// ============================================
// Дополнительные классы для примера
// ============================================

namespace Vibe_Game.Core.Engine
{
    /// <summary>
    /// Реализация менеджера состояний игры
    /// </summary>
    public class GameStateManager : IGameStateManager
    {
        public GameState CurrentState { get; private set; }
        public event EventHandler<GameState> StateChanged;

        public void ChangeState(GameState newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
                StateChanged?.Invoke(this, newState);
            }
        }
    }

    /// <summary>
    /// Реализация эффекта тряски камеры
    /// </summary>
    public class CameraShake : ICameraShake
    {
        private Vector2 _shakeOffset;
        private float _shakeTimer;
        private float _shakeIntensity;
        private readonly IRandomService _random;

        public CameraShake(IRandomService random)
        {
            _random = random ?? throw new System.ArgumentNullException(nameof(random));
        }

        public Vector2 GetShakeOffset() => _shakeOffset;

        public void Shake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeTimer = duration;
        }

        public void Update(GameTime gameTime)
        {
            if (_shakeTimer > 0)
            {
                _shakeTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _shakeOffset = new Vector2(
                    (_random.NextFloat() * 2 - 1) * _shakeIntensity,
                    (_random.NextFloat() * 2 - 1) * _shakeIntensity
                );

                if (_shakeTimer <= 0)
                    _shakeOffset = Vector2.Zero;
            }
        }
    }

    /// <summary>
    /// Реализация репозитория статистики (сохранение в файл)
    /// </summary>
    public class FileStatsRepository : IStatsRepository
    {
        private readonly string _statsFilePath = "stats.json";

        public void SaveRunStats(RunStats stats)
        {
            // TODO: Реализовать сохранение в JSON
            throw new System.NotImplementedException();
        }

        public RunStats LoadBestStats()
        {
            // TODO: Реализовать загрузку из JSON
            throw new System.NotImplementedException();
        }

        public List<RunStats> LoadAllStats()
        {
            // TODO: Реализовать загрузку всех статистик
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// Реализация сервиса случайных чисел
    /// </summary>
    public class RandomService : IRandomService
    {
        private readonly System.Random _random = new System.Random();

        public int Next(int max) => _random.Next(max);
        public int Next(int min, int max) => _random.Next(min, max);
        public float NextFloat() => (float)_random.NextDouble();
        public float NextFloat(float min, float max) => min + (max - min) * NextFloat();
        public bool Chance(float probability) => NextFloat() < probability;
        public T RandomItem<T>(List<T> list) => list[Next(list.Count)];

        public Vector2 RandomDirection4Way()
        {
            var dirs = new[] 
            { 
                new Vector2(0, -1), 
                new Vector2(1, 0), 
                new Vector2(0, 1), 
                new Vector2(-1, 0) 
            };
            return dirs[Next(4)];
        }
    }
}
