using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Levels
{
    public static float height = 500;
    public static float width = 500;

    public static float scale = 1.0f;
    public static float IconScale = 0.1f;
    public static float padding = 0.01f;

    public static float RoomGenerationChance = 0.5f;

    public static Sprite TreasureRoomIcon;
    public static Sprite BossRoomIcon;
    public static Sprite currentroomIcon;
    public static Sprite unexploredroomIcon;
    public static Sprite DefaultRoomIcon;

    public static List<Room> Rooms = new List<Room>();
    public static Room CurrentRoom;
}

public class Room
{
    public int RoomNumber = 0;
    public Vector2 Location;
    public Sprite RoomImage;

}