using System;
using UnityEngine.Events;

public class PlayerEvents
{
	public event Action onEnablePlayerMovement;
	public event Action onDisablePlayerMovement;
	public event Action<int> onGainExperience;
	public event Action<int> onPlayerExperienceChange;
	public event Action<int> onPlayerLevelChange;
	public event Action<string> onEnemyKilled;
	public event Action<int, int> onHealthChange;
	public event Action<int> onArmorChange;
	public event Action<StatType, int> onStatChange;

	public void EnablePlayerMovement() => onEnablePlayerMovement?.Invoke();
	public void DisablePlayerMovement() => onDisablePlayerMovement?.Invoke();
	public void GainExperience(int experience) => onGainExperience?.Invoke(experience);
	public void PlayerExperienceChange(int experience) => onPlayerExperienceChange?.Invoke(experience);
	public void PlayerLevelChange(int level) => onPlayerLevelChange?.Invoke(level);
	public void EnemyKilled(string target) => onEnemyKilled?.Invoke(target);
	public void HealthChange(int currentHealth, int maxHealth) => onHealthChange?.Invoke(currentHealth, maxHealth);
	public void ArmorChange(int newArmor) => onArmorChange?.Invoke(newArmor);
	public void StatChange(StatType statType, int newValue) => onStatChange?.Invoke(statType, newValue);
}
