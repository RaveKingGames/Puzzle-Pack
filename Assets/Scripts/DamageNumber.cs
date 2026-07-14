using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageNumber : MonoBehaviour
{
    public Text damageText;
    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (damageText == null)
            damageText = GetComponent<Text>();
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Show(float damage, Vector2 screenPos, RectTransform canvasRect)
    {
        StartCoroutine(ShowRoutine(damage, screenPos, canvasRect));
    }

    IEnumerator ShowRoutine(float damage, Vector2 screenPos, RectTransform canvasRect)
    {
        damageText.text = $"-{Mathf.CeilToInt(damage)}";

        // Стартовый scale — почти ноль
        transform.localScale = Vector3.zero;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPos);
        rect.localPosition = localPos + new Vector2(Random.Range(-20f, 20f), 30f);

        float duration = 0.6f;
        float elapsed = 0f;
        float floatHeight = 50f;
        float startY = rect.localPosition.y;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Scale: быстрое увеличение, потом плавное
            float scale = Mathf.Lerp(0f, 1.3f, Mathf.Clamp01(t * 3f));
            if (t > 0.3f) scale = Mathf.Lerp(1.3f, 1f, (t - 0.3f) / 0.7f);
            transform.localScale = Vector3.one * scale;

            // Позиция: поднимается вверх
            Vector2 pos = rect.localPosition;
            pos.y = startY + floatHeight * t;
            rect.localPosition = pos;

            // Альфа: появляется быстро, исчезает плавно
            float alpha = 1f;
            if (t > 0.5f) alpha = 1f - (t - 0.5f) / 0.5f;
            canvasGroup.alpha = alpha;

            yield return null;
        }

        Destroy(gameObject);
    }
}