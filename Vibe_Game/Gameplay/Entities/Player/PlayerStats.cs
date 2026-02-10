using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibe_Game.Gameplay.Entities.Player
{
    public class PlayerStats
    {
        // Основные характеристики
        public float Health { get; set; } = 6f;
        public float MaxHealth { get; set; } = 6f;
        public float Damage { get; set; } = 3.5f;
        public float Speed { get; set; } = 1.0f;


        // Модификаторы (будут добавляться предметами)
        public float DamageMultiplier { get; set; } = 1.0f;
        public float SpeedMultiplier { get; set; } = 1.0f;

        public void ApplyItemEffect(ItemEffect effect)
        {
            // TODO: Применение эффектов предметов
            Damage += effect.DamageModifier;
            Speed += effect.SpeedModifier;
            Health += effect.HealthModifier;
            // и т.д.
        }
    }

    public class ItemEffect
    {
        public float DamageModifier { get; set; }
        public float SpeedModifier { get; set; }
        public float HealthModifier { get; set; }
    }
}
