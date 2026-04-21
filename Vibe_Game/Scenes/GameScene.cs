using System;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities.Enemies;
using Vibe_Game.Gameplay.Entities.Player;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Scenes
{
    public class GameScene : BaseScene
    {
        private static readonly Point StartRoomGrid = new(WorldConfig.CenterGrid, WorldConfig.CenterGrid);

        private readonly GameSceneState _state = new();
        private readonly IPlayerRenderer _playerRenderer;
        private readonly IInputService _inputService;
        private readonly IPlayerContentLoader _contentLoader;

        private GameSceneWorld _world;
        private GameSceneEnemyController _enemyController;
        private GameSceneProjectileController _projectileController;
        private GameSceneRenderer _renderer;
        private GameSceneAttackContext _attackContext;
        private bool _isPaused;
        private int _selectedPauseOption;

        public GameScene(Game game, IPlayerRenderer pr, IInputService isrv, IPlayerContentLoader pcl)
            : base(game)
        {
            _playerRenderer = pr;
            _inputService = isrv;
            _contentLoader = pcl;
        }

        public override void Initialize()
        {
            _world = new GameSceneWorld(_state);
            _enemyController = new GameSceneEnemyController(_state, _world);
            _projectileController = new GameSceneProjectileController(_state, _world);
            _renderer = new GameSceneRenderer(GameInstance, _state, _projectileController, _enemyController);
            _attackContext = new GameSceneAttackContext(_state, _world, _projectileController, _enemyController);

            Vector2 startPos = GetStartWorldPosition();
            _state.Player = new Player(startPos, _playerRenderer, _inputService, _contentLoader, _attackContext);
            _state.Player.EquippedWeapon = new ForwardProjectileWeapon(0.3f, 250f, 5, 10f, 3f);

            LoadFloor(floorIndex: 1);

            base.Initialize();
        }

        public override void LoadContent()
        {
            Enemy.LoadSharedTextures(GameInstance.Content);
            _state.Player.LoadContent(GameInstance.Content);
            _renderer.LoadContent(GameInstance.Content);
        }

        public override void Update(GameTime gameTime)
        {
            if (_inputService.IsActionPressed(InputAction.Pause))
            {
                _isPaused = !_isPaused;
                if (_isPaused)
                    _selectedPauseOption = 0;

                return;
            }

            if (_isPaused)
            {
                UpdatePauseMenu();
                return;
            }

            _attackContext.Sync(gameTime);

            Vector2 oldPos = _state.Player.Position;
            _state.Player.Update(gameTime);

            _world.CheckTileCollision(oldPos);

            if (_state.CurrentRoomGrid != _state.LastRoomGrid)
            {
                _state.VisitedRooms.Add(_state.CurrentRoomGrid);
                _enemyController.ActivateEnemies(_state.CurrentRoomGrid);
                _world.OnRoomEntered(_state.CurrentRoomGrid, _state.LastRoomGrid);
                _state.LastRoomGrid = _state.CurrentRoomGrid;
            }

            _projectileController.Update(gameTime);
            _enemyController.Update(gameTime);
            _world.UpdateCurrentRoomState();
            _state.IsPlayerStandingOnFloorExit = _world.TryGetFloorExitTarget(out int nextFloorIndex);

            if (_state.IsPlayerStandingOnFloorExit && _inputService.IsActionPressed(InputAction.Interact))
                LoadFloor(nextFloorIndex);

            _world.UpdateCamera(GetCamera());

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            var spriteBatch = GetSpriteBatch();
            var pixel = GetPixelTexture();

            if (spriteBatch == null || pixel == null)
                return;

            _renderer.Draw(_attackContext, GetCamera(), spriteBatch, pixel);

            if (_isPaused)
                _renderer.DrawPauseOverlay(spriteBatch, pixel, _selectedPauseOption);
        }

        private void LoadFloor(int floorIndex)
        {
            var levelGenerator = new LevelGenerator();

            _state.CurrentFloorIndex = Math.Clamp(floorIndex, 1, _state.MaxFloorIndex);
            _state.FloorMap = levelGenerator.GenerateFloor(_state.CurrentFloorIndex);
            _state.Projectiles.Clear();
            _state.VisitedRooms.Clear();
            _state.IsPlayerStandingOnFloorExit = false;
            _state.CurrentRoomGrid = StartRoomGrid;
            _state.LastRoomGrid = StartRoomGrid;

            Vector2 startPos = GetStartWorldPosition();
            _state.Player.Position = startPos;
            _state.CameraPosition = startPos;
            _state.VisitedRooms.Add(StartRoomGrid);

            _world.InitializeDoorStates();
            _enemyController.SpawnEnemies(_state.CurrentFloorIndex);
            _world.RefreshEnemyOccupancy();
        }

        private static Vector2 GetStartWorldPosition()
        {
            return new Vector2(
                StartRoomGrid.X * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                StartRoomGrid.Y * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f
            );
        }

        private void UpdatePauseMenu()
        {
            if (IsMenuUpPressed())
                _selectedPauseOption = (_selectedPauseOption - 1 + 2) % 2;

            if (IsMenuDownPressed())
                _selectedPauseOption = (_selectedPauseOption + 1) % 2;

            if (!IsConfirmPressed())
                return;

            if (_selectedPauseOption == 0)
            {
                _isPaused = false;
                return;
            }

            _isPaused = false;
            ((Game1)GameInstance).ShowMainMenu();
        }

        private bool IsMenuUpPressed()
        {
            return _inputService.IsActionPressed(InputAction.MoveUp) ||
                   _inputService.IsActionPressed(InputAction.ShootUp);
        }

        private bool IsMenuDownPressed()
        {
            return _inputService.IsActionPressed(InputAction.MoveDown) ||
                   _inputService.IsActionPressed(InputAction.ShootDown);
        }

        private bool IsConfirmPressed()
        {
            return _inputService.IsActionPressed(InputAction.Fire) ||
                   _inputService.IsActionPressed(InputAction.Interact);
        }
    }
}
