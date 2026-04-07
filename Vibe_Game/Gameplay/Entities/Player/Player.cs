using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Gameplay.Entities.Player
{
    public class Player : Entity
    {
        private readonly IPlayerRenderer _renderer;
        private readonly IInputService _inputService;
        private readonly IPlayerContentLoader _contentLoader;
        private readonly IAttackContext _attackContext;

        public PlayerController Controller { get; private set; }
        public PlayerStats Stats { get; private set; }
        public IWeapon EquippedWeapon { get; set; }

        private Vector2 _lastShootDirection;

        public Player(
            Vector2 position,
            IPlayerRenderer renderer,
            IInputService inputService,
            IPlayerContentLoader contentLoader,
            IAttackContext attackContext)
            : base()
        {
            Position = position;
            _renderer = renderer ?? throw new System.ArgumentNullException(nameof(renderer));
            _inputService = inputService ?? throw new System.ArgumentNullException(nameof(inputService));
            _contentLoader = contentLoader ?? throw new System.ArgumentNullException(nameof(contentLoader));
            _attackContext = attackContext ?? throw new System.ArgumentNullException(nameof(attackContext));

            Controller = new PlayerController(this, _inputService);
            Stats = new PlayerStats();

            Color = Color.White;
        }

        public override void LoadContent(ContentManager content)
        {
            _contentLoader.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            Controller.Update(gameTime);

            _lastShootDirection = Controller.ShootDirection;

            if (EquippedWeapon != null)
            {
                EquippedWeapon.Update(gameTime, _attackContext);

                Vector2 dir = Controller.ShootDirection;
                switch (EquippedWeapon.FireMode)
                {
                    case WeaponFireMode.AutoWhileDirectionHeld:
                        if (IsAnyShootDirectionHeld() && dir != Vector2.Zero)
                            EquippedWeapon.TryPrimaryAttack(_attackContext, Position, dir);
                        break;
                    case WeaponFireMode.DirectionHeldPlusButtonPress:
                        if (IsAnyShootDirectionHeld()
                            && dir != Vector2.Zero
                            && _inputService.IsActionPressed(InputAction.Fire))
                            EquippedWeapon.TryPrimaryAttack(_attackContext, Position, dir);
                        break;
                }
            }

            if (EquippedWeapon is SwordWeapon sword)
            {
                sword.UpdateOwnerPosition(Position);
            }

            Velocity = Controller.CurrentVelocity;
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _renderer.Draw(spriteBatch, Position, _lastShootDirection, Color);
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(
                (int)Position.X - PlayerConfig.Radius,
                (int)Position.Y - PlayerConfig.Radius,
                PlayerConfig.Size,
                PlayerConfig.Size
            );
        }

        private bool IsAnyShootDirectionHeld()
        {
            return _inputService.IsActionDown(InputAction.ShootUp)
                || _inputService.IsActionDown(InputAction.ShootDown)
                || _inputService.IsActionDown(InputAction.ShootLeft)
                || _inputService.IsActionDown(InputAction.ShootRight);
        }
    }
}
