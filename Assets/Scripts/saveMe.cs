using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class saveMe : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}