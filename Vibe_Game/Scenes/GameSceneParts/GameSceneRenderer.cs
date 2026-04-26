using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Utilities;
using Vibe_Game.Gameplay.Entities;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Scenes
{
    internal sealed class GameSceneRenderer
    {
        private const int MinimapRoomSize = 24;
        private const int MinimapSpacing = 27;
        private const int MinimapOffset = 12;
        private const float MinimapTextScale = 0.42f;

        private readonly Game _game;
        private readonly GameSceneState _state;
        private readonly GameSceneProjectileController _projectiles;
        private readonly GameSceneEnemyController _enemies;
        private SpriteFont _roomFont;
        private Texture2D _tileTexture;

        public GameSceneRenderer(
            Game game,
            GameSceneState state,
            GameSceneProjectileController projectiles,
            GameSceneEnemyController enemies)
        {
            _game = game;
            _state = state;
            _projectiles = projectiles;
            _enemies = enemies;
        }

        public void LoadContent(ContentManager content)
        {
            _roomFont = content.Load<SpriteFont>("room_font");
            _tileTexture = content.Load<Texture2D>("player_sheet");
        }

        public void Draw(IAttackContext attackContext, Camera camera, SpriteBatch spriteBatch, Texture2D pixel)
        {
            _game.GraphicsDevice.Clear(GameColors.Background);

            spriteBatch.Begin(transformMatrix: camera?.GetShakenMatrix(), samplerState: SamplerState.PointClamp);

            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    if (_state.FloorMap[x, y] != null)
                        DrawSingleRoom(spriteBatch, pixel, _state.FloorMap[x, y], x, y);
                }
            }

            var drawables = new List<Entity>();

            drawables.AddRange(_enemies.GetEnemies());
            drawables.Add(_state.Player);

            // сортировка по Y
            drawables.Sort((a, b) => a.Position.Y.CompareTo(b.Position.Y));

            // отрисовка
            foreach (var d in drawables)
                d.Draw(spriteBatch);
            DrawCurrentRoomLabel(spriteBatch);

            // Отрисовка проджектайлов
            _projectiles.Draw(spriteBatch, pixel);

#if DEBUG
            if (_state.Player.EquippedWeapon is SwordWeapon sword)
                sword.Draw(spriteBatch, attackContext);
#endif

            spriteBatch.End();

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawMinimap(spriteBatch, pixel);
            DrawPlayerHealthHud(spriteBatch, pixel);
            DrawBossHealthBar(spriteBatch, pixel);
            spriteBatch.End();
        }

        public void DrawPauseOverlay(SpriteBatch spriteBatch, Texture2D pixel, int selectedOption)
        {
            if (_roomFont == null)
                return;

            Viewport viewport = _game.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            Rectangle panel = new Rectangle(viewport.Width / 2 - 220, viewport.Height / 2 - 150, 440, 300);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(pixel, fullscreen, GameColors.MenuOverlay);
            spriteBatch.Draw(pixel, panel, GameColors.MenuPanel);
            spriteBatch.DrawRectangle(pixel, panel, GameColors.MenuOutline, 2);

            DrawCenteredText(spriteBatch, "PAUSED", _roomFont, new Vector2(panel.Center.X, panel.Y + 58f), GameColors.RoomLabel, 1.2f, GameColors.RoomLabelShadow);
            DrawCenteredText(spriteBatch, "ESC TO RESUME", _roomFont, new Vector2(panel.Center.X, panel.Y + 96f), GameColors.MenuMuted, 0.55f);

            DrawPauseOption(spriteBatch, pixel, panel, 0, "CONTINUE", selectedOption == 0);
            DrawPauseOption(spriteBatch, pixel, panel, 1, "EXIT TO MENU", selectedOption == 1);
            spriteBatch.End();
        }

        private void DrawSingleRoom(SpriteBatch spriteBatch, Texture2D pixel, Room room, int gx, int gy)
        {
            int wx = gx * WorldConfig.RoomWidthPx;
            int wy = gy * WorldConfig.RoomHeightPx;

            for (int tx = 0; tx < WorldConfig.RoomWidthTiles; tx++)
            {
                for (int ty = 0; ty < WorldConfig.RoomHeightTiles; ty++)
                {
                    Color color = room.Tiles[tx, ty].Tint;
                    spriteBatch.Draw(
                        _tileTexture ?? pixel,
                        new Rectangle(wx + tx * WorldConfig.TileSize, wy + ty * WorldConfig.TileSize, WorldConfig.TileSize, WorldConfig.TileSize),
                        color
                    );
                }
            }
        }

        private void DrawMinimap(SpriteBatch spriteBatch, Texture2D pixel)
        {
            for (int x = 0; x < WorldConfig.GridSize; x++)
            {
                for (int y = 0; y < WorldConfig.GridSize; y++)
                {
                    Point grid = new Point(x, y);
                    if (_state.FloorMap[x, y] == null || !_state.VisitedRooms.Contains(grid))
                        continue;

                    Rectangle rect = new Rectangle(MinimapOffset + x * MinimapSpacing, MinimapOffset + y * MinimapSpacing, MinimapRoomSize, MinimapRoomSize);

                    Color roomColor = _state.FloorMap[x, y].Type switch
                    {
                        LevelGenerator.RoomType.Start => GameColors.MinimapStart,
                        LevelGenerator.RoomType.Boss => GameColors.MinimapBoss,
                        LevelGenerator.RoomType.Shop => GameColors.MinimapShop,
                        LevelGenerator.RoomType.Treasure => GameColors.MinimapTreasure,
                        LevelGenerator.RoomType.Secret => GameColors.MinimapSecret,
                        LevelGenerator.RoomType.SuperSecret => GameColors.MinimapSuperSecret,
                        LevelGenerator.RoomType.Challenge => GameColors.MinimapChallenge,
                        LevelGenerator.RoomType.Sacrifice => GameColors.MinimapSacrifice,
                        _ => GameColors.MinimapDefault
                    };

                    bool isCurrent = x == _state.CurrentRoomGrid.X && y == _state.CurrentRoomGrid.Y;
                    spriteBatch.Draw(pixel, rect, isCurrent ? roomColor : roomColor * 0.55f);
                    spriteBatch.DrawRectangle(pixel, rect, isCurrent ? GameColors.MinimapCurrent : GameColors.MinimapVisitedOutline, 1);

                    if (_roomFont != null)
                    {
                        DrawCenteredText(
                            spriteBatch,
                            GetRoomTypeShortLabel(_state.FloorMap[x, y].Type),
                            _roomFont,
                            new Vector2(rect.Center.X, rect.Center.Y),
                            GameColors.RoomLabel,
                            MinimapTextScale);
                    }
                }
            }
        }

        private void DrawCurrentRoomLabel(SpriteBatch spriteBatch)
        {
            if (_roomFont == null)
                return;

            Room room = _state.FloorMap[_state.CurrentRoomGrid.X, _state.CurrentRoomGrid.Y];
            if (room == null)
                return;

            Vector2 roomCenter = new Vector2(
                _state.CurrentRoomGrid.X * WorldConfig.RoomWidthPx + WorldConfig.RoomWidthPx / 2f,
                _state.CurrentRoomGrid.Y * WorldConfig.RoomHeightPx + WorldConfig.RoomHeightPx / 2f - 24f
            );

            string label = GetRoomTypeLabel(room.Type);
            DrawCenteredText(spriteBatch, label, _roomFont, roomCenter, GameColors.RoomLabel, 1f, GameColors.RoomLabelShadow);

            if (_state.IsPlayerStandingOnFloorExit)
            {
                DrawCenteredText(
                    spriteBatch,
                    "PRESS E TO DESCEND",
                    _roomFont,
                    roomCenter + new Vector2(0f, 34f),
                    GameColors.FloorHint,
                    0.7f,
                    GameColors.RoomLabelShadow);
            }
        }

        private static string GetRoomTypeShortLabel(LevelGenerator.RoomType roomType)
        {
            return roomType switch
            {
                LevelGenerator.RoomType.Start => "ST",
                LevelGenerator.RoomType.Boss => "BO",
                LevelGenerator.RoomType.Shop => "SH",
                LevelGenerator.RoomType.Treasure => "TR",
                LevelGenerator.RoomType.Secret => "SE",
                LevelGenerator.RoomType.SuperSecret => "SS",
                LevelGenerator.RoomType.Challenge => "CH",
                LevelGenerator.RoomType.Sacrifice => "SA",
                _ => "NO"
            };
        }

        private static string GetRoomTypeLabel(LevelGenerator.RoomType roomType)
        {
            return roomType switch
            {
                LevelGenerator.RoomType.Start => "START ROOM",
                LevelGenerator.RoomType.Boss => "BOSS ROOM",
                LevelGenerator.RoomType.Shop => "SHOP ROOM",
                LevelGenerator.RoomType.Treasure => "TREASURE ROOM",
                LevelGenerator.RoomType.Secret => "SECRET ROOM",
                LevelGenerator.RoomType.SuperSecret => "SUPER SECRET ROOM",
                LevelGenerator.RoomType.Challenge => "CHALLENGE ROOM",
                LevelGenerator.RoomType.Sacrifice => "SACRIFICE ROOM",
                _ => "NORMAL ROOM"
            };
        }

        private static void DrawCenteredText(
            SpriteBatch spriteBatch,
            string text,
            SpriteFont font,
            Vector2 center,
            Color color,
            float scale,
            Color? shadowColor = null)
        {
            Vector2 size = font.MeasureString(text) * scale;
            Vector2 origin = size / 2f;
            Vector2 position = center - origin;

            if (shadowColor.HasValue)
            {
                spriteBatch.DrawString(font, text, position + new Vector2(2f, 2f), shadowColor.Value, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }

            spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        private void DrawPlayerHealthHud(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (_state.Player?.Stats == null)
                return;

            int maxCells = Math.Max(1, (int)MathF.Ceiling(_state.Player.Stats.MaxHealth));
            float currentHealth = MathHelper.Clamp(_state.Player.Stats.Health, 0f, _state.Player.Stats.MaxHealth);

            const int cellSize = 28;
            const int spacing = 8;
            const int marginRight = 18;
            const int marginTop = 18;
            const int textureInset = 4;

            int totalWidth = maxCells * cellSize + (maxCells - 1) * spacing;
            int startX = _game.GraphicsDevice.Viewport.Width - marginRight - totalWidth;
            int y = marginTop;

            for (int i = 0; i < maxCells; i++)
            {
                int x = startX + i * (cellSize + spacing);
                Rectangle cellRect = new Rectangle(x, y, cellSize, cellSize);
                Rectangle innerRect = new Rectangle(x + textureInset, y + textureInset, cellSize - textureInset * 2, cellSize - textureInset * 2);

                spriteBatch.Draw(pixel, cellRect, new Color(20, 20, 26, 220));
                spriteBatch.DrawRectangle(pixel, cellRect, new Color(150, 150, 170), 1);
                spriteBatch.Draw(pixel, innerRect, new Color(45, 45, 55, 210));

                float cellHealth = MathHelper.Clamp(currentHealth - i, 0f, 1f);
                if (cellHealth <= 0f)
                    continue;

                Rectangle fillRect = new Rectangle(
                    innerRect.X + 1,
                    innerRect.Y + 1,
                    Math.Max(1, (int)((innerRect.Width - 2) * cellHealth)),
                    Math.Max(1, innerRect.Height - 2)
                );

                spriteBatch.Draw(pixel, fillRect, new Color(90, 220, 110));
            }
        }

        private void DrawBossHealthBar(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Find boss in current room
            Room currentRoom = _state.FloorMap[_state.CurrentRoomGrid.X, _state.CurrentRoomGrid.Y];
            if (currentRoom?.enemies == null)
                return;

            Gameplay.Entities.Enemies.BossEnemy boss = null;
            foreach (var enemy in currentRoom.enemies)
            {
                if (enemy is Gameplay.Entities.Enemies.BossEnemy b && b.IsAlive)
                {
                    boss = b;
                    break;
                }
            }

            if (boss == null)
                return;

            Viewport viewport = _game.GraphicsDevice.Viewport;

            const int barHeight = 24;
            const int barMarginBottom = 40;
            const int barMarginSide = 100;
            const int barPadding = 4;

            int barWidth = viewport.Width - barMarginSide * 2;
            int barX = barMarginSide;
            int barY = viewport.Height - barMarginBottom - barHeight;

            // Background
            Rectangle bgRect = new Rectangle(barX, barY, barWidth, barHeight);
            spriteBatch.Draw(pixel, bgRect, new Color(20, 15, 25, 240));
            spriteBatch.DrawRectangle(pixel, bgRect, new Color(180, 60, 60), 2);

            // Health fill
            float healthPercent = (float)boss.Health / boss.MaxHealth;
            int fillWidth = Math.Max(0, (int)(barWidth * healthPercent));
            Rectangle fillRect = new Rectangle(barX + barPadding, barY + barPadding, fillWidth - barPadding * 2, barHeight - barPadding * 2);
            spriteBatch.Draw(pixel, fillRect, new Color(220, 50, 50));

            // Boss name label
            if (_roomFont != null)
            {
                DrawCenteredText(
                    spriteBatch,
                    "BOSS",
                    _roomFont,
                    new Vector2(viewport.Width / 2, barY - 20),
                    new Color(255, 200, 200),
                    0.6f,
                    new Color(20, 10, 10, 180)
                );
            }
        }

        private void DrawPauseOption(SpriteBatch spriteBatch, Texture2D pixel, Rectangle panel, int optionIndex, string label, bool isSelected)
        {
            Rectangle optionRect = new Rectangle(panel.X + 52, panel.Y + 146 + optionIndex * 58, panel.Width - 104, 42);
            spriteBatch.Draw(pixel, optionRect, isSelected ? GameColors.MenuSelection : new Color(54, 52, 66));
            DrawCenteredText(
                spriteBatch,
                label,
                _roomFont,
                new Vector2(optionRect.Center.X, optionRect.Center.Y),
                isSelected ? GameColors.MenuBackground : GameColors.RoomLabel,
                0.75f);
        }
    }
}
