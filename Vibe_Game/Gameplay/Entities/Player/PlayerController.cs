using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Gameplay.Entities.Player
{
    public class PlayerController
    {
        public IInputService _inputService;

        public float MoveSpeed { get; set; } = 200f;
        public Vector2 CurrentVelocity { get; private set; }
        public Vector2 ShootDirection { get; private set; }

        private readonly List<InputAction> _shootPressOrder = new();

        public PlayerController(Player player, IInputService inputService)
        {
            _ = player;
            _inputService = inputService;
        }

        public void Update(GameTime gameTime)
        {
            HandleMovement();
            UpdateShootDirectionIsaacStyle();
        }

        private void HandleMovement()
        {
            Vector2 inputDirection = Vector2.Zero;

            if (_inputService.IsActionDown(InputAction.MoveUp))
                inputDirection.Y -= 1;
            if (_inputService.IsActionDown(InputAction.MoveDown))
                inputDirection.Y += 1;
            if (_inputService.IsActionDown(InputAction.MoveLeft))
                inputDirection.X -= 1;
            if (_inputService.IsActionDown(InputAction.MoveRight))
                inputDirection.X += 1;

            if (inputDirection != Vector2.Zero)
                inputDirection.Normalize();

            CurrentVelocity = inputDirection * MoveSpeed;
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
