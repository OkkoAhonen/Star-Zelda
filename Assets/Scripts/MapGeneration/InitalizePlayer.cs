using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitalizePlayer : MonoBehaviour
{
    public Transform playerTransform;
    public CharacterController PlayerController;


    void Start()
    {
        Player.Transform = playerTransform;
        Player.CharacterController = PlayerController;

    }


}
