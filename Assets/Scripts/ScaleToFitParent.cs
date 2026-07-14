using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class ScaleToFitParent : MonoBehaviour
{
    [Tooltip("Родительская панель, в которую вписываемся. Если не задано — используется родитель")]
    public RectTransform parentRect;

    [Header("Исходная высота в пикселях при scale = 1")]
    public float referenceHeight = 845f; // 5 ячеек × 169px

    private void Awake()
    {
        if (parentRect == null && transform.parent != null)
            parentRect = transform.parent.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
            StartCoroutine(UpdateScaleRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateScaleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            ApplyScale();
        }
    }

    private void ApplyScale()
    {
        if (parentRect == null) return;

        float parentHeight = parentRect.rect.height;
        float scale = parentHeight / referenceHeight;

        transform.localScale = new Vector3(scale, scale, 1f);

        if (GridManager.Instance != null)
            GridManager.Instance.RepositionAllItems();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
            ApplyScale();
    }
#endif
}