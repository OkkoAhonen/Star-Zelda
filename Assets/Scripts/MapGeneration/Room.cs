using UnityEngine;
using UnityEngine.UI;
public class Room
{
    public RoomTemplate template; // Mink� mallin mukaan t�m� huone on luotu
    public Vector2 Location;      // Sijainti karttaruudukossa (esim. 0,0 tai 1,0).
    public Image RoomImage;       // Viittaus minikartan UI-Imageen.
    public GameObject RoomInstance { get; set; } // Viittaus instansioituun huone-GameObjectiin sceness�.
    public bool IsExplored { get; set; } = false; // Onko pelaaja k�ynyt t�ss� huoneessa?
    public int recursionDepth = 0; // Kuinka "syv�ll�" t�m� huone on generoinnissa (debug/failsafe)

    // Konstruktori helpottaa luomista
    public Room(RoomTemplate template, Vector2 location, int depth)
    {
        this.template = template;
        this.Location = location;
        this.recursionDepth = depth;
        // Oletuksena ei tutkittu, paitsi start room
        this.IsExplored = (template.type == RoomType.Start);
    }
}