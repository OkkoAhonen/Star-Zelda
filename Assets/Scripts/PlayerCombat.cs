using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public int hitDamage = 3;
    public bool hit = false;
    public float currentHealth;
    public float enemyHealth;

    public GameObject player;
    public GameObject sword;

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void enemyDamage(int hit)
    {
    }
}
