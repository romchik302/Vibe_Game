using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Vibe_Game.Core.Utilities;

namespace Vibe_Game.Gameplay.Weapons;

public sealed class SwordWeapon : WeaponBase
{
    public override WeaponFireMode FireMode => WeaponFireMode.DirectionHeldPlusButtonPress;

    private readonly int _damage;
    private float _swordLength;     // Длина меча
    private float _swordWidth;      // Ширина меча
    private readonly float _attackAngle;     // Угол атаки (дуга)
    private readonly float _attackDuration;  // Длительность анимации удара

    private float _attackTimer;              // Таймер анимации удара
    private Vector2 _attackDirection;        // Направление удара (фиксируется в момент атаки)
    private Vector2 _currentPlayerPosition;  // ТЕКУЩАЯ позиция игрока (обновляется каждый кадр)

    private float _startAngle;               // Начальный угол атаки
    private float _endAngle;                 // Конечный угол атаки

    // Кто уже получил урон в этой атаке
    private HashSet<object> _hitEnemies = new();

    // Публичные свойства для изменения размера оружия в рантайме
    public float SwordLength
    {
        get => _swordLength;
        set
        {
            _swordLength = Math.Max(10f, value);
        }
    }

    public float SwordWidth
    {
        get => _swordWidth;
        set
        {
            _swordWidth = Math.Max(2f, value);
        }
    }

    public int Damage
    {
        get => _damage;
    }

    public SwordWeapon(
        int damage = 25,
        float swordLength = 60f,
        float swordWidth = 10f,
        float attackAngle = MathF.PI / 1.5f,  // 120 градусов
        float attackDuration = 0.12f,         // Длительность анимации
        float cooldownSeconds = 0.4f)
        : base("Sword", cooldownSeconds)
    {
        _damage = damage;
        _swordLength = swordLength;
        _swordWidth = swordWidth;
        _attackAngle = attackAngle;
        _attackDuration = attackDuration;
    }

    public override void Update(GameTime gameTime, IAttackContext context)
    {
        base.Update(gameTime, context);

        if (_attackTimer <= 0) return;

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _attackTimer -= dt;

        // Прогресс анимации (от 0 до 1)
        float totalTime = _attackDuration;
        float elapsedTime = totalTime - _attackTimer;
        float progress = Math.Clamp(elapsedTime / totalTime, 0f, 1f);

        // ПРОВЕРЯЕМ УРОН КАЖДЫЙ КАДР во время всей анимации
        CheckAndDealDamage(context, progress);

        // Сброс после окончания анимации
        if (_attackTimer <= 0)
        {
            _hitEnemies.Clear();
        }
    }

    private float GetCurrentAngle(float progress)
    {
        // Интерполяция угла от startAngle до endAngle
        return MathHelper.Lerp(_startAngle, _endAngle, progress);
    }

    private void CheckAndDealDamage(IAttackContext context, float progress)
    {
        // Получаем текущий угол меча
        float currentAngle = GetCurrentAngle(progress);

        // Получаем прямоугольник хитбокса меча
        Rectangle swordBounds = GetSwordBounds(currentAngle);

        // Находим всех врагов в этой области
        var enemies = context.GetEnemiesInArea(swordBounds);

        foreach (var enemy in enemies)
        {
            if (!_hitEnemies.Contains(enemy))
            {
                _hitEnemies.Add(enemy);
                context.DamageEnemy(enemy, _damage);
            }
        }
    }

