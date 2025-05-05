using System.Collections.Generic;
using UnityEngine;

// Tämä attribuutti mahdollistaa tämän tyyppisten assetien luomisen
// Unityn Project-ikkunassa (Create -> Generation -> Level Settings).
[CreateAssetMenu(fileName = "NewLevelSettings", menuName = "Generation/Level Settings")]
public class LevelGenerationSettings : ScriptableObject
{
    [Header("Map Layout")]
    // Maksimietäisyys (ruutuina) lähtöhuoneesta (0,0), johon huoneita voidaan generoida.
    public int roomLimit = 3;
    // Todennäköisyys (0.0 - 1.0) sille, että generointi yrittää jatkua uuteen huoneeseen.
    // Korkeampi arvo = enemmän haaroja ja huoneita (todennäköisemmin).
    [Range(0f, 1f)]
    public float branchingChance = 0.6f;

    // --- MUUTETUT/UUDET KENTÄT ---
    [Tooltip("Minimum TOTAL number of rooms to generate (including start, boss, treasure etc.)")]
    public int minTotalRooms = 10; // Uusi minimi kaikille huoneille

    [Tooltip("Maximum TOTAL number of rooms allowed (generation stops if this limit is reached).")]
    public int maxTotalRooms = 20; // Uusi maksimi kaikille huoneille

    // Vanha minRooms poistettu (tai kommentoi pois jos haluat säilyttää sen jostain syystä)
    // public int minRooms = 8;

    [Header("Minimap Settings")]
    public float minimapIconBaseSize = 500f;
    public float minimapIconScale = 0.06f;
    public float minimapPadding = 0.1f;

    [Header("Room Templates")]
    [Tooltip("Template for the starting room (REQUIRED). Must have a Prefab assigned.")]
    public RoomTemplate startRoomTemplate; // Varmista että Prefab on asetettu!

    [Tooltip("Template for the boss room (REQUIRED). Must have a Prefab assigned.")]
    public RoomTemplate bossRoomTemplate; // Varmista että Prefab on asetettu!

    [Tooltip("Template for the treasure room (Optional). Assign if used.")]
    public RoomTemplate treasureRoomTemplate;

    [Tooltip("List of possible templates for normal rooms (REQUIRED, at least one valid entry). Ensure assigned Templates have Prefabs.")]
    public List<RoomTemplate> normalRoomTemplates; // Varmista että listassa on ainakin yksi, ja sillä Prefab!

    [Header("Minimap Icons")]
    public Sprite currentRoomIcon;
    public Sprite bossRoomIcon;
    public Sprite treasureRoomIcon;
    public Sprite normalRoomIcon;
    public Sprite unexploredRoomIcon;

    [Header("Generation Control")]
    public int seed = 0;
    public bool useSeed = false;
    public int maxRecursionDepth = 50;

    [Tooltip("How many times to retry the ENTIRE level generation if critical placement (like Boss room) fails.")]
    public int maxRegenerationAttempts = 10; // Nostettu oletusarvoa hieman varmuuden lisäämiseksi

    [Header("Gameplay")]
    public float roomChangeTime = 1f;
}