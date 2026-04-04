namespace Vibe_Game.Gameplay.Weapons;

/// <summary>
/// Как игрок инициирует основную атаку этого оружия.
/// </summary>
public enum WeaponFireMode
{
    /// <summary>Слёзы как в Isaac: пока зажата стрелка прицела, выстрелы идут по кулдауну оружия.</summary>
    AutoWhileDirectionHeld,

    /// <summary>Нужна зажатая стрелка и одно нажатие Fire (заготовка под меч / разовый удар).</summary>
    DirectionHeldPlusButtonPress,
}
