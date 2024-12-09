using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemystats2 : MonoBehaviour
{

    public int healt = 50;
    public EnemyController controller;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        healt = healt - 2;
        controller.currentState = EnemyState.Wander;
    }
}
