using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChangeRooms : MonoBehaviour
{

    public Transform Rooms;
    public float RoomSpawnOffset = 0.5f;

    private Sprite previousImage;

    private void Start()
    {
        previousImage = Levels.DefaultRoomIcon;
        EnableDoors(Player.CurrentRoom);
        
    }

    void ChangeRoomIcon(Room CurrentRoom, Room NewRoom)
    {
        CurrentRoom.RoomImage.sprite = previousImage;
        previousImage = NewRoom.RoomImage.sprite;
        NewRoom.RoomImage.sprite = Levels.currentroomIcon;
    }

    bool changeRoomCooldown = false;

    void EndChangeRoomCooldown()
    {
        changeRoomCooldown = false;
    }

    void EnableDoors(Room R)
    {
        Transform T = Rooms.Find(Player.CurrentRoom.RoomNumber.ToString());
        Transform Doors = T.Find("Doors");

        for (int i = 0; i < Doors.childCount - 1; i++)
        {
            Doors.GetChild(i).gameObject.SetActive(false);
        }

        //check what doors there

        //Left
        {       
            Vector2 NewPosition = R.Location + new Vector2(-1, 0);

            if(Levels.Rooms.Exists(x => x.Location == NewPosition))
            {
                Doors.Find("LeftDoor").gameObject.SetActive(true);
            }
        }
        //Up
        { 
            Vector2 NewPosition = R.Location + new Vector2(0, 1);

            if (Levels.Rooms.Exists(x => x.Location == NewPosition))
            {
                Doors.Find("TopDoor").gameObject.SetActive(true);
            }
        }


        //Down
        { 
            Vector2 NewPosition = R.Location + new Vector2(0, -1);

            if (Levels.Rooms.Exists(x => x.Location == NewPosition))
            {
                Doors.Find("BottomDoor").gameObject.SetActive(true);
            }
        }

        //Right
        { 
            Vector2 NewPosition = R.Location + new Vector2(1, 0);

            if (Levels.Rooms.Exists(x => x.Location == NewPosition))
            {
                Doors.Find("RightDoor").gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(changeRoomCooldown)
        {
            return;
        }

        else
        {
            changeRoomCooldown=true;
            Invoke(nameof(EndChangeRoomCooldown), Levels.RoomChangeTime);
        }


        if (collision.gameObject.name == "LeftDoor")
        {

            Debug.Log("kolliisio Left");
            //Where are we?
            Vector2 Location = Player.CurrentRoom.Location;

            //Where are we going?

            Location = Location + new Vector2(-1, 0);

            if (Levels.Rooms.Exists(x => x.Location == Location))
            {
                Room R = Levels.Rooms.First(x => x.Location == Location);

                //Disable the room where player is
                Rooms.Find(Player.CurrentRoom.RoomNumber.ToString()).gameObject.SetActive(false);

                //Find the new room
                GameObject NewRoom = Rooms.Find(R.RoomNumber.ToString()).gameObject;
                NewRoom.SetActive(true);


                //move the player to the door area
                Player.Transform.position = NewRoom.transform.Find("Doors").transform.Find("RightDoor").position + new Vector3(-1, 0, 0);

                ChangeRoomIcon(Player.CurrentRoom, R);

                Player.CurrentRoom = R;

                EnableDoors(R);

            }
        }

        if (collision.gameObject.name == "RightDoor")
        {

            Debug.Log("kolliisio Right");
            //Where are we?
            Vector2 Location = Player.CurrentRoom.Location;

            //Where are we going?

            Location = Location + new Vector2(1, 0);

            if (Levels.Rooms.Exists(x => x.Location == Location))
            {
                Room R = Levels.Rooms.First(x => x.Location == Location);

                //Disable the room where player is
                Rooms.Find(Player.CurrentRoom.RoomNumber.ToString()).gameObject.SetActive(false);

                //Find the new room
                GameObject NewRoom = Rooms.Find(R.RoomNumber.ToString()).gameObject;
                NewRoom.SetActive(true);


                //move the player to the door area
                Player.Transform.position = NewRoom.transform.Find("Doors").transform.Find("LeftDoor").position + new Vector3(1, 0, 0);

                ChangeRoomIcon(Player.CurrentRoom, R);

                Player.CurrentRoom = R;

                EnableDoors(R);

            }
        }

        if (collision.gameObject.name == "TopDoor")
        {

            Debug.Log("kolliisio");
            //Where are we?
            Vector2 Location = Player.CurrentRoom.Location;

            //Where are we going?

            Location = Location + new Vector2(0, 1);

            if (Levels.Rooms.Exists(x => x.Location == Location))
            {
                Room R = Levels.Rooms.First(x => x.Location == Location);

                //Disable the room where player is
                Rooms.Find(Player.CurrentRoom.RoomNumber.ToString()).gameObject.SetActive(false);

                //Find the new room
                GameObject NewRoom = Rooms.Find(R.RoomNumber.ToString()).gameObject;
                NewRoom.SetActive(true);


                //move the player to the door area
                Player.Transform.position = NewRoom.transform.Find("Doors").transform.Find("BottomDoor").position + new Vector3(0, 1, 0);

                ChangeRoomIcon(Player.CurrentRoom, R);

                Player.CurrentRoom = R;

                EnableDoors(R);

            }
        }

        if (collision.gameObject.name == "BottomDoor")
        {

            Debug.Log("kolliisio");
            //Where are we?
            Vector2 Location = Player.CurrentRoom.Location;

            //Where are we going?

            Location = Location + new Vector2(0, -1);

            if (Levels.Rooms.Exists(x => x.Location == Location))
            {
                Room R = Levels.Rooms.First(x => x.Location == Location);

                //Disable the room where player is
                Rooms.Find(Player.CurrentRoom.RoomNumber.ToString()).gameObject.SetActive(false);

                //Find the new room
                GameObject NewRoom = Rooms.Find(R.RoomNumber.ToString()).gameObject;
                NewRoom.SetActive(true);


                //move the player to the door area
                Player.Transform.position = NewRoom.transform.Find("Doors").transform.Find("TopDoor").position + new Vector3(0, -1, 0);

                ChangeRoomIcon(Player.CurrentRoom, R);

                Player.CurrentRoom = R;

                EnableDoors(R);

            }
        }



    }

}
