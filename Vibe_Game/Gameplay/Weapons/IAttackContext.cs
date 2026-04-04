using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Vibe_Game.Gameplay.Weapons;

/// <summary>
/// Всё, что нужно оружию за один вызов атаки (и опционально каждый кадр).
/// Реализует сцена или отдельный "менеджер боя".
/// </summary>
public interface IAttackContext
{
    GameTime GameTime { get; }
    void SpawnProjectile(ProjectileSpawnArgs args);
    bool WouldCollideAtWorld(Vector2 worldPosition, float collisionRadius);
    SpriteBatch SpriteBatch { get; } // если нужно рисовать эффекты оружия в Draw
}