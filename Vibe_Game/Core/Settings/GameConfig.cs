using Microsoft.Xna.Framework;

namespace Vibe_Game.Core.Settings
{
    // Настройки размеров и генерации мира
    public static class WorldConfig
    {
        public const int TileSize = 32;

        // Размеры комнаты в тайлах
        public const int RoomWidthTiles = 20;
        public const int RoomHeightTiles = 11;

        // Размеры комнаты в пикселях (вычисляются автоматически)
        public const int RoomWidthPx = RoomWidthTiles * TileSize;
        public const int RoomHeightPx = RoomHeightTiles * TileSize;

        // Настройки карты комнат (13x13)
        public const int GridSize = 13;
        public const int CenterGrid = 6;
    }

    // Настройки игрока
    public static class PlayerConfig
    {
        // Физический размер героя
        public const int Size = 24;
        public const int Radius = Size / 2; // 12 пикселей

        // Отступ для коллизии (чуть меньше радиуса, чтобы скользить по стенам)
        public const float CollisionOffset = 11.9f;

        // Базовая скорость
        public const float BaseSpeed = 200f;
    }

    // Палитра игры
    public static class GameColors
    {

        public static readonly Color Background = new Color(15, 10, 20);
        public static readonly Color Floor = new Color(35, 25, 40);
        public static readonly Color Wall = new Color(70, 60, 80);

        // Интерактивные объекты
        public static readonly Color ButtonLocked = Color.Yellow;
        public static readonly Color ButtonUnlocked = Color.Lime;

        // Цвета на мини-карте
        public static readonly Color MinimapStart = Color.DodgerBlue;
        public static readonly Color MinimapBoss = Color.Crimson;
        public static readonly Color MinimapCurrent = Color.Red;
        public static readonly Color MinimapDefault = Color.LightGray;
    }
}