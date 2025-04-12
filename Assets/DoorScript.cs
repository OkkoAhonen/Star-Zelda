using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{

    public Animator Animator;
    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Animator == null)
        {
            Debug.Log("Animator not found");
            return; // Lopeta Update, jos Animator puuttuu
        }

        if (Input.GetKeyDown(KeyCode.Space)) // Käytän GetKeyDown, koska GetButtonDown vaatii Input Managerin asetuksia
        {
            Animator.SetBool("InHouse", true);
            
        }
    }
}
