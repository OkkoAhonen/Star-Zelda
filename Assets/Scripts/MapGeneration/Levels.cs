using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Staattinen luokka, joka sis‰lt‰‰ globaaleja asetuksia ja tietoja koko tasolle (level).
// Staattista luokkaa ei voi instansioida (eli et voi tehd‰ `new Levels()`), 
// vaan sen j‰seniin viitataan suoraan luokan nimen kautta (esim. Levels.width).
public static class Levels
{
    // N‰m‰ vaikuttavat minikartan ikonien kokoon ja sijoitteluun.
    // Nimet ovat hieman harhaanjohtavia, koska ne eiv‰t suoraan m‰‰rit‰ pelimaailman
    // huoneiden kokoa, vaan minikartan elementtien kokoa suhteessa johonkin perusmittaan.
    public static float height = 500;
    public static float width = 500;  // Ei k‰ytet‰ t‰ll‰ hetkell‰ DrawRoomOnMap-metodissa.

    // Yleinen skaalauskerroin, jota k‰ytet‰‰n minikartan ikonien sijainnin laskennassa.
    public static float scale = 1f;
    // Skaalauskerroin nimenomaan minikartan ikoneille. M‰‰ritt‰‰ ikonin koon.
    public static float IconScale = 0.06f;
    // Tyhj‰ tila (padding) minikartan ikonien v‰lill‰.
    public static float padding = 0.01f;

    // Todenn‰kˆisyys sille, ett‰ uutta huonetta *ei* yritet‰ generoida tiettyyn suuntaan.
    // Huom: Nimi on hieman k‰‰nteinen. Jos arvo on 0.5, 50% kerroista yritet‰‰n generoida.
    // Jos arvo olisi 0.2, 80% kerroista yritet‰‰n generoida.
    public static float RoomGenerationChance = 0.5f;

    // Maksimiet‰isyys (ruutuina) l‰htˆhuoneesta (0,0), johon huoneita voidaan generoida.
    // Rajoittaa kartan levi‰mist‰.
    public static int RoomLimit = 3;

    // Sprite-referenssit minikartan eri huonetyyppien ikoneille.
    // N‰m‰ asetetaan GenerateLevel-skriptin Awake-metodissa.
    public static Sprite TreasureRoomIcon;
    public static Sprite BossRoomIcon;
    public static Sprite currentroomIcon;     // Ikoni pelaajan nykyiselle huoneelle
    public static Sprite unexploredroomIcon;  // Ei k‰ytet‰ t‰ll‰ hetkell‰ logiikassa? Vaikuttaa silt‰, ett‰ DefaultRoomIcon hoitaa t‰m‰n.
    public static Sprite DefaultRoomIcon;     // Ikoni tavalliselle, lˆydetylle huoneelle

    // Globaali lista, joka sis‰lt‰‰ kaikki t‰h‰n tasoon generoidut Room-objektit.
    // T‰m‰ on keskeinen tietorakenne kartan hallintaan.
    public static List<Room> Rooms = new List<Room>();

    // Aika sekunneissa, jonka pelaajan pit‰‰ odottaa ennen kuin voi vaihtaa huonetta uudelleen.
    // Est‰‰ nopean edestakaisin siirtymisen.
    public static float RoomChangeTime = 1f;
}

// Edustaa yht‰ generoitua huonetta kartalla ja pelimaailmassa.
