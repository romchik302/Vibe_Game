using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Vibe_Game.Gameplay.Weapons;

public interface IAttackContext
{
    GameTime GameTime { get; }
    void SpawnProjectile(ProjectileSpawnArgs args);
    bool WouldCollideAtWorld(Vector2 worldPosition, float collisionRadius);
    SpriteBatch SpriteBatch { get; }

    void DamageEnemiesInArea(Vector2 center, float radius, int damage);
    object GetEnemyAtPoint(Vector2 point, float radius);
    void DamageEnemy(object enemy, int damage);
    Vector2 GetPlayerPosition();
    Vector2 GetCameraPosition();

    // НОВЫЙ МЕТОД - получает всех врагов в прямоугольнике
    List<object> GetEnemiesInArea(Rectangle bounds);
}