using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkDisplayPrefab : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    
    public void Setup(Perk perk, bool isUnlocked)
    {
        icon.sprite = perk.Icon;
        nameText.text = perk.Name;
        descriptionText.text = perk.Description;
        
        // Gray out if locked
        icon.color = isUnlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
    }
} 