using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Projectiles;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Scenes
{
    internal sealed class GameSceneProjectileController
    {
        private readonly GameSceneState _state;
        private readonly GameSceneWorld _world;

        public GameSceneProjectileController(GameSceneState state, GameSceneWorld world)
        {
            _state = state;
            _world = world;
        }

        public void Spawn(ProjectileSpawnArgs args)
        {
            Projectile projectile = new Projectile(
                args.Position,
                args.Direction,
                args.Speed,
                args.Damage,
                args.LifetimeSeconds,
                args.Radius,
                args.RecoilForce,
                args.IsFriendlyToPlayer,
                args.IgnoreWallCollisions,
                args.Length
            );

            if (args.UseOrbitMotion)
            {
                projectile.ConfigureOrbit(
                    args.OrbitCenter,
                    args.OrbitRadius,
                    args.OrbitStartAngle,
                    args.OrbitAngularSpeed,
                    args.OrbitDurationSeconds,
                    args.ReleaseAfterOrbit,
                    args.ReleaseDirection
                );
            }

            _state.Projectiles.Add(projectile);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = _state.Projectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = _state.Projectiles[i];
                if (!projectile.IsAlive)
                {
                    _state.Projectiles.RemoveAt(i);
                    continue;
                }

                Vector2 next = projectile.Position + projectile.Velocity * dt;
                if (!projectile.IgnoreWallCollisions && _world.IsWorldPointBlocked(next))
                {
                    projectile.IsAlive = false;
                    _state.Projectiles.RemoveAt(i);
                    continue;
                }

                projectile.Update(gameTime);

                int rx = (int)(projectile.Position.X / WorldConfig.RoomWidthPx);
                int ry = (int)(projectile.Position.Y / WorldConfig.RoomHeightPx);

                rx = System.Math.Clamp(rx, 0, WorldConfig.GridSize - 1);
                ry = System.Math.Clamp(ry, 0, WorldConfig.GridSize - 1);

                Room room = _state.FloorMap[rx, ry];

                if (projectile.IsFriendlyToPlayer && room?.enemies != null)
                {
                    foreach (var enemy in room.enemies)
                    {
                        if (!enemy.IsAlive)
                            continue;

                        if (projectile.GetBounds().Intersects(enemy.GetBounds()))
                        {
                            enemy.TakeDamage((int)projectile.Damage);

                            if (projectile.RecoilForce > 0)
                                enemy.ApplyRecoil(projectile.Direction, projectile.RecoilForce);

                            projectile.IsAlive = false;
                            break;
                        }
                    }
                }
                else if (!projectile.IsFriendlyToPlayer)
                {
                    if (projectile.GetBounds().Intersects(_state.Player.GetBounds()))
                    {
                        _state.Player.TakeDamage(projectile.Damage);
                        projectile.IsAlive = false;
                    }
                }

                if (!projectile.IsAlive)
                    _state.Projectiles.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (Projectile projectile in _state.Projectiles)
            {
                if (!projectile.IsAlive)
                    continue;

                if (projectile.Length > 0f)
                {
                    // Draw elongated projectile (spike) using rotation
                    float angle = System.MathF.Atan2(projectile.Direction.Y, projectile.Direction.X);
                    float halfLength = projectile.Length / 2f;
                    float halfWidth = projectile.Radius;

                    Rectangle spikeRect = new Rectangle(
                        (int)(-halfLength),
                        (int)(-halfWidth),
                        (int)projectile.Length,
                        (int)(halfWidth * 2)
                    );

                    spriteBatch.Draw(
                        pixel,
                        projectile.Position,
                        spikeRect,
                        projectile.IsFriendlyToPlayer ? Color.SkyBlue : Color.OrangeRed,
                        angle,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        0f
                    );
                }
                else
                {
                    // Draw normal circular projectile
                    int radius = (int)projectile.Radius;

                    spriteBatch.Draw(
                        pixel,
                        new Rectangle(
                            (int)projectile.Position.X - radius,
                            (int)projectile.Position.Y - radius,
                            radius * 2,
                            radius * 2
                        ),
                        projectile.IsFriendlyToPlayer ? Color.SkyBlue : Color.OrangeRed
                    );
                }
            }
        }
    }
}
