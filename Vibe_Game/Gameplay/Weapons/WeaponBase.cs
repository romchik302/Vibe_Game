using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Vibe_Game.Gameplay.Weapons;

public abstract class WeaponBase : IWeapon
{
    public string DisplayName { get; }

    public virtual WeaponFireMode FireMode => WeaponFireMode.AutoWhileDirectionHeld;

    private float _cooldownRemaining;

    protected float CooldownSeconds { get; set; }
    protected bool IsOnCooldown => _cooldownRemaining > 0f;

    protected WeaponBase(string displayName, float cooldownSeconds)
    {
        DisplayName = displayName;
        CooldownSeconds = cooldownSeconds;
    }

    public virtual void Update(GameTime gameTime, IAttackContext context)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_cooldownRemaining > 0f)
            _cooldownRemaining = System.MathF.Max(0f, _cooldownRemaining - dt);
    }

    protected bool TryStartCooldown()
    {
        if (_cooldownRemaining > 0f)
            return false;
        _cooldownRemaining = CooldownSeconds;
        return true;
    }

    public abstract bool TryPrimaryAttack(IAttackContext context, Vector2 ownerPosition, Vector2 facingDirection);

    public virtual void Draw(SpriteBatch spriteBatch, IAttackContext context) { }
}
