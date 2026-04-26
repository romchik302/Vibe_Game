using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Gameplay.Entities.Enemies;

public sealed class BossEnemy : Enemy
{
    private enum BossAttackType
    {
        SpikeBurst,
        SpinningSpikes,
        BurrowStrike,
        SummonMinions
    }

    private enum BurrowPhase
    {
        None,
        MovingTrail
    }

    private readonly IWallCollisionChecker _collision;
    private readonly Random _rng = new();
    private Texture2D _pixel;
    private BossAttackType? _lastAttack;
    private float _attackTimer;
    private float _cooldownTimer;
    private bool _isInAttack;
    private BurrowPhase _burrowPhase;
    private Vector2 _burrowTrailPosition;

    public Action<ProjectileSpawnArgs> ProjectileSpawner { get; set; }
    public Action<Vector2, bool> SummonEnemy { get; set; }
    public Action<float> DamagePlayer { get; set; }

    public Vector2 ChaseTarget { get; set; }

    public float MoveSpeed { get; set; } = EnemyConfig.BossMoveSpeed;
    public float CollisionRadius { get; set; } = EnemyConfig.BossRadius;
    public float AttackPauseMin { get; set; } = EnemyConfig.BossAttackPauseMin;
    public float AttackPauseMax { get; set; } = EnemyConfig.BossAttackPauseMax;
    public float ContactDamage { get; set; } = 1f;

    public int SpikeBurstProjectileCount { get; set; } = EnemyConfig.BossSpikeBurstProjectileCount;
    public float SpikeBurstProjectileSpeed { get; set; } = EnemyConfig.BossSpikeBurstProjectileSpeed;
    public float SpikeBurstProjectileLifetime { get; set; } = EnemyConfig.BossSpikeBurstProjectileLifetime;
    public float SpikeBurstProjectileRadius { get; set; } = EnemyConfig.BossSpikeBurstProjectileRadius;
    public float SpikeBurstSpawnRadius { get; set; } = EnemyConfig.BossSpikeBurstSpawnRadius;

    public int SpinningSpikeCount { get; set; } = EnemyConfig.BossSpinningSpikeCount;
    public float SpinningSpikeOrbitRadius { get; set; } = EnemyConfig.BossSpinningSpikeOrbitRadius;
    public float SpinningSpikeAngularSpeed { get; set; } = EnemyConfig.BossSpinningSpikeAngularSpeed;
    public float SpinningSpikeOrbitDuration { get; set; } = EnemyConfig.BossSpinningSpikeOrbitDuration;
    public float SpinningSpikeReleaseSpeed { get; set; } = EnemyConfig.BossSpinningSpikeReleaseSpeed;

    public float BurrowTravelDuration { get; set; } = EnemyConfig.BossBurrowTravelDuration;
    public float BurrowTrailSpeed { get; set; } = EnemyConfig.BossBurrowTrailSpeed;
    public float BurrowStrikeRadius { get; set; } = EnemyConfig.BossBurrowStrikeRadius;
    public bool IsInvulnerableDuringBurrow { get; set; } = EnemyConfig.BossInvulnerableDuringBurrow;

    public int SummonMinCount { get; set; } = EnemyConfig.BossSummonMinCount;
    public int SummonMaxCount { get; set; } = EnemyConfig.BossSummonMaxCount;
    public float SummonSpawnRadius { get; set; } = EnemyConfig.BossSummonSpawnRadius;
    public float SummonShooterChance { get; set; } = EnemyConfig.BossSummonShooterChance;
    public float SummonAttackWeight { get; set; } = EnemyConfig.BossSummonAttackWeight;

    public override bool IsInvulnerable => IsInvulnerableDuringBurrow && _burrowPhase != BurrowPhase.None;
    public override bool CanDealContactDamage => _burrowPhase == BurrowPhase.None;

    public BossEnemy(
        Vector2 position,
        IWallCollisionChecker collision,
        float moveSpeed,
        int maxHealth,
        float collisionRadius)
        : base(position, maxHealth)
    {
        _collision = collision ?? throw new ArgumentNullException(nameof(collision));
        MoveSpeed = moveSpeed;
        CollisionRadius = collisionRadius;
        RecoilResistance = 0.9f;
        PenetrationRadius = 2f;
        RandomBehaviorChance = 0f;
    }

    public BossEnemy(Vector2 position, IWallCollisionChecker collision)
        : this(position, collision, EnemyConfig.BossMoveSpeed, EnemyConfig.BossMaxHealth, EnemyConfig.BossRadius)
    {
    }

    protected override void UpdateEnemy(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_burrowPhase != BurrowPhase.None)
        {
            UpdateBurrow(dt);
            return;
        }

