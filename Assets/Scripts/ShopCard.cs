using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image iconImage;
    public Text priceText;
    public RectTransform priceContainer;

    [Header("Префаб предмета")]
    public GameObject itemPrefab;

    private ItemData itemData;
    private GameObject draggedItem;
    private Canvas canvas;
    private CanvasGroup cardCanvasGroup;
    private float baseX;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        cardCanvasGroup = GetComponent<CanvasGroup>();
        if (cardCanvasGroup == null) cardCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (priceContainer != null) baseX = priceContainer.localPosition.x;
    }

    public void Setup(ItemData data)
    {
        itemData = data;
        UpdatePrice(GetCurrentDiscount());
        if (iconImage != null)
        {
            Sprite icon = Resources.Load<Sprite>($"Items/{itemData.folderName}/icon");
            if (icon != null) iconImage.sprite = icon;
        }
    }

    public void UpdatePrice(float discountPercent)
    {
        if (itemData == null) return;
        int originalPrice = itemData.price;
        int finalPrice = Mathf.Max(1, Mathf.RoundToInt(originalPrice * (1 - discountPercent / 100f)));

        if (priceText != null)
        {
            if (discountPercent > 0)
                priceText.text = $"{finalPrice} (-{discountPercent}%)";
            else
                priceText.text = originalPrice.ToString();

            if (priceContainer != null)
            {
                int charCount = priceText.text.Length;
                Vector3 pos = priceContainer.localPosition;
                pos.x = baseX - 10f * (charCount - 1);
                priceContainer.localPosition = pos;
            }
        }
    }

    private float GetCurrentDiscount()
    {
        float total = 0;
        if (GridManager.Instance != null)
        {
            foreach (var item in GridManager.Instance.inventoryItems)
            {
                if (item != null) total += item.baseShopDiscount;
            }
        }
        return Mathf.Min(total, 90);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemData != null && InfoPanel.Instance != null)
            InfoPanel.Instance.Show(itemData, GetComponent<RectTransform>());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        InfoPanel.Instance?.Hide();
        if (itemData == null || itemPrefab == null) return;

        cardCanvasGroup.alpha = 0.5f;
        cardCanvasGroup.blocksRaycasts = false;

        draggedItem = Instantiate(itemPrefab, canvas.transform);
        Item item = draggedItem.GetComponent<Item>();
        if (item != null)
        {
            item.itemData = itemData;
            item.ApplyItemData();

            if (GridManager.Instance != null && GridManager.Instance.InventoryContainer != null)
                item.transform.localScale = GridManager.Instance.InventoryContainer.lossyScale;

            item.RefreshOccupiedCells();
            item.grabbedCell = new Vector2Int(2, 2);

            Vector2 cellLocal = item.GetCellLocalPosition(item.grabbedCell);
            RectTransform itemRect = draggedItem.GetComponent<RectTransform>();
            itemRect.position = eventData.position - cellLocal;

            CanvasGroup cg = draggedItem.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 0f; cg.blocksRaycasts = false; }
            draggedItem.transform.SetAsLastSibling();

            StartCoroutine(ShowDraggedItem(cg));
        }
        else
        {
            Destroy(draggedItem);
            cardCanvasGroup.alpha = 1f;
            cardCanvasGroup.blocksRaycasts = true;
        }
    }

    System.Collections.IEnumerator ShowDraggedItem(CanvasGroup cg) { yield return null; if (cg != null) cg.alpha = 0.7f; }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem == null) return;
        Item item = draggedItem.GetComponent<Item>();
        if (item == null) return;

        Vector2 cellLocal = item.GetCellLocalPosition(item.grabbedCell);
        RectTransform itemRect = draggedItem.GetComponent<RectTransform>();
        itemRect.position = eventData.position - cellLocal;

        if (GridManager.Instance != null)
        {
            bool overShop = GridManager.Instance.IsOverShop(eventData.position);
            if (!overShop)
            {
                Vector2Int invCell = GridManager.Instance.GetClosestInventoryCell(eventData.position);
                bool canPlace = GridManager.Instance.CanPlaceItem(item, invCell.x, invCell.y);
                GridManager.Instance.HighlightInventorySlots(item, invCell.x, invCell.y, canPlace);
            }
            else
            {
                GridManager.Instance.ClearInventoryHighlight();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cardCanvasGroup.alpha = 1f;
        cardCanvasGroup.blocksRaycasts = true;

        if (draggedItem == null) return;

        // Восстанавливаем прозрачность и возможность клика у перетаскиваемого предмета
        CanvasGroup cg = draggedItem.GetComponent<CanvasGroup>();
        if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; }

        bool placed = false;
        if (GridManager.Instance != null)
        {
            bool overShop = GridManager.Instance.IsOverShop(eventData.position);
            if (!overShop)
            {
                Item item = draggedItem.GetComponent<Item>();
                if (item != null)
                {
                    Vector2Int invCell = GridManager.Instance.GetClosestInventoryCell(eventData.position);
                    if (GridManager.Instance.CanPlaceItem(item, invCell.x, invCell.y))
                    {
                        if (GridManager.Instance.coins >= item.price)
                        {
                            GridManager.Instance.BuyItem(item);
                            GridManager.Instance.PlaceItem(item, invCell.x, invCell.y);
                            ShopManager.Instance.RemoveCard(gameObject);
                            ShopManager.Instance.UpdateAllCardPrices();
                            placed = true;
                        }
                    }
                }
            }
        }

        GridManager.Instance?.ClearInventoryHighlight();
        if (!placed && draggedItem != null) Destroy(draggedItem);
    }
}