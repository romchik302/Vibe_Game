using Microsoft.Xna.Framework;

namespace Vibe_Game.Gameplay.Weapons;

public sealed class ForwardProjectileWeapon : WeaponBase
{
    public override WeaponFireMode FireMode => WeaponFireMode.AutoWhileDirectionHeld;

    private readonly float _projectileSpeed;
    private readonly int _damage;
    private readonly float _spawnOffset;
    private readonly float _lifetime;
    private readonly float _radius;
    private readonly float _recoilForce;

    public ForwardProjectileWeapon(
    float cooldownSeconds,
    float projectileSpeed,
    int damage,
    float spawnOffsetPixels,
    float lifetimeSeconds,
    float radius = 4f,
    float recoilForce = 100f)
    : base("Forward Shot", cooldownSeconds)
    {
        _projectileSpeed = projectileSpeed;
        _damage = damage;
        _spawnOffset = spawnOffsetPixels;
        _lifetime = lifetimeSeconds;
        _radius = radius;
        _recoilForce = recoilForce;
    }

    public override bool TryPrimaryAttack(IAttackContext context, Vector2 ownerPosition, Vector2 facingDirection)
    {
        if (facingDirection == Vector2.Zero)
            return false;
        if (!TryStartCooldown())
            return false;

        var dir = Vector2.Normalize(facingDirection);
        var spawn = ownerPosition + dir * _spawnOffset;

        context.SpawnProjectile(new ProjectileSpawnArgs
        {
            Position = spawn,
            Direction = dir,
            Speed = _projectileSpeed,
            Damage = _damage,
            LifetimeSeconds = _lifetime,
            Radius = _radius,
            RecoilForce = _recoilForce,
            IsFriendlyToPlayer = true
        });

        return true;
    }
}