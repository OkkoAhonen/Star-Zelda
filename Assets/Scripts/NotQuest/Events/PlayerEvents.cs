using System;

public class PlayerEvents
{
    public event Action onEnablePlayerMovement;
    public event Action<int> onGainExperience;
    public event Action<int> onPlayerExperienceChange;
    public event Action<int> onPlayerLevelChange;
    public event Action<string> onEnemyKilled;
    public event Action onDisablePlayerMovement;

    public void DisablePlayerMovement() => onDisablePlayerMovement?.Invoke();
    public void EnablePlayerMovement() => onEnablePlayerMovement?.Invoke();
    public void GainExperience(int experience) => onGainExperience?.Invoke(experience);
    public void PlayerExperienceChange(int experience) => onPlayerExperienceChange?.Invoke(experience);
    public void PlayerLevelChange(int level) => onPlayerLevelChange?.Invoke(level);
    public void EnemyKilled(string target) => onEnemyKilled?.Invoke(target);
}