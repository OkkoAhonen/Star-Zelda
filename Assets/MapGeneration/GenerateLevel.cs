using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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


    bool CheckIfRoomsAroundGeneratedRoom(Vector2 v, string direction)
    {
        switch (direction)
        {
            case "Right":
                {
                    //Check Down, Left and up

                    if (Levels.Rooms.Exists(x => x.Location == new Vector2(v.x - 1, v.y)) ||
                       Levels.Rooms.Exists(x => x.Location == new Vector2(v.x, v.y - 1)) ||
                       Levels.Rooms.Exists(x => x.Location == new Vector2(v.x, v.y + 1)))
                        return true;
                    break;
                }
            case "Left":
                {
                    //Check Down, Right and up

                    if (Levels.Rooms.Exists(x => x.Location == new Vector2(v.x + 1, v.y)) ||
                       Levels.Rooms.Exists(x => x.Location == new Vector2(v.x, v.y - 1)) ||
                       Levels.Rooms.Exists(x => x.Location == new Vector2(v.x, v.y + 1)))
                        return true;
                    break;
                }
            case "Up":
                {
                    //Check Down, Right and Left

                    if (Levels.Rooms.Exists(x => x.Location == new Vector2(v.x + 1, v.y)) ||
                       Levels.Rooms.Exists(x => x.Location == new Vector2(v.x -1, v.y)) ||
                       Levels.Rooms.Exists(x => x.Location == new Vector2(v.x, v.y - 1)))
                        return true;
                    break;
                }
            case "Down":
                {
                    //Check Down, up and Left

                    if (Levels.Rooms.Exists(x => x.Location == new Vector2(v.x, v.y + 1)) ||
                       Levels.Rooms.Exists(x => x.Location == new Vector2(v.x - 1, v.y)) ||
                       Levels.Rooms.Exists(x => x.Location == new Vector2(v.x + 1, v.y)))
                        return true;
                    break;
                }

        }

        return false;
    }



    void Generate(Room room)
    {
        failsafe++;
        if (failsafe > 40)
        {
            return;
        }
        

        DrawRoomOnMap (room);
        
        //Left
        if (Random.value > Levels.RoomGenerationChance)
        {


            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(-1, 0) + room.Location;
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            if (!CheckIfRoomExists(NewRoom.Location))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(NewRoom.Location, "Right"))
                {
                    if (Mathf.Abs(NewRoom.Location.x) < Levels.RoomLimit && Mathf.Abs(NewRoom.Location.y) < Levels.RoomLimit)
                        Generate(NewRoom);
                }
            }

        }

        //Right
        if (Random.value > Levels.RoomGenerationChance)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(1, 0) + room.Location;
            NewRoom.RoomImage = Levels.DefaultRoomIcon;

            if (!CheckIfRoomExists(NewRoom.Location))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(NewRoom.Location, "Left"))
                {
                    if (Mathf.Abs(NewRoom.Location.x) < Levels.RoomLimit && Mathf.Abs(NewRoom.Location.y) < Levels.RoomLimit)
                        Generate(NewRoom);
                }
            }


        }

        //UP
        if (Random.value > Levels.RoomGenerationChance)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(0, 1) + room.Location;
            NewRoom.RoomImage = Levels.DefaultRoomIcon;

            if (!CheckIfRoomExists(NewRoom.Location))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(NewRoom.Location, "Down"))
                {
                    if (Mathf.Abs(NewRoom.Location.x) < Levels.RoomLimit && Mathf.Abs(NewRoom.Location.y) < Levels.RoomLimit)
                        Generate(NewRoom);
                }
            }

        }

        //Down
        if (Random.value > Levels.RoomGenerationChance)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(0, -1) + room.Location;
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            if (!CheckIfRoomExists(NewRoom.Location))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(NewRoom.Location, "Up"))
                {
                    if (Mathf.Abs(NewRoom.Location.x) < Levels.RoomLimit && Mathf.Abs(NewRoom.Location.y) < Levels.RoomLimit)
                        Generate(NewRoom);
                }
            }
        }




    }


    // Start is called before the first frame update
    private void Awake()
    {
        Levels.DefaultRoomIcon = EmptyRoom;         //1
        Levels.BossRoomIcon = Broom;                //2
        Levels.TreasureRoomIcon = TreasureRoom;     //3
        Levels.currentroomIcon = CurrentRoom;       //4
        Levels.unexploredroomIcon = unexploretRoom; //5
    }
    void Start()
    {


        Room StarRoom = new Room();
        StarRoom.Location = new Vector2(0, 0);
        StarRoom.RoomImage = Levels.currentroomIcon;

        DrawRoomOnMap(StarRoom); //Star room draw


        
        //left
        if(Random.value > Levels.RoomGenerationChance)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(-1, 0);
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            if (!CheckIfRoomExists(NewRoom.Location))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(NewRoom.Location, "Right"))
                    Generate(NewRoom);
            }
        }

        //Right
        if (Random.value > Levels.RoomGenerationChance)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(1, 0);
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            if (!CheckIfRoomExists(NewRoom.Location))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(NewRoom.Location, "Left"))
                    Generate(NewRoom);
            }
        }

        //UP
        if (Random.value > Levels.RoomGenerationChance)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(0, 1);
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            if (!CheckIfRoomExists(NewRoom.Location))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(NewRoom.Location, "Down"))
                    Generate(NewRoom);
            }
        }

        //Down
        if (Random.value > Levels.RoomGenerationChance)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(0, -1);
            NewRoom.RoomImage = Levels.DefaultRoomIcon;
            if (!CheckIfRoomExists(NewRoom.Location))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(NewRoom.Location, "Up"))
                    Generate(NewRoom);
            }
        }




    }


    // Update is called once per fram



    bool regenerating = false;

    void stopRegenerating()
    {
        regenerating = false;
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) && !regenerating) 
        {
            regenerating=true;
            Invoke(nameof(stopRegenerating), 1);
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                Destroy(child.gameObject);
            
            }
            Levels.Rooms.Clear();

            Start();

        }
    }
}
