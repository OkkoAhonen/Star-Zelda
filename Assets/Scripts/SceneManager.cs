using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneManager : MonoBehaviour
{
    // Vaihda scene nimell�
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Vaihda scene indeksi� k�ytt�en
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Lataa uudelleen nykyinen scene
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Lopeta peli (toimii vain buildatussa peliss�)
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void loadCaveScene1()
    {
        SceneManager.LoadScene("Okon kokeilu Scene");
    }

    public void LoadVillageScene()
    {
        SceneManager.LoadScene("Town kokeilu");
    }
}
