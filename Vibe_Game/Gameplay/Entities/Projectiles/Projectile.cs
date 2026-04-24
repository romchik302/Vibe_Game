using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Gameplay.Entities;

namespace Vibe_Game.Gameplay.Projectiles;

public sealed class Projectile : Entity
{
    public Vector2 Direction { get; }
    public float Speed { get; }
    public float Damage { get; }
    public float LifeLeft { get; private set; }
    public float Radius {  get; private set; }
    public float RecoilForce { get; }  // Сила отдачи при попадании
    public bool IsFriendlyToPlayer { get; }

    public Projectile(
        Vector2 position,
        Vector2 direction,
        float speed,
        float damage,
        float lifetimeSeconds,
        float radius,
        float recoilForce = 0f,
        bool isFriendlyToPlayer = true)
    {
        Position = position;
        Direction = Vector2.Normalize(direction);
        Speed = speed;
        Damage = damage;
        LifeLeft = lifetimeSeconds;
        Velocity = Direction * speed;
        Radius = radius;
        RecoilForce = recoilForce;
        IsFriendlyToPlayer = isFriendlyToPlayer;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        LifeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (LifeLeft <= 0f)
            IsAlive = false;
    }
    public override Rectangle GetBounds()
    {
        int r = (int)Radius;
        return new Rectangle(
            (int)Position.X - r,
            (int)Position.Y - r,
            r * 2,
            r * 2
        );
    }
}