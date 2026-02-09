using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Vibe_Game.Core.Scenes;

namespace Vibe_Game.Core.Engine
{
    public class SceneManager : DrawableGameComponent
    {
        private readonly Dictionary<string, Scene> _scenes = new();
        private Scene _currentScene;

        public SceneManager(Game game) : base(game)
        {
            game.Components.Add(this);
        }

        public void AddScene(string name, Scene scene)
        {
            _scenes[name] = scene;
            scene.Initialize();
        }

        public void SwitchTo(string name)
        {
            if (_scenes.TryGetValue(name, out var scene))
            {
                _currentScene?.UnloadContent();
                _currentScene = scene;
                _currentScene.LoadContent();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (_currentScene != null && Enabled)
                _currentScene.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_currentScene != null && Visible)
                _currentScene.Draw(gameTime);
        }
    }
}
