using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Mathematics;



public class HealthBar : MonoBehaviour
{
    
    public GameObject heartPrefab;
    public PlayerMovement2D playerMovement2D;
    


    List<Health> hearts = new List<Health>();

    private void OnEnable()
    {
        PlayerMovement2D.OnplayerDamaged += DrawHearts;
       
    }
    private void OnDisable()
    {
        PlayerMovement2D.OnplayerDamaged -= DrawHearts;
    }

    public void DrawHearts()
    {
        ClearHearts();

        //determine how many hearts to make total
        //Bsed off the max health

        float maxHealthRemainder = playerMovement2D.MaxHealth % 2;
        int heartsToMake = (int)(playerMovement2D.MaxHealth/ 2 + maxHealthRemainder);

        for(int i = 0; i < heartsToMake; i++) {

            CreateEmptyHeart();
        
        }
        for(int i = 0;i < hearts.Count; i++)
        {
            int heartStatusRemainder = (int)math.clamp(playerMovement2D.health - (i * 2), 0, 2);
            hearts[i].SetHeartImage((HeartStatus)heartStatusRemainder);
        
        
        }

    }



    public void CreateEmptyHeart()
    {
        GameObject newHeart = Instantiate(heartPrefab);
        newHeart.transform.SetParent(transform);

        Health heartComponent = newHeart.GetComponent<Health>();
        heartComponent.SetHeartImage(HeartStatus.Empty);
        hearts.Add(heartComponent);

    }

    public void ClearHearts()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }

        hearts = new List<Health>();
    }
    // Start is called before the first frame update
    void Start()
    {
        DrawHearts();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
