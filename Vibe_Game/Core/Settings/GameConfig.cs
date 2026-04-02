using Microsoft.Xna.Framework;

namespace Vibe_Game.Core.Settings
{
    // ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜
    public static class WorldConfig
    {
        public const int TileSize = 32;

        // ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜
        public const int RoomWidthTiles = 20;
        public const int RoomHeightTiles = 11;

        // ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜ (˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜˜˜)
        public const int RoomWidthPx = RoomWidthTiles * TileSize;
        public const int RoomHeightPx = RoomHeightTiles * TileSize;

        // ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜ (13x13)
        public const int GridSize = 13;
        public const int CenterGrid = 6;
    }

    // ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
    public static class PlayerConfig
    {
        // ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜
        public const int Size = 24;
        public const int Radius = Size / 2; // 12 ˜˜˜˜˜˜˜˜

        // ˜˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜˜˜ (˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜˜˜, ˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜ ˜˜˜˜˜˜)
        public const float CollisionOffset = 11.9f;

        // ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
        public const float BaseSpeed = 200f;
    }

    /// <summary>˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜ ˜˜˜˜˜˜˜˜˜ (˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜˜˜˜˜˜).</summary>
    public static class EnemyConfig
    {
        public const int DefaultFlyingRadius = 10;
        public const float DefaultFlyingMoveSpeed = 100f;
        public const int DefaultFlyingMaxHealth = 4;

        /// <summary>Âåðîÿòíîñòü îäíîãî ëåòàþùåãî âðàãà â ïîäõîäÿùåé êîìíàòå (0..1).</summary>
        public const float FlyingSpawnChancePerRoom = 0.45f;
    }

    // ˜˜˜˜˜˜˜ ˜˜˜˜
    public static class GameColors
    {

        public static readonly Color Background = new Color(15, 10, 20);
        public static readonly Color Floor = new Color(35, 25, 40);
        public static readonly Color Wall = new Color(70, 60, 80);

        // ˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜
        public static readonly Color ButtonLocked = Color.Yellow;
        public static readonly Color ButtonUnlocked = Color.Lime;

        // ˜˜˜˜˜ ˜˜ ˜˜˜˜-˜˜˜˜˜
        public static readonly Color MinimapStart = Color.DodgerBlue;
        public static readonly Color MinimapBoss = Color.Crimson;
        public static readonly Color MinimapCurrent = Color.Red;
        public static readonly Color MinimapDefault = Color.LightGray;
    }
}