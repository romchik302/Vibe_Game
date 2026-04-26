using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Gameplay.Entities.Player
{
    public class PlayerController
    {
        private readonly IInputService _inputService;
        private Vector2 _currentVelocity;

        public float Acceleration { get; set; } = 1800f;     // пикс/сек²
        public float MaxSpeed { get; set; } = 150f;          // пикс/сек
        public float BaseFriction { get; set; } = 1200f;      // торможение без ввода (пикс/сек²)
        public float CurrentFrictionMultiplier { get; set; } = 1f;

        public Vector2 CurrentVelocity => _currentVelocity;
        public Vector2 ShootDirection { get; private set; }

        private readonly List<InputAction> _shootPressOrder = new();

        public PlayerController(Player player, IInputService inputService)
        {
            _ = player;
            _inputService = inputService;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            HandleMovement(dt);
            UpdateShootDirectionIsaacStyle();
        }

        private void HandleMovement(float dt)
        {
            Vector2 inputDirection = Vector2.Zero;
            if (_inputService.IsActionDown(InputAction.MoveUp)) inputDirection.Y -= 1;
            if (_inputService.IsActionDown(InputAction.MoveDown)) inputDirection.Y += 1;
            if (_inputService.IsActionDown(InputAction.MoveLeft)) inputDirection.X -= 1;
            if (_inputService.IsActionDown(InputAction.MoveRight)) inputDirection.X += 1;
            if (inputDirection != Vector2.Zero) inputDirection.Normalize();

            // Ускорение при наличии ввода
            if (inputDirection != Vector2.Zero)
            {
                Vector2 targetVelocity = inputDirection * MaxSpeed;
                Vector2 deltaV = targetVelocity - _currentVelocity;
                float maxDelta = Acceleration * dt;
                if (deltaV.LengthSquared() > maxDelta * maxDelta)
                    deltaV = Vector2.Normalize(deltaV) * maxDelta;
                _currentVelocity += deltaV;
            }
            else
            {
                // Торможение (трение)
                float friction = BaseFriction * CurrentFrictionMultiplier;
                float frictionDelta = friction * dt;
                float speed = _currentVelocity.Length();
                if (speed > frictionDelta)
                    _currentVelocity -= _currentVelocity / speed * frictionDelta;
                else
                    _currentVelocity = Vector2.Zero;
            }

            // Ограничение максимальной скорости (на всякий случай)
            if (_currentVelocity.LengthSquared() > MaxSpeed * MaxSpeed)
                _currentVelocity = Vector2.Normalize(_currentVelocity) * MaxSpeed;
        }

        /// <summary>
        /// Только ортогональ: активное направление — по первой зажатой стрелке прицела;
        /// при нескольких нажатиях в одном кадре порядок фиксирован: Left, Right, Up, Down.
        /// </summary>
        private void UpdateShootDirectionIsaacStyle()
        {
            TryRegisterShoot(InputAction.ShootLeft);
            TryRegisterShoot(InputAction.ShootRight);
            TryRegisterShoot(InputAction.ShootUp);
            TryRegisterShoot(InputAction.ShootDown);

            TryUnregisterShoot(InputAction.ShootLeft);
            TryUnregisterShoot(InputAction.ShootRight);
            TryUnregisterShoot(InputAction.ShootUp);
            TryUnregisterShoot(InputAction.ShootDown);

            if (_shootPressOrder.Count > 0)
                ShootDirection = ToCardinal(_shootPressOrder[0]);
        }



        private void TryRegisterShoot(InputAction action)
        {
            if (!_inputService.IsActionPressed(action))
                return;
            if (!_shootPressOrder.Contains(action))
                _shootPressOrder.Add(action);
        }

        private void TryUnregisterShoot(InputAction action)
        {
            if (!_inputService.IsActionUp(action))
                return;
            _shootPressOrder.Remove(action);
        }
        public void SetFrictionMultiplier(float multiplier)
        {
            CurrentFrictionMultiplier = MathHelper.Max(0.1f, multiplier);
        }
        private static Vector2 ToCardinal(InputAction action)
        {
            return action switch
            {
                InputAction.ShootLeft => new Vector2(-1f, 0f),
                InputAction.ShootRight => new Vector2(1f, 0f),
                InputAction.ShootUp => new Vector2(0f, -1f),
                InputAction.ShootDown => new Vector2(0f, 1f),
                _ => Vector2.Zero
            };
        }
    }
}
