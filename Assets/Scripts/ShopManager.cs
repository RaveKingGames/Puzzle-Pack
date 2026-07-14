using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ShopItem
{
    public ItemData itemData;
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    [Header("Префаб карточки")] public GameObject cardPrefab;
    [Header("Контейнер карточек")] public RectTransform shopContainer;
    [Header("Префаб панели информации")] public GameObject infoPanelPrefab;
    [Header("Canvas")] public Canvas canvas;
    [Header("Список предметов")] public AllItemsData allItemsData;
    [Header("Настройки магазина")] public int baseCardsCount = 3;
    public int maxCardsCount = 6;
    public int rerollCost = 1;
    [Header("Кнопка реролла")] public Button rerollButton;
    [Header("Текст цены реролла")] public Text rerollCostText;

    private int currentCardsCount;
    private List<GameObject> spawnedCards = new List<GameObject>();
    private bool nextRerollFree = false;

    void Awake()
    {
        Instance = this;
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (InfoPanel.Instance == null && infoPanelPrefab != null && canvas != null)
        {
            GameObject infoObj = Instantiate(infoPanelPrefab, canvas.transform);
            InfoPanel infoPanel = infoObj.GetComponent<InfoPanel>();
            if (infoPanel != null) infoPanel.Init(canvas);
        }
    }

    void Start()
    {
        currentCardsCount = baseCardsCount;
        if (rerollButton != null) rerollButton.onClick.AddListener(RerollShop);
        UpdateRerollText();
        RefreshShop();
    }

    void UpdateRerollText()
    {
        if (rerollCostText != null)
        {
            float freeChance = GetTotalFreeRerollChance();
            nextRerollFree = UnityEngine.Random.value * 100f < freeChance;
            rerollCostText.text = nextRerollFree ? "0" : rerollCost.ToString();
            rerollCostText.color = Color.white;
        }
    }

    public void RerollShop()
    {
        if (GridManager.Instance == null) return;

        int cost = nextRerollFree ? 0 : rerollCost;

        if (GridManager.Instance.coins >= cost)
        {
            GridManager.Instance.coins -= cost;
            GridManager.Instance.UpdateCoinsUI();
            RefreshShop();
            UpdateRerollText();
        }
    }

    private float GetTotalFreeRerollChance()
    {
        float total = 0;
        foreach (var item in GridManager.Instance.inventoryItems)
        {
            if (item != null) total += item.baseFreeRerollChance;
        }
        return Mathf.Min(total, 100);
    }

    public void RefreshShop()
    {
        foreach (var card in spawnedCards) Destroy(card);
        spawnedCards.Clear();
        if (allItemsData == null || allItemsData.items.Count == 0) return;

        int cardsToSpawn = currentCardsCount + GetExtraCardsCount();
        cardsToSpawn = Mathf.Min(cardsToSpawn, maxCardsCount);

        for (int i = 0; i < cardsToSpawn; i++)
        {
            ItemData randomItem = allItemsData.items[UnityEngine.Random.Range(0, allItemsData.items.Count)];
            SpawnCard(randomItem);
        }
    }

    private int GetExtraCardsCount()
    {
        int extra = 0;
        foreach (var item in GridManager.Instance.inventoryItems)
        {
            if (item != null) extra += item.baseExtraShopCards;
        }
        return extra;
    }

    void SpawnCard(ItemData data)
    {
        GameObject cardObj = Instantiate(cardPrefab, shopContainer);
        ShopCard card = cardObj.GetComponent<ShopCard>();
        if (card != null) card.Setup(data);
        spawnedCards.Add(cardObj);
    }

    public void RemoveCard(GameObject cardObj)
    {
        spawnedCards.Remove(cardObj);
        Destroy(cardObj);
    }

    public void UpdateAllCardPrices()
    {
        float discount = GetTotalShopDiscount();
        foreach (var cardObj in spawnedCards)
        {
            ShopCard card = cardObj.GetComponent<ShopCard>();
            if (card != null) card.UpdatePrice(discount);
        }
    }

    private float GetTotalShopDiscount()
    {
        float total = 0;
        foreach (var item in GridManager.Instance.inventoryItems)
        {
            if (item != null) total += item.baseShopDiscount;
        }
        return Mathf.Min(total, 90);
    }
}