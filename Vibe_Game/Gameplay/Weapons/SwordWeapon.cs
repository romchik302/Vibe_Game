using Microsoft.Xna.Framework;

namespace Vibe_Game.Gameplay.Weapons;

/// <summary>
/// Заготовка под ближний бой: игрок держит стрелку направления и нажимает Fire для удара.
/// Логика удара и хитбокс — позже.
/// </summary>
public sealed class SwordWeapon : WeaponBase
{
    public override WeaponFireMode FireMode => WeaponFireMode.DirectionHeldPlusButtonPress;

    public SwordWeapon(float cooldownSeconds = 0.4f)
        : base("Sword", cooldownSeconds)
    {
    }

    public override bool TryPrimaryAttack(IAttackContext context, Vector2 ownerPosition, Vector2 facingDirection)
    {
        if (facingDirection == Vector2.Zero)
            return false;
        if (!TryStartCooldown())
            return false;

        // TODO: активировать короткий свинг, хитбокс, урон по врагам
        return true;
    }
}
