// Gameplay/Entities/Player/Player.cs (дополнения)
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

        // Для анимации
        private PlayerRenderer _animationRenderer; // Приведение типа для доступа к Update

        // Таймер неуязвимости после получения урона (в секундах)
        private float _invincibilityTimer = 0f;
        private const float InvincibilityDuration = 1.4f;

        private float _flashingTimer = 0f;
        private const float FlashingDuration = 0.2f;

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

            // Сохраняем ссылку на анимационный рендерер, если он используется
            _animationRenderer = renderer as PlayerRenderer;
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

            // ОБНОВЛЯЕМ ТАЙМЕР НЕУЯЗВИМОСТИ
            if (_invincibilityTimer > 0)
            {
                _invincibilityTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_invincibilityTimer < 0) _invincibilityTimer = 0;

                _flashingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_flashingTimer > FlashingDuration)
                {
                    if (Color != Color.White)
                    {
                        Color = Color.White;
                    }
                    else
                    {
                        Color = Color.White * 0.25f;
                    }

                    _flashingTimer = 0f;
                }
            }
            else
            {
                // Возвращаем нормальный цвет после окончания неуязвимости
                if (Color != Color.White)
                    Color = Color.White;
            }

            // ОБНОВЛЯЕМ АНИМАЦИЮ
            if (_animationRenderer != null)
            {
                _animationRenderer.Update(gameTime, Velocity, _lastShootDirection);
            }

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

        public void TakeDamage(float amount)
        {
            if (_invincibilityTimer > 0) return; // Игрок неуязвим
            if (amount <= 0) return;

            Stats.TakeDamage(amount);
            _invincibilityTimer = InvincibilityDuration;

            // Визуальный эффект - мигание 
            Color = Color.White * 0.25f;
        }

        public bool IsInvincible => _invincibilityTimer > 0;

        private bool IsAnyShootDirectionHeld()
        {
            return _inputService.IsActionDown(InputAction.ShootUp)
                || _inputService.IsActionDown(InputAction.ShootDown)
                || _inputService.IsActionDown(InputAction.ShootLeft)
                || _inputService.IsActionDown(InputAction.ShootRight);
        }
        public void SetMovementFrictionMultiplier(float multiplier)
        {
            Controller.SetFrictionMultiplier(multiplier);
        }
    }
}