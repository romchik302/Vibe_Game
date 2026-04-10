using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Enemies;

public class ChasingEnemy : Enemy
{
    private readonly IWallCollisionChecker _collision;
    protected readonly float _collisionRadius;
    protected readonly float _moveSpeed;

    protected Texture2D _pixel;
    protected Texture2D _spriteSheet;
    protected Rectangle _sourceRect;
    protected int _frameWidth;
    protected int _frameHeight;
    protected int _frameCount;
    protected int _frameIndex;
    protected float _animTimer;
    protected virtual float AnimFrameDuration => 0.1f;
    public Vector2 ChaseTarget { get; set; }

    public ChasingEnemy(
        Vector2 position,
        IWallCollisionChecker collision,  // Изменили интерфейс
        float moveSpeed,
        int maxHealth,
        float collisionRadius)
        : base(position, maxHealth)
    {
        _collision = collision ?? throw new System.ArgumentNullException(nameof(collision));
        _moveSpeed = moveSpeed;
        _collisionRadius = collisionRadius;
        Color = Color.White;
        RecoilResistance = 0.7f;  // Средне отскакивает (50% сопротивление)
    }

    /// <summary>Удобный конструктор с константами из <see cref="EnemyConfig"/>.</summary>
    public ChasingEnemy(Vector2 position, IWallCollisionChecker collision)
        : this(
            position,
            collision,
            EnemyConfig.DefaultChasingMoveSpeed,
            EnemyConfig.DefaultChasingMaxHealth,
            EnemyConfig.DefaultChasingRadius)
    {
        EnsureSpriteConfigured();
    }

    protected override Vector2 ResolveRecoilCollision(Vector2 oldPos, Vector2 newPos)
    {
        Vector2 delta = newPos - oldPos;
        return ResolveWallCollision(oldPos, delta);
    }

    protected override float GetCollisionRadius()
    {
        return _collisionRadius;
    }

    protected override void UpdateEnemy(GameTime gameTime)
    {
        EnsureSpriteConfigured();
        UpdateAnimation(gameTime);

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 toTarget = ChaseTarget - Position;
        UpdateFacingFromDirection(toTarget, allowVertical: false);
        if (toTarget.LengthSquared() < 2f)
        {
            Velocity = Vector2.Zero;
            return;
        }

        toTarget.Normalize();
        Vector2 delta = toTarget * (_moveSpeed * dt);

        // Используем полную проверку коллизий со стенами (как у игрока)
        Position = ResolveWallCollision(Position, delta);
        Velocity = Vector2.Zero;
    }

    /// <summary>
    /// Полная проверка коллизий со стенами (включая внутренние)
    /// </summary>
    protected Vector2 ResolveWallCollision(Vector2 oldPos, Vector2 delta)
    {
        Vector2 target = oldPos + delta;
        Vector2 final = target;

        // Проверяем движение по X
        if (delta.X != 0f && HasWallCollisionAt(new Vector2(target.X, oldPos.Y)))
            final.X = oldPos.X;

        // Проверяем движение по Y (используя уже исправленный X)
        if (delta.Y != 0f && HasWallCollisionAt(new Vector2(final.X, target.Y)))
            final.Y = oldPos.Y;

        return final;
    }

    /// <summary>
    /// Проверяет 4 угла хитбокса на коллизию со стенами
    /// </summary>
    private bool HasWallCollisionAt(Vector2 centerWorld)
    {
        float o = _collisionRadius;
        return _collision.IsPointBlockedByWall(new Vector2(centerWorld.X - o, centerWorld.Y - o))
            || _collision.IsPointBlockedByWall(new Vector2(centerWorld.X + o, centerWorld.Y - o))
            || _collision.IsPointBlockedByWall(new Vector2(centerWorld.X - o, centerWorld.Y + o))
            || _collision.IsPointBlockedByWall(new Vector2(centerWorld.X + o, centerWorld.Y + o));
    }

    public override Rectangle GetBounds()
    {
        int r = (int)_collisionRadius;
        int d = r * 2;
        return new Rectangle((int)Position.X - r, (int)Position.Y - r, d, d);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || !IsActivated || spriteBatch == null)
            return;

        if (_spriteSheet != null)
        {
            spriteBatch.Draw(
                _spriteSheet,
                Position,
                _sourceRect,
                Color.White,
                0f,
                new Vector2(_frameWidth / 2f, _frameHeight / 2f),
                1f,
                GetHorizontalSpriteEffect(),
                0f
            );
            return;
        }

        if (_pixel == null)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        var rect = GetBounds();
        // Красный цвет для преследующего врага (отличается от фиолетового летающего)
        spriteBatch.Draw(_pixel, rect, new Color(255, 50, 50, 230));
    }

    protected virtual void EnsureSpriteConfigured()
    {
        if (_spriteSheet != null)
            return;

        _spriteSheet = SharedChasingTexture;
        if (_spriteSheet == null)
            return;

        _frameHeight = _spriteSheet.Height;
        _frameWidth = _frameHeight > 0 ? _frameHeight : _spriteSheet.Width;
        _frameWidth = Math.Clamp(_frameWidth, 1, _spriteSheet.Width);
        _frameCount = Math.Max(1, _spriteSheet.Width / _frameWidth);
        _sourceRect = new Rectangle(0, 0, _frameWidth, _frameHeight);
    }

    protected void UpdateAnimation(GameTime gameTime)
    {
        if (_spriteSheet == null)
            return;

        _animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_animTimer < AnimFrameDuration)
            return;

        _animTimer = 0f;
        _frameIndex = (_frameIndex + 1) % _frameCount;
        _sourceRect.X = _frameIndex * _frameWidth;
    }
}