    private Rectangle GetSwordBounds(float angle)
    {
        Vector2 handle = GetSwordHandle();
        Vector2 swordDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        Vector2 perpendicular = new Vector2(-swordDir.Y, swordDir.X);

        Vector2 tip = handle + swordDir * _swordLength;

        // Вычисляем 4 угла прямоугольника меча
        Vector2 p1 = handle + perpendicular * (_swordWidth / 2);
        Vector2 p2 = tip + perpendicular * (_swordWidth / 2);
        Vector2 p3 = tip - perpendicular * (_swordWidth / 2);
        Vector2 p4 = handle - perpendicular * (_swordWidth / 2);

        // Находим мин/макс координаты для прямоугольника
        float minX = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
        float minY = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));
        float maxX = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
        float maxY = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));

        return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
    }

    public override bool TryPrimaryAttack(IAttackContext context, Vector2 ownerPosition, Vector2 facingDirection)
    {
        if (facingDirection == Vector2.Zero) return false;
        if (_attackTimer > 0) return false;
        if (!TryStartCooldown()) return false;

        _currentPlayerPosition = ownerPosition;
        _attackDirection = Vector2.Normalize(facingDirection);
        _attackTimer = _attackDuration;
        _hitEnemies.Clear();

        // Вычисляем углы атаки
        float directionAngle = (float)Math.Atan2(_attackDirection.Y, _attackDirection.X);
        _startAngle = directionAngle - _attackAngle / 2;
        _endAngle = directionAngle + _attackAngle / 2;

        return true;
    }

    public void UpdateOwnerPosition(Vector2 ownerPosition)
    {
        _currentPlayerPosition = ownerPosition;
    }

    private Vector2 GetSwordHandle()
    {
        return _currentPlayerPosition;
    }

    private Vector2 GetSwordTip(float angle)
    {
        Vector2 swordDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        return GetSwordHandle() + swordDir * _swordLength;
    }

    public override void Draw(SpriteBatch spriteBatch, IAttackContext context)
    {
        if (_attackTimer <= 0) return;

        var pixel = GetPixelTexture(spriteBatch);
        if (pixel == null) return;

        float totalTime = _attackDuration;
        float elapsedTime = totalTime - _attackTimer;
        float progress = Math.Clamp(elapsedTime / totalTime, 0f, 1f);

        float currentAngle = GetCurrentAngle(progress);

        Vector2 handle = GetSwordHandle();
        Vector2 tip = GetSwordTip(currentAngle);

        if (Vector2.Distance(handle, tip) < 0.1f) return;

        // Альфа: 0 -> 1 -> 0
        float alpha;
        if (progress <= 0.5f)
            alpha = progress * 2f;
        else
            alpha = 2f - (progress * 2f);
        alpha = Math.Clamp(alpha, 0f, 1f);

        // Цвет меча
        Color swordColor;
        if (progress <= 0.5f)
            swordColor = Color.Lerp(Color.White, Color.Red, progress * 2f);
        else
            swordColor = Color.Lerp(Color.Red, Color.White, (progress - 0.5f) * 2f);
        swordColor *= alpha;

        // Рисуем сам меч
        DrawLine(spriteBatch, pixel, handle, tip, swordColor, _swordWidth);

        // Рисуем след
        Vector2 perpendicular = new Vector2(-(tip - handle).Y, (tip - handle).X);
        perpendicular.Normalize();

        Color trailColor = Color.OrangeRed * (alpha * 0.6f);
        DrawLine(spriteBatch, pixel, tip - perpendicular * _swordWidth * 0.5f,
                 tip + perpendicular * _swordWidth * 0.5f, trailColor, _swordWidth * 0.6f);

#if DEBUG
        // Отладочная отрисовка хитбокса
        Rectangle bounds = GetSwordBounds(currentAngle);
        spriteBatch.Draw(pixel, bounds, Color.Cyan * 0.3f);

        // Границы хитбокса
        spriteBatch.DrawRectangle(pixel, bounds, Color.Cyan, 1);

        // Центр меча
        Vector2 center = (handle + tip) / 2;
        spriteBatch.Draw(pixel, new Rectangle((int)center.X - 2, (int)center.Y - 2, 4, 4), Color.Yellow);

        // Направление атаки
        Vector2 debugEnd = handle + _attackDirection * _swordLength;
        DrawLine(spriteBatch, pixel, handle, debugEnd, Color.Green * 0.3f, 1f);
#endif
    }

    private Texture2D _pixelTexture;
    private Texture2D GetPixelTexture(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null || _pixelTexture.IsDisposed)
        {
            _pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        return _pixelTexture;
    }

    private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        float length = edge.Length();

        if (length <= 0.01f) return;

        spriteBatch.Draw(texture, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
    }
}