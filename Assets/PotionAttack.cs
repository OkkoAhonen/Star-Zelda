using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PotionAttack : MonoBehaviour
{
    [SerializeField] private Vector2 potionplace = Vector2.zero;
    public float potionAttackTimer = 2f;
    public float timer = 0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        potionAttack();
    }

    public void potionAttack()
    {
        
        
        timer += Time.deltaTime;
        if(potionAttackTimer <= timer) { 
            potionplace = Input.mousePosition;
            Debug.Log(potionplace);
            timer= 0f;
        }
    }
}
