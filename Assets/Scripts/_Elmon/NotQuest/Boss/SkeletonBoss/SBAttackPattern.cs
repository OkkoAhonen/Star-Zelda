using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Boss/SBAttack Pattern")]
public class SBAttackPattern : ScriptableObject
{
    public string patternName;
    public float speedMultiplier = 1f;

    // sequence of steps
    public List<SBPatternStep> steps = new List<SBPatternStep>();
}