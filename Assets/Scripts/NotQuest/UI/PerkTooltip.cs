using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkTooltip : MonoBehaviour
{
	[SerializeField] private TMP_Text nameText;
	[SerializeField] private TMP_Text descriptionText;
	[SerializeField] private Image perkIcon;
	private CanvasGroup canvasGroup;
	private RectTransform rectTransform;

	private readonly Vector2 CURSOR_OFFSET = new Vector2(-100f, -90f);

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		rectTransform = GetComponent<RectTransform>();
		HideTooltip();
	}

	private void Update()
	{
		if (canvasGroup.alpha > 0)
		{
			Vector2 mousePos = Input.mousePosition;
			
			mousePos += CURSOR_OFFSET;
			
			// Keep tooltip on screen
			Vector2 screenPoint = Camera.main.WorldToViewportPoint(mousePos);
			bool isOffRight = screenPoint.x + (rectTransform.rect.width / Screen.width) > 1f;
			bool isOffBottom = screenPoint.y - (rectTransform.rect.height / Screen.height) < 0f;

			// Adjust position if tooltip would go off screen
			if (isOffRight) mousePos.x -= rectTransform.rect.width + CURSOR_OFFSET.x * 2;
			if (isOffBottom) mousePos.y += rectTransform.rect.height + Mathf.Abs(CURSOR_OFFSET.y) * 2;

			rectTransform.position = mousePos;
		}
	}

	public void ShowTooltip(string name, string description, Sprite icon, bool isUnlocked)
	{
		nameText.text = name;
		descriptionText.text = description;
		perkIcon.sprite = icon;
		perkIcon.color = isUnlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
		canvasGroup.alpha = 1;
		canvasGroup.blocksRaycasts = false;
	}

	public void HideTooltip()
	{
		canvasGroup.alpha = 0;
		canvasGroup.blocksRaycasts = false;
	}
}
