using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Engine;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Utilities;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Scenes
{
    internal sealed class GameSceneRenderer
    {
        private const int MinimapRoomSize = 20;
        private const int MinimapSpacing = 22;
        private const int MinimapOffset = 10;

        private readonly Game _game;
        private readonly GameSceneState _state;
        private readonly GameSceneProjectileController _projectiles;
        private readonly GameSceneEnemyController _enemies;

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

            _projectiles.Draw(spriteBatch, pixel);
            _enemies.Draw(spriteBatch);
            _state.Player.Draw(spriteBatch);

#if DEBUG
            if (_state.Player.EquippedWeapon is SwordWeapon sword)
                sword.Draw(spriteBatch, attackContext);
#endif

            spriteBatch.End();

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawMinimap(spriteBatch, pixel);
            DrawPlayerHealthHud(spriteBatch, pixel);
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
                        pixel,
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
                    if (_state.FloorMap[x, y] == null)
                        continue;

                    Rectangle rect = new Rectangle(MinimapOffset + x * MinimapSpacing, MinimapOffset + y * MinimapSpacing, MinimapRoomSize, MinimapRoomSize);

                    Color roomColor = _state.FloorMap[x, y].Type switch
                    {
                        LevelGenerator.RoomType.Start => GameColors.MinimapStart,
                        LevelGenerator.RoomType.Boss => GameColors.MinimapBoss,
                        _ => GameColors.MinimapDefault
                    };

                    spriteBatch.Draw(pixel, rect, roomColor * 0.5f);

                    if (x == _state.CurrentRoomGrid.X && y == _state.CurrentRoomGrid.Y)
                        spriteBatch.DrawRectangle(pixel, rect, GameColors.MinimapCurrent, 1);
                }
            }
        }

        private void DrawPlayerHealthHud(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (_state.Player?.Stats == null)
                return;

            int maxCells = Math.Max(1, (int)MathF.Ceiling(_state.Player.Stats.MaxHealth));
            float currentHealth = MathHelper.Clamp(_state.Player.Stats.Health, 0f, _state.Player.Stats.MaxHealth);

            const int cellSize = 22;
            const int spacing = 6;
            const int marginRight = 16;
            const int marginTop = 16;
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
    }
}
