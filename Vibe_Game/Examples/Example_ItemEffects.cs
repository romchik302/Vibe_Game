// Пример системы эффектов предметов с применением OCP
// Показывает, как расширять функциональность без изменения существующего кода

using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Gameplay.ItemsExample
{
    /// <summary>
    /// Рефакторинг PlayerStats с поддержкой системы эффектов
    /// </summary>
    public class PlayerStats
    {
        // Основные характеристики
        public float Health { get; set; } = 6f;
        public float MaxHealth { get; set; } = 6f;
        public float Damage { get; set; } = 3.5f;
        public float Speed { get; set; } = 1.0f;

        // Модификаторы
        public float DamageMultiplier { get; set; } = 1.0f;
        public float SpeedMultiplier { get; set; } = 1.0f;

        // Список активных эффектов
        private readonly List<IItemEffect> _activeEffects = new List<IItemEffect>();

        /// <summary>
        /// Применить эффект предмета (расширяемо через интерфейсы)
        /// </summary>
        public void ApplyEffect(IItemEffect effect)
        {
            if (effect == null)
                throw new System.ArgumentNullException(nameof(effect));

            effect.Apply(this);
            _activeEffects.Add(effect);
        }

        /// <summary>
        /// Удалить эффект предмета
        /// </summary>
        public void RemoveEffect(IItemEffect effect)
        {
            if (effect == null)
                return;

            if (_activeEffects.Remove(effect))
            {
                effect.Remove(this);
            }
        }

        /// <summary>
        /// Получить все активные эффекты
        /// </summary>
        public IEnumerable<IItemEffect> GetActiveEffects()
        {
            return _activeEffects.AsReadOnly();
        }

        /// <summary>
        /// Проверить, есть ли эффект определённого типа
        /// </summary>
        public bool HasEffect<T>() where T : IItemEffect
        {
            return _activeEffects.OfType<T>().Any();
        }
    }

    // ============================================
    // ПРИМЕРЫ РЕАЛИЗАЦИЙ ЭФФЕКТОВ
    // ============================================

    /// <summary>
    /// Увеличение урона
    /// </summary>
    public class DamageBoostEffect : IItemEffect
    {
        private readonly float _damageIncrease;

        public string Name => $"Damage Boost +{_damageIncrease}";
        public string Description => $"Увеличивает урон на {_damageIncrease}";

        public DamageBoostEffect(float damageIncrease)
        {
            _damageIncrease = damageIncrease;
        }

        public void Apply(PlayerStats stats)
        {
            stats.DamageMultiplier += _damageIncrease;
        }

        public void Remove(PlayerStats stats)
        {
            stats.DamageMultiplier -= _damageIncrease;
        }
    }

    /// <summary>
    /// Увеличение скорости
    /// </summary>
    public class SpeedBoostEffect : IItemEffect
    {
        private readonly float _speedIncrease;

        public string Name => $"Speed Boost +{_speedIncrease}";
        public string Description => $"Увеличивает скорость на {_speedIncrease * 100}%";

        public SpeedBoostEffect(float speedIncrease)
        {
            _speedIncrease = speedIncrease;
        }

        public void Apply(PlayerStats stats)
        {
            stats.SpeedMultiplier += _speedIncrease;
        }

        public void Remove(PlayerStats stats)
        {
            stats.SpeedMultiplier -= _speedIncrease;
        }
    }

    /// <summary>
    /// Увеличение здоровья
    /// </summary>
    public class HealthBoostEffect : IItemEffect
    {
        private readonly float _healthIncrease;

        public string Name => $"Health Boost +{_healthIncrease}";
        public string Description => $"Увеличивает максимальное здоровье на {_healthIncrease}";

        public HealthBoostEffect(float healthIncrease)
        {
            _healthIncrease = healthIncrease;
        }

        public void Apply(PlayerStats stats)
        {
            stats.MaxHealth += _healthIncrease;
            stats.Health += _healthIncrease; // Также восстанавливаем текущее здоровье
        }

        public void Remove(PlayerStats stats)
        {
            stats.MaxHealth -= _healthIncrease;
            // Не уменьшаем текущее здоровье при снятии эффекта
        }
    }

    /// <summary>
    /// Комбинированный эффект (пример композиции)
    /// </summary>
    public class CompositeItemEffect : IItemEffect
    {
        private readonly List<IItemEffect> _effects;

        public string Name { get; }
        public string Description { get; }

        public CompositeItemEffect(string name, string description, params IItemEffect[] effects)
        {
            Name = name;
            Description = description;
            _effects = new List<IItemEffect>(effects);
        }

        public void Apply(PlayerStats stats)
        {
            foreach (var effect in _effects)
            {
                effect.Apply(stats);
            }
        }

        public void Remove(PlayerStats stats)
        {
            foreach (var effect in _effects)
            {
                effect.Remove(stats);
            }
        }
    }

    /// <summary>
    /// Эффект с временным действием (пример расширенной функциональности)
    /// </summary>
    public class TemporaryEffect : IItemEffect
    {
        private readonly IItemEffect _baseEffect;
        private readonly float _duration;
        private float _remainingTime;

        public string Name => $"{_baseEffect.Name} (Temporary)";
        public string Description => $"{_baseEffect.Description}. Длительность: {_duration} сек";

        public TemporaryEffect(IItemEffect baseEffect, float duration)
        {
            _baseEffect = baseEffect ?? throw new System.ArgumentNullException(nameof(baseEffect));
            _duration = duration;
            _remainingTime = duration;
        }

        public void Apply(PlayerStats stats)
        {
            _baseEffect.Apply(stats);
        }

        public void Remove(PlayerStats stats)
        {
            _baseEffect.Remove(stats);
        }

        /// <summary>
        /// Обновить таймер эффекта
        /// </summary>
        public bool Update(float deltaTime)
        {
            _remainingTime -= deltaTime;
            return _remainingTime > 0;
        }
    }

    // ============================================
    // ПРИМЕР ИСПОЛЬЗОВАНИЯ
    // ============================================

    public class ItemEffectExample
    {
        public static void ExampleUsage()
        {
            var stats = new PlayerStats();

            // Применяем простой эффект
            var damageBoost = new DamageBoostEffect(0.5f);
            stats.ApplyEffect(damageBoost);
            // Теперь DamageMultiplier = 1.5f

            // Применяем комбинированный эффект
            var comboEffect = new CompositeItemEffect(
                "Super Boost",
                "Увеличивает урон и скорость",
                new DamageBoostEffect(0.3f),
                new SpeedBoostEffect(0.2f)
            );
            stats.ApplyEffect(comboEffect);

            // Проверяем наличие эффекта
            if (stats.HasEffect<DamageBoostEffect>())
            {
                // Делаем что-то
            }

            // Удаляем эффект
            stats.RemoveEffect(damageBoost);
        }
    }
}
