using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Player
{
    public static Transform Transform { get; set; }

    public static float Speed = 12f;

    public static Room CurrentRoom;

    public static CharacterController CharacterController;
}