        if (_isInAttack)
        {
            _attackTimer -= dt;
            if (_attackTimer <= 0f)
                EndAttackAndEnterCooldown();

            return;
        }

        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= dt;
            Velocity = Vector2.Zero;
            return;
        }

        MoveTowardPlayer(dt);
        StartAttack(ChooseNextAttack());
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || !IsActivated || spriteBatch == null)
            return;

        if (_burrowPhase != BurrowPhase.None)
        {
            DrawBurrowTrail(spriteBatch);
            DrawDebugOverlay(spriteBatch);
            return;
        }

        if (_pixel == null)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        Rectangle body = GetBounds();
        spriteBatch.Draw(_pixel, body, new Color(95, 45, 45, 240));
        spriteBatch.Draw(_pixel, new Rectangle(body.X + 6, body.Y + 6, body.Width - 12, body.Height - 12), new Color(170, 65, 65, 235));
        DrawDebugOverlay(spriteBatch);
    }

    public override Rectangle GetBounds()
    {
        int r = (int)CollisionRadius;
        return new Rectangle((int)Position.X - r, (int)Position.Y - r, r * 2, r * 2);
    }

    private void MoveTowardPlayer(float dt)
    {
        Vector2 toTarget = ChaseTarget - Position;
        if (toTarget.LengthSquared() < 4f)
        {
            Velocity = Vector2.Zero;
            return;
        }

        toTarget.Normalize();
        Position = ResolveWallCollision(Position, toTarget * MoveSpeed * dt);
        Velocity = Vector2.Zero;
    }

    private BossAttackType ChooseNextAttack()
    {
        (BossAttackType type, float weight)[] weighted =
        {
            (BossAttackType.SpikeBurst, 1f),
            (BossAttackType.SpinningSpikes, 1f),
            (BossAttackType.BurrowStrike, 1f),
            (BossAttackType.SummonMinions, SummonAttackWeight)
        };

        float total = 0f;
        for (int i = 0; i < weighted.Length; i++)
        {
            if (_lastAttack.HasValue && weighted[i].type == _lastAttack.Value)
                continue;

            total += weighted[i].weight;
        }

        if (total <= 0f)
            return BossAttackType.SpikeBurst;

        float roll = (float)_rng.NextDouble() * total;
        float cumulative = 0f;
        for (int i = 0; i < weighted.Length; i++)
        {
            if (_lastAttack.HasValue && weighted[i].type == _lastAttack.Value)
                continue;

            cumulative += weighted[i].weight;
            if (roll <= cumulative)
                return weighted[i].type;
        }

        return BossAttackType.SpikeBurst;
    }

    private void StartAttack(BossAttackType attack)
    {
        _isInAttack = true;
        _lastAttack = attack;
        Velocity = Vector2.Zero;

        switch (attack)
        {
            case BossAttackType.SpikeBurst:
                ExecuteSpikeBurst();
                _attackTimer = 0.25f;
                break;

            case BossAttackType.SpinningSpikes:
                ExecuteSpinningSpikes();
                _attackTimer = SpinningSpikeOrbitDuration + 0.15f;
                break;

            case BossAttackType.BurrowStrike:
                BeginBurrow();
                break;

            case BossAttackType.SummonMinions:
                ExecuteSummon();
                _attackTimer = 0.4f;
                break;
        }
    }

    private void EndAttackAndEnterCooldown()
    {
        _isInAttack = false;
        _attackTimer = 0f;
        _cooldownTimer = NextRange(AttackPauseMin, AttackPauseMax);
    }

    private void ExecuteSpikeBurst()
    {
        if (ProjectileSpawner == null)
            return;

        int count = Math.Max(8, SpikeBurstProjectileCount);
        float step = MathHelper.TwoPi / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * step;
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            ProjectileSpawner.Invoke(new ProjectileSpawnArgs
            {
                Position = Position + dir * SpikeBurstSpawnRadius,
                Direction = dir,
                Speed = SpikeBurstProjectileSpeed,
                Damage = ContactDamage,
                LifetimeSeconds = SpikeBurstProjectileLifetime,
                Radius = SpikeBurstProjectileRadius,
                RecoilForce = 0f,
                IsFriendlyToPlayer = false,
                IgnoreWallCollisions = true,
                Length = 40f  // Elongated spike length
            });
        }
    }

    private void ExecuteSpinningSpikes()
    {
        if (ProjectileSpawner == null)
            return;

        int count = Math.Max(6, SpinningSpikeCount);
        float step = MathHelper.TwoPi / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * step;
            Vector2 outward = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            ProjectileSpawner.Invoke(new ProjectileSpawnArgs
            {
                Position = Position + outward * SpinningSpikeOrbitRadius,
                Direction = Vector2.Zero,
                Speed = SpinningSpikeReleaseSpeed,
                Damage = ContactDamage,
                LifetimeSeconds = SpinningSpikeOrbitDuration + 2f,
                Radius = SpikeBurstProjectileRadius,
                RecoilForce = 0f,
                IsFriendlyToPlayer = false,
                UseOrbitMotion = true,
                OrbitCenter = Position,
                OrbitRadius = SpinningSpikeOrbitRadius,
                OrbitStartAngle = angle,
                OrbitAngularSpeed = SpinningSpikeAngularSpeed,
                OrbitDurationSeconds = SpinningSpikeOrbitDuration,
                ReleaseAfterOrbit = true,
                ReleaseDirection = outward
            });
        }
    }

    private void BeginBurrow()
    {
        _burrowPhase = BurrowPhase.MovingTrail;
        _burrowTrailPosition = Position;
        _attackTimer = BurrowTravelDuration;
    }

    private void UpdateBurrow(float dt)
    {
        if (_burrowPhase != BurrowPhase.MovingTrail)
            return;

        _attackTimer -= dt;
        Vector2 toTarget = ChaseTarget - _burrowTrailPosition;
        if (toTarget.LengthSquared() > 1f)
        {
            Vector2 dir = Vector2.Normalize(toTarget);
            _burrowTrailPosition += dir * BurrowTrailSpeed * dt;
        }

        if (_attackTimer > 0f)
            return;

        Position = _burrowTrailPosition;
        _burrowPhase = BurrowPhase.None;
        TryBurrowStrikePlayer();
        EndAttackAndEnterCooldown();
    }

    private void TryBurrowStrikePlayer()
    {
        if (DamagePlayer == null)
            return;

        float dist = Vector2.Distance(Position, ChaseTarget);
        if (dist <= BurrowStrikeRadius)
            DamagePlayer.Invoke(ContactDamage);
    }

    private void ExecuteSummon()
    {
        if (SummonEnemy == null)
            return;

        int summonCount = _rng.Next(Math.Max(1, SummonMinCount), Math.Max(SummonMinCount + 1, SummonMaxCount + 1));
        for (int i = 0; i < summonCount; i++)
        {
            float angle = (float)_rng.NextDouble() * MathHelper.TwoPi;
            float radius = NextRange(SummonSpawnRadius * 0.55f, SummonSpawnRadius);
            Vector2 spawnPos = Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            bool spawnShooter = _rng.NextDouble() < SummonShooterChance;
            SummonEnemy.Invoke(spawnPos, spawnShooter);
        }
    }

    private Vector2 ResolveWallCollision(Vector2 oldPos, Vector2 delta)
    {
        Vector2 target = oldPos + delta;
        Vector2 final = target;
        float r = CollisionRadius;

        if (delta.X != 0f)
        {
            bool blockedX = _collision.IsPointBlockedByWall(new Vector2(target.X - r, oldPos.Y - r))
                || _collision.IsPointBlockedByWall(new Vector2(target.X + r, oldPos.Y - r))
                || _collision.IsPointBlockedByWall(new Vector2(target.X - r, oldPos.Y + r))
                || _collision.IsPointBlockedByWall(new Vector2(target.X + r, oldPos.Y + r));
            if (blockedX)
                final.X = oldPos.X;
        }

        if (delta.Y != 0f)
        {
            bool blockedY = _collision.IsPointBlockedByWall(new Vector2(final.X - r, target.Y - r))
                || _collision.IsPointBlockedByWall(new Vector2(final.X + r, target.Y - r))
                || _collision.IsPointBlockedByWall(new Vector2(final.X - r, target.Y + r))
                || _collision.IsPointBlockedByWall(new Vector2(final.X + r, target.Y + r));
            if (blockedY)
                final.Y = oldPos.Y;
        }

        return final;
    }

    private void DrawBurrowTrail(SpriteBatch spriteBatch)
    {
        if (_pixel == null)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        int radius = 12;
        Rectangle trail = new Rectangle((int)_burrowTrailPosition.X - radius, (int)_burrowTrailPosition.Y - radius, radius * 2, radius * 2);
        spriteBatch.Draw(_pixel, trail, new Color(135, 170, 85, 220));
    }

    private float NextRange(float min, float max)
    {
        if (max < min)
            (min, max) = (max, min);
        return min + (float)_rng.NextDouble() * (max - min);
    }

    protected override Rectangle? GetDebugAttackBounds()
    {
        int r = (int)BurrowStrikeRadius;
        return new Rectangle((int)Position.X - r, (int)Position.Y - r, r * 2, r * 2);
    }
}
