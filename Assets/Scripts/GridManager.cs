using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Ячейки инвентаря 5x5 (25 штук)")]
    public GameObject[] inventorySlots = new GameObject[25];

    [Header("Магазин (для IsOverShop)")]
    public RectTransform shopContainer;

    [Header("Валюта")]
    public int coins = 10;
    public Text coinsText;

    private Item[,] inventoryGrid;
    public List<Item> inventoryItems = new List<Item>();
    public Item[,] InventoryGrid => inventoryGrid;
    private Canvas parentCanvas;
    private Transform inventoryContainer;
    private float cellSize;

    public Transform InventoryContainer => inventoryContainer;

    void Awake()
    {
        Instance = this;
        inventoryGrid = new Item[5, 5];
        parentCanvas = GetComponentInParent<Canvas>();

        if (inventorySlots.Length > 0 && inventorySlots[0] != null)
        {
            inventoryContainer = inventorySlots[0].transform.parent;
            GridLayoutGroup gridLayout = inventoryContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
                cellSize = gridLayout.cellSize.x;
            else
                cellSize = inventorySlots[0].GetComponent<RectTransform>().rect.width;
        }
    }

    void Start()
    {
        UpdateCoinsUI();
        UpdateStatsPreview();
    }

    void SetWorldPositionFromSlot(GameObject itemObj, Vector2Int grabbedCell, GameObject slot, bool isInventory)
    {
        Vector3 containerScale = inventoryContainer != null ? inventoryContainer.lossyScale : Vector3.one;
        float realCellW = cellSize * containerScale.x;
        float realCellH = cellSize * containerScale.y;

        float centerX = 2f;
        float centerY = 2f;
        float offsetX = (grabbedCell.x - centerX) * realCellW;
        float offsetY = (centerY - grabbedCell.y) * realCellH;

        itemObj.transform.position = slot.transform.position - new Vector3(offsetX, offsetY, 0);
    }

    public void RepositionAllItems()
    {
        Vector3 invScale = inventoryContainer != null ? inventoryContainer.lossyScale : Vector3.one;
        foreach (var item in inventoryItems)
        {
            if (item == null || item.isDragging) continue;
            Vector2Int pos = item.positionInGrid;
            int slotIndex = pos.y * 5 + pos.x;
            SetWorldPositionFromSlot(item.gameObject, item.grabbedCell, inventorySlots[slotIndex], true);
            item.SetVisualScale(invScale);
        }
    }

    public Vector2Int GetClosestInventoryCell(Vector2 screenPoint)
    {
        float minDist = float.MaxValue;
        int closestX = 0, closestY = 0;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                int index = y * 5 + x;
                if (inventorySlots[index] != null)
                {
                    float dist = Vector2.Distance(screenPoint, inventorySlots[index].transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestX = x;
                        closestY = y;
                    }
                }
            }
        }
        return new Vector2Int(closestX, closestY);
    }

    private Item GetMergeTarget(Item draggedItem, int targetX, int targetY)
    {
        Item mergeTarget = null;
        foreach (var cell in draggedItem.occupiedCells)
        {
            int checkX = targetX + (cell.x - draggedItem.grabbedCell.x);
            int checkY = targetY + (cell.y - draggedItem.grabbedCell.y);
            if (checkX < 0 || checkX >= 5 || checkY < 0 || checkY >= 5) return null;
            Item occupant = inventoryGrid[checkX, checkY];
            if (occupant == null || occupant == draggedItem) return null;
            if (mergeTarget == null)
                mergeTarget = occupant;
            else if (mergeTarget != occupant) return null;
        }
        return mergeTarget;
    }

    public bool CanPlaceItem(Item item, int targetX, int targetY)
    {
        if (item == null) return false;

        Item mergeTarget = GetMergeTarget(item, targetX, targetY);
        if (mergeTarget != null)
        {
            if (item.CanMergeWith(mergeTarget))
            {
                Debug.Log($"[CanPlaceItem] Можно слить с {mergeTarget.itemName}");
                return true;
            }
            else
            {
                Debug.Log($"[CanPlaceItem] Нельзя слить с {mergeTarget.itemName}");
                return false;
            }
        }

        foreach (var cell in item.occupiedCells)
        {
            int checkX = targetX + (cell.x - item.grabbedCell.x);
            int checkY = targetY + (cell.y - item.grabbedCell.y);
            if (checkX < 0 || checkX >= 5 || checkY < 0 || checkY >= 5)
            {
                Debug.Log($"[CanPlaceItem] Выход за границы: {checkX},{checkY}");
                return false;
            }
            Item occupant = inventoryGrid[checkX, checkY];
            if (occupant != null && occupant != item)
            {
                Debug.Log($"[CanPlaceItem] Клетка {checkX},{checkY} занята {occupant.itemName}");
                return false;
            }
        }
        Debug.Log("[CanPlaceItem] Можно разместить");
        return true;
    }

    public void PlaceItem(Item item, int targetX, int targetY)
    {
        Item mergeTarget = GetMergeTarget(item, targetX, targetY);
        if (mergeTarget != null && item.CanMergeWith(mergeTarget))
        {
            mergeTarget.MergeWith(item);
            RemoveFromInventory(item);
            Destroy(item.gameObject);
            Debug.Log($"Слияние! Новый уровень: {mergeTarget.itemLevel}");
            BattleManager.Instance?.RecalculateBattleStats();
            ShopManager.Instance?.UpdateAllCardPrices();
            UpdateStatsPreview();
            return;
        }

        RemoveFromInventory(item);
        Vector2Int grabbed = item.grabbedCell;
        foreach (var cell in item.occupiedCells)
        {
            int posX = targetX + (cell.x - grabbed.x);
            int posY = targetY + (cell.y - grabbed.y);
            if (posX >= 0 && posX < 5 && posY >= 0 && posY < 5)
                inventoryGrid[posX, posY] = item;
        }
        item.positionInGrid = new Vector2Int(targetX, targetY);
        int slotIndex = targetY * 5 + targetX;
        SetWorldPositionFromSlot(item.gameObject, grabbed, inventorySlots[slotIndex], true);

        Vector3 invScale = inventoryContainer != null ? inventoryContainer.lossyScale : Vector3.one;
        item.SetVisualScale(invScale);
        if (!inventoryItems.Contains(item)) inventoryItems.Add(item);
        ShopManager.Instance?.UpdateAllCardPrices();
        UpdateStatsPreview();
    }

    public void RemoveFromInventory(Item item)
    {
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
                if (inventoryGrid[x, y] == item)
                    inventoryGrid[x, y] = null;
        inventoryItems.Remove(item);
        ShopManager.Instance?.UpdateAllCardPrices();
        UpdateStatsPreview();
    }

    public bool BuyItem(Item item)
    {
        if (item == null) return false;
        int finalPrice = GetDiscountedPrice(item.price);
        if (coins >= finalPrice)
        {
            coins -= finalPrice;
            UpdateCoinsUI();
            return true;
        }
        else
        {
            Debug.Log($"Недостаточно монет! Нужно {finalPrice}, есть {coins}");
            return false;
        }
    }

    public void SellItem(Item item)
    {
        if (item == null) return;
        int sellPrice = GetSellPrice(item.price);
        coins += sellPrice;
        UpdateCoinsUI();
        Debug.Log($"Продано: {item.itemName} за {sellPrice} монет");
        RemoveFromInventory(item);
        Destroy(item.gameObject);
        ShopManager.Instance?.UpdateAllCardPrices();
    }

    private int GetDiscountedPrice(int basePrice)
    {
        float discount = GetTotalShopDiscount();
        return Mathf.Max(1, Mathf.RoundToInt(basePrice * (1 - discount / 100f)));
    }

    private int GetSellPrice(int basePrice)
    {
        if (HasFullPriceSell()) return basePrice;
        return basePrice / 2;
    }

    private float GetTotalShopDiscount()
    {
        float total = 0;
        foreach (var item in inventoryItems)
        {
            if (item != null) total += item.baseShopDiscount;
        }
        return Mathf.Min(total, 90);
    }

    private bool HasFullPriceSell()
    {
        foreach (var item in inventoryItems)
        {
            if (item != null && item.baseFullPriceSell) return true;
        }
        return false;
    }

    public bool IsOverShop(Vector2 screenPoint)
    {
        if (shopContainer == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(shopContainer, screenPoint);
    }

    public void UpdateCoinsUI()
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    public void HighlightInventorySlots(Item item, int targetX, int targetY, bool canPlace)
    {
        ClearInventoryHighlight();
        Color green = canPlace ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
        Color blue = new Color(0.3f, 0.5f, 1f, 0.35f);

        foreach (var cell in item.occupiedCells)
        {
            int cx = targetX + (cell.x - item.grabbedCell.x);
            int cy = targetY + (cell.y - item.grabbedCell.y);
            if (cx >= 0 && cx < 5 && cy >= 0 && cy < 5)
            {
                int idx = cy * 5 + cx;
                Image img = inventorySlots[idx].GetComponent<Image>();
                if (img != null) img.color = green;
            }
        }

        if (item.effectCells != null)
        {
            foreach (var effCell in item.effectCells)
            {
                int ex = targetX + (effCell.x - item.grabbedCell.x);
                int ey = targetY + (effCell.y - item.grabbedCell.y);
                if (ex >= 0 && ex < 5 && ey >= 0 && ey < 5)
                {
                    int idx = ey * 5 + ex;
                    bool isOccupied = false;
                    foreach (var cell in item.occupiedCells)
                    {
                        int ox = targetX + (cell.x - item.grabbedCell.x);
                        int oy = targetY + (cell.y - item.grabbedCell.y);
                        if (ex == ox && ey == oy) { isOccupied = true; break; }
                    }
                    if (!isOccupied)
                    {
                        Image img = inventorySlots[idx].GetComponent<Image>();
                        if (img != null) img.color = blue;
                    }
                }
            }
        }
    }

    public void ClearInventoryHighlight()
    {
        Color defaultColor = Color.white;
        foreach (var slot in inventorySlots)
        {
            if (slot != null)
            {
                Image img = slot.GetComponent<Image>();
                if (img != null) img.color = defaultColor;
            }
        }
    }

    public List<Item> GetNeighbors(Item item)
    {
        List<Item> neighbors = new List<Item>();
        Vector2Int pos = item.positionInGrid;
        foreach (var cell in item.occupiedCells)
        {
            int posX = pos.x + (cell.x - item.grabbedCell.x);
            int posY = pos.y + (cell.y - item.grabbedCell.y);
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in dirs)
            {
                int nx = posX + dir.x;
                int ny = posY + dir.y;
                if (nx >= 0 && nx < 5 && ny >= 0 && ny < 5)
                {
                    Item neighbor = inventoryGrid[nx, ny];
                    if (neighbor != null && neighbor != item && !neighbors.Contains(neighbor))
                        neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
    }

    public void UpdateStatsPreview()
    {
        float totalHP = BattleManager.Instance != null ? BattleManager.Instance.basePlayerHP : 50f;
        float totalArmor = 0f;

        foreach (var item in inventoryItems)
        {
            if (item != null)
            {
                totalHP += item.baseBonusHP;
                totalArmor += item.baseBonusArmor;
            }
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.UpdatePreviewStats(totalHP, totalArmor);
        }
    }
}