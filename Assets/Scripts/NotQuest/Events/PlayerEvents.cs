using System;
using UnityEngine.Events;

public class PlayerEvents
{
	public event Action onEnablePlayerMovement;
	public event Action onDisablePlayerMovement;
	public event Action<int> onGainExperience;
	public event Action<int> onPlayerLevelChangeTo;
	public event Action<string> onEnemyKilled;
	public event Action<int, int> onHealthChangeTo;
	public event Action<int> onChangeArmorBy;
	public event Action<StatType, int> onStatChange;
    public event Action<int> onChangeGoldTo;
	public event Action onShop;

	public void EnablePlayerMovement() => onEnablePlayerMovement?.Invoke();
	public void DisablePlayerMovement() => onDisablePlayerMovement?.Invoke();
	public void GainExperience(int experience) => onGainExperience?.Invoke(experience);
	public void PlayerLevelChangeTo(int level) => onPlayerLevelChangeTo?.Invoke(level);
	public void EnemyKilled(string target) => onEnemyKilled?.Invoke(target);
	public void HealthChangeTo(int currentHealth, int maxHealth) => onHealthChangeTo?.Invoke(currentHealth, maxHealth);
	public void ChangeArmorBy(int newArmor) => onChangeArmorBy?.Invoke(newArmor);
	public void StatChange(StatType statType, int newValue) => onStatChange?.Invoke(statType, newValue);
    public void ChangeGoldTo(int gold) => onChangeGoldTo?.Invoke(gold);
	public void Shop() => onShop?.Invoke();

}
