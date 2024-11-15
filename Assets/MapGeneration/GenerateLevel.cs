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

    void DrawRoomOnMap(Sprite s, Vector2 Location)
    {
        GameObject Maptile = new GameObject("MapTile");
        Image RoomImage = Maptile.AddComponent<Image>();
        RoomImage.sprite = s;
        RectTransform rectTransform = RoomImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Levels.height, Levels.height) * Levels.IconScale;
        rectTransform.position = Location * (Levels.IconScale * Levels.height * Levels.scale + (Levels.padding * Levels.height * Levels.scale));
        RoomImage.transform.SetParent(transform, false);
    }

    void Generate(Vector2 Location)
    {
        if (Random.value > Levels.RoomGenerationChance)
        {
            
            DrawRoomOnMap(Levels.unexploredroomIcon, Location + new Vector2(0, 1));
            Generate(Location + new Vector2(0, 1));
        }

        //left
        if (Random.value > Levels.RoomGenerationChance)
        {
            
            DrawRoomOnMap(Levels.unexploredroomIcon, Location +  new Vector2(1, 0));
            Generate(Location + new Vector2(1, 0));
        }

        //right
        if (Random.value > Levels.RoomGenerationChance)
        {
            
            DrawRoomOnMap(Levels.unexploredroomIcon, Location + new Vector2(-1, 0));
            Generate(Location + new Vector2(-1, 0));
        }

        //Down
        if (Random.value > Levels.RoomGenerationChance)
        {
            
            DrawRoomOnMap(Levels.unexploredroomIcon, Location + new Vector2(0, -1));
            Generate(Location + new Vector2(0, -1));
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

        DrawRoomOnMap(Levels.currentroomIcon, new Vector2(0, 0)); //Star room draw


        //up
        if (Random.value > Levels.RoomGenerationChance)
        {
            Generate(new Vector2(0, 1));
            DrawRoomOnMap(Levels.unexploredroomIcon, new Vector2(0, 1));
        }

        //left
        if (Random.value > Levels.RoomGenerationChance) 
        {
            Generate(new Vector2(-1, 0));
            DrawRoomOnMap(Levels.unexploredroomIcon, new Vector2(1, 0)); 
        }

        //right
        if (Random.value > Levels.RoomGenerationChance)
        {
            Generate(new Vector2(1, 0));
            DrawRoomOnMap(Levels.unexploredroomIcon, new Vector2(-1, 0));
        }

        //Down
        if (Random.value > Levels.RoomGenerationChance)
        {
            Generate(new Vector2(1, -1));
            DrawRoomOnMap(Levels.unexploredroomIcon, new Vector2(0, -1));
        }
    }



    // Update is called once per fram
}
