using Microsoft.Xna.Framework;

namespace Vibe_Game.Gameplay.Weapons;

public readonly struct ProjectileSpawnArgs
{
    public Vector2 Position { get; init; }
    public Vector2 Direction { get; init; }
    public float Speed { get; init; }
    public float Damage { get; init; }
    public float LifetimeSeconds { get; init; }
    public float Radius { get; init; }
}