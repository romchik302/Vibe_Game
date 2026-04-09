using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>
/// Базовый враг: здоровье, активация при входе в комнату, общий жизненный цикл.
/// Конкретное поведение (как ходит, какая коллизия) — в наследниках через <see cref="UpdateEnemy"/>.
/// Наследует <see cref="Entity"/>: позиция, IsAlive, при желании Texture для отрисовки по умолчанию.
/// </summary>
public abstract class Enemy : Entity
{
    public int Health { get; set; }
    public int MaxHealth { get; protected set; }

    /// <summary>Сопротивление отдаче (0.0 = полная отдача, 1.0 = не отталкивается).</summary>
    public float RecoilResistance { get; protected set; } = 0f;

    /// <summary>Радиус врезания в игрока (на какое расстояние враг может зайти в игрока).</summary>
    public float PenetrationRadius { get; protected set; } = 0f;

    /// <summary>Пока false — AI и движение не крутятся (враг «спит» до входа игрока в комнату).</summary>
    public bool IsActivated { get; private set; }

    /// <summary>Velocity отдачи (плавное отталкивание).</summary>
    private Vector2 _recoilVelocity = Vector2.Zero;

    /// <summary>Коэффициент затухания отдачи (чем меньше, тем дольше длится отдача).</summary>
    private const float RecoilDamping = 0.85f;

    protected Enemy(Vector2 position, int maxHealth)
    {
        Position = position;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    /// <summary>Вызывается сценой один раз при первом входе игрока в комнату этого врага.</summary>
    public void Activate()
    {
        if (IsActivated)
            return;

        IsActivated = true;
        OnActivated();
    }

    /// <summary>Хуки: звук, анимация пробуждения и т.д.</summary>
    protected virtual void OnActivated()
    {
    }

    /// <summary>Получить урон; при Health &lt;= 0 выставляет IsAlive = false.</summary>
    public virtual void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            IsAlive = false;
        }
    }

    /// <summary>Применить отдачу от оружия (толчок в направлении).</summary>
    public virtual void ApplyRecoil(Vector2 recoilDirection, float recoilForce)
    {
        if (!IsAlive || !IsActivated)
            return;

        // Учитываем сопротивление отдаче
        float effectiveForce = recoilForce * (1.0f - RecoilResistance);
        if (effectiveForce <= 0)
            return;

        // Добавляем отдачу к velocity (плавное отталкивание)
        _recoilVelocity += recoilDirection * effectiveForce;
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsAlive || !IsActivated)
            return;

        // Применяем velocity отдачи
        if (_recoilVelocity != Vector2.Zero)
        {
            Vector2 oldPos = Position;
            Position += _recoilVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Проверяем коллизию со стенами при отдаче
            Position = ResolveRecoilCollision(oldPos, Position);

            _recoilVelocity *= RecoilDamping; // Затухание

            // Останавливаем если очень маленькая скорость
            if (_recoilVelocity.LengthSquared() < 0.01f)
                _recoilVelocity = Vector2.Zero;
        }

        UpdateEnemy(gameTime);
    }

    /// <summary>Проверяет коллизию со стенами при отдаче и возвращает скорректированную позицию.</summary>
    protected virtual Vector2 ResolveRecoilCollision(Vector2 oldPos, Vector2 newPos)
    {
        return newPos; // По умолчанию без проверки
    }

    /// <summary>Движение и AI конкретного типа врага.</summary>
    protected abstract void UpdateEnemy(GameTime gameTime);

    /// <summary>Возвращает радиус коллизии врага для проверки стен.</summary>
    protected virtual float GetCollisionRadius()
    {
        return 10f;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || !IsActivated)
            return;

        base.Draw(spriteBatch);
    }

}
