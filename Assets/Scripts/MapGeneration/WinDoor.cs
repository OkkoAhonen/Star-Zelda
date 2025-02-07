using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinDoor : MonoBehaviour
{
    
    
    // Start is called before the first frame update
    void Start()
    {
        // Etsi SimpleSceneManager automaattisesti

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SimpleSceneManager.Instance.LoadVillageScene();

    }
}
