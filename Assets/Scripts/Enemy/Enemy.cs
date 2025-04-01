using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyStats")]
public class Enemy : ScriptableObject
{
    [Header("Enemy Stats")]
    public int maxHealth;
    public float speed;
    public float attackSpeed;
    public float strength;
    public float detectionRange;

    public bool canfire;


    
}
