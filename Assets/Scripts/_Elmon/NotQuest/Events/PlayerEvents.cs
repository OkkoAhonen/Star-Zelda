using System;
using UnityEngine.Events;

public class PlayerEvents
{
	public event Action onEnablePlayerMovement;
	public event Action onDisablePlayerMovement;
	public event Action<float> onGainExperience;
	public event Action<int> onPlayerLevelChangeTo;
	public event Action<string> onEnemyKilled;
	public event Action<int, int> onHealthChangeTo;
	public event Action<int> onChangeArmorTo;
	public event Action onStatsChanged;
    public event Action<int> onChangeGoldBy;
	public event Action onShop;

	public void EnablePlayerMovement() => onEnablePlayerMovement?.Invoke();
	public void DisablePlayerMovement() => onDisablePlayerMovement?.Invoke();
	public void GainExperience(float experience) => onGainExperience?.Invoke(experience);
	public void PlayerLevelChangeTo(int level) => onPlayerLevelChangeTo?.Invoke(level);
	public void EnemyKilled(string target) => onEnemyKilled?.Invoke(target);
	public void HealthChangeTo(int currentHealth, int maxHealth) => onHealthChangeTo?.Invoke(currentHealth, maxHealth);
	public void ChangeArmorTo(int newArmor) => onChangeArmorTo?.Invoke(newArmor);
	public void StatsChanged() => onStatsChanged?.Invoke();
    public void ChangeGoldBy(int gold) => onChangeGoldBy?.Invoke(gold);
	public void Shop() => onShop?.Invoke();

}
