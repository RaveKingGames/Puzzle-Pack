using UnityEngine;
using UnityEngine.UI;

public class StatRow : MonoBehaviour
{
    [Header("UI элементы")]
    public Image iconImage;
    public Text statNameText;
    public Text statValueText;

    public void Setup(Sprite icon, string statName, string statValue)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }
        
        if (statNameText != null)
        {
            statNameText.text = statName;
            statNameText.enabled = !string.IsNullOrEmpty(statName);
        }
        
        if (statValueText != null)
        {
            statValueText.text = statValue;
            statValueText.enabled = !string.IsNullOrEmpty(statValue);
        }
    }
}