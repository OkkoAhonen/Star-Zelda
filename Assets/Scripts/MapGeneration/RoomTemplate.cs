using UnityEngine;

// ScriptableObject huonemalleille.
[CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "Generation/Room Template")]
public class RoomTemplate : ScriptableObject
{
    [Header("Identification")]
    public RoomType type;

    [Header("Visuals")]
    public GameObject prefab; // Tämän tulee sisältää Doors-objekti jne.

    [Header("Camera Behavior")]
    [Tooltip("Should the camera follow the player within this room instead of being static?")]
    public bool followPlayerCamera = false;

    // POISTETTU KENTTÄ:
    // [Tooltip("Collider used to determine the bounds...")]
    // public Collider2D roomBoundsCollider;

    // ... (Muut mahdolliset asetukset) ...
}