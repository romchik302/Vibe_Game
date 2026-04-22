using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Enemies;

public class ChasingEnemy : Enemy
{
    private readonly IWallCollisionChecker _collision;

    protected readonly float _collisionRadius; // базовый (визуальный)
    protected readonly float _moveSpeed;

    // Хитбоксы
    protected float _bodyRadius;   // самый маленький (стены)
    protected float _attackRadius; // чуть больше (урон)

    protected Texture2D _pixel;
    protected Texture2D _spriteSheet;
    protected Rectangle _sourceRect;

    protected int _frameWidth;
    protected int _frameHeight;
    protected int _frameCount;
    protected int _frameIndex;

    protected float _animTimer;
    protected virtual float AnimFrameDuration => 0.2f;

    public Vector2 ChaseTarget { get; set; }

    public ChasingEnemy(
        Vector2 position,
        IWallCollisionChecker collision,
        float moveSpeed,
        int maxHealth,
        float collisionRadius)
        : base(position, maxHealth)
    {
        _collision = collision ?? throw new ArgumentNullException(nameof(collision));
        _moveSpeed = moveSpeed;
        _collisionRadius = collisionRadius;

        // Настройка хитбоксов
        _bodyRadius = collisionRadius * 0.8f;
        _attackRadius = collisionRadius * 0.9f;

        Color = Color.White;
        RecoilResistance = 0.7f;
    }

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
        return _bodyRadius;
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

        Position = ResolveWallCollision(Position, delta);
        Velocity = Vector2.Zero;
    }

    protected Vector2 ResolveWallCollision(Vector2 oldPos, Vector2 delta)
    {
        Vector2 target = oldPos + delta;
        Vector2 final = target;

        Vector2 bodyCenter = GetBodyCenter(target);

        if (delta.X != 0f && HasWallCollisionAt(new Vector2(bodyCenter.X, oldPos.Y)))
            final.X = oldPos.X;

        bodyCenter = GetBodyCenter(new Vector2(final.X, target.Y));

        if (delta.Y != 0f && HasWallCollisionAt(new Vector2(final.X, bodyCenter.Y)))
            final.Y = oldPos.Y;

        return final;
    }

    private bool HasWallCollisionAt(Vector2 centerWorld)
    {
        float o = _bodyRadius;

        return _collision.IsPointBlockedByWall(new Vector2(centerWorld.X - o, centerWorld.Y - o))
            || _collision.IsPointBlockedByWall(new Vector2(centerWorld.X + o, centerWorld.Y - o))
            || _collision.IsPointBlockedByWall(new Vector2(centerWorld.X - o, centerWorld.Y + o))
            || _collision.IsPointBlockedByWall(new Vector2(centerWorld.X + o, centerWorld.Y + o));
    }

    // >>> ГЛАВНОЕ: смещённый центр под "тело"
    protected Vector2 GetBodyCenter(Vector2 basePos)
    {
        float offsetX = _frameWidth * 0.25f;

        if (Facing == FacingDirection.Left)
            offsetX = -offsetX;

        return basePos + new Vector2(offsetX, 0);
    }

    protected Vector2 GetBodyCenter()
    {
        return GetBodyCenter(Position);
    }

    public override Rectangle GetBounds()
    {
        var center = GetBodyCenter();

        int r = (int)_bodyRadius;
        int d = r * 2;

        return new Rectangle((int)center.X - r, (int)center.Y - r, d, d);
    }

    public Rectangle GetAttackBounds()
    {
        var center = GetBodyCenter();

        int r = (int)_attackRadius;
        int d = r * 2;

        return new Rectangle((int)center.X - r, (int)center.Y - r, d, d);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || !IsActivated || spriteBatch == null)
            return;

        if (_spriteSheet != null)
        {
            // >>> Смещаем origin вправо (тело справа)
            var origin = new Vector2(_frameWidth * 0.75f, _frameHeight / 2f);

            spriteBatch.Draw(
                _spriteSheet,
                Position,
                _sourceRect,
                Color.White,
                0f,
                origin,
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

        // >>> ФИКС: ровно 4 кадра
        _frameCount = 4;
        _frameWidth = _spriteSheet.Width / _frameCount;

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