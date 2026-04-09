using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Enemies;

internal class AdaptiveChasingEnemy : ChasingEnemy
{
    private readonly float _initialChaseRadius;
    private readonly float _expandedChaseRadius;
    private bool _hasPlayerEnteredRadius = false;
    private float _currentChaseRadius;

    public AdaptiveChasingEnemy(
        Vector2 position,
        IWallCollisionChecker collision,
        float moveSpeed,
        int maxHealth,
        float collisionRadius,
        float initialChaseRadius,
        float expandedChaseRadius)
        : base(position, collision, moveSpeed, maxHealth, collisionRadius)
    {
        _initialChaseRadius = initialChaseRadius;
        _expandedChaseRadius = expandedChaseRadius;
        _currentChaseRadius = initialChaseRadius;
        RecoilResistance = 0.8f;  // Тяжело отскакивает (80% сопротивление)
    }

    public AdaptiveChasingEnemy(Vector2 position, IWallCollisionChecker collision)
        : this(
            position,
            collision,
            EnemyConfig.AdaptiveChasingMoveSpeed,
            EnemyConfig.AdaptiveChasingMaxHealth,
            EnemyConfig.AdaptiveChasingRadius,
            EnemyConfig.AdaptiveChasingInitialRadius,
            EnemyConfig.AdaptiveChasingExpandedRadius)
    {
    }

    protected override void UpdateEnemy(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float distanceToPlayer = Vector2.Distance(Position, ChaseTarget);

        CheckPlayerEnteredRadius(distanceToPlayer);

        if (distanceToPlayer > _currentChaseRadius)
        {
            Velocity = Vector2.Zero;
            return;
        }

        Vector2 toTarget = ChaseTarget - Position;
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

    private void CheckPlayerEnteredRadius(float distanceToPlayer)
    {
        if (distanceToPlayer <= _currentChaseRadius && !_hasPlayerEnteredRadius)
        {
            _hasPlayerEnteredRadius = true;
            _currentChaseRadius = _expandedChaseRadius;
        }
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

        // Рисуем контур радиуса (очень тонкий, не мешает видеть врага)
#if DEBUG
        DrawChaseRadiusOutline(spriteBatch);
#endif

        // Рисуем врага
        var rect = GetBounds();
        Color enemyColor = _hasPlayerEnteredRadius
            ? new Color(255, 30, 30, 230)   // Ярко-красный после активации
            : new Color(200, 80, 80, 230);   // Темно-красный до активации

        spriteBatch.Draw(_pixel, rect, enemyColor);
    }

    /// <summary>
    /// Рисует только контур круга (не заливает)
    /// </summary>
    private void DrawChaseRadiusOutline(SpriteBatch spriteBatch)
    {
        if (_pixel == null) return;

        float radius = _currentChaseRadius;
        Vector2 center = Position;

        // Рисуем точки по кругу
        int pointCount = 40;
        Color dotColor = _hasPlayerEnteredRadius
            ? new Color(255, 80, 80, 200)   // Ярко-красные точки
            : new Color(255, 150, 150, 150); // Бледно-красные точки

        for (int i = 0; i < pointCount; i++)
        {
            float angle = (i * MathHelper.TwoPi / pointCount);
            float x = center.X + (float)Math.Cos(angle) * radius;
            float y = center.Y + (float)Math.Sin(angle) * radius;

            // Рисуем точку 3x3 пикселя
            spriteBatch.Draw(_pixel, new Rectangle((int)x - 1, (int)y - 1, 3, 3), dotColor);
        }
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness = 1)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        float length = edge.Length();

        spriteBatch.Draw(_pixel,
            new Rectangle((int)start.X, (int)start.Y, (int)length, thickness),
            null,
            color,
            angle,
            Vector2.Zero,
            SpriteEffects.None,
            0);
    }

    public void ResetRadiusState()
    {
        _hasPlayerEnteredRadius = false;
        _currentChaseRadius = _initialChaseRadius;
    }

    public bool HasPlayerEnteredRadius => _hasPlayerEnteredRadius;
    public float CurrentChaseRadius => _currentChaseRadius;
}