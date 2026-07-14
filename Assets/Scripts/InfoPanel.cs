using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InfoPanel : MonoBehaviour, IPointerClickHandler
{
    public static InfoPanel Instance;

    [Header("UI")]
    public GameObject panel;
    public Text nameText;
    public Text typeText;
    public Text descriptionText;
    public Text levelText;
    public Image iconImage;          // << НОВОЕ
    public Button closeButton;

    [Header("Статы")]
    public GameObject statRowPrefab;
    public Transform statContainer;
    public StatIconsData statIconsData;

    private RectTransform panelRect;
    private RectTransform canvasRect;
    private Canvas canvas;
    private List<GameObject> spawnedRows = new List<GameObject>();

    private void Awake() { Instance = this; }

    public void Init(Canvas parentCanvas)
    {
        canvas = parentCanvas;
        canvasRect = parentCanvas.transform as RectTransform;
        panelRect = panel.GetComponent<RectTransform>();
        panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    private float GetPanelHeightForRowCount(int rowCount)
    {
        if (rowCount <= 0) return 420f;
        if (rowCount <= 2) return 530f;
        if (rowCount <= 4) return 640f;
        if (rowCount <= 6) return 750f;
        if (rowCount <= 8) return 860f;
        if (rowCount <= 10) return 970f;
        return 1080f;
    }

    private void UpdatePanelHeight()
    {
        if (panelRect == null) return;
        float newHeight = GetPanelHeightForRowCount(spawnedRows.Count);
        Vector2 size = panelRect.sizeDelta;
        size.y = newHeight;
        panelRect.sizeDelta = size;
    }

    private void LoadIcon(string folderName)
    {
        if (iconImage == null || string.IsNullOrEmpty(folderName)) return;
        Sprite icon = Resources.Load<Sprite>($"Items/{folderName}/icon");
        if (icon != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    public void Show(string itemName, string description, RectTransform targetRect, int itemLevel = 0)
    {
        nameText.text = itemName;
        descriptionText.text = description;

        Item itemRef = targetRect.GetComponent<Item>();
        if (typeText != null)
        {
            if (itemRef != null && itemRef.itemData != null)
                typeText.text = itemRef.itemData.GetTypeName();
            else
                typeText.text = "";
        }

        if (levelText != null)
        {
            if (itemLevel > 0)
            {
                levelText.text = $"{itemLevel} ур.";
                levelText.gameObject.SetActive(true);
            }
            else
            {
                if (itemRef != null)
                {
                    levelText.text = $"{itemRef.itemLevel} ур.";
                    levelText.gameObject.SetActive(true);
                }
                else levelText.gameObject.SetActive(false);
            }
        }

        // Загружаем иконку
        if (itemRef != null && itemRef.itemData != null)
            LoadIcon(itemRef.itemData.folderName);
        else
            LoadIcon(null);

        foreach (var row in spawnedRows) Destroy(row);
        spawnedRows.Clear();

        if (itemRef != null && statRowPrefab != null && statContainer != null && statIconsData != null)
        {
            var stats = itemRef.GetStats(statIconsData);
            foreach (var (icon, name, value) in stats)
            {
                GameObject row = Instantiate(statRowPrefab, statContainer);
                StatRow statRow = row.GetComponent<StatRow>();
                if (statRow != null) statRow.Setup(icon, name, value);
                spawnedRows.Add(row);
            }
        }

        UpdatePanelHeight();

        transform.SetAsLastSibling();

        Vector2 targetWorldPos;
        if (itemRef != null && GridManager.Instance != null)
        {
            Vector2Int itemPos = itemRef.positionInGrid;
            int slotX = itemPos.x + (2 - itemRef.grabbedCell.x);
            int slotY = itemPos.y + (2 - itemRef.grabbedCell.y);
            slotX = Mathf.Clamp(slotX, 0, 4);
            slotY = Mathf.Clamp(slotY, 0, 4);
            int slotIndex = slotY * 5 + slotX;
            targetWorldPos = GridManager.Instance.inventorySlots[slotIndex].transform.position;
        }
        else targetWorldPos = targetRect.position;

        Vector2 targetLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetWorldPos),
            canvas.worldCamera, out targetLocalPos);

        float panelWidth = panelRect.rect.width * panelRect.lossyScale.x;
        float panelHeight = panelRect.rect.height * panelRect.lossyScale.y;
        float gap = 10f;
        float finalY;

        if (itemRef != null)
        {
            float slotHeight = GridManager.Instance.inventorySlots[0].GetComponent<RectTransform>().rect.height;
            float slotScaleY = GridManager.Instance.inventorySlots[0].transform.lossyScale.y;
            float realSlotHeight = slotHeight * slotScaleY;
            if (BattleManager.Instance != null && BattleManager.Instance.IsBattling)
                finalY = targetLocalPos.y + realSlotHeight / 2 + panelHeight / 2 + gap;
            else
                finalY = targetLocalPos.y - realSlotHeight / 2 - panelHeight / 2 - gap;
        }
        else
        {
            float targetHeight = targetRect.rect.height * targetRect.lossyScale.y;
            float aboveY = targetLocalPos.y + targetHeight / 2 + panelHeight / 2 + gap;
            float belowY = targetLocalPos.y - targetHeight / 2 - panelHeight / 2 - gap;
            float canvasTop = canvasRect.rect.height / 2;
            finalY = (aboveY - panelHeight / 2 <= canvasTop) ? aboveY : belowY;
        }

        panelRect.localPosition = new Vector3(targetLocalPos.x, finalY, 0);

        float leftEdge = panelRect.localPosition.x - panelWidth / 2;
        float rightEdge = panelRect.localPosition.x + panelWidth / 2;
        float canvasLeft = -canvasRect.rect.width / 2;
        float canvasRight = canvasRect.rect.width / 2;
        if (leftEdge < canvasLeft) panelRect.localPosition += Vector3.right * (canvasLeft - leftEdge + gap);
        if (rightEdge > canvasRight) panelRect.localPosition += Vector3.left * (rightEdge - canvasRight + gap);

        panel.SetActive(true);
    }

    public void Show(ItemData data, RectTransform targetRect)
    {
        if (data == null) return;

        nameText.text = data.itemName;
        descriptionText.text = data.GetAutoDescription();

        if (typeText != null) typeText.text = data.GetTypeName();

        if (levelText != null)
        {
            levelText.text = "1 ур.";
            levelText.gameObject.SetActive(true);
        }

        // Загружаем иконку
        LoadIcon(data.folderName);

        foreach (var row in spawnedRows) Destroy(row);
        spawnedRows.Clear();

        if (statRowPrefab != null && statContainer != null && statIconsData != null)
        {
            var stats = data.GetBaseStats(statIconsData);
            foreach (var (icon, name, value) in stats)
            {
                GameObject row = Instantiate(statRowPrefab, statContainer);
                StatRow statRow = row.GetComponent<StatRow>();
                if (statRow != null) statRow.Setup(icon, name, value);
                spawnedRows.Add(row);
            }
        }

        UpdatePanelHeight();

        transform.SetAsLastSibling();

        Vector2 targetWorldPos = targetRect.position;
        Vector2 targetLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, targetWorldPos),
            canvas.worldCamera, out targetLocalPos);

        float panelWidth = panelRect.rect.width * panelRect.lossyScale.x;
        float panelHeight = panelRect.rect.height * panelRect.lossyScale.y;
        float gap = 10f;
        float targetHeight = targetRect.rect.height * targetRect.lossyScale.y;
        float aboveY = targetLocalPos.y + targetHeight / 2 + panelHeight / 2 + gap;
        float belowY = targetLocalPos.y - targetHeight / 2 - panelHeight / 2 - gap;
        float canvasTop = canvasRect.rect.height / 2;
        float finalY = (aboveY - panelHeight / 2 <= canvasTop) ? aboveY : belowY;

        panelRect.localPosition = new Vector3(targetLocalPos.x, finalY, 0);

        float leftEdge = panelRect.localPosition.x - panelWidth / 2;
        float rightEdge = panelRect.localPosition.x + panelWidth / 2;
        float canvasLeft = -canvasRect.rect.width / 2;
        float canvasRight = canvasRect.rect.width / 2;
        if (leftEdge < canvasLeft) panelRect.localPosition += Vector3.right * (canvasLeft - leftEdge + gap);
        if (rightEdge > canvasRight) panelRect.localPosition += Vector3.left * (rightEdge - canvasRight + gap);

        panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData) { }
}