using System.Collections.Generic;
using UnityEngine;

// T‰m‰ attribuutti mahdollistaa t‰m‰n tyyppisten assetien luomisen
// Unityn Project-ikkunassa (Create -> Generation -> Level Settings).
[CreateAssetMenu(fileName = "NewLevelSettings", menuName = "Generation/Level Settings")]
public class LevelGenerationSettings : ScriptableObject
{
    [Header("Map Layout")]
    // Maksimiet‰isyys (ruutuina) l‰htˆhuoneesta (0,0), johon huoneita voidaan generoida.
    public int roomLimit = 3;
    // Todenn‰kˆisyys (0.0 - 1.0) sille, ett‰ generointi yritt‰‰ jatkua uuteen huoneeseen.
    // Korkeampi arvo = enemm‰n haaroja ja huoneita (todenn‰kˆisemmin).
    [Range(0f, 1f)]
    public float branchingChance = 0.6f; // Nimi muutettu selke‰mm‰ksi
    // Generoinnin takuu: Minimim‰‰r‰ huoneita, joita yritet‰‰n generoida (pl. erikoishuoneet).
    public int minRooms = 8;

    [Header("Minimap Settings")]
    // N‰m‰ vaikuttavat minikartan ikonien kokoon ja sijoitteluun.
    // K‰ytet‰‰n vain height, koska ikonit ovat neliˆit‰.
    public float minimapIconBaseSize = 500f; // Selke‰mpi nimi
    // Skaalauskerroin nimenomaan minikartan ikoneille. M‰‰ritt‰‰ ikonin koon.
    public float minimapIconScale = 0.06f;
    // Tyhj‰ tila (padding) minikartan ikonien v‰lill‰ suhteessa ikonin kokoon.
    public float minimapPadding = 0.1f; // Suhteellinen padding on usein helpompi hallita

    [Header("Room Templates")]
    // Viittaukset huonemalleihin (RoomTemplate ScriptableObjecteihin).
    // N‰m‰ asetetaan Inspectorissa vet‰m‰ll‰ oikeat assetit kenttiin.
    public RoomTemplate startRoomTemplate;
    public RoomTemplate bossRoomTemplate;
    public RoomTemplate treasureRoomTemplate;
    // Lista kaikista mahdollisista "tavallisista" huoneista, joita voidaan arpoa.
    public List<RoomTemplate> normalRoomTemplates;
    // Voit lis‰t‰ listoja muille tyypeille (Shop, Secret jne.)

    [Header("Minimap Icons")]
    // Sprite-referenssit minikartan eri huonetyyppien ikoneille.
    // N‰m‰ voidaan nyt asettaa suoraan t‰h‰n assettiin Inspectorissa.
    public Sprite currentRoomIcon;
    public Sprite bossRoomIcon;
    public Sprite treasureRoomIcon;
    public Sprite normalRoomIcon; // Korvaa vanhan DefaultRoomIconin
    public Sprite unexploredRoomIcon; // Lis‰t‰‰n tuki t‰lle

    [Header("Generation Control")]
    // Satunnaislukugeneraattorin siemenluku. Jos useSeed = true, sama siemenluku tuottaa aina saman kartan.
    public int seed = 0;
    public bool useSeed = false;
    // Failsafe: Maksimi rekursiosyvyys est‰m‰‰n liian syv‰t/jumittuvat generoinnit.
    public int maxRecursionDepth = 50;
    // Failsafe: Maksimi yritysm‰‰r‰ generoida kartta uudelleen, jos esim. aarrehuonetta ei saada sijoitettua.
    public int maxRegenerationAttempts = 5;

    [Header("Gameplay")]
    // Aika sekunneissa, jonka pelaajan pit‰‰ odottaa ennen kuin voi vaihtaa huonetta uudelleen.
    public float roomChangeTime = 1f;
}