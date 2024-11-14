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

    // Start is called before the first frame update
    void Start()
    {
        Levels.DefaultRoomIcon = EmptyRoom;         //1
        Levels.BossRoomIcon = Broom;                //2
        Levels.TreasureRoomIcon = TreasureRoom;     //3
        Levels.currentroomIcon = CurrentRoom;       //4
        Levels.unexploredroomIcon = unexploretRoom; //5






        GameObject Maptile = new GameObject("MapTile");
        Image RoomImage = Maptile.AddComponent<Image>();
        RoomImage.sprite = Levels.DefaultRoomIcon;
        RectTransform rectTransform = RoomImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Levels.height, Levels.height) * Levels.IconScale;
        RoomImage.transform.SetParent(transform, false);    
    }

    // Update is called once per fram
}
