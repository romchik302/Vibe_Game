using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Gameplay.Entities;

namespace Vibe_Game.Gameplay.Projectiles;

public sealed class Projectile : Entity
{
    public Vector2 Direction { get; private set; }
    public float Speed { get; }
    public float Damage { get; }
    public float LifeLeft { get; private set; }
    public float Radius {  get; private set; }
    public float RecoilForce { get; }  // Сила отдачи при попадании
    public bool IsFriendlyToPlayer { get; }
    public bool IsOrbiting { get; private set; }
    public Vector2 OrbitCenter { get; private set; }
    public float OrbitRadius { get; private set; }
    public float OrbitAngle { get; private set; }
    public float OrbitAngularSpeed { get; private set; }
    public float OrbitTimeLeft { get; private set; }
    public bool ReleaseAfterOrbit { get; private set; }
    public Vector2 ReleaseDirection { get; private set; }
    public bool IgnoreWallCollisions { get; private set; }
    public float Length { get; private set; }

    public Projectile(
        Vector2 position,
        Vector2 direction,
        float speed,
        float damage,
        float lifetimeSeconds,
        float radius,
        float recoilForce = 0f,
        bool isFriendlyToPlayer = true,
        bool ignoreWallCollisions = false,
        float length = 0f)
    {
        Position = position;
        Direction = direction.LengthSquared() > 0.0001f ? Vector2.Normalize(direction) : Vector2.UnitX;
        Speed = speed;
        Damage = damage;
        LifeLeft = lifetimeSeconds;
        Velocity = Direction * speed;
        Radius = radius;
        RecoilForce = recoilForce;
        IsFriendlyToPlayer = isFriendlyToPlayer;
        IgnoreWallCollisions = ignoreWallCollisions;
        Length = length;
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (IsOrbiting)
        {
            OrbitAngle += OrbitAngularSpeed * dt;
            Position = OrbitCenter + new Vector2(MathF.Cos(OrbitAngle), MathF.Sin(OrbitAngle)) * OrbitRadius;
            OrbitTimeLeft -= dt;

            if (OrbitTimeLeft <= 0f)
            {
                IsOrbiting = false;
                if (ReleaseAfterOrbit)
                {
                    Vector2 releaseDir = ReleaseDirection;
                    if (releaseDir.LengthSquared() < 0.0001f)
                        releaseDir = new Vector2(MathF.Cos(OrbitAngle), MathF.Sin(OrbitAngle));

                    ReleaseDirection = Vector2.Normalize(releaseDir);
                    Direction = ReleaseDirection;
                    Velocity = Direction * Speed;
                }
                else
                {
                    IsAlive = false;
                }
            }
        }
        else
        {
            base.Update(gameTime);
        }

        LifeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (LifeLeft <= 0f)
            IsAlive = false;
    }
    public override Rectangle GetBounds()
    {
        int r = (int)Radius;
        return new Rectangle(
            (int)Position.X - r,
            (int)Position.Y - r,
            r * 2,
            r * 2
        );
    }

    public void ConfigureOrbit(
        Vector2 center,
        float radius,
        float startAngle,
        float angularSpeed,
        float durationSeconds,
        bool releaseAfterOrbit,
        Vector2 releaseDirection)
    {
        IsOrbiting = true;
        OrbitCenter = center;
        OrbitRadius = radius;
        OrbitAngle = startAngle;
        OrbitAngularSpeed = angularSpeed;
        OrbitTimeLeft = durationSeconds;
        ReleaseAfterOrbit = releaseAfterOrbit;
        ReleaseDirection = releaseDirection;
        Velocity = Vector2.Zero;
        Position = OrbitCenter + new Vector2(MathF.Cos(OrbitAngle), MathF.Sin(OrbitAngle)) * OrbitRadius;
    }
}