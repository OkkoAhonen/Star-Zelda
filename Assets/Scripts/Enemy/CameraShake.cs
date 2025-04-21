using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // Singleton instance (staattinen viittaus t‰h‰n skriptiin)
    public static CameraShake instance;

    private Vector3 originalPos;
    private Coroutine currentShakeCoroutine = null; // Seuraa aktiivista t‰rin‰-coroutinea

    void Awake()
    {
        // Singleton pattern: Varmistaa, ett‰ vain yksi CameraShake-instanssi on olemassa
        if (instance == null)
        {
            instance = this;
            // Voit harkita DontDestroyOnLoad(gameObject); jos kamerasi s‰ilyy scenejen v‰lill‰
        }
        else if (instance != this)
        {
            // Jos toinen instanssi yritt‰‰ synty‰, tuhotaan se
            Debug.LogWarning("Toinen CameraShake-instanssi lˆydetty ja tuhottu.", gameObject);
            Destroy(gameObject);
        }
    }

    // Julkinen metodi, jota muut skriptit voivat kutsua t‰rin‰n aloittamiseksi
    public void StartShake(float duration, float magnitude)
    {
        // Jos t‰rin‰ on jo p‰‰ll‰, pys‰ytet‰‰n vanha ennen uuden aloittamista
        // T‰m‰ est‰‰ useita p‰‰llekk‰isi‰ t‰rinˆit‰ ja resetoi vanhan kesken j‰‰neen
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            // Palauta sijainti heti, jos vanha keskeytettiin
            if (transform != null) transform.localPosition = originalPos;
        }

        // Aloita uusi t‰rin‰ Coroutine ja tallenna viittaus siihen
        currentShakeCoroutine = StartCoroutine(Shake(duration, magnitude));
    }

    // Coroutine, joka suorittaa itse t‰rin‰n
    private IEnumerator Shake(float duration, float magnitude)
    {
        // Tallenna kameran alkuper‰inen paikallinen sijainti t‰rin‰n alussa
        // K‰yt‰ localPositionia, jotta toimii oikein, vaikka kamera olisi parentoitu
        originalPos = transform.localPosition;

        float elapsed = 0.0f; // Kulunut aika

        // Silmukka, joka kest‰‰ m‰‰ritetyn duration-ajan
        while (elapsed < duration)
        {
            // Laske satunnainen siirtym‰ X- ja Y-akseleilla magnituden perusteella
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Aseta kameran sijainti alkuper‰iseen sijaintiin lis‰ttyn‰ satunnaisella siirtym‰ll‰
            // Z-arvo pidet‰‰n ennallaan (tai originalPos.z)
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            // Lis‰‰ kulunutta aikaa
            elapsed += Time.deltaTime;

            // Odota seuraavaan frameen
            yield return null;
        }

        // Kun t‰rin‰ on p‰‰ttynyt, palauta kamera tarkasti alkuper‰iseen sijaintiinsa
        // T‰m‰ on t‰rke‰‰, jotta kamera ei j‰‰ v‰‰r‰‰n asentoon pienten ep‰tarkkuuksien vuoksi
        if (transform != null) transform.localPosition = originalPos;

        // Nollaa coroutine-viittaus, kun t‰rin‰ on valmis
        currentShakeCoroutine = null;
    }
}