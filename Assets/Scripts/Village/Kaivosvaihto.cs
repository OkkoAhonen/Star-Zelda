using UnityEngine;
using UnityEngine.SceneManagement;

public class Kaivosvaihto : MonoBehaviour
{
    //private GameObject player;

    void Start()
    {
        //player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene(1);
        }
        //PlayerStats stats = player.GetComponent<PlayerStats>();
        //if( Input.GetKeyDown(KeyCode.E))
        //{
        //    if(stats.Startpoint >= 3) 
        //    { 
        //        SceneManager.LoadScene(1);
        //    }
        //    else
        //    {
        //        Debug.Log("Talk to everyone first");
        //    }
        //}
    }
}
