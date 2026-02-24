
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Utilities;

namespace Vibe_Game.Core.Engine
{ 
    public class Camera
    {
        // Основные свойства
        public Vector2 Position { get; private set; }
        public float Zoom { get; set; } = 2.0f; // Зум х2 для удобства
        public float Rotation { get; set; }

        // Границы комнаты
        private Rectangle _roomBounds;
        private bool _hasBounds = false;

        // Размер экрана
        public int ViewportWidth { get; }
        public int ViewportHeight { get; }

        private readonly RandomHelper _random;

        public Camera(int viewportWidth, int viewportHeight)
        {
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            _random = new RandomHelper();
        }

        // Матрица трансформации (самое важное!)
        public Matrix TransformMatrix
        {
            get
            {
                return
                    Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(Zoom) *
                    Matrix.CreateTranslation(ViewportWidth * 0.5f, ViewportHeight * 0.5f, 0);
            }
        }

        // Установить границы комнаты
        public void SetBounds(Rectangle bounds)
        {
            _roomBounds = bounds;
            _hasBounds = true;
        }

        // Следовать за целью с ограничениями
        public void Follow(Vector2 target)
        {
            // Плавное движение к цели с помощью Lerp
            Position = Vector2.Lerp(Position, target, 0.1f);

            // Если есть границы - ограничиваем
            if (_hasBounds)
            {
                // Видимая область с учётом зума
                var visibleArea = GetVisibleArea();

                // Если камера выходит за границы - корректируем
                if (visibleArea.Left < _roomBounds.Left)
                    Position = new Vector2(Position.X + (_roomBounds.Left - visibleArea.Left), Position.Y);

                if (visibleArea.Right > _roomBounds.Right)
                    Position = new Vector2(Position.X - (visibleArea.Right - _roomBounds.Right), Position.Y);

                if (visibleArea.Top < _roomBounds.Top)
                    Position = new Vector2(Position.X, Position.Y + (_roomBounds.Top - visibleArea.Top));

                if (visibleArea.Bottom > _roomBounds.Bottom)
                    Position = new Vector2(Position.X, Position.Y - (visibleArea.Bottom - _roomBounds.Bottom));
            }
        }

        // Получить видимую область в мировых координатах
        public Rectangle GetVisibleArea()
        {
            var inverseMatrix = Matrix.Invert(TransformMatrix);

            // Четыре угла экрана в мировых координатах
            var tl = Vector2.Transform(Vector2.Zero, inverseMatrix);
            var tr = Vector2.Transform(new Vector2(ViewportWidth, 0), inverseMatrix);
            var bl = Vector2.Transform(new Vector2(0, ViewportHeight), inverseMatrix);
            var br = Vector2.Transform(new Vector2(ViewportWidth, ViewportHeight), inverseMatrix);

            // Находим ограничивающий прямоугольник
            var min = new Vector2(
                MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
                MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y)))
            );
            var max = new Vector2(
                MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
                MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y)))
            );

            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        // Конвертация координат
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, TransformMatrix);
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(TransformMatrix));
        }

        // Очень загадочная тряска камеры при получении урона
        private Vector2 _shakeOffset;
        private float _shakeTimer;
        private float _shakeIntensity;

        public void Shake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeTimer = duration;
        }

        public void UpdateShake(GameTime gameTime)
        {
            if (_shakeTimer > 0)
            {
                _shakeTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _shakeOffset = new Vector2(
                    (_random.NextFloat() * 2 - 1) * _shakeIntensity,
                    (_random.NextFloat() * 2 - 1) * _shakeIntensity
                );

                if (_shakeTimer <= 0)
                    _shakeOffset = Vector2.Zero;
            }
        }
        // Матрица с учётом тряски
        public Matrix GetShakenMatrix()
        {
            return
                Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                Matrix.CreateTranslation(_shakeOffset.X, _shakeOffset.Y, 0) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom) *
                Matrix.CreateTranslation(ViewportWidth * 0.5f, ViewportHeight * 0.5f, 0);
        }
    }
}