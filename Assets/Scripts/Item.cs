using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]
public class ItemCell
{
    public GameObject cellObject;
    public bool isActive;
}

public class Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Данные предмета")]
    public ItemData itemData;

    [Header("Отображаемые компоненты")]
    public Image backgroundImage;

    [Header("Ячейки предмета (все 25)")]
    public ItemCell[] cells = new ItemCell[25];

    [HideInInspector] public string itemName;
    [HideInInspector] public int price;
    [HideInInspector] public string description;
    [HideInInspector] public ItemType itemType;
    [HideInInspector] public int itemLevel = 1;
    [HideInInspector] public float damage;
    [HideInInspector] public float attackSpeed;
    [HideInInspector] public float baseDamage;
    [HideInInspector] public float baseAttackSpeed;
    
    // Прокачиваемые характеристики
    [HideInInspector] public float basePoisonDamage;
    [HideInInspector] public float baseBurnDamage;
    [HideInInspector] public float baseBleedDamage;
    [HideInInspector] public float baseThornsDamage;
    [HideInInspector] public float baseCritChance;
    [HideInInspector] public float baseCritDamage;
    [HideInInspector] public float baseVampirism;
    [HideInInspector] public float baseSlowPercent;
    [HideInInspector] public float baseStunChance;
    [HideInInspector] public float baseExecuteThreshold;
    [HideInInspector] public float baseExtraProjectileChance;
    [HideInInspector] public float baseBonusHP;
    [HideInInspector] public float baseBonusArmor;
    [HideInInspector] public float baseHPRegen;
    [HideInInspector] public float baseArmorRegen;
    [HideInInspector] public float baseDamageReduction;
    [HideInInspector] public float baseDamageReductionPercent;
    [HideInInspector] public float baseBlockChance;
    [HideInInspector] public float baseDodgeChance;
    [HideInInspector] public float baseInvulnerabilityDuration;
    [HideInInspector] public float basePoisonResist;
    [HideInInspector] public float baseFireResist;
    [HideInInspector] public int baseCoinsPerLevel;
    [HideInInspector] public int baseCoinsPerKill;
    [HideInInspector] public float baseShopDiscount;
    [HideInInspector] public float baseFreeRerollChance;
    [HideInInspector] public int baseExtraShopCards;
    [HideInInspector] public bool baseFullPriceSell;
    [HideInInspector] public float baseDamagePerKillInBattle;
    [HideInInspector] public float baseDamagePerItemInInventory;
    [HideInInspector] public float baseBonusPerEmptyCell;
    [HideInInspector] public float baseNeighborBonusDamage;
    [HideInInspector] public float baseNeighborBonusSpeed;
    [HideInInspector] public float baseNeighborBonusCrit;
    [HideInInspector] public float baseNeighborBonusVampirism;
    [HideInInspector] public float baseNeighborBonusArmor;
    [HideInInspector] public float baseNeighborBonusHP;
    [HideInInspector] public float baseCategoryBonusDamage;
    [HideInInspector] public float baseCategoryBonusSpeed;
    [HideInInspector] public float baseCategoryBonusCrit;
    // Мана
    [HideInInspector] public float baseBonusMana;
    [HideInInspector] public float baseManaRegen;
    [HideInInspector] public float baseManaCostPerSecond;
    [HideInInspector] public float baseManaCostPerAttack;
    [HideInInspector] public bool baseIsMagical;
    
    [HideInInspector] public Vector2Int[] occupiedCells;
    [HideInInspector] public Vector2Int grabbedCell;
    [HideInInspector] public Vector2Int positionInGrid;
    [HideInInspector] public bool isInShop;
    [HideInInspector] public int shopSlotIndex = -1;
    [HideInInspector] public bool isDragging;
    [HideInInspector] public Vector2Int[] effectCells;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 dragStartItemPos;
    private Vector2 dragStartMousePos;
    private Transform dragStartParent;
    private Vector2Int dragStartGrabbedCell;
    private Coroutine attackAnimationCoroutine;
    private Sprite[] levelSprites = new Sprite[5];

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        if (cells == null || cells.Length != 25)
        {
            cells = new ItemCell[25];
            for (int i = 0; i < 25; i++) cells[i] = new ItemCell();
        }

        if (itemData != null) ApplyItemData();
        RefreshOccupiedCells();
        UpdateCellVisuals();
    }

    float GetLevelMultiplier(bool isPercent)
    {
        if (isPercent)
            return itemLevel switch { 1 => 1f, 2 => 1.3f, 3 => 1.8f, _ => 1f };
        else
            return itemLevel switch { 1 => 1f, 2 => 1.8f, 3 => 3.2f, _ => 1f };
    }

    public void ApplyItemData()
    {
        if (itemData == null) return;
        itemName = itemData.itemName;
        price = itemData.price;
        description = itemData.GetAutoDescription();
        itemType = itemData.itemType;
        itemLevel = 1;
        
        float numMult = GetLevelMultiplier(false);
        float pctMult = GetLevelMultiplier(true);
        
        baseDamage = itemData.damage * numMult;
        baseAttackSpeed = itemData.attackSpeed;
        damage = baseDamage;
        attackSpeed = baseAttackSpeed;
        
        basePoisonDamage = itemData.poisonDamage * numMult;
        baseBurnDamage = itemData.burnDamage * numMult;
        baseBleedDamage = itemData.bleedDamage * numMult;
        baseThornsDamage = itemData.thornsDamage * numMult;
        baseCritChance = itemData.critChance * pctMult;
        baseCritDamage = itemData.critDamage * numMult;
        baseVampirism = itemData.vampirism * pctMult;
        baseSlowPercent = itemData.slowPercent * pctMult;
        baseStunChance = itemData.stunChance * pctMult;
        baseExecuteThreshold = itemData.executeThreshold * pctMult;
        baseExtraProjectileChance = itemData.extraProjectileChance * pctMult;
        baseBonusHP = itemData.bonusHP * numMult;
        baseBonusArmor = itemData.bonusArmor * numMult;
        baseHPRegen = itemData.hpRegen * numMult;
        baseArmorRegen = itemData.armorRegen * numMult;
        baseDamageReduction = itemData.damageReduction * numMult;
        baseDamageReductionPercent = itemData.damageReductionPercent * pctMult;
        baseBlockChance = itemData.blockChance * pctMult;
        baseDodgeChance = itemData.dodgeChance * pctMult;
        baseInvulnerabilityDuration = itemData.invulnerabilityDuration * numMult;
        basePoisonResist = itemData.poisonResist * pctMult;
        baseFireResist = itemData.fireResist * pctMult;
        baseCoinsPerLevel = Mathf.RoundToInt(itemData.coinsPerLevel * numMult);
        baseCoinsPerKill = Mathf.RoundToInt(itemData.coinsPerKill * numMult);
        baseShopDiscount = itemData.shopDiscount * pctMult;
        baseFreeRerollChance = itemData.freeRerollChance * pctMult;
        baseExtraShopCards = Mathf.RoundToInt(itemData.extraShopCards * numMult);
        baseFullPriceSell = itemData.fullPriceSell;
        baseDamagePerKillInBattle = itemData.damagePerKillInBattle * numMult;
        baseDamagePerItemInInventory = itemData.damagePerItemInInventory * numMult;
        baseBonusPerEmptyCell = itemData.bonusPerEmptyCell * numMult;
        baseNeighborBonusDamage = itemData.neighborBonusDamage * pctMult;
        baseNeighborBonusSpeed = itemData.neighborBonusSpeed * pctMult;
        baseNeighborBonusCrit = itemData.neighborBonusCrit * pctMult;
        baseNeighborBonusVampirism = itemData.neighborBonusVampirism * pctMult;
        baseNeighborBonusArmor = itemData.neighborBonusArmor * numMult;
        baseNeighborBonusHP = itemData.neighborBonusHP * numMult;
        baseCategoryBonusDamage = itemData.categoryBonusDamage * pctMult;
        baseCategoryBonusSpeed = itemData.categoryBonusSpeed * pctMult;
        baseCategoryBonusCrit = itemData.categoryBonusCrit * pctMult;
        // Мана
        baseBonusMana = itemData.bonusMana * numMult;
        baseManaRegen = itemData.manaRegen * numMult;
        baseManaCostPerSecond = itemData.manaCostPerSecond * numMult;
        baseManaCostPerAttack = itemData.manaCostPerAttack * numMult;
        baseIsMagical = itemData.isMagical;

        for (int i = 0; i < 5; i++)
            levelSprites[i] = Resources.Load<Sprite>($"Items/{itemData.folderName}/level_{i + 1}");

        for (int i = 0; i < 25; i++)
        {
            if (cells[i] == null) cells[i] = new ItemCell();
            cells[i].isActive = (itemData.cellMask.Length > i && itemData.cellMask[i]);
        }

        List<Vector2Int> effList = new List<Vector2Int>();
        if (itemData.effectMask != null)
        {
            for (int y = 0; y < 5; y++)
                for (int x = 0; x < 5; x++)
                {
                    int idx = x + y * 5;
                    if (itemData.effectMask.Length > idx && itemData.effectMask[idx])
                        effList.Add(new Vector2Int(x, y));
                }
        }
        effectCells = effList.ToArray();
    }

    void RecalculateAllStats()
    {
        float numMult = GetLevelMultiplier(false);
        float pctMult = GetLevelMultiplier(true);
        
        baseDamage = itemData.damage * numMult;
        damage = baseDamage;
        attackSpeed = baseAttackSpeed;
        
        basePoisonDamage = itemData.poisonDamage * numMult;
        baseBurnDamage = itemData.burnDamage * numMult;
        baseBleedDamage = itemData.bleedDamage * numMult;
        baseThornsDamage = itemData.thornsDamage * numMult;
        baseCritChance = itemData.critChance * pctMult;
        baseCritDamage = itemData.critDamage * numMult;
        baseVampirism = itemData.vampirism * pctMult;
        baseSlowPercent = itemData.slowPercent * pctMult;
        baseStunChance = itemData.stunChance * pctMult;
        baseExecuteThreshold = itemData.executeThreshold * pctMult;
        baseExtraProjectileChance = itemData.extraProjectileChance * pctMult;
        baseBonusHP = itemData.bonusHP * numMult;
        baseBonusArmor = itemData.bonusArmor * numMult;
        baseHPRegen = itemData.hpRegen * numMult;
        baseArmorRegen = itemData.armorRegen * numMult;
        baseDamageReduction = itemData.damageReduction * numMult;
        baseDamageReductionPercent = itemData.damageReductionPercent * pctMult;
        baseBlockChance = itemData.blockChance * pctMult;
        baseDodgeChance = itemData.dodgeChance * pctMult;
        baseInvulnerabilityDuration = itemData.invulnerabilityDuration * numMult;
        basePoisonResist = itemData.poisonResist * pctMult;
        baseFireResist = itemData.fireResist * pctMult;
        baseCoinsPerLevel = Mathf.RoundToInt(itemData.coinsPerLevel * numMult);
        baseCoinsPerKill = Mathf.RoundToInt(itemData.coinsPerKill * numMult);
        baseShopDiscount = itemData.shopDiscount * pctMult;
        baseFreeRerollChance = itemData.freeRerollChance * pctMult;
        baseExtraShopCards = Mathf.RoundToInt(itemData.extraShopCards * numMult);
        baseDamagePerKillInBattle = itemData.damagePerKillInBattle * numMult;
        baseDamagePerItemInInventory = itemData.damagePerItemInInventory * numMult;
        baseBonusPerEmptyCell = itemData.bonusPerEmptyCell * numMult;
        baseNeighborBonusDamage = itemData.neighborBonusDamage * pctMult;
        baseNeighborBonusSpeed = itemData.neighborBonusSpeed * pctMult;
        baseNeighborBonusCrit = itemData.neighborBonusCrit * pctMult;
        baseNeighborBonusVampirism = itemData.neighborBonusVampirism * pctMult;
        baseNeighborBonusArmor = itemData.neighborBonusArmor * numMult;
        baseNeighborBonusHP = itemData.neighborBonusHP * numMult;
        baseCategoryBonusDamage = itemData.categoryBonusDamage * pctMult;
        baseCategoryBonusSpeed = itemData.categoryBonusSpeed * pctMult;
        baseCategoryBonusCrit = itemData.categoryBonusCrit * pctMult;
        // Мана
        baseBonusMana = itemData.bonusMana * numMult;
        baseManaRegen = itemData.manaRegen * numMult;
        baseManaCostPerSecond = itemData.manaCostPerSecond * numMult;
        baseManaCostPerAttack = itemData.manaCostPerAttack * numMult;
    }

    public void RefreshOccupiedCells()
    {
        List<Vector2Int> list = new List<Vector2Int>();
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                int index = y * 5 + x;
                if (cells[index] != null && cells[index].isActive)
                    list.Add(new Vector2Int(x, y));
            }
        occupiedCells = list.ToArray();
        UpdateCellVisuals();
    }

    public Vector2Int GetAnchorCell()
    {
        if (occupiedCells.Length == 0) return Vector2Int.zero;
        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var cell in occupiedCells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.y < minY) minY = cell.y;
        }
        return new Vector2Int(minX, minY);
    }

    public void UpdateCellVisuals()
    {
        int levelIndex = Mathf.Clamp(itemLevel - 1, 0, 4);
        if (backgroundImage != null)
        {
            if (levelSprites != null && levelSprites.Length > levelIndex && levelSprites[levelIndex] != null)
            {
                backgroundImage.sprite = levelSprites[levelIndex];
                backgroundImage.enabled = true;
            }
            else backgroundImage.enabled = false;
        }

        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                int index = y * 5 + x;
                if (cells[index] != null && cells[index].cellObject != null)
                {
                    Image img = cells[index].cellObject.GetComponent<Image>();
                    if (img != null)
                    {
                        img.enabled = cells[index].isActive;
                        if (cells[index].isActive) img.color = new Color(1, 1, 1, 0);
                    }
                }
            }
    }

    public void SetVisualScale(Vector3 scale) => transform.localScale = scale;
    public void SetSellMode(bool selling) { if (backgroundImage != null) backgroundImage.color = selling ? Color.red : Color.white; }

    public bool CanMergeWith(Item other)
    {
        if (other == null || other == this) return false;
        if (itemName != other.itemName) return false;
        if (itemLevel >= 3 || other.itemLevel >= 3) return false;
        if (itemLevel != other.itemLevel) return false;
        return true;
    }

    public void MergeWith(Item other)
    {
        itemLevel = Mathf.Clamp(itemLevel + 1, 1, 3);
        RecalculateAllStats();
        description = itemData.GetAutoDescription();
        UpdateCellVisuals();
    }

    Vector2Int GetClosestActiveCell(Vector2 screenPoint)
    {
        float minDist = float.MaxValue;
        Vector2Int closest = occupiedCells.Length > 0 ? occupiedCells[0] : Vector2Int.zero;
        foreach (var cell in occupiedCells)
        {
            int index = cell.y * 5 + cell.x;
            if (cells[index] != null && cells[index].cellObject != null)
            {
                Vector2 cellWorld = cells[index].cellObject.transform.position;
                float dist = Vector2.Distance(screenPoint, cellWorld);
                if (dist < minDist) { minDist = dist; closest = cell; }
            }
        }
        return closest;
    }

    public Vector2 GetCellLocalPosition(Vector2Int cell)
    {
        int index = cell.y * 5 + cell.x;
        if (cells[index] != null && cells[index].cellObject != null)
            return cells[index].cellObject.transform.localPosition;
        return Vector2.zero;
    }

    public void PlayAttackAnimation()
    {
        if (attackAnimationCoroutine != null) StopCoroutine(attackAnimationCoroutine);
        attackAnimationCoroutine = StartCoroutine(AttackShake());
    }

    IEnumerator AttackShake()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 punchScale = originalScale * 1.15f;
        float duration = 0.15f, elapsed = 0f;
        while (elapsed < duration * 0.3f) { elapsed += Time.deltaTime; transform.localScale = Vector3.Lerp(originalScale, punchScale, elapsed / (duration * 0.3f)); yield return null; }
        elapsed = 0f;
        while (elapsed < duration * 0.7f) { elapsed += Time.deltaTime; float t = 1f - (1f - elapsed / (duration * 0.7f)) * (1f - elapsed / (duration * 0.7f)); transform.localScale = Vector3.Lerp(punchScale, originalScale, t); yield return null; }
        transform.localScale = originalScale;
    }

    public List<(Sprite icon, string name, string value)> GetStats(StatIconsData icons)
    {
        var stats = new List<(Sprite, string, string)>();
        if (itemData == null || icons == null) return stats;

        bool inBattle = BattleManager.Instance != null && BattleManager.Instance.IsBattling;

        float theoreticalDmg = baseDamage;
        float theoreticalSpd = baseAttackSpeed;

        if (GridManager.Instance != null)
        {
            var grid = GridManager.Instance.InventoryGrid;
            var allItems = GridManager.Instance.inventoryItems;
            var (modDamage, modSpeed, _) = BattleManager.CalculateTheoreticalModifiers(allItems, grid);
            if (modDamage.ContainsKey(this)) theoreticalDmg = modDamage[this];
            if (modSpeed.ContainsKey(this)) theoreticalSpd = modSpeed[this];
        }

        // Мана
        if (baseBonusMana != 0 && icons.manaIcon != null)
            stats.Add((icons.manaIcon, "Мана", (baseBonusMana > 0 ? "+" : "") + baseBonusMana.ToString("0")));
        if (baseManaRegen > 0 && icons.manaRegenIcon != null)
            stats.Add((icons.manaRegenIcon, "Реген. маны", $"{baseManaRegen}/сек"));
        if (baseManaCostPerSecond > 0 && icons.manaCostIcon != null)
            stats.Add((icons.manaCostIcon, "Расход маны", $"{baseManaCostPerSecond}/сек"));
        if (baseManaCostPerAttack > 0 && icons.manaCostIcon != null)
            stats.Add((icons.manaCostIcon, "Расход маны", $"{baseManaCostPerAttack}/атака"));

        // Урон
        if (baseDamage > 0 && icons.damageIcon != null)
        {
            float killBonus = inBattle ? BattleManager.Instance.GetKillBonus(this) : 0f;
            float totalDmg = theoreticalDmg + killBonus;
            float bonus = totalDmg - baseDamage;
            string dmgText = totalDmg.ToString("0");
            if (bonus > 0) dmgText += $" (+{bonus})";
            stats.Add((icons.damageIcon, "Урон", dmgText));
        }

        // Скорость атаки
        if (baseAttackSpeed > 0 && baseDamage > 0 && icons.attackSpeedIcon != null)
        {
            float diff = baseAttackSpeed - theoreticalSpd;
            string spdText = theoreticalSpd.ToString("0.0") + " сек";
            if (Mathf.Abs(diff) > 0.001f)
            {
                if (diff > 0) spdText += $" (-{diff:0.0})";
                else spdText += $" (+{-diff:0.0})";
            }
            stats.Add((icons.attackSpeedIcon, "Скорость атаки", spdText));
        }

        // Крит. шанс
        if (baseCritChance > 0 && icons.critChanceIcon != null)
            stats.Add((icons.critChanceIcon, "Крит. шанс", baseCritChance.ToString("0") + "%"));
        // Крит. урон
        if (baseCritDamage > 200f && baseCritChance > 0 && icons.critDamageIcon != null)
            stats.Add((icons.critDamageIcon, "Крит. урон", baseCritDamage.ToString("0") + "%"));
        // Здоровье
        if (baseBonusHP != 0 && icons.healthIcon != null)
            stats.Add((icons.healthIcon, "Здоровье", (baseBonusHP > 0 ? "+" : "") + baseBonusHP.ToString("0")));
        // Броня
        if (baseBonusArmor != 0 && icons.armorIcon != null)
            stats.Add((icons.armorIcon, "Броня", (baseBonusArmor > 0 ? "+" : "") + baseBonusArmor.ToString("0")));
        // Уклонение
        if (baseDodgeChance > 0 && icons.dodgeIcon != null)
            stats.Add((icons.dodgeIcon, "Уклонение", baseDodgeChance.ToString("0") + "%"));
        // Вампиризм
        if (baseVampirism > 0 && icons.vampirismIcon != null)
            stats.Add((icons.vampirismIcon, "Вампиризм", baseVampirism.ToString("0") + "%"));
        // Отравление
        if (basePoisonDamage > 0 && icons.poisonIcon != null)
            stats.Add((icons.poisonIcon, "Отравление", $"{basePoisonDamage}/сек ({itemData.poisonDuration}с)"));
        // Поджог
        if (baseBurnDamage > 0 && icons.burnIcon != null)
            stats.Add((icons.burnIcon, "Поджог", $"{baseBurnDamage}/сек ({itemData.burnDuration}с)"));
        // Замедление
        if (baseSlowPercent > 0 && icons.slowIcon != null)
            stats.Add((icons.slowIcon, "Замедление", baseSlowPercent.ToString("0") + "%"));
        // Оглушение
        if (baseStunChance > 0 && icons.stunIcon != null)
            stats.Add((icons.stunIcon, "Оглушение", baseStunChance.ToString("0") + "%"));
        // Шипы
        if (baseThornsDamage > 0 && icons.thornsIcon != null)
            stats.Add((icons.thornsIcon, "Шипы", baseThornsDamage.ToString("0")));
        // Разрез
        if (baseBleedDamage > 0 && icons.bleedIcon != null)
            stats.Add((icons.bleedIcon, "Разрез", $"+{baseBleedDamage}/удар"));
        // Казнь
        if (baseExecuteThreshold > 0 && icons.guillotineIcon != null)
            stats.Add((icons.guillotineIcon, "Казнь", $"<{baseExecuteThreshold}% HP"));
        // Доп. снаряд
        if (baseExtraProjectileChance > 0 && icons.extraProjectileIcon != null)
            stats.Add((icons.extraProjectileIcon, "Доп. снаряд", baseExtraProjectileChance.ToString("0") + "%"));
        // Реген. HP
        if (baseHPRegen > 0 && icons.hpRegenIcon != null)
            stats.Add((icons.hpRegenIcon, "Реген. HP", $"{baseHPRegen}/сек"));
        // Реген. брони
        if (baseArmorRegen > 0 && icons.armorRegenIcon != null)
            stats.Add((icons.armorRegenIcon, "Реген. брони", $"{baseArmorRegen}/сек"));
        // Блок
        if (baseBlockChance > 0 && icons.blockChanceIcon != null)
            stats.Add((icons.blockChanceIcon, "Блок", baseBlockChance.ToString("0") + "%"));
        // Сопр. яду
        if (basePoisonResist > 0 && icons.poisonResistIcon != null)
            stats.Add((icons.poisonResistIcon, "Сопр. яду", basePoisonResist.ToString("0") + "%"));
        // Сопр. огню
        if (baseFireResist > 0 && icons.fireResistIcon != null)
            stats.Add((icons.fireResistIcon, "Сопр. огню", baseFireResist.ToString("0") + "%"));
        // Снижение урона
        if (baseDamageReduction > 0 && icons.damageReductionIcon != null)
            stats.Add((icons.damageReductionIcon, "Урон врага снижен на", baseDamageReduction.ToString("0")));
        // Снижение урона %
        if (baseDamageReductionPercent > 0 && icons.damageReductionIcon != null)
            stats.Add((icons.damageReductionIcon, "Урон врага снижен на", baseDamageReductionPercent.ToString("0") + "%"));
        // Неуязвимость
        if (baseInvulnerabilityDuration > 0 && icons.invulnerabilityIcon != null)
            stats.Add((icons.invulnerabilityIcon, "Неуязвимость", $"{baseInvulnerabilityDuration}с"));

        // Соседям
        if (baseNeighborBonusDamage > 0 && icons.damageIcon != null)
            stats.Add((icons.damageIcon, "Соседям: урон", "+" + baseNeighborBonusDamage + "%"));
        if (baseNeighborBonusSpeed > 0 && icons.attackSpeedIcon != null)
            stats.Add((icons.attackSpeedIcon, "Соседям: скорость", "+" + baseNeighborBonusSpeed + "%"));
        if (baseNeighborBonusCrit > 0 && icons.critChanceIcon != null)
            stats.Add((icons.critChanceIcon, "Соседям: крит", "+" + baseNeighborBonusCrit + "%"));
        if (baseNeighborBonusVampirism > 0 && icons.vampirismIcon != null)
            stats.Add((icons.vampirismIcon, "Соседям: вампиризм", "+" + baseNeighborBonusVampirism + "%"));
        if (baseNeighborBonusArmor > 0 && icons.armorIcon != null)
            stats.Add((icons.armorIcon, "Соседям: броня", "+" + baseNeighborBonusArmor));
        if (baseNeighborBonusHP > 0 && icons.healthIcon != null)
            stats.Add((icons.healthIcon, "Соседям: здоровье", "+" + baseNeighborBonusHP));

        // Бафф по категории
        if (itemData.hasCategoryBuff)
        {
            string targetName = itemData.GetCategoryTargetName();
            if (baseCategoryBonusDamage > 0 && icons.damageIcon != null)
                stats.Add((icons.damageIcon, $"Урон всех {targetName}", "+" + baseCategoryBonusDamage + "%"));
            if (baseCategoryBonusSpeed > 0 && icons.attackSpeedIcon != null)
                stats.Add((icons.attackSpeedIcon, $"Скорость всех {targetName}", "+" + baseCategoryBonusSpeed + "%"));
            if (baseCategoryBonusCrit > 0 && icons.critChanceIcon != null)
                stats.Add((icons.critChanceIcon, $"Крит всех {targetName}", "+" + baseCategoryBonusCrit + "%"));
        }

        // Экономические
        if (baseCoinsPerLevel > 0 && icons.coinsPerLevelIcon != null)
            stats.Add((icons.coinsPerLevelIcon, "Монет за уровень", "+" + baseCoinsPerLevel));
        if (baseCoinsPerKill > 0 && icons.coinsPerKillIcon != null)
            stats.Add((icons.coinsPerKillIcon, "Монет за убийство", "+" + baseCoinsPerKill));
        if (baseShopDiscount > 0 && icons.shopDiscountIcon != null)
            stats.Add((icons.shopDiscountIcon, "Скидка", baseShopDiscount.ToString("0") + "%"));
        if (baseFreeRerollChance > 0 && icons.freeRerollIcon != null)
            stats.Add((icons.freeRerollIcon, "Реролл", $"{baseFreeRerollChance}% шанс бесплатно"));
        if (baseExtraShopCards > 0 && icons.extraCardIcon != null)
            stats.Add((icons.extraCardIcon, "Карточки", "+" + baseExtraShopCards));
        if (baseFullPriceSell && icons.fullPriceIcon != null)
            stats.Add((icons.fullPriceIcon, "Продажа", "100% цены"));

        // Особые
        if (baseBonusPerEmptyCell > 0 && icons.damagePerEmptyIcon != null)
            stats.Add((icons.damagePerEmptyIcon, "Урон за пустоту", "+" + baseBonusPerEmptyCell));
        if (itemData.phoenixResurrection && icons.invulnerabilityIcon != null)
            stats.Add((icons.invulnerabilityIcon, "Феникс", $"{itemData.phoenixHPPercent}% HP"));
        if (itemData.mirrorNeighbor) stats.Add((null, "Зеркало", "Копирует соседа"));
        if (itemData.dummy) stats.Add((null, "Пустышка", ""));
        if (itemData.timeCompression > 0 && icons.timeCompressionIcon != null)
            stats.Add((icons.timeCompressionIcon, "Сжатие времени", "+" + itemData.timeCompression + "%"));
        if (baseDamagePerItemInInventory > 0 && icons.damagePerItemIcon != null)
            stats.Add((icons.damagePerItemIcon, "Урон за предмет", "+" + baseDamagePerItemInInventory));
        if (baseDamagePerKillInBattle > 0 && icons.damagePerKillIcon != null)
            stats.Add((icons.damagePerKillIcon, "Урон за убийство", "+" + baseDamagePerKillInBattle));

        return stats;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        foreach (var cell in occupiedCells)
        {
            int index = cell.y * 5 + cell.x;
            if (cells[index] != null && cells[index].cellObject != null)
            {
                RectTransform cellRect = cells[index].cellObject.GetComponent<RectTransform>();
                if (cellRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(cellRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                {
                    if (cellRect.rect.Contains(localPoint))
                    {
                        if (!string.IsNullOrEmpty(description) && InfoPanel.Instance != null)
                            InfoPanel.Instance.Show(itemName, description, GetComponent<RectTransform>());
                        return;
                    }
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        InfoPanel.Instance?.Hide();
        dragStartItemPos = rect.position;
        dragStartMousePos = eventData.position;
        dragStartParent = transform.parent;
        dragStartGrabbedCell = grabbedCell;
        grabbedCell = GetClosestActiveCell(eventData.position);
        canvasGroup.alpha = 0.7f; canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform); transform.SetAsLastSibling();
        isDragging = true; SetSellMode(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position = dragStartItemPos + (eventData.position - dragStartMousePos);

        if (GridManager.Instance != null)
        {
            Vector2Int invCell = GridManager.Instance.GetClosestInventoryCell(eventData.position);
            float distInv = Vector2.Distance(eventData.position,
                GridManager.Instance.inventorySlots[invCell.y * 5 + invCell.x].transform.position);

            bool overShop = GridManager.Instance.IsOverShop(eventData.position);
            float distShop = overShop ? 0f : float.MaxValue;
            if (overShop)
                distShop = Vector2.Distance(eventData.position, GridManager.Instance.shopContainer.position);

            if (!overShop || distInv <= distShop)
            {
                bool canPlace = GridManager.Instance.CanPlaceItem(this, invCell.x, invCell.y);
                GridManager.Instance.HighlightInventorySlots(this, invCell.x, invCell.y, canPlace);
                SetSellMode(false);
            }
            else
            {
                GridManager.Instance.ClearInventoryHighlight();
                SetSellMode(true);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; isDragging = false; SetSellMode(false);
        if (GridManager.Instance == null) { ReturnToStartPosition(); return; }
        GridManager.Instance.ClearInventoryHighlight();

        Vector2Int invCell = GridManager.Instance.GetClosestInventoryCell(eventData.position);
        float distInv = Vector2.Distance(eventData.position,
            GridManager.Instance.inventorySlots[invCell.y * 5 + invCell.x].transform.position);

        bool overShop = GridManager.Instance.IsOverShop(eventData.position);
        float distShop = overShop ? 0f : float.MaxValue;
        if (overShop)
            distShop = Vector2.Distance(eventData.position, GridManager.Instance.shopContainer.position);

        if (!overShop || distInv <= distShop)
        {
            if (GridManager.Instance.CanPlaceItem(this, invCell.x, invCell.y))
                GridManager.Instance.PlaceItem(this, invCell.x, invCell.y);
            else
                ReturnToStartPosition();
        }
        else
        {
            GridManager.Instance.SellItem(this);
        }
    }

    void ReturnToStartPosition()
    {
        grabbedCell = dragStartGrabbedCell;
        rect.position = dragStartItemPos;
        transform.SetParent(dragStartParent);
    }
}