using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Projectile : MonoBehaviour
{
    private RectTransform rect;
    private Image image;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void Fly(Vector2 fromScreenPos, Vector2 toScreenPos, RectTransform canvasRect, System.Action<Vector2> onHit)
    {
        StartCoroutine(FlyRoutine(fromScreenPos, toScreenPos, canvasRect, onHit));
    }

    IEnumerator FlyRoutine(Vector2 fromScreenPos, Vector2 toScreenPos, RectTransform canvasRect, System.Action<Vector2> onHit)
    {
        Vector2 localFrom, localTo;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, fromScreenPos, canvas.worldCamera, out localFrom);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, toScreenPos, canvas.worldCamera, out localTo);

        rect.localPosition = localFrom;
        
        // Поворачиваем спрайт в сторону цели
        Vector2 direction = localTo - localFrom;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        rect.rotation = Quaternion.Euler(0, 0, angle);
        
        float duration = 0.2f;
        float elapsed = 0f;
        float fadeStart = 0.6f; // Начинаем затухать с 60% пути

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - (1f - t) * (1f - t); // ease out
            rect.localPosition = Vector2.Lerp(localFrom, localTo, t);
            
            // Плавное затухание и уменьшение в конце
            if (t > fadeStart)
            {
                float fadeT = (t - fadeStart) / (1f - fadeStart);
                canvasGroup.alpha = 1f - fadeT;
                transform.localScale = Vector3.one * (1f - fadeT * 0.5f);
            }
            
            yield return null;
        }

        // Вызываем onHit в точке прибытия
        onHit?.Invoke(toScreenPos);
        
        Destroy(gameObject);
    }
}