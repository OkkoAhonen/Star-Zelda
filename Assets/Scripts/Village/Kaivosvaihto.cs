using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Kaivosvaihto : MonoBehaviour
{
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if( Input.GetKeyDown(KeyCode.E))
        {
            if(stats.Startpoint >= 3) 
            { 
                SceneManager.LoadScene(1);
            }
            else
            {
                Debug.Log("Talk to everyone first");
            }
        }
    }
}
