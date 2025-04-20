using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseAnimationManager : MonoBehaviour
{
    public Animator Roof;
    public Animator Door;

    public bool hasExitedTheHouse;

    void Start()
    {
        Roof = GameObject.Find("CabinTop").GetComponent<Animator>();
        Door = GameObject.Find("CabinDoor").GetComponent<Animator>();
        if (Roof == null)
        {
            Debug.LogError("Animator-komponenttia ei löydy CabinTop-objektista!");
        }
        if (Door == null)
        {
            Debug.LogError("Animator-komponenttia ei löydy CabinDoor-objektista!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Hae nykyinen "InHouse"-arvo (voidaan hakea vain yhdestä Animatorista, koska niiden pitäisi olla synkronissa)
            bool currentState = Roof != null ? Roof.GetBool("InHouse") : false;
            bool newState = !currentState;

            // Aseta arvo molemmille Animatorille
            if (Roof != null)
            {
                Roof.SetBool("InHouse", newState);
                Debug.Log("Roof InHouse asetettu arvoon " + newState);
            }
            if (Door != null)
            {
                Door.SetBool("InHouse", newState);
                Debug.Log("Door InHouse asetettu arvoon " + newState);
            }
        }
    }
}