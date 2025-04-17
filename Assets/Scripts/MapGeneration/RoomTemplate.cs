using UnityEngine;

// ScriptableObject huonemalleille.
[CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "Generation/Room Template")]
public class RoomTemplate : ScriptableObject
{
    [Header("Identification")]
    // Mink‰ tyyppinen huone t‰m‰ malli edustaa.
    public RoomType type;

    [Header("Visuals")]
    // Prefab, joka instansioidaan pelimaailmaan t‰t‰ huonetta varten.
    // T‰m‰n prefabin tulee sis‰lt‰‰ "Doors"-niminen lapsiobjekti,
    // jonka alla on trigger colliderit nimill‰ "LeftDoor", "RightDoor", "TopDoor", "BottomDoor".
    public GameObject prefab;

    // Voit lis‰t‰ t‰nne muita huonetyyppikohtaisia asetuksia,
    // esim. vihollislistoja, esineit‰, jne.
}