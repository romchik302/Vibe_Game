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

    public Projectile(Vector2 position, Vector2 direction, float speed, float damage, float lifetimeSeconds)
    {
        Position = position;
        Direction = Vector2.Normalize(direction);
        Speed = speed;
        Damage = damage;
        LifeLeft = lifetimeSeconds;
        Velocity = Direction * speed;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        LifeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (LifeLeft <= 0f)
            IsAlive = false;
    }
}