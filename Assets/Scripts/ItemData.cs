using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Weapon,
    Armor,
    Jewelry,
    Artifact,
    Supply
}

[Serializable]
public class ItemData
{
    [Header("Основное")]
    public string itemName = "New Item";
    public ItemType itemType;
    public int price = 5;
    public string folderName = "NewItem";
    [TextArea(2, 4)] public string loreDescription = "";

    [Header("Форма (зелёные клетки)")]
    public bool[] cellMask = new bool[25];

    [Header("Аура (синие клетки)")]
    public bool[] effectMask = new bool[25];

    [Header("Бафф по категории")]
    public bool hasCategoryBuff = false;
    public ItemType categoryTargetType = ItemType.Weapon;
    [Range(0, 500)] public float categoryBonusDamage;
    [Range(0, 500)] public float categoryBonusSpeed;
    [Range(0, 500)] public float categoryBonusCrit;

    [Header("Мана")]
    public bool isMagical = false;
    public float bonusMana;
    public float manaRegen;
    public float manaCostPerSecond;
    public float manaCostPerAttack;

    [Header("Боевые")]
    public float damage;
    public float attackSpeed = 1f;
    [Range(0, 100)] public float critChance;
    [Range(0, 500)] public float critDamage = 200f;
    [Range(0, 100)] public float dodgeChance;
    [Range(0, 100)] public float vampirism;
    public float poisonDamage;
    public float poisonDuration;
    public float burnDamage;
    public float burnDuration;
    [Range(0, 100)] public float slowPercent;
    [Range(0, 100)] public float stunChance;
    public float thornsDamage;
    public float bleedDamage;
    [Range(0, 100)] public float executeThreshold;
    [Range(0, 100)] public float extraProjectileChance;

    [Header("Защитные")]
    public float bonusHP;
    public float bonusArmor;
    public float hpRegen;
    public float armorRegen;
    [Range(0, 100)] public float blockChance;
    [Range(0, 100)] public float poisonResist;
    [Range(0, 100)] public float fireResist;
    public float damageReduction;
    [Range(0, 100)] public float damageReductionPercent;
    public float invulnerabilityDuration;

    [Header("Ауры (значения)")]
    [Range(0, 500)] public float neighborBonusDamage;
    [Range(0, 500)] public float neighborBonusSpeed;
    [Range(0, 500)] public float neighborBonusCrit;
    [Range(0, 500)] public float neighborBonusVampirism;
    public float neighborBonusArmor;
    public float neighborBonusHP;
    public float bonusPerEmptyCell;

    [Header("Экономические")]
    public int coinsPerLevel;
    public int coinsPerKill;
    [Range(0, 100)] public float shopDiscount;
    [Range(0, 100)] public float freeRerollChance;
    public int extraShopCards;
    public bool fullPriceSell;

    [Header("Растущие")]
    public float damagePerKillInBattle;
    public float damagePerItemInInventory;

    [Header("Особые")]
    public bool phoenixResurrection;
    [Range(0, 100)] public float phoenixHPPercent = 50f;
    public bool mirrorNeighbor;
    public bool dummy;
    [Range(0, 500)] public float timeCompression;

    public string GetAutoDescription()
    {
        if (!string.IsNullOrEmpty(loreDescription))
            return loreDescription;
        return itemName;
    }

    public string GetTypeName()
    {
        return itemType switch
        {
            ItemType.Weapon => "Оружие",
            ItemType.Armor => "Броня",
            ItemType.Jewelry => "Украшение",
            ItemType.Artifact => "Артефакт",
            ItemType.Supply => "Припас",
            _ => "Неизвестно"
        };
    }

    public string GetCategoryTargetName()
    {
        return categoryTargetType switch
        {
            ItemType.Weapon => "оружий",
            ItemType.Armor => "брони",
            ItemType.Jewelry => "украшений",
            ItemType.Artifact => "артефактов",
            ItemType.Supply => "припасов",
            _ => categoryTargetType.ToString()
        };
    }

