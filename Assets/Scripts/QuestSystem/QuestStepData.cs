using UnityEngine;

[CreateAssetMenu(fileName = "QuestStepData", menuName = "Quests/Step")]
public class QuestStepData : ScriptableObject
{
    public StepType stepType;
    public int requiredAmount;
    public string targetName; // Coin, First Pillar, etc.
}
