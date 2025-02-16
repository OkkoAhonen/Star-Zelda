using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkTooltip : MonoBehaviour
{
	[SerializeField] private TMP_Text nameText;
	[SerializeField] private TMP_Text descriptionText;
	[SerializeField] private Image perkIcon;
	private CanvasGroup canvasGroup;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		HideTooltip();
	}

	public void ShowTooltip(string name, string description, Sprite icon, bool isUnlocked)
	{
		nameText.text = name;
		descriptionText.text = description;
		perkIcon.sprite = icon;
		perkIcon.color = isUnlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f); // Gray out if locked
		canvasGroup.alpha = 1;
		canvasGroup.blocksRaycasts = true;
	}

	public void HideTooltip()
	{
		canvasGroup.alpha = 0;
		canvasGroup.blocksRaycasts = false;
	}
}
