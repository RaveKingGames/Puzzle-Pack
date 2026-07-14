using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EnemyData
{
    public string enemyName;
    public Sprite sprite;
    public float hp;
    public float damage;
    public float attackSpeed;
}

[System.Serializable]
public class LocationData
{
    public string locationName;
    public Sprite backgroundSprite;
    public List<EnemyData> enemies;
    public List<EnemyData> bosses;
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    
    [Header("Префаб врага")]
    public GameObject enemyPrefab;
    
    [Header("Локации (5 штук)")]
    public List<LocationData> locations;
    
    [Header("Фон локации")]
    public SpriteRenderer backgroundRenderer;
    
    [Header("Контейнер для врагов")]
    public Transform battleArea;
    
    [Header("Панель инвентаря/магазина")]
    public RectTransform inventoryStorePanel;
    public bool IsBattling => isBattling;
    
    [Header("Кнопки")]
    public Button fightButton;
    
    [Header("Статы игрока")]
    public float basePlayerHP = 50f;
    public float basePlayerArmor = 0f;
    
    [Header("UI Здоровья/Защиты игрока")]
    public Text playerHealthText;
    public RectTransform playerHealthFill;
    public RectTransform playerArmorFill;
    public Image playerHealthIcon;
    public Sprite heartSprite;
    public Sprite shieldSprite;
    
    [Header("UI Маны")]
    public ManaBar manaBar;
    
    [Header("UI Здоровья врага")]
    public Text enemyHealthText;
    public RectTransform enemyHealthFill;
    public GameObject enemyHealthPanel;
    public Text enemyNameText;
    
    [Header("UI Уровня")]
    public Text levelText;
    
    [Header("VFX")]
    public GameObject projectilePrefab;
    public GameObject damageNumberPrefab;
    public GameObject floatingIconPrefab;
    public StatIconsData statIconsData;
    public RectTransform mainCanvasRect;
    public RectTransform enemyAnchorPoint;
    public RectTransform worldContainer;
    public float projectileSpreadRadius = 80f;
    
    [Header("Анимация падения")]
    public float squashDuration = 0.1f;
    public float squashAmount = 20f;
    public float fallDuration = 0.25f;
    public float shakeAmount = 8f;
    public float shakeDuration = 0.15f;
    public float bounceDuration = 0.2f;
    public float bounceHeight = 40f;
    public int bounceCount = 3;
    
    [Header("Анимация подъёма")]
    public float riseAnticipation = 0.08f;
    public float riseDuration = 0.2f;
    public float overshootHeight = 25f;
    
    private float playerCurrentHP;
    private float playerMaxHP;
    private float playerCurrentArmor;
    private float playerMaxArmor;
    private float playerCurrentMana;
    private float playerMaxMana;
    private Enemy currentEnemy;
    private List<Item> playerItems = new List<Item>();
    private bool isBattling = false;
    private bool levelComplete = false;
    
    private int currentLocationIndex = 0;
    public int currentLevel = 1;
    
    private Queue<EnemyData> enemyQueue = new Queue<EnemyData>();
    private int totalEnemiesOnLevel;
    private int enemiesDefeated;
    
    private Coroutine panelAnimationCoroutine;
    private float panelOriginalY;
    
    private Dictionary<Item, float> itemTimers = new Dictionary<Item, float>();
    private float enemyAttackTimer = 0f;

    private float totalHPRegen;
    private float totalArmorRegen;
    private float totalVampirism;
    private float totalDodgeChance;
    private float totalBlockChance;
    private float totalDamageReduction;
    private float totalDamageReductionPercent;
    private float totalTimeCompression;
    private float totalManaRegen;
    private float totalManaCostPerSecond;
    private Dictionary<Item, float> modifiedDamage = new Dictionary<Item, float>();
    private Dictionary<Item, float> modifiedSpeed = new Dictionary<Item, float>();
    private Dictionary<Item, float> modifiedCrit = new Dictionary<Item, float>();
    private Dictionary<Item, float> killBonuses = new Dictionary<Item, float>();
    
    void Awake() => Instance = this;
    
    void Start()
    {
        if (fightButton != null) fightButton.onClick.AddListener(StartBattle);
        playerMaxHP = basePlayerHP;
        playerCurrentHP = basePlayerHP;
        playerCurrentArmor = basePlayerArmor;
        playerMaxArmor = basePlayerArmor;
        playerCurrentMana = 0;
        playerMaxMana = 0;
        UpdateBackground();
        UpdatePreviewStats(basePlayerHP, basePlayerArmor);  // ← обновить превью при старте
        UpdateLevelUI();
        if (enemyHealthPanel != null) enemyHealthPanel.SetActive(false);
        if (inventoryStorePanel != null) panelOriginalY = inventoryStorePanel.anchoredPosition.y;
        if (battleArea != null) battleArea.gameObject.SetActive(false);
        if (manaBar != null) manaBar.UpdateManaBar(0, 0);
    }

