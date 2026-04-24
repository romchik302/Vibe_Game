using System;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Gameplay.Entities.Enemies;

public sealed class ShooterFlyingEnemy : FlyingEnemy
{
    private readonly Random _rng = new();
    private bool _wasPlayerInsideAggro;
    private bool _isInAttackStop;
    private bool _hasShotOnCurrentStop;
    private float _attackStopTimeLeft;

    public Action<ProjectileSpawnArgs> ProjectileSpawner { get; set; }

    public float AggroRadius { get; set; } = EnemyConfig.ShooterAggroRadius;
    public float StopDurationMin { get; set; } = EnemyConfig.ShooterStopDurationMin;
    public float StopDurationMax { get; set; } = EnemyConfig.ShooterStopDurationMax;
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

        if (!_isInAttackStop && isInsideAggro && !_wasPlayerInsideAggro)
            BeginAttackStop();

        _wasPlayerInsideAggro = isInsideAggro;

        if (_isInAttackStop)
        {
            UpdateAttackStop(dt);
            return;
        }

        base.UpdateEnemy(gameTime);
    }

    private void BeginAttackStop()
    {
        _isInAttackStop = true;
        _hasShotOnCurrentStop = false;
        _attackStopTimeLeft = NextFloat(StopDurationMin, StopDurationMax);
        Velocity = Vector2.Zero;
        ResetRandomMovementBehavior();
        TryShootAtPlayer();
    }

    private void UpdateAttackStop(float dt)
    {
        Velocity = Vector2.Zero;
        _attackStopTimeLeft -= dt;
        if (_attackStopTimeLeft > 0f)
            return;

        _isInAttackStop = false;
        _hasShotOnCurrentStop = false;
        _attackStopTimeLeft = 0f;
    }

    private void TryShootAtPlayer()
    {
        if (_hasShotOnCurrentStop || ProjectileSpawner == null)
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

        _hasShotOnCurrentStop = true;
    }

    private float NextFloat(float min, float max)
    {
        if (max < min)
            (min, max) = (max, min);

        return min + (float)_rng.NextDouble() * (max - min);
    }
}
