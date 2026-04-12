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
        private readonly GameSceneState _state = new();
        private readonly IPlayerRenderer _playerRenderer;
        private readonly IInputService _inputService;
        private readonly IPlayerContentLoader _contentLoader;

        private GameSceneWorld _world;
        private GameSceneEnemyController _enemyController;
        private GameSceneProjectileController _projectileController;
        private GameSceneRenderer _renderer;
        private GameSceneAttackContext _attackContext;

        public GameScene(Game game, IPlayerRenderer pr, IInputService isrv, IPlayerContentLoader pcl)
            : base(game)
        {
            _playerRenderer = pr;
            _inputService = isrv;
            _contentLoader = pcl;
        }

        public override void Initialize()
        {
            var levelGenerator = new LevelGenerator();
            _state.FloorMap = levelGenerator.GenerateFloor(1);

            _world = new GameSceneWorld(_state);
            _enemyController = new GameSceneEnemyController(_state, _world);
            _projectileController = new GameSceneProjectileController(_state, _world);
            _renderer = new GameSceneRenderer(GameInstance, _state, _projectileController, _enemyController);
            _attackContext = new GameSceneAttackContext(_state, _world, _projectileController, _enemyController);

            _enemyController.SpawnEnemies(floorIndex: 1);

            _state.CurrentRoomGrid = new Point(WorldConfig.CenterGrid, WorldConfig.CenterGrid);

            Vector2 startPos = new Vector2(
                WorldConfig.CenterGrid * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                WorldConfig.CenterGrid * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f
            );

            _state.Player = new Player(startPos, _playerRenderer, _inputService, _contentLoader, _attackContext);
            _state.Player.EquippedWeapon = new SwordWeapon();
            _state.CameraPosition = startPos;

            base.Initialize();
        }

        public override void LoadContent()
        {
            Enemy.LoadSharedTextures(GameInstance.Content);
            _state.Player.LoadContent(GameInstance.Content);
        }

        public override void Update(GameTime gameTime)
        {
            _attackContext.Sync(gameTime);

            Vector2 oldPos = _state.Player.Position;
            _state.Player.Update(gameTime);

            _world.CheckTileCollision(oldPos);
            _world.TryUnlockButton();
            _projectileController.Update(gameTime);

            if (_state.CurrentRoomGrid != _state.LastRoomGrid)
            {
                _enemyController.ActivateEnemies(_state.CurrentRoomGrid);
                _state.LastRoomGrid = _state.CurrentRoomGrid;
            }

            _enemyController.Update(gameTime);
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
        }
    }
}
