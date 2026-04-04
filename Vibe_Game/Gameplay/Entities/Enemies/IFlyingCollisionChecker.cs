using Microsoft.Xna.Framework;

namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>
/// Проверка «куда летающий юнит зайти не может».
/// Реализует сцена: для летающих — обычно периметр комнаты + двери, без внутренних декоративных стен.
/// </summary>
public interface IFlyingCollisionChecker
{
    /// <param name="worldPosition">Точка в мировых пикселях.</param>
    /// <returns>true, если для летающего существа эта точка считается заблокированной.</returns>
    bool IsFlyingBlocked(Vector2 worldPosition);
}
