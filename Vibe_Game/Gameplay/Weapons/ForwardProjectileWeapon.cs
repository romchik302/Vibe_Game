using Microsoft.Xna.Framework;

namespace Vibe_Game.Gameplay.Weapons;

public sealed class ForwardProjectileWeapon : WeaponBase
{
    public override WeaponFireMode FireMode => WeaponFireMode.AutoWhileDirectionHeld;

    private readonly float _projectileSpeed;
    private readonly int _damage;
    private readonly float _spawnOffset;
    private readonly float _lifetime;

    public ForwardProjectileWeapon(
        float cooldownSeconds,
        float projectileSpeed,
        int damage,
        float spawnOffsetPixels,
        float lifetimeSeconds)
        : base("Forward Shot", cooldownSeconds)
    {
        _projectileSpeed = projectileSpeed;
        _damage = damage;
        _spawnOffset = spawnOffsetPixels;
        _lifetime = lifetimeSeconds;
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
            LifetimeSeconds = _lifetime
        });

        return true;
    }
}