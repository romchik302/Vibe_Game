namespace Vibe_Game.Core.Interfaces;

public interface IWallCollisionChecker
{
    /// <summary>
    /// Проверяет, заблокирована ли точка стенами уровня (включая внутренние стены)
    /// </summary>
    bool IsPointBlockedByWall(Microsoft.Xna.Framework.Vector2 worldPosition);
}