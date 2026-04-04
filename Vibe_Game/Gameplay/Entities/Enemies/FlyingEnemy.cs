using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>
/// Летающий враг: преследует <see cref="ChaseTarget"/>, не использует коллизию с внутренними
/// стенами комнаты — только то, что возвращает <see cref="IFlyingCollisionChecker"/> (задаёт сцена).
/// Движение разрешается по осям X и Y отдельно (скольжение вдоль стен периметра).
/// </summary>
public class FlyingEnemy : Enemy
{
    private readonly IFlyingCollisionChecker _collision;
    private readonly float _collisionRadius;
    private readonly float _moveSpeed;

    private Texture2D _pixel;

    /// <summary>Цель преследования в мировых координатах (обычно позиция игрока), обновлять каждый кадр из сцены.</summary>
    public Vector2 ChaseTarget { get; set; }

    /// <summary>
    /// <paramref name="collision"/> — проверка блока для лётчика (периметр комнаты / двери).
    /// <paramref name="collisionRadius"/> — половина ширины хита при проверке углов тела.
    /// </summary>
    public FlyingEnemy(
        Vector2 position,
        IFlyingCollisionChecker collision,
        float moveSpeed,
        int maxHealth,
        float collisionRadius)
        : base(position, maxHealth)
    {
        _collision = collision ?? throw new System.ArgumentNullException(nameof(collision));
        _moveSpeed = moveSpeed;
        _collisionRadius = collisionRadius;
        Color = Color.White;
    }

    /// <summary>Удобный конструктор с константами из <see cref="EnemyConfig"/>.</summary>
    public FlyingEnemy(Vector2 position, IFlyingCollisionChecker collision)
        : this(
            position,
            collision,
            EnemyConfig.DefaultFlyingMoveSpeed,
            EnemyConfig.DefaultFlyingMaxHealth,
            EnemyConfig.DefaultFlyingRadius)
    {
    }

    protected override void UpdateEnemy(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 toTarget = ChaseTarget - Position;
        if (toTarget.LengthSquared() < 2f)
        {
            Velocity = Vector2.Zero;
            return;
        }

        toTarget.Normalize();
        Vector2 delta = toTarget * (_moveSpeed * dt);

        Position = ResolveFlyingSlide(Position, delta);
        Velocity = Vector2.Zero;
    }

    /// <summary>
    /// Пытаемся сдвинуться на delta; при блокировке по одной оси оставляем движение по другой (как у игрока по стенам).
    /// </summary>
    private Vector2 ResolveFlyingSlide(Vector2 oldPos, Vector2 delta)
    {
        Vector2 target = oldPos + delta;
        Vector2 final = target;

        if (delta.X != 0f && HasFlyingBodyCollisionAt(new Vector2(target.X, oldPos.Y)))
            final.X = oldPos.X;

        if (delta.Y != 0f && HasFlyingBodyCollisionAt(new Vector2(final.X, target.Y)))
            final.Y = oldPos.Y;

        return final;
    }

    private bool HasFlyingBodyCollisionAt(Vector2 centerWorld)
    {
        float o = _collisionRadius;
        return _collision.IsFlyingBlocked(new Vector2(centerWorld.X - o, centerWorld.Y - o))
            || _collision.IsFlyingBlocked(new Vector2(centerWorld.X + o, centerWorld.Y - o))
            || _collision.IsFlyingBlocked(new Vector2(centerWorld.X - o, centerWorld.Y + o))
            || _collision.IsFlyingBlocked(new Vector2(centerWorld.X + o, centerWorld.Y + o));
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

        if (_pixel == null)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        var rect = GetBounds();
        spriteBatch.Draw(_pixel, rect, new Color(200, 80, 200, 230));
    }
}
