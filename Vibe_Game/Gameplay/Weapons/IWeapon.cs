using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Vibe_Game.Gameplay.Weapons
{
    public interface IWeapon
    {
        /// <summary>Имя для UI/отладки.</summary>
        string DisplayName { get; }

        /// <summary>Когда именно вызывать <see cref="TryPrimaryAttack"/> из игрока.</summary>
        WeaponFireMode FireMode { get; }

        /// <summary>Базовая отдача оружия (сила толчка в обратную сторону от направления атаки).</summary>
        float BaseRecoil { get; }

        void Update(GameTime gameTime, IAttackContext context);
        /// <summary>true, если атака реально произошла (кулдаун стартанул и т.д.).</summary>
        bool TryPrimaryAttack(IAttackContext context, Vector2 ownerPosition, Vector2 facingDirection);
        void Draw(SpriteBatch spriteBatch, IAttackContext context);
    }
}
