using UnityEngine;

[CreateAssetMenu(fileName = "StatIconsData", menuName = "Game/Stat Icons Data")]
public class StatIconsData : ScriptableObject
{
    [Header("Боевые")]
    public Sprite damageIcon;
    public Sprite attackSpeedIcon;
    public Sprite critChanceIcon;
    public Sprite critDamageIcon;
    public Sprite dodgeIcon;
    public Sprite vampirismIcon;
    public Sprite poisonIcon;
    public Sprite burnIcon;
    public Sprite slowIcon;
    public Sprite stunIcon;
    public Sprite thornsIcon;
    public Sprite bleedIcon;
    public Sprite guillotineIcon;
    public Sprite extraProjectileIcon;

    [Header("Защитные")]
    public Sprite healthIcon;
    public Sprite armorIcon;
    public Sprite hpRegenIcon;
    public Sprite armorRegenIcon;
    public Sprite blockChanceIcon;
    public Sprite poisonResistIcon;
    public Sprite fireResistIcon;
    public Sprite damageReductionIcon;
    public Sprite invulnerabilityIcon;

    [Header("Мана")]
    public Sprite manaIcon;
    public Sprite manaRegenIcon;
    public Sprite manaCostIcon;

    [Header("Экономические")]
    public Sprite coinsPerLevelIcon;
    public Sprite coinsPerKillIcon;
    public Sprite shopDiscountIcon;
    public Sprite freeRerollIcon;
    public Sprite extraCardIcon;
    public Sprite fullPriceIcon;

    [Header("Особые")]
    public Sprite damagePerItemIcon;
    public Sprite damagePerKillIcon;
    public Sprite damagePerEmptyIcon;
    public Sprite timeCompressionIcon;
}