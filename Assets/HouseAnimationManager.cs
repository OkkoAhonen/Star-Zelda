using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseAnimationManager : MonoBehaviour
{
    public Animator Roof;
    // Start is called before the first frame update
    void Start()
    {
        Roof = GameObject.Find("CabinTop").GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Roof != null)
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            { 
                Roof.SetBool("InHouse", true);
                Debug.Log("InHouse set to true");
            }

        }
    }
}
