using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FloatingIcon : MonoBehaviour
{
    private Image iconImage;
    private RectTransform rect;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        iconImage = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowLocal(Sprite icon)
    {
        StartCoroutine(ShowLocalRoutine(icon));
    }

    IEnumerator ShowLocalRoutine(Sprite icon)
    {
        iconImage.sprite = icon;
        transform.localScale = Vector3.zero;
        rect.localPosition = Vector3.zero;

        float duration = 0.7f;
        float elapsed = 0f;
        float floatHeight = 60f;
        float startY = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float scale = Mathf.Lerp(0f, 1.2f, Mathf.Clamp01(t * 2.5f));
            if (t > 0.4f) scale = Mathf.Lerp(1.2f, 0.8f, (t - 0.4f) / 0.6f);
            transform.localScale = Vector3.one * scale;

            rect.localPosition = new Vector2(Random.Range(-15f, 15f) * t, startY + floatHeight * t);

            canvasGroup.alpha = t < 0.3f ? t / 0.3f : (1f - (t - 0.5f) / 0.5f);

            yield return null;
        }

        Destroy(gameObject);
    }
}