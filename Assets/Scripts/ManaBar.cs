using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    public static ManaBar Instance;
    
    public GameObject manaBarObject;      // родительский объект
    public RectTransform manaFill;        // заполнение (двигаем по Y)
    public Text manaText;                 // текст "50/100"
    
    private float minY = -180f;           // 0%
    private float maxY = -45f;            // 100%
    
    void Awake() => Instance = this;
    
    public void UpdateManaBar(float currentMana, float maxMana)
    {
        if (manaBarObject == null) return;
        
        if (maxMana <= 0)
        {
            manaBarObject.SetActive(false);
            return;
        }
        
        manaBarObject.SetActive(true);
        
        float percent = currentMana / maxMana;
        float y = Mathf.Lerp(minY, maxY, percent);
        Vector2 pos = manaFill.anchoredPosition;
        pos.y = y;
        manaFill.anchoredPosition = pos;
        
        if (manaText != null)
            manaText.text = $"{Mathf.CeilToInt(currentMana)}/{Mathf.CeilToInt(maxMana)}";
    }
}