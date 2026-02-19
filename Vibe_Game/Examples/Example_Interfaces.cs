// Примеры интерфейсов для рефакторинга проекта
// Эти интерфейсы должны быть созданы в папке Core/Interfaces/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Vibe_Game.Core.IExample
{
    // ============================================
    // ИНТЕРФЕЙСЫ ДЛЯ СЕРВИСОВ
    // ============================================

    /// <summary>
    /// Сервис для обработки ввода пользователя
    /// </summary>
    public interface IInputService
    {
        /// <summary>
        /// Проверяет, была ли нажата клавиша для действия (однократное нажатие)
        /// </summary>
        bool IsActionPressed(InputAction action);

        /// <summary>
        /// Проверяет, удерживается ли клавиша для действия
        /// </summary>
        bool IsActionDown(InputAction action);

        /// <summary>
        /// Обновляет состояние ввода (должен вызываться каждый кадр)
        /// </summary>
        void Update();
    }

    /// <summary>
    /// Действия, которые может выполнять игрок
    /// </summary>
    public enum InputAction
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Shoot,
        Pause,
        Interact
    }

    /// <summary>
    /// Управление привязками клавиш
    /// </summary>
    public interface IInputBindings
    {
        /// <summary>
        /// Получить клавиши для действия
        /// </summary>
        IEnumerable<Keys> GetKeysForAction(InputAction action);

        /// <summary>
        /// Установить привязку клавиш для действия
        /// </summary>
        void SetBinding(InputAction action, Keys[] keys);

        /// <summary>
        /// Загрузить привязки из файла
        /// </summary>
        void LoadFromFile(string path);

        /// <summary>
        /// Сохранить привязки в файл
        /// </summary>
        void SaveToFile(string path);
    }

    /// <summary>
    /// Интерфейс камеры
    /// </summary>
    public interface ICamera
    {
        Vector2 Position { get; }
        float Zoom { get; set; }
        float Rotation { get; set; }
        int ViewportWidth { get; }
        int ViewportHeight { get; }
        Matrix TransformMatrix { get; }

        void SetBounds(Rectangle bounds);
        void Follow(Vector2 target);
        void Shake(float intensity, float duration);
        void UpdateShake(GameTime gameTime);
        Rectangle GetVisibleArea();
        Vector2 WorldToScreen(Vector2 worldPosition);
        Vector2 ScreenToWorld(Vector2 screenPosition);
    }

    /// <summary>
    /// Эффект тряски камеры
    /// </summary>
    public interface ICameraShake
    {
        Vector2 GetShakeOffset();
        void Shake(float intensity, float duration);
        void Update(GameTime gameTime);
    }

    /// <summary>
    /// Управление состоянием игры
    /// </summary>
    public interface IGameStateManager
    {
        GameState CurrentState { get; }
        event EventHandler<GameState> StateChanged;
        void ChangeState(GameState newState);
    }

    /// <summary>
    /// Состояния игры
    /// </summary>
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Репозиторий для сохранения статистики
    /// </summary>
    public interface IStatsRepository
    {
        void SaveRunStats(RunStats stats);
        RunStats LoadBestStats();
        List<RunStats> LoadAllStats();
    }

    /// <summary>
    /// Статистика пробега
    /// </summary>
    public class RunStats
    {
        public int Score { get; set; }
        public float PlayTime { get; set; }
        public int Floor { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Сервис для генерации случайных чисел (для тестируемости)
    /// </summary>
    public interface IRandomService
    {
        int Next(int max);
        int Next(int min, int max);
        float NextFloat();
        float NextFloat(float min, float max);
        bool Chance(float probability);
        T RandomItem<T>(List<T> list);
        Vector2 RandomDirection4Way();
    }

    // ============================================
    // ИНТЕРФЕЙСЫ ДЛЯ СУЩНОСТЕЙ
    // ============================================

    /// <summary>
    /// Рендерер для игрока
    /// </summary>
    public interface IPlayerRenderer
    {
        void Draw(SpriteBatch spriteBatch, Vector2 position, Vector2 shootDirection, Color color);
    }

    /// <summary>
    /// Загрузчик контента для игрока
    /// </summary>
    public interface IPlayerContentLoader
    {
        void LoadContent(ContentManager content);
        bool IsContentLoaded { get; }
    }

    /// <summary>
    /// Эффект предмета
    /// </summary>
    public interface IItemEffect
    {
        string Name { get; }
        string Description { get; }
        void Apply(PlayerStats stats);
        void Remove(PlayerStats stats);
    }

    /// <summary>
    /// Система коллизий
    /// </summary>
    public interface ICollisionSystem
    {
        void Register(ICollidable collidable);
        void Unregister(ICollidable collidable);
        IEnumerable<ICollidable> GetCollisions(Rectangle bounds);
        void Update(GameTime gameTime);
    }

    /// <summary>
    /// Объект, который может участвовать в коллизиях
    /// </summary>
    public interface ICollidable
    {
        Rectangle GetBounds();
        bool IsAlive { get; }
        void OnCollision(ICollidable other);
    }
}
