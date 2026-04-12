using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Scenes
{
    internal sealed class MainMenuScene : BaseScene
    {
        private readonly IInputService _inputService;
        private readonly string[] _options = { "START GAME", "EXIT" };
        private SpriteFont _font;
        private int _selectedIndex;

        public MainMenuScene(Game game, IInputService inputService)
            : base(game)
        {
            _inputService = inputService;
        }

        public override void LoadContent()
        {
            _font = GameInstance.Content.Load<SpriteFont>("room_font");
        }

        public override void Update(GameTime gameTime)
        {
            if (IsNavigateUpPressed())
                _selectedIndex = (_selectedIndex - 1 + _options.Length) % _options.Length;

            if (IsNavigateDownPressed())
                _selectedIndex = (_selectedIndex + 1) % _options.Length;

            if (IsConfirmPressed())
                ActivateSelectedOption();
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GetSpriteBatch();
            Texture2D pixel = GetPixelTexture();
            if (spriteBatch == null || pixel == null || _font == null)
                return;

            Viewport viewport = GameInstance.GraphicsDevice.Viewport;
            Rectangle panelRect = new Rectangle(viewport.Width / 2 - 240, viewport.Height / 2 - 170, 480, 340);

            GameInstance.GraphicsDevice.Clear(GameColors.MenuBackground);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            spriteBatch.Draw(pixel, panelRect, GameColors.MenuPanel);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Y - 3, panelRect.Width + 6, 3), GameColors.MenuOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Bottom, panelRect.Width + 6, 3), GameColors.MenuOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Y, 3, panelRect.Height), GameColors.MenuOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.Right, panelRect.Y, 3, panelRect.Height), GameColors.MenuOutline);

            DrawCenteredText(spriteBatch, "VIBE GAME", new Vector2(viewport.Width / 2f, panelRect.Y + 62f), GameColors.RoomLabel, 1.4f, GameColors.RoomLabelShadow);
            DrawCenteredText(spriteBatch, "W / S OR ARROWS TO MOVE", new Vector2(viewport.Width / 2f, panelRect.Y + 112f), GameColors.MenuMuted, 0.55f);
            DrawCenteredText(spriteBatch, "SPACE OR E TO SELECT", new Vector2(viewport.Width / 2f, panelRect.Y + 138f), GameColors.MenuMuted, 0.55f);

            for (int i = 0; i < _options.Length; i++)
            {
                bool isSelected = i == _selectedIndex;
                Rectangle optionRect = new Rectangle(panelRect.X + 58, panelRect.Y + 180 + i * 56, panelRect.Width - 116, 40);
                spriteBatch.Draw(pixel, optionRect, isSelected ? GameColors.MenuSelection : new Color(54, 52, 66));
                DrawCenteredText(
                    spriteBatch,
                    _options[i],
                    new Vector2(optionRect.Center.X, optionRect.Center.Y),
                    isSelected ? GameColors.MenuBackground : GameColors.RoomLabel,
                    0.8f);
            }

            spriteBatch.End();
        }

        private void ActivateSelectedOption()
        {
            if (_selectedIndex == 0)
            {
                ((Game1)GameInstance).StartNewGame();
                return;
            }

            GameInstance.Exit();
        }

        private bool IsNavigateUpPressed()
        {
            return _inputService.IsActionPressed(InputAction.MoveUp) ||
                   _inputService.IsActionPressed(InputAction.ShootUp);
        }

        private bool IsNavigateDownPressed()
        {
            return _inputService.IsActionPressed(InputAction.MoveDown) ||
                   _inputService.IsActionPressed(InputAction.ShootDown);
        }

        private bool IsConfirmPressed()
        {
            return _inputService.IsActionPressed(InputAction.Fire) ||
                   _inputService.IsActionPressed(InputAction.Interact);
        }

        private void DrawCenteredText(SpriteBatch spriteBatch, string text, Vector2 center, Color color, float scale, Color? shadowColor = null)
        {
            Vector2 size = _font.MeasureString(text) * scale;
            Vector2 position = center - size / 2f;

            if (shadowColor.HasValue)
                spriteBatch.DrawString(_font, text, position + new Vector2(2f, 2f), shadowColor.Value, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            spriteBatch.DrawString(_font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
