using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAttackMelee : MonoBehaviour
{
    public GameObject attackpoint;
    public float radius;
    public LayerMask enemies;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void attack1()
    {
        Collider2D[] enemy = Physics2D.OverlapCircleAll(attackpoint.transform.position, radius, enemies );

        foreach (Collider2D enemyGameobje in enemy)
        {
            Debug.Log("Hit enemy");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackpoint.transform.position, radius);
    }
}
