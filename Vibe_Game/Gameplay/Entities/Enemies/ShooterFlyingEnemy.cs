using System;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Gameplay.Entities.Enemies;

public sealed class ShooterFlyingEnemy : FlyingEnemy
{
    private float _shotCooldownLeft;

    public Action<ProjectileSpawnArgs> ProjectileSpawner { get; set; }

    public float AggroRadius { get; set; } = EnemyConfig.ShooterAggroRadius;
    public float ShotIntervalSeconds { get; set; } = EnemyConfig.ShooterShotIntervalSeconds;
    public float ReentryShotCooldownSeconds { get; set; } = EnemyConfig.ShooterReentryShotCooldownSeconds;
    public float ShotSpeed { get; set; } = EnemyConfig.ShooterProjectileSpeed;
    public float ShotLifetimeSeconds { get; set; } = EnemyConfig.ShooterProjectileLifetime;
    public float ShotRadius { get; set; } = EnemyConfig.ShooterProjectileRadius;
    public float ShotRecoilForce { get; set; } = EnemyConfig.ShooterProjectileRecoilForce;
    public int ShotDamage { get; set; } = EnemyConfig.ShooterProjectileDamage;

    public ShooterFlyingEnemy(
        Vector2 position,
        IFlyingCollisionChecker collision,
        float moveSpeed,
        int maxHealth,
        float collisionRadius)
        : base(position, collision, moveSpeed, maxHealth, collisionRadius)
    {
    }

    public ShooterFlyingEnemy(Vector2 position, IFlyingCollisionChecker collision)
        : this(
            position,
            collision,
            EnemyConfig.ShooterMoveSpeed,
            EnemyConfig.ShooterMaxHealth,
            EnemyConfig.ShooterRadius)
    {
    }

    protected override void UpdateEnemy(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float distanceToPlayer = Vector2.Distance(Position, ChaseTarget);
        bool isInsideAggro = distanceToPlayer <= AggroRadius;

        if (isInsideAggro)
        {
            // Прокручиваем базовую анимацию, но без движения.
            Vector2 cachedTarget = ChaseTarget;
            ChaseTarget = Position;
            base.UpdateEnemy(gameTime);
            ChaseTarget = cachedTarget;

            UpdateAttackMode(dt);
            return;
        }

        base.UpdateEnemy(gameTime);
    }

    private void UpdateAttackMode(float dt)
    {
        Vector2 toPlayer = ChaseTarget - Position;
        UpdateFacingFromDirection(toPlayer, allowVertical: false);

        Velocity = Vector2.Zero;
        ResetRandomMovementBehavior();
        _shotCooldownLeft -= dt;
        if (_shotCooldownLeft <= 0f)
        {
            TryShootAtPlayer();
            _shotCooldownLeft = MathF.Max(
                0.05f,
                MathF.Max(ShotIntervalSeconds, ReentryShotCooldownSeconds)
            );
        }
    }

    private void TryShootAtPlayer()
    {
        if (ProjectileSpawner == null)
            return;

        Vector2 toPlayer = ChaseTarget - Position;
        if (toPlayer.LengthSquared() < 0.0001f)
            return;

        Vector2 direction = Vector2.Normalize(toPlayer);
        ProjectileSpawner.Invoke(new ProjectileSpawnArgs
        {
            Position = Position,
            Direction = direction,
            Speed = ShotSpeed,
            Damage = ShotDamage,
            LifetimeSeconds = ShotLifetimeSeconds,
            Radius = ShotRadius,
            RecoilForce = ShotRecoilForce,
            IsFriendlyToPlayer = false
        });
    }
}
