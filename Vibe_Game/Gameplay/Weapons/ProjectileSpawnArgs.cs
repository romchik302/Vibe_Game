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
    public float RecoilForce { get; init; }  // Сила отдачи при попадании
    public bool IsFriendlyToPlayer { get; init; }
    public bool UseOrbitMotion { get; init; }
    public Vector2 OrbitCenter { get; init; }
    public float OrbitRadius { get; init; }
    public float OrbitStartAngle { get; init; }
    public float OrbitAngularSpeed { get; init; }
    public float OrbitDurationSeconds { get; init; }
    public bool ReleaseAfterOrbit { get; init; }
    public Vector2 ReleaseDirection { get; init; }
    public bool IgnoreWallCollisions { get; init; }  // Проходит сквозь стены
    public float Length { get; init; }  // Длина вытянутого проджектила (для шипов)
}