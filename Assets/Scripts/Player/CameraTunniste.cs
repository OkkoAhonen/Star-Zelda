using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTunniste : MonoBehaviour
{
   
    public CameraFollow cameraFollow;

    private void OnTriggerStay2D(Collider2D collision)
    {
        cameraFollow.IsitBoss = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        cameraFollow.IsitBoss = false;
    }
}
