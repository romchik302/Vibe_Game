using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Vibe_Game.Scenes;

namespace Vibe_Game.Core.Engine
{
    public class SceneManager : DrawableGameComponent
    {
        private readonly Dictionary<string, BaseScene> _scenes = new();
        private BaseScene _currentScene;

        public SceneManager(Microsoft.Xna.Framework.Game game) : base(game)
        {
            // УБРАТЬ: game.Components.Add(this); ← УДАЛИ ЭТУ СТРОКУ!
            // Базовый конструктор уже это делает
            System.Diagnostics.Debug.WriteLine("SceneManager создан");
        }

        public void AddScene(string name, BaseScene scene)
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
