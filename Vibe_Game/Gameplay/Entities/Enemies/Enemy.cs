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

    /// <summary>Пока false — AI и движение не крутятся (враг «спит» до входа игрока в комнату).</summary>
    public bool IsActivated { get; private set; }

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

    public override void Update(GameTime gameTime)
    {
        if (!IsAlive || !IsActivated)
            return;

        UpdateEnemy(gameTime);
    }

    /// <summary>Движение и AI конкретного типа врага.</summary>
    protected abstract void UpdateEnemy(GameTime gameTime);

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || !IsActivated)
            return;

        base.Draw(spriteBatch);
    }

}
