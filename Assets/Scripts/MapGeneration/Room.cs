using UnityEngine;
using UnityEngine.UI;
public class Room
{
    public RoomTemplate template; // Minkä mallin mukaan tämä huone on luotu
    public Vector2 Location;      // Sijainti karttaruudukossa (esim. 0,0 tai 1,0).
    public Image RoomImage;       // Viittaus minikartan UI-Imageen.
    public GameObject RoomInstance { get; set; } // Viittaus instansioituun huone-GameObjectiin scenessä.
    public bool IsExplored { get; set; } = false; // Onko pelaaja käynyt tässä huoneessa?
    public int recursionDepth = 0; // Kuinka "syvällä" tämä huone on generoinnissa (debug/failsafe)

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