    public List<(Sprite icon, string name, string value)> GetBaseStats(StatIconsData icons)
    {
        var stats = new List<(Sprite, string, string)>();
        if (icons == null) return stats;

        // Мана
        if (bonusMana != 0 && icons.manaIcon != null)
            stats.Add((icons.manaIcon, "Мана", (bonusMana > 0 ? "+" : "") + bonusMana.ToString("0")));
        if (manaRegen > 0 && icons.manaRegenIcon != null)
            stats.Add((icons.manaRegenIcon, "Реген. маны", $"{manaRegen}/сек"));
        if (manaCostPerSecond > 0 && icons.manaCostIcon != null)
            stats.Add((icons.manaCostIcon, "Расход маны", $"{manaCostPerSecond}/сек"));
        if (manaCostPerAttack > 0 && icons.manaCostIcon != null)
            stats.Add((icons.manaCostIcon, "Расход маны", $"{manaCostPerAttack}/атака"));

        // Урон
        if (damage > 0 && icons.damageIcon != null)
        {
            string dmgText = damage.ToString("0");
            stats.Add((icons.damageIcon, "Урон", dmgText));
        }

        // Скорость атаки
        if (attackSpeed > 0 && damage > 0 && icons.attackSpeedIcon != null)
            stats.Add((icons.attackSpeedIcon, "Скорость атаки", attackSpeed.ToString("0.0") + " сек"));

        // Крит. шанс
        if (critChance > 0 && icons.critChanceIcon != null)
            stats.Add((icons.critChanceIcon, "Крит. шанс", critChance + "%"));

        // Крит. урон
        if (critDamage > 200f && icons.critDamageIcon != null)
            stats.Add((icons.critDamageIcon, "Крит. урон", critDamage + "%"));

        // Здоровье
        if (bonusHP != 0 && icons.healthIcon != null)
            stats.Add((icons.healthIcon, "Здоровье", (bonusHP > 0 ? "+" : "") + bonusHP.ToString("0")));

        // Броня
        if (bonusArmor != 0 && icons.armorIcon != null)
            stats.Add((icons.armorIcon, "Броня", (bonusArmor > 0 ? "+" : "") + bonusArmor.ToString("0")));

        // Боевые эффекты
        if (dodgeChance > 0 && icons.dodgeIcon != null) stats.Add((icons.dodgeIcon, "Уклонение", dodgeChance + "%"));
        if (vampirism > 0 && icons.vampirismIcon != null) stats.Add((icons.vampirismIcon, "Вампиризм", vampirism + "%"));
        if (poisonDamage > 0 && icons.poisonIcon != null) stats.Add((icons.poisonIcon, "Отравление", $"{poisonDamage}/сек ({poisonDuration}с)"));
        if (burnDamage > 0 && icons.burnIcon != null) stats.Add((icons.burnIcon, "Поджог", $"{burnDamage}/сек ({burnDuration}с)"));
        if (slowPercent > 0 && icons.slowIcon != null) stats.Add((icons.slowIcon, "Замедление", slowPercent + "%"));
        if (stunChance > 0 && icons.stunIcon != null) stats.Add((icons.stunIcon, "Оглушение", stunChance + "%"));
        if (thornsDamage > 0 && icons.thornsIcon != null) stats.Add((icons.thornsIcon, "Шипы", thornsDamage.ToString("0")));
        if (bleedDamage > 0 && icons.bleedIcon != null) stats.Add((icons.bleedIcon, "Разрез", $"+{bleedDamage}/удар"));
        if (executeThreshold > 0 && icons.guillotineIcon != null) stats.Add((icons.guillotineIcon, "Казнь", $"<{executeThreshold}% HP"));
        if (extraProjectileChance > 0 && icons.extraProjectileIcon != null) stats.Add((icons.extraProjectileIcon, "Доп. снаряд", extraProjectileChance + "%"));

        // Защитные
        if (hpRegen > 0 && icons.hpRegenIcon != null) stats.Add((icons.hpRegenIcon, "Реген. HP", $"{hpRegen}/сек"));
        if (armorRegen > 0 && icons.armorRegenIcon != null) stats.Add((icons.armorRegenIcon, "Реген. брони", $"{armorRegen}/сек"));
        if (blockChance > 0 && icons.blockChanceIcon != null) stats.Add((icons.blockChanceIcon, "Блок", blockChance + "%"));
        if (poisonResist > 0 && icons.poisonResistIcon != null) stats.Add((icons.poisonResistIcon, "Сопр. яду", poisonResist + "%"));
        if (fireResist > 0 && icons.fireResistIcon != null) stats.Add((icons.fireResistIcon, "Сопр. огню", fireResist + "%"));
        if (damageReduction > 0 && icons.damageReductionIcon != null) stats.Add((icons.damageReductionIcon, "Урон врага снижен на", damageReduction.ToString("0")));
        if (damageReductionPercent > 0 && icons.damageReductionIcon != null) stats.Add((icons.damageReductionIcon, "Урон врага снижен на", damageReductionPercent + "%"));
        if (invulnerabilityDuration > 0 && icons.invulnerabilityIcon != null) stats.Add((icons.invulnerabilityIcon, "Неуязвимость", $"{invulnerabilityDuration}с"));

        // Бонусы соседним предметам
        if (neighborBonusDamage > 0 && icons.damageIcon != null) stats.Add((icons.damageIcon, "Соседям: урон", "+" + neighborBonusDamage + "%"));
        if (neighborBonusSpeed > 0 && icons.attackSpeedIcon != null) stats.Add((icons.attackSpeedIcon, "Соседям: скорость", "+" + neighborBonusSpeed + "%"));
        if (neighborBonusCrit > 0 && icons.critChanceIcon != null) stats.Add((icons.critChanceIcon, "Соседям: крит", "+" + neighborBonusCrit + "%"));
        if (neighborBonusVampirism > 0 && icons.vampirismIcon != null) stats.Add((icons.vampirismIcon, "Соседям: вампиризм", "+" + neighborBonusVampirism + "%"));
        if (neighborBonusArmor > 0 && icons.armorIcon != null) stats.Add((icons.armorIcon, "Соседям: броня", "+" + neighborBonusArmor));
        if (neighborBonusHP > 0 && icons.healthIcon != null) stats.Add((icons.healthIcon, "Соседям: здоровье", "+" + neighborBonusHP));

        // Бафф по категории
        if (hasCategoryBuff)
        {
            string targetName = GetCategoryTargetName();
            if (categoryBonusDamage > 0 && icons.damageIcon != null) stats.Add((icons.damageIcon, $"Урон всех {targetName}", "+" + categoryBonusDamage + "%"));
            if (categoryBonusSpeed > 0 && icons.attackSpeedIcon != null) stats.Add((icons.attackSpeedIcon, $"Скорость всех {targetName}", "+" + categoryBonusSpeed + "%"));
            if (categoryBonusCrit > 0 && icons.critChanceIcon != null) stats.Add((icons.critChanceIcon, $"Крит всех {targetName}", "+" + categoryBonusCrit + "%"));
        }

        // Экономические
        if (coinsPerLevel > 0 && icons.coinsPerLevelIcon != null) stats.Add((icons.coinsPerLevelIcon, "Монет за уровень", "+" + coinsPerLevel));
        if (coinsPerKill > 0 && icons.coinsPerKillIcon != null) stats.Add((icons.coinsPerKillIcon, "Монет за убийство", "+" + coinsPerKill));
        if (shopDiscount > 0 && icons.shopDiscountIcon != null) stats.Add((icons.shopDiscountIcon, "Скидка", shopDiscount + "%"));
        if (freeRerollChance > 0 && icons.freeRerollIcon != null) stats.Add((icons.freeRerollIcon, "Реролл", $"{freeRerollChance}% шанс бесплатно"));
        if (extraShopCards > 0 && icons.extraCardIcon != null) stats.Add((icons.extraCardIcon, "Карточки", "+" + extraShopCards));
        if (fullPriceSell && icons.fullPriceIcon != null) stats.Add((icons.fullPriceIcon, "Продажа", "100% цены"));

        // Особые
        if (bonusPerEmptyCell > 0 && icons.damagePerEmptyIcon != null) stats.Add((icons.damagePerEmptyIcon, "Урон за пустоту", "+" + bonusPerEmptyCell));
        if (phoenixResurrection && icons.invulnerabilityIcon != null) stats.Add((icons.invulnerabilityIcon, "Феникс", $"{phoenixHPPercent}% HP"));
        if (mirrorNeighbor) stats.Add((null, "Зеркало", "Копирует соседа"));
        if (dummy) stats.Add((null, "Пустышка", ""));
        if (timeCompression > 0 && icons.timeCompressionIcon != null) stats.Add((icons.timeCompressionIcon, "Сжатие времени", "+" + timeCompression + "%"));
        if (damagePerItemInInventory > 0 && icons.damagePerItemIcon != null) stats.Add((icons.damagePerItemIcon, "Урон за предмет", "+" + damagePerItemInInventory));
        if (damagePerKillInBattle > 0 && icons.damagePerKillIcon != null) stats.Add((icons.damagePerKillIcon, "Урон за убийство", "+" + damagePerKillInBattle));

        return stats;
    }
}