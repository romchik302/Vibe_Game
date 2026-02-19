using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Vibe_Game.Core.Interfaces
{
    public interface ICamera
    {
        public Vector2 Position { get; }
        float Zoom { get; set; }
        float Rotation { get; set; }
        int ViewportWidth { get; }
        int ViewportHeight { get; }
        Matrix TransformMatrix { get; }
        void SetBounds(Rectangle bounds);
        void Follow(Vector2 target);
        void Shake(float intensity, float duration);
        void UpdateShake(GameTime gameTime);
        Rectangle GetVisibleArea();
        Vector2 WorldToScreen(Vector2 worldPosition);
        Vector2 ScreenToWorld(Vector2 screenPosition);

    }
}
