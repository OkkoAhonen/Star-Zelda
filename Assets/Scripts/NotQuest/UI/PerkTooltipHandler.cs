using UnityEngine;
using UnityEngine.EventSystems;

public class PerkTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private PerkTooltip tooltip;
	private Perk perk;
	private bool isUnlocked;

	public void Initialize(PerkTooltip tooltip, Perk perk, bool isUnlocked)
	{
		this.tooltip = tooltip;
		this.perk = perk;
		this.isUnlocked = isUnlocked;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		tooltip.ShowTooltip(perk.Name, perk.Description, perk.Icon, isUnlocked);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		tooltip.HideTooltip();
	}
}
