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
        string TileName = "MapTile";
        if (R.RoomNumber == 1) TileName = "BossRoom";
        if (R.RoomNumber == 3) TileName = "TreasureRoom";
        GameObject Maptile = new GameObject(TileName);
        Image RoomImage = Maptile.AddComponent<Image>();
        RoomImage.sprite = R.RoomSprite;
        R.RoomImage = RoomImage;
        RectTransform rectTransform = RoomImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Levels.height, Levels.height) * Levels.IconScale;
        rectTransform.position = R.Location * (Levels.IconScale * Levels.height * Levels.scale + (Levels.padding * Levels.height * Levels.scale));
        RoomImage.transform.SetParent(transform, false);

        Levels.Rooms.Add(R);
    }

    int RandomRoomNumber()
    {
        //return Random.RandomRange(6, GameObject.Find("Rooms").transform.childCount);
        return 6;
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
            NewRoom.RoomSprite = Levels.DefaultRoomIcon;
            NewRoom.RoomNumber = RandomRoomNumber();
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
            NewRoom.RoomSprite = Levels.DefaultRoomIcon;
            NewRoom.RoomNumber = RandomRoomNumber();
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
            NewRoom.RoomSprite = Levels.DefaultRoomIcon;
            NewRoom.RoomNumber = RandomRoomNumber();
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
            NewRoom.RoomSprite = Levels.DefaultRoomIcon;
            NewRoom.RoomNumber = RandomRoomNumber();
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

    private void GenerateBossRoom()
    {
        float MaxNumber = 0f;
        Vector2 FartestRoom = new Vector2();

        foreach (Room R in Levels.Rooms)
        {
            if (Mathf.Abs(R.Location.x) + Mathf.Abs(R.Location.y) >= MaxNumber)
            {
                MaxNumber = Mathf.Abs(R.Location.x) + Mathf.Abs(R.Location.y);
                FartestRoom = R.Location;
            }

        }

        Room BossRoom = new Room();
        BossRoom.RoomSprite = Levels.BossRoomIcon;
        BossRoom.RoomNumber = 1;

        //Left

        if (!CheckIfRoomExists(FartestRoom + new Vector2(-1, 0)))
        {
            if(!CheckIfRoomsAroundGeneratedRoom(FartestRoom + new Vector2(-1, 0), "Right"))
            {
                BossRoom.Location = FartestRoom + new Vector2(-1, 0);
            }
        }

        //Right

        else if (!CheckIfRoomExists(FartestRoom + new Vector2(1, 0)))
        {
            if (!CheckIfRoomsAroundGeneratedRoom(FartestRoom + new Vector2(1, 0), "Left"))
            {
                BossRoom.Location = FartestRoom + new Vector2(1, 0);
            }
        }

        //Up

        else if (!CheckIfRoomExists(FartestRoom + new Vector2(0, 1)))
        {
            if (!CheckIfRoomsAroundGeneratedRoom(FartestRoom + new Vector2(0, 1), "Down"))
            {
                BossRoom.Location = FartestRoom + new Vector2(0, 1);
            }
        }

        //Down

        else if (!CheckIfRoomExists(FartestRoom + new Vector2(0, -1)))
        {
            if (!CheckIfRoomsAroundGeneratedRoom(FartestRoom + new Vector2(0, -1), "Up"))
            {
                BossRoom.Location = FartestRoom + new Vector2(0, -1);
            }
        }

        DrawRoomOnMap(BossRoom);
    }

    void SuffleList<T>(List<T> list)
    {
        int n = list.Count;
        System.Random rng = new System.Random();
        
        while(n > 1)
        {
            n--;
            int k = rng.Next(n);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

    }


    private bool GenerateSpecialRoom(Sprite MapIcon, int RoomNumber )
    {
        List<Room> SuffledList = new List<Room>(Levels.Rooms);
        Room SpecialRoom = new Room();
        SpecialRoom.RoomSprite = MapIcon;
        SpecialRoom.RoomNumber = RoomNumber;

        bool FoundAvailkableLocation = false;

        foreach (Room R in SuffledList)
        {
            Vector2 SpecialRoomLocation = R.Location;

            if(R.RoomNumber < 6)
            {
                continue;
            }


            //Left

            if (!CheckIfRoomExists(SpecialRoomLocation + new Vector2(-1, 0)))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(SpecialRoomLocation + new Vector2(-1, 0), "Right"))
                {
                    SpecialRoom.Location = SpecialRoomLocation + new Vector2(-1, 0);
                    FoundAvailkableLocation = true;
                }
            }

            //Right

            else if (!CheckIfRoomExists(SpecialRoomLocation + new Vector2(1, 0)))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(SpecialRoomLocation + new Vector2(1, 0), "Left"))
                {
                    SpecialRoom.Location = SpecialRoomLocation + new Vector2(1, 0);
                    FoundAvailkableLocation = true;
                }
            }

            //Up

            else if (!CheckIfRoomExists(SpecialRoomLocation + new Vector2(0, 1)))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(SpecialRoomLocation + new Vector2(0, 1), "Down"))
                {
                    SpecialRoom.Location = SpecialRoomLocation + new Vector2(0, 1);
                    FoundAvailkableLocation = true;
                }
            }

            //Down

            else if (!CheckIfRoomExists(SpecialRoomLocation + new Vector2(0, -1)))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(SpecialRoomLocation + new Vector2(0, -1), "Up"))
                {
                    SpecialRoom.Location = SpecialRoomLocation + new Vector2(0, -1);
                    FoundAvailkableLocation = true;
                }
            }

            if (FoundAvailkableLocation)
            {
                DrawRoomOnMap(SpecialRoom);
                return true;
            }
            
        }

        return false;

    }

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
        StarRoom.RoomSprite = Levels.currentroomIcon;
        StarRoom.RoomNumber = 0;

        Player.CurrentRoom = StarRoom;

        DrawRoomOnMap(StarRoom); //Star room draw


        
        //left
        if(Random.value > Levels.RoomGenerationChance)
        {
            Room NewRoom = new Room();
            NewRoom.Location = new Vector2(-1, 0);
            NewRoom.RoomSprite = Levels.DefaultRoomIcon;
            NewRoom.RoomNumber = RandomRoomNumber();
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
            NewRoom.RoomSprite = Levels.DefaultRoomIcon;
            NewRoom.RoomNumber = RandomRoomNumber();
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
            NewRoom.RoomSprite = Levels.DefaultRoomIcon;
            NewRoom.RoomNumber = RandomRoomNumber();
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
            NewRoom.RoomSprite = Levels.DefaultRoomIcon;
            NewRoom.RoomNumber = RandomRoomNumber();
            if (!CheckIfRoomExists(NewRoom.Location))
            {
                if (!CheckIfRoomsAroundGeneratedRoom(NewRoom.Location, "Up"))
                    Generate(NewRoom);
            }
        }

        GenerateBossRoom();

        bool Treasure = GenerateSpecialRoom(Levels.TreasureRoomIcon, 3);

        if(!Treasure)
        {
            Regenerate();
        }


    }


    // Update is called once per fram


    void Regenerate()
    {
        regenerating = true;
        failsafe = 0;
        Levels.Rooms.Clear();
        Invoke(nameof(stopRegenerating), 1);
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);

        }


        Start();

    }
    bool regenerating = false;

    void stopRegenerating()
    {
        regenerating = false;
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) && !regenerating) 
        {
            Regenerate();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && !regenerating)
        {
            regenerating=true;
            Invoke(nameof(stopRegenerating), 1);

            string Log = "Room List: \n-----------------\n";

            foreach(Room R in Levels.Rooms)
            {
                Log += "Room#:" + R.RoomNumber + "Location: " + R.Location + "\n";
            }
            Debug.Log(Log);
        }
    }
}
