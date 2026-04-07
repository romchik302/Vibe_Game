// Gameplay/Entities/Player/PlayerRenderer.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Gameplay.Entities.Player;

internal class PlayerRenderer : IPlayerRenderer
{
    private Texture2D _spriteSheet;
    private Rectangle _currentFrame;
    private float _animationTimer;
    private int _currentFrameIndex;

    private readonly int _frameWidth = 22;
    private readonly int _frameHeight = 32;
    private readonly float _frameDuration = 0.12f;

    private enum Direction { Right, Up, Down, Left }
    private Direction _currentDirection = Direction.Down;
    private bool _isMoving;

    private const int WALK_FRAMES_X = 7;
    private const int WALK_FRAMES_Y = 6;

    // Idle настройки
    private const int IDLE_ROWS_START = 4;
    private const int IDLE_ROWS_COUNT = 3;
    private const int IDLE_FRAMES = 5;

    private const float IDLE_FRAME_DURATION = 0.18f;
    private const float IDLE_COOLDOWN = 1.2f;          // пауза между idle
    private const float STOP_COOLDOWN = 0.5f;          // пауза после движения

    private Random _random = new Random();

    private int _idleRow;
    private float _idleCooldownTimer = 0f;
    private bool _isIdlePlaying = false;

    private bool _wasMoving = false;

    public PlayerRenderer(Texture2D spriteSheet)
    {
        _spriteSheet = spriteSheet ?? throw new ArgumentNullException(nameof(spriteSheet));
        _currentFrame = new Rectangle(0, 0, _frameWidth, _frameHeight);
    }

    public void Update(GameTime gameTime, Vector2 velocity, Vector2 shootDirection)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _animationTimer += dt;

        _isMoving = velocity.LengthSquared() > 0.01f;

        Vector2 direction = velocity != Vector2.Zero ? velocity : shootDirection;
        UpdateDirection(direction);

        if (_isMoving)
        {
            UpdateWalk();
        }
        else
        {
            UpdateIdle(dt);
        }

        UpdateCurrentFrame();
    }

    private void UpdateWalk()
    {
        int frameCount = GetFrameCount();

        if (_animationTimer >= _frameDuration)
        {
            _animationTimer = 0f;
            _currentFrameIndex = (_currentFrameIndex + 1) % frameCount;
        }

        // сбрасываем idle
        _isIdlePlaying = false;
        _idleCooldownTimer = STOP_COOLDOWN;

        _wasMoving = true;
    }

    private void UpdateIdle(float dt)
    {
        // если только остановились → ждём
        if (_wasMoving)
        {
            _idleCooldownTimer = STOP_COOLDOWN;
            _wasMoving = false;
            _isIdlePlaying = false;
            return;
        }

        // cooldown
        if (_idleCooldownTimer > 0)
        {
            _idleCooldownTimer -= dt;
            return;
        }

        // если idle не запущен → выбираем одну анимацию
        if (!_isIdlePlaying)
        {
            _idleRow = _random.Next(IDLE_ROWS_START, IDLE_ROWS_START + IDLE_ROWS_COUNT);
            _currentFrameIndex = 0;
            _isIdlePlaying = true;
            _animationTimer = 0f;
        }

        // проигрываем 5 кадров подряд
        if (_animationTimer >= IDLE_FRAME_DURATION)
        {
            _animationTimer = 0f;
            _currentFrameIndex++;

            if (_currentFrameIndex >= IDLE_FRAMES)
            {
                // закончили анимацию → ставим паузу
                _isIdlePlaying = false;
                _idleCooldownTimer = IDLE_COOLDOWN;
                _currentFrameIndex = 0;
            }
        }
    }

    private void UpdateDirection(Vector2 direction)
    {
        if (direction == Vector2.Zero) return;

        Direction newDirection;

        if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            newDirection = direction.X > 0 ? Direction.Right : Direction.Left;
        else
            newDirection = direction.Y > 0 ? Direction.Up : Direction.Down;

        // если направление изменилось → фикс индекса
        if (newDirection != _currentDirection)
        {
            _currentDirection = newDirection;

            int maxFrames = GetFrameCount();

            // гарантируем что индекс не выйдет за пределы
            if (_currentFrameIndex >= maxFrames)
                _currentFrameIndex = 0;

            _animationTimer = 0f; // чтобы не было скачка
        }
    }

    private int GetAnimationRow()
    {
        return _currentDirection switch
        {
            Direction.Right => 0,
            Direction.Up => 1,
            Direction.Down => 2,
            Direction.Left => 3,
            _ => 0
        };
    }

    private int GetFrameCount()
    {
        return (_currentDirection == Direction.Left || _currentDirection == Direction.Right)
            ? WALK_FRAMES_X
            : WALK_FRAMES_Y;
    }

    private void UpdateCurrentFrame()
    {
        if (_isMoving)
        {
            int row = GetAnimationRow();

            _currentFrame.X = _currentFrameIndex * _frameWidth;
            _currentFrame.Y = row * _frameHeight;
        }
        else if (_isIdlePlaying)
        {
            _currentFrame.X = _currentFrameIndex * _frameWidth;
            _currentFrame.Y = _idleRow * _frameHeight;
        }

        _currentFrame.Width = _frameWidth;
        _currentFrame.Height = _frameHeight;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Vector2 shootDirection, Color color)
    {
        if (_spriteSheet == null) return;

        spriteBatch.Draw(
            _spriteSheet,
            position,
            _currentFrame,
            color,
            0f,
            new Vector2(_frameWidth / 2f, _frameHeight / 2f),
            1f,
            SpriteEffects.None,
            0f
        );
    }
}