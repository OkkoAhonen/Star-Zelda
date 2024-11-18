using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateLevel : MonoBehaviour
{

    public Sprite CurrentRoom; //
    public Sprite Broom;
    public Sprite EmptyRoom;
    public Sprite unexploretRoom;
    public Sprite TreasureRoom;

    void DrawRoomOnMap(Room R)
    {
        GameObject Maptile = new GameObject("MapTile");
        Image RoomImage = Maptile.AddComponent<Image>();
        RoomImage.sprite = R.RoomImage;
        RectTransform rectTransform = RoomImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Levels.height, Levels.height) * Levels.IconScale;
        rectTransform.position = R.Location * (Levels.IconScale * Levels.height * Levels.scale + (Levels.padding * Levels.height * Levels.scale));
        RoomImage.transform.SetParent(transform, false);

        Levels.Rooms.Add(R);
    }

    bool CheckIfRoomExists(Vector2 v)
    {
        return (Levels.Rooms.Exists(x => x.Location == v));
    }

    int failsafe = 0;

    void Generate(Room room)
    {
        failsafe++;
        if (failsafe > 40)
        {
            return;
        }
        

        DrawRoomOnMap (room);

        if (Random.value > 0.5f)
        {


            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(-1, 0) + room.Location;
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            if (!CheckIfRoomExists(NewRoom.Location))
            {
                Generate(NewRoom);
            }

        }

        //Right
        if (Random.value > 0.5f)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(1, 0) + room.Location;
            NewRoom.RoomImage = Levels.DefaultRoomIcon;

            if (!CheckIfRoomExists(NewRoom.Location))
            {
                Generate(NewRoom);
            }


        }

        //UP
        if (Random.value > 0.5f)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(0, 1) + room.Location;
            NewRoom.RoomImage = Levels.DefaultRoomIcon;

            if (!CheckIfRoomExists(NewRoom.Location))
            {   
                Generate(NewRoom);
            }
    
        }

        //Down
        if (Random.value > 0.5f)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(0, -1) + room.Location;
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            if (!CheckIfRoomExists(NewRoom.Location))
            {
                Generate(NewRoom);
            }
        }




    }


    // Start is called before the first frame update
    void Start()
    {
        Levels.DefaultRoomIcon = EmptyRoom;         //1
        Levels.BossRoomIcon = Broom;                //2
        Levels.TreasureRoomIcon = TreasureRoom;     //3
        Levels.currentroomIcon = CurrentRoom;       //4
        Levels.unexploredroomIcon = unexploretRoom; //5

        Room StarRoom = new Room();
        StarRoom.Location = new Vector2(0, 0);
        StarRoom.RoomImage = Levels.currentroomIcon;

        DrawRoomOnMap(StarRoom); //Star room draw


        
        //left
        if(Random.value > 0.5f)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(-1, 0);
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            Generate(NewRoom);
        }

        //Right
        if (Random.value > 0.5f)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(1, 0);
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            Generate(NewRoom);
        }

        //UP
        if (Random.value > 0.5f)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(0, 1);
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            Generate(NewRoom);
        }

        //Down
        if (Random.value > 0.5f)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(0, -1);
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            Generate(NewRoom);
        }




    }


    // Update is called once per fram
}
