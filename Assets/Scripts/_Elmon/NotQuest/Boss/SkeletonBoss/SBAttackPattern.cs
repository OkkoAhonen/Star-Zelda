using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Boss/SBAttack Pattern")]
public class SBAttackPattern : ScriptableObject
{
    // For your reference in the inspector
    public string patternName;

    // Sequence of steps this pattern will execute
    public List<SBPatternStep> steps = new List<SBPatternStep>();
}