    IEnumerator ShakeWorldContainer()
    {
        if (worldContainer == null) yield break;
        Vector3 origPos = worldContainer.localPosition;
        float duration = 0.1f;
        float elapsed = 0f;
        float intensity = 8f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float decay = 1f - (elapsed / duration);
            float x = Random.Range(-1f, 1f) * intensity * decay;
            float y = Random.Range(-1f, 1f) * intensity * decay;
            worldContainer.localPosition = origPos + new Vector3(x, y, 0);
            yield return null;
        }
        worldContainer.localPosition = origPos;
    }

    public void SpawnFloatingIcon(Item item, Sprite icon)
    {
        if (floatingIconPrefab == null || item == null || icon == null) return;
        
        GameObject iconObj = Instantiate(floatingIconPrefab, item.transform);
        iconObj.transform.localPosition = Vector3.zero;
        
        FloatingIcon floatingIcon = iconObj.GetComponent<FloatingIcon>();
        if (floatingIcon != null)
            floatingIcon.ShowLocal(icon);
        else
            Destroy(iconObj);
    }


    public static (Dictionary<Item, float> damage, Dictionary<Item, float> speed, Dictionary<Item, float> crit) 
    CalculateTheoreticalModifiers(List<Item> items, Item[,] grid)
    {
        var modDamage = new Dictionary<Item, float>();
        var modSpeed = new Dictionary<Item, float>();
        var modCrit = new Dictionary<Item, float>();

        foreach (var item in items)
        {
            if (item?.itemData == null) continue;
            modDamage[item] = item.baseDamage;
            modSpeed[item] = item.baseAttackSpeed;
            modCrit[item] = item.baseCritChance;
        }

        foreach (var sourceItem in items)
        {
            if (sourceItem?.itemData == null) continue;
            var neighbors = GetNeighborsByEffectMaskStatic(sourceItem, grid);
            foreach (var neighbor in neighbors)
            {
                if (!modDamage.ContainsKey(neighbor)) continue;
                modDamage[neighbor] += modDamage[neighbor] * (sourceItem.baseNeighborBonusDamage / 100f);
                modSpeed[neighbor] *= (1 - sourceItem.baseNeighborBonusSpeed / 100f);
                modCrit[neighbor] += sourceItem.baseNeighborBonusCrit;
            }
        }

        foreach (var sourceItem in items)
        {
            if (sourceItem?.itemData == null || !sourceItem.itemData.hasCategoryBuff) continue;
            ItemType targetType = sourceItem.itemData.categoryTargetType;
            foreach (var targetItem in items)
            {
                if (targetItem?.itemData == null || targetItem.itemData.itemType != targetType) continue;
                if (!modDamage.ContainsKey(targetItem)) continue;
                modDamage[targetItem] += modDamage[targetItem] * (sourceItem.baseCategoryBonusDamage / 100f);
                modSpeed[targetItem] *= (1 - sourceItem.baseCategoryBonusSpeed / 100f);
                modCrit[targetItem] += sourceItem.baseCategoryBonusCrit;
            }
        }

        int itemCount = items.Count;
        foreach (var item in items)
        {
            if (item?.itemData == null || !modDamage.ContainsKey(item)) continue;
            modDamage[item] += item.baseDamagePerItemInInventory * itemCount;
        }

        return (modDamage, modSpeed, modCrit);
    }

    public void RecalculateBattleStats()
    {
        if (!isBattling) return;
        var theoretical = CalculateTheoreticalModifiers(playerItems, GridManager.Instance.InventoryGrid);
        modifiedDamage = theoretical.damage;
        modifiedSpeed = theoretical.speed;
        modifiedCrit = theoretical.crit;
    }

    private static List<Item> GetNeighborsByEffectMaskStatic(Item source, Item[,] grid)
    {
        List<Item> result = new List<Item>();
        if (source?.itemData?.effectMask == null) return result;
        Vector2Int pos = source.positionInGrid;
        Vector2Int grabbed = source.grabbedCell;
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                if (!source.itemData.effectMask[x + y * 5]) continue;
                int wx = pos.x + (x - grabbed.x);
                int wy = pos.y + (y - grabbed.y);
                if (wx >= 0 && wx < 5 && wy >= 0 && wy < 5)
                {
                    Item neighbor = grid[wx, wy];
                    if (neighbor != null && neighbor != source && !result.Contains(neighbor))
                        result.Add(neighbor);
                }
            }
        return result;
    }

    private List<Item> GetNeighborsByEffectMask(Item source)
    {
        return GetNeighborsByEffectMaskStatic(source, GridManager.Instance.InventoryGrid);
    }
    
    void UpdateLevelUI() { if (levelText != null) levelText.text = $"Уровень {currentLevel}"; }
    
    public void UpdatePreviewStats(float hp, float armor)
    {
        if (playerHealthText != null)
        {
            if (armor > 0)
            {
                if (playerHealthIcon != null && shieldSprite != null) playerHealthIcon.sprite = shieldSprite;
                playerHealthText.text = $"{Mathf.CeilToInt(armor)}/{Mathf.CeilToInt(armor)}";
                if (playerArmorFill != null) playerArmorFill.gameObject.SetActive(true);
                if (playerHealthFill != null) playerHealthFill.gameObject.SetActive(false);
            }
            else
            {
                if (playerHealthIcon != null && heartSprite != null) playerHealthIcon.sprite = heartSprite;
                playerHealthText.text = $"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(hp)}";
                if (playerHealthFill != null) playerHealthFill.gameObject.SetActive(true);
                if (playerArmorFill != null) playerArmorFill.gameObject.SetActive(false);
            }
        }
    }
    
    void UpdatePlayerHealthUI()
    {
        if (playerHealthText == null) return;

        if (playerCurrentArmor > 0)
        {
            if (playerHealthIcon != null && shieldSprite != null) playerHealthIcon.sprite = shieldSprite;
            playerHealthText.text = $"{Mathf.CeilToInt(playerCurrentArmor)}/{Mathf.CeilToInt(playerMaxArmor)}";
            if (playerArmorFill != null)
            {
                playerArmorFill.gameObject.SetActive(true);
                float armorPercent = playerMaxArmor > 0 ? playerCurrentArmor / playerMaxArmor : 1f;
                float xPos = Mathf.Lerp(-1000f, 0f, armorPercent);
                Vector2 pos = playerArmorFill.anchoredPosition; pos.x = xPos;
                playerArmorFill.anchoredPosition = pos;
            }
        }
        else
        {
            if (playerHealthIcon != null && heartSprite != null) playerHealthIcon.sprite = heartSprite;
            playerHealthText.text = $"{Mathf.CeilToInt(playerCurrentHP)}/{Mathf.CeilToInt(playerMaxHP)}";
            if (playerArmorFill != null) playerArmorFill.gameObject.SetActive(false);
        }

        // Красная полоска HP всегда видна и обновляется
        if (playerHealthFill != null)
        {
            playerHealthFill.gameObject.SetActive(true);
            float hpPercent = playerMaxHP > 0 ? playerCurrentHP / playerMaxHP : 1f;
            float xPos = Mathf.Lerp(-1000f, 0f, hpPercent);
            Vector2 pos = playerHealthFill.anchoredPosition; pos.x = xPos;
            playerHealthFill.anchoredPosition = pos;
        }

        if (manaBar != null)
            manaBar.UpdateManaBar(playerCurrentMana, playerMaxMana);
    }
    
    public void UpdateEnemyHealthUI()
    {
        if (currentEnemy == null) return;
        if (enemyHealthPanel != null) enemyHealthPanel.SetActive(true);
        if (enemyNameText != null) enemyNameText.text = currentEnemy.enemyName;
        if (enemyHealthText != null) enemyHealthText.text = $"{Mathf.CeilToInt(currentEnemy.CurrentHP)}/{currentEnemy.MaxHP}";
        if (enemyHealthFill != null)
        {
            float hpPercent = currentEnemy.CurrentHP / currentEnemy.MaxHP;
            float xPos = Mathf.Lerp(-1000f, 0f, hpPercent);
            Vector2 pos = enemyHealthFill.anchoredPosition; pos.x = xPos; enemyHealthFill.anchoredPosition = pos;
        }
    }
    
    void UpdateBackground()
    {
        if (backgroundRenderer != null && locations.Count > currentLocationIndex)
            backgroundRenderer.sprite = locations[currentLocationIndex].backgroundSprite;
    }
    
    LocationData GetCurrentLocation()
    {
        if (locations.Count == 0) return null;
        return locations[Mathf.Clamp(currentLocationIndex, 0, locations.Count - 1)];
    }
    
    public void StartBattle()
    {
        if (isBattling) return;
        playerItems.Clear();
        foreach (var item in GridManager.Instance.inventoryItems)
            if (item != null) playerItems.Add(item);
        if (playerItems.Count == 0) { Debug.Log("Нет предметов в инвентаре!"); return; }

        totalHPRegen = 0; totalArmorRegen = 0; totalVampirism = 0;
        totalDodgeChance = 0; totalBlockChance = 0;
        totalDamageReduction = 0; totalDamageReductionPercent = 0;
        totalTimeCompression = 0;
        totalManaRegen = 0;
        totalManaCostPerSecond = 0;
        playerMaxMana = 0;
        modifiedDamage.Clear(); modifiedSpeed.Clear(); modifiedCrit.Clear();
        killBonuses.Clear();

        foreach (var item in playerItems)
        {
            if (item.itemData == null) continue;
            killBonuses[item] = 0f;
            totalHPRegen += item.baseHPRegen;
            totalArmorRegen += item.baseArmorRegen;
            totalVampirism += item.baseVampirism;
            totalDodgeChance += item.baseDodgeChance;
            totalBlockChance += item.baseBlockChance;
            totalDamageReduction += item.baseDamageReduction;
            totalDamageReductionPercent += item.baseDamageReductionPercent;
            totalTimeCompression += item.itemData.timeCompression;
            totalManaRegen += item.baseManaRegen;
            totalManaCostPerSecond += item.baseManaCostPerSecond;
            playerMaxMana += item.baseBonusMana;
        }

        var theoretical = CalculateTheoreticalModifiers(playerItems, GridManager.Instance.InventoryGrid);
        modifiedDamage = theoretical.damage;
        modifiedSpeed = theoretical.speed;
        modifiedCrit = theoretical.crit;

        if (totalTimeCompression > 0)
        {
            var keys = new List<Item>(modifiedSpeed.Keys);
            foreach (var item in keys)
            {
                modifiedSpeed[item] *= (1 - totalTimeCompression / 100f);
                if (modifiedSpeed[item] < 0.05f) modifiedSpeed[item] = 0.05f;
            }
        }

        playerMaxHP = basePlayerHP;
        playerMaxArmor = basePlayerArmor;
        float totalBonusHP = 0, totalBonusArmor = 0;
        foreach (var item in playerItems)
        {
            if (item.itemData == null) continue;
            totalBonusHP += item.baseBonusHP;
            totalBonusArmor += item.baseBonusArmor;
        }
        playerMaxHP += totalBonusHP;
        playerMaxArmor += totalBonusArmor;
        playerCurrentHP = playerMaxHP;
        playerCurrentArmor = playerMaxArmor;
        playerCurrentMana = playerMaxMana;
        levelComplete = false;
        UpdatePlayerHealthUI();
        if (battleArea != null) battleArea.gameObject.SetActive(true);
        if (inventoryStorePanel != null)
        {
            float panelHeight = inventoryStorePanel.rect.height;
            float targetY = -panelHeight / 2f;
            if (panelAnimationCoroutine != null) StopCoroutine(panelAnimationCoroutine);
            panelAnimationCoroutine = StartCoroutine(DropPanel(targetY));
        }
        StartCoroutine(SpawnLevel(currentLevel));
    }
        
    void InitItemTimers()
    {
        itemTimers.Clear();
        foreach (var item in playerItems) if (item != null) itemTimers[item] = 0f;
        enemyAttackTimer = 0f;
    }
    
    void DamagePlayer(float rawDamage)
    {
        if (Random.value * 100f < totalDodgeChance)
        {
            SpawnIconFromItemsWithStat(i => i.baseDodgeChance > 0, statIconsData?.dodgeIcon);
            return;
        }
        if (Random.value * 100f < totalBlockChance)
        {
            SpawnIconFromItemsWithStat(i => i.baseBlockChance > 0, statIconsData?.blockChanceIcon);
            return;
        }
        
        StartCoroutine(ShakeWorldContainer());
        
        float damage = rawDamage - totalDamageReduction;
        if (damage < 0) damage = 0;
        damage *= (1 - totalDamageReductionPercent / 100f);
        if (damage < 0) damage = 0;
        if (playerCurrentArmor > 0)
        {
            if (damage <= playerCurrentArmor) { playerCurrentArmor -= damage; damage = 0; }
            else { damage -= playerCurrentArmor; playerCurrentArmor = 0; }
        }
        if (damage > 0) playerCurrentHP -= damage;
        if (playerCurrentHP < 0) playerCurrentHP = 0;
    }

    void SpawnIconFromItemsWithStat(System.Func<Item, bool> condition, Sprite icon)
    {
        if (icon == null) return;
        List<Item> matching = new List<Item>();
        foreach (var item in playerItems)
            if (item != null && condition(item)) matching.Add(item);
        if (matching.Count > 0)
        {
            Item randomItem = matching[Random.Range(0, matching.Count)];
            SpawnFloatingIcon(randomItem, icon);
        }
    }
    
    void SpawnProjectileAndDamage(Item item, float damage, string enemyName)
    {
        if (projectilePrefab == null || damageNumberPrefab == null || mainCanvasRect == null) return;
        if (currentEnemy == null || enemyAnchorPoint == null) return;
        Vector2 cellLocal = item.GetCellLocalPosition(new Vector2Int(2, 2));
        Vector3 itemWorldPos = item.transform.TransformPoint(cellLocal);
        Vector2 fromScreenPos = RectTransformUtility.WorldToScreenPoint(null, itemWorldPos);
        Vector2 enemyAnchorScreen = RectTransformUtility.WorldToScreenPoint(null, enemyAnchorPoint.position);
        Vector2 toScreenPos = enemyAnchorScreen + Random.insideUnitCircle * projectileSpreadRadius;
        Enemy targetEnemy = currentEnemy;
        GameObject projectileObj = Instantiate(projectilePrefab, mainCanvasRect);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Fly(fromScreenPos, toScreenPos, mainCanvasRect, (hitScreenPos) =>
            {
                if (targetEnemy != null && targetEnemy.IsAlive())
                {
                    targetEnemy.TakeDamage(damage);
                    float heal = damage * (totalVampirism / 100f);
                    playerCurrentHP = Mathf.Min(playerCurrentHP + heal, playerMaxHP);
                }
                GameObject damageObj = Instantiate(damageNumberPrefab, mainCanvasRect);
                DamageNumber dmgNum = damageObj.GetComponent<DamageNumber>();
                if (dmgNum != null) dmgNum.Show(damage, hitScreenPos, mainCanvasRect);
            });
        }
    }

    IEnumerator DropPanel(float targetY)
    {
        Vector2 pos = inventoryStorePanel.anchoredPosition;
        float startY = pos.y;
        Vector2 originalScale = inventoryStorePanel.localScale;
        float elapsed = 0f;
        while (elapsed < squashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / squashDuration;
            float scaleY = Mathf.Lerp(1f, 1f - squashAmount / inventoryStorePanel.rect.height, t);
            float scaleX = Mathf.Lerp(1f, 1f + squashAmount / inventoryStorePanel.rect.height * 0.5f, t);
            inventoryStorePanel.localScale = new Vector2(scaleX, scaleY);
            pos.y = startY + squashAmount * 0.3f * t;
            inventoryStorePanel.anchoredPosition = pos;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            t = t * t * t;
            pos.y = Mathf.Lerp(startY, targetY, t);
            inventoryStorePanel.localScale = Vector2.Lerp(
                new Vector2(1f + squashAmount / inventoryStorePanel.rect.height * 0.5f, 1f - squashAmount / inventoryStorePanel.rect.height),
                Vector2.one, t);
            inventoryStorePanel.anchoredPosition = pos;
            GridManager.Instance.RepositionAllItems();
            yield return null;
        }
        inventoryStorePanel.localScale = originalScale;
        pos.y = targetY;
        inventoryStorePanel.anchoredPosition = pos;
        elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float decay = 1f - (elapsed / shakeDuration);
            float shake = Mathf.Sin(elapsed * 40f) * shakeAmount * decay;
            pos.y = targetY + shake;
            inventoryStorePanel.anchoredPosition = pos;
            GridManager.Instance.RepositionAllItems();
            yield return null;
        }
        float currentBounceHeight = bounceHeight;
        for (int i = 0; i < bounceCount; i++)
        {
            float bounceStart = pos.y;
            float bounceTarget = targetY - currentBounceHeight;
            elapsed = 0f;
            float halfBounce = bounceDuration * 0.5f;
            while (elapsed < halfBounce)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfBounce;
                pos.y = Mathf.Lerp(bounceStart, bounceTarget, t * t);
                inventoryStorePanel.anchoredPosition = pos;
                GridManager.Instance.RepositionAllItems();
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < halfBounce)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfBounce;
                t = 1f - (1f - t) * (1f - t);
                pos.y = Mathf.Lerp(bounceTarget, targetY, t);
                inventoryStorePanel.anchoredPosition = pos;
                GridManager.Instance.RepositionAllItems();
                yield return null;
            }
            currentBounceHeight *= 0.4f;
            bounceDuration *= 0.7f;
        }
        pos.y = targetY;
        inventoryStorePanel.anchoredPosition = pos;
        inventoryStorePanel.localScale = originalScale;
        GridManager.Instance.RepositionAllItems();
    }

    IEnumerator RaisePanel(float targetY)
    {
        Vector2 pos = inventoryStorePanel.anchoredPosition;
        float startY = pos.y;
        Vector2 originalScale = inventoryStorePanel.localScale;
        float elapsed = 0f;
        while (elapsed < riseAnticipation)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / riseAnticipation;
            float scaleY = Mathf.Lerp(1f, 1f - squashAmount * 0.5f / inventoryStorePanel.rect.height, t);
            float scaleX = Mathf.Lerp(1f, 1f + squashAmount * 0.3f / inventoryStorePanel.rect.height, t);
            inventoryStorePanel.localScale = new Vector2(scaleX, scaleY);
            pos.y = startY - squashAmount * 0.2f * t;
            inventoryStorePanel.anchoredPosition = pos;
            yield return null;
        }
        elapsed = 0f;
        float overshootY = targetY + overshootHeight;
        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / riseDuration;
            t = t * t;
            pos.y = Mathf.Lerp(startY, overshootY, t);
            inventoryStorePanel.localScale = Vector2.Lerp(
                new Vector2(1f + squashAmount * 0.3f / inventoryStorePanel.rect.height, 1f - squashAmount * 0.5f / inventoryStorePanel.rect.height),
                Vector2.one, t);
            inventoryStorePanel.anchoredPosition = pos;
            GridManager.Instance.RepositionAllItems();
            yield return null;
        }
        elapsed = 0f;
        float settleDuration = bounceDuration;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / settleDuration;
            float decay = Mathf.Exp(-t * 5f);
            float oscillation = Mathf.Sin(t * Mathf.PI * 3f) * overshootHeight * 0.5f * decay;
            pos.y = targetY + oscillation;
            inventoryStorePanel.anchoredPosition = pos;
            GridManager.Instance.RepositionAllItems();
            yield return null;
        }
        pos.y = targetY;
        inventoryStorePanel.anchoredPosition = pos;
        inventoryStorePanel.localScale = originalScale;
        GridManager.Instance.RepositionAllItems();
        if (battleArea != null) battleArea.gameObject.SetActive(false);
        if (enemyHealthPanel != null) enemyHealthPanel.SetActive(false);
    }

    IEnumerator SpawnEnemyWithAnimation(EnemyData data)
    {
        if (enemyPrefab == null || battleArea == null) yield break;
        GameObject enemyObj = Instantiate(enemyPrefab, battleArea);
        enemyObj.transform.localPosition = Vector3.zero;
        Enemy newEnemy = enemyObj.GetComponent<Enemy>();
        if (newEnemy != null)
        {
            newEnemy.Init(data.enemyName, data.hp, data.damage, data.attackSpeed, data.sprite);
            newEnemy.OnEnemyDamaged += UpdateEnemyHealthUI;
            currentEnemy = newEnemy;
            currentEnemy.OnEnemyDeath += OnEnemyKilled;
            UpdateEnemyHealthUI();
            yield return StartCoroutine(newEnemy.SpawnAnimation());
            if (currentEnemy != null && currentEnemy == newEnemy && currentEnemy.IsAlive())
                enemyAttackTimer = currentEnemy.attackSpeed * 0.3f;
            else Debug.LogWarning("Враг уничтожен до завершения анимации появления");
        }
    }
    
    IEnumerator SpawnLevel(int level)
    {
        isBattling = true;
        foreach (Transform child in battleArea) Destroy(child.gameObject);
        LocationData loc = GetCurrentLocation();
        if (loc == null) yield break;
        int enemyCount = GetEnemyCountForLevel(level);
        bool isBoss = IsBossLevel(level);
        enemiesDefeated = 0; totalEnemiesOnLevel = enemyCount;
        Debug.Log($"Локация: {loc.locationName}, Уровень: {level}, Врагов: {enemyCount}, Босс: {isBoss}");
        enemyQueue.Clear();
        for (int i = 0; i < enemyCount; i++)
        {
            EnemyData data = isBoss && i == 0 ? GetRandomBoss(loc) : GetRandomEnemy(loc);
            if (data != null) enemyQueue.Enqueue(data);
        }
        if (enemyQueue.Count > 0)
        {
            EnemyData firstData = enemyQueue.Dequeue();
            yield return StartCoroutine(SpawnEnemyWithAnimation(firstData));
        }
        InitItemTimers();
        StartCoroutine(AutoBattle());
    }
    
    void SpawnNextEnemy()
    {
        if (enemyQueue.Count == 0) return;
        EnemyData data = enemyQueue.Dequeue();
        StartCoroutine(SpawnEnemyWithAnimation(data));
    }
    
    IEnumerator AutoBattle()
    {
        while (isBattling && !levelComplete)
        {
            if (currentEnemy == null || !currentEnemy.IsAlive()) { yield return null; continue; }
            float deltaTime = Time.deltaTime;
            
            // Реген HP и брони
            playerCurrentHP = Mathf.Min(playerCurrentHP + totalHPRegen * deltaTime, playerMaxHP);
            if (playerCurrentArmor > 0)
                playerCurrentArmor = Mathf.Min(playerCurrentArmor + totalArmorRegen * deltaTime, playerMaxArmor);
            
            // Реген и расход маны
            float manaRegenThisFrame = totalManaRegen * deltaTime;
            float manaCostThisFrame = totalManaCostPerSecond * deltaTime;
            playerCurrentMana += manaRegenThisFrame - manaCostThisFrame;
            playerCurrentMana = Mathf.Clamp(playerCurrentMana, 0, playerMaxMana);
            
            UpdatePlayerHealthUI();

            List<Item> keys = new List<Item>(itemTimers.Keys);
            foreach (Item item in keys)
            {
                if (item == null) { itemTimers.Remove(item); continue; }
                float speed = modifiedSpeed.ContainsKey(item) ? modifiedSpeed[item] : item.baseAttackSpeed;
                float timer = itemTimers[item] - deltaTime;
                if (timer <= 0f)
                {
                    Enemy enemyRef = currentEnemy;
                    if (enemyRef != null && enemyRef.IsAlive() && item.baseDamage > 0)
                    {
                        // Проверка маны для атаки
                        bool canAttack = true;
                        if (item.baseIsMagical && item.baseManaCostPerAttack > 0)
                        {
                            if (playerCurrentMana >= item.baseManaCostPerAttack)
                            {
                                playerCurrentMana -= item.baseManaCostPerAttack;
                            }
                            else
                            {
                                canAttack = false;
                            }
                        }

                        if (canAttack)
                        {
                            float dmg = modifiedDamage.ContainsKey(item) ? modifiedDamage[item] : item.baseDamage;
                            float critChance = modifiedCrit.ContainsKey(item) ? modifiedCrit[item] : item.baseCritChance;
                            bool crit = Random.value * 100f < critChance;
                            if (crit)
                            {
                                dmg *= (item.baseCritDamage / 100f);
                                SpawnFloatingIcon(item, statIconsData?.critDamageIcon);
                            }

                            if (item.baseBleedDamage > 0)
                            {
                                enemyRef.AddBleedStack(item.baseBleedDamage);
                                dmg += enemyRef.GetBleedDamage();
                            }

                            if (item.baseExecuteThreshold > 0 && enemyRef.CurrentHP / enemyRef.MaxHP * 100f <= item.baseExecuteThreshold)
                            {
                                dmg = enemyRef.CurrentHP;
                                SpawnFloatingIcon(item, statIconsData?.guillotineIcon);
                            }

                            if (item.basePoisonDamage > 0 && item.itemData.poisonDuration > 0)
                                enemyRef.ApplyPoison(item.basePoisonDamage, item.itemData.poisonDuration);
                            if (item.baseBurnDamage > 0 && item.itemData.burnDuration > 0)
                                enemyRef.ApplyBurn(item.baseBurnDamage, item.itemData.burnDuration);
                            if (item.baseSlowPercent > 0)
                                enemyRef.ApplySlow(item.baseSlowPercent);
                            if (item.baseStunChance > 0 && Random.value * 100f < item.baseStunChance)
                            {
                                enemyRef.ApplyStun();
                                SpawnFloatingIcon(item, statIconsData?.stunIcon);
                            }

                            item.PlayAttackAnimation();
                            SpawnProjectileAndDamage(item, dmg, enemyRef.enemyName);

                            if (item.baseExtraProjectileChance > 0 && Random.value * 100f < item.baseExtraProjectileChance)
                            {
                                StartCoroutine(FireExtraProjectile(item, dmg));
                                SpawnFloatingIcon(item, statIconsData?.extraProjectileIcon);
                            }
                        }
                    }
                    timer = speed;
                }
                itemTimers[item] = timer;
            }

            Enemy enemyAtkRef = currentEnemy;
            if (enemyAtkRef != null && enemyAtkRef.IsAlive() && !enemyAtkRef.IsStunned)
            {
                enemyAttackTimer -= deltaTime;
                if (enemyAttackTimer <= 0f)
                {
                    float enemyDmg = enemyAtkRef.GetDamage();
                    DamagePlayer(enemyDmg);
                    ApplyThornsDamage(enemyDmg);
                    UpdatePlayerHealthUI();
                    if (playerCurrentHP <= 0) { playerCurrentHP = 0; UpdatePlayerHealthUI(); OnPlayerDefeated(); yield break; }
                    enemyAttackTimer = enemyAtkRef.GetModifiedAttackSpeed();
                }
            }
            yield return null;
        }
    }

    IEnumerator FireExtraProjectile(Item item, float damage)
    {
        yield return new WaitForSeconds(0.1f);
        Enemy enemyRef = currentEnemy;
        if (enemyRef != null && enemyRef.IsAlive())
            SpawnProjectileAndDamage(item, damage, enemyRef.enemyName);
    }

    void ApplyThornsDamage(float incomingDamage)
    {
        float totalThorns = 0;
        foreach (var item in playerItems)
            if (item?.itemData != null) totalThorns += item.baseThornsDamage;
        Enemy enemyRef = currentEnemy;
        if (totalThorns > 0 && enemyRef != null)
            enemyRef.TakeDamage(totalThorns);
    }
    
    public float GetModifiedDamage(Item item)
    {
        if (modifiedDamage.ContainsKey(item)) return modifiedDamage[item];
        return item.baseDamage;
    }

    public float GetModifiedSpeed(Item item)
    {
        if (modifiedSpeed.ContainsKey(item)) return modifiedSpeed[item];
        return item.baseAttackSpeed;
    }

    public float GetKillBonus(Item item)
    {
        if (killBonuses.ContainsKey(item)) return killBonuses[item];
        return 0;
    }

    void OnEnemyKilled(Enemy enemy)
    {
        Debug.Log($"{enemy.enemyName} убит!");
        enemiesDefeated++;

        foreach (var item in playerItems)
        {
            if (item?.itemData != null && item.baseDamagePerKillInBattle > 0)
            {
                if (modifiedDamage.ContainsKey(item))
                    modifiedDamage[item] += item.baseDamagePerKillInBattle;
                if (killBonuses.ContainsKey(item))
                    killBonuses[item] += item.baseDamagePerKillInBattle;
            }
        }

        int killCoins = GetTotalCoinsPerKill();
        if (killCoins > 0)
        {
            GridManager.Instance.coins += killCoins;
            GridManager.Instance.UpdateCoinsUI();
        }

        if (currentEnemy != null) currentEnemy.OnEnemyDamaged -= UpdateEnemyHealthUI;
        currentEnemy = null;
        if (enemyQueue.Count > 0) { Debug.Log($"Следующий враг! Осталось: {enemyQueue.Count}"); StartCoroutine(SpawnNextWithDelay(0.5f)); }
        else if (enemiesDefeated >= totalEnemiesOnLevel) { levelComplete = true; StartCoroutine(DelayedVictory()); }
        else UpdateEnemyHealthUI();
    }
    
    private int GetTotalCoinsPerKill()
    {
        int total = 0;
        foreach (var item in playerItems)
            if (item?.itemData != null) total += item.baseCoinsPerKill;
        return total;
    }

    IEnumerator SpawnNextWithDelay(float delay) { yield return new WaitForSeconds(delay); if (!levelComplete) SpawnNextEnemy(); }

    IEnumerator DelayedVictory() { yield return new WaitForSeconds(0.3f); OnAllEnemiesDefeated(); }
    
    void OnAllEnemiesDefeated()
    {
        isBattling = false; currentEnemy = null; InfoPanel.Instance?.Hide();
        int coinsEarned = 10 + GetTotalCoinsPerLevel();
        GridManager.Instance.coins += coinsEarned; GridManager.Instance.UpdateCoinsUI();
        
        // Сброс к базовым значениям
        playerMaxHP = basePlayerHP;
        playerCurrentHP = basePlayerHP;
        playerMaxArmor = basePlayerArmor;
        playerCurrentArmor = basePlayerArmor;

        // Добавляем бонусы от предметов из инвентаря
        foreach (var item in GridManager.Instance.inventoryItems)
        {
            if (item != null)
            {
                playerMaxHP += item.baseBonusHP;
                playerCurrentHP += item.baseBonusHP;
                playerMaxArmor += item.baseBonusArmor;
                playerCurrentArmor += item.baseBonusArmor;
            }
        }

        UpdatePlayerHealthUI();
        UpdateEnemyHealthUI();
        foreach (Transform child in battleArea) Destroy(child.gameObject);
        if (inventoryStorePanel != null) { if (panelAnimationCoroutine != null) StopCoroutine(panelAnimationCoroutine); panelAnimationCoroutine = StartCoroutine(RaisePanel(0f)); }
        currentLevel++; UpdateLevelUI();
        if (currentLevel > 1 && (currentLevel - 1) % 20 == 0) { currentLocationIndex++; UpdateBackground(); }
        if (ShopManager.Instance != null) ShopManager.Instance.RefreshShop();
        Debug.Log($"Победа! +{coinsEarned} монет. Уровень {currentLevel}");
    }

    private int GetTotalCoinsPerLevel() { int total = 0; foreach (var item in playerItems) if (item?.itemData != null) total += item.baseCoinsPerLevel; return total; }
    
    void OnPlayerDefeated()
    {
        isBattling = false; levelComplete = true; currentEnemy = null; InfoPanel.Instance?.Hide();
        UpdateEnemyHealthUI();
        foreach (Transform child in battleArea) Destroy(child.gameObject);
        if (inventoryStorePanel != null) { if (panelAnimationCoroutine != null) StopCoroutine(panelAnimationCoroutine); panelAnimationCoroutine = StartCoroutine(RaisePanel(0f)); }
        Debug.Log("Поражение!");
    }
    
    int GetEnemyCountForLevel(int level) { int lic = ((level-1)%20)+1; if (lic<=2) return 1; if (lic<=4) return 2; if (lic==5) return 1; if (lic<=7) return 3; if (lic<=9) return 4; if (lic==10) return 1; if (lic<=12) return 5; if (lic<=14) return 6; if (lic==15) return 1; if (lic<=17) return 7; if (lic<=19) return 8; return 1; }
    bool IsBossLevel(int level) { int lic = ((level-1)%20)+1; return lic == 5 || lic == 10 || lic == 15 || lic == 20; }
    EnemyData GetRandomEnemy(LocationData loc) { if (loc.enemies.Count==0) return null; return loc.enemies[Random.Range(0, loc.enemies.Count)]; }
    EnemyData GetRandomBoss(LocationData loc) { if (loc.bosses.Count==0) return null; return loc.bosses[Random.Range(0, loc.bosses.Count)]; }
}