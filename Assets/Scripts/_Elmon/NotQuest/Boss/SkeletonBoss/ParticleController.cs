using UnityEngine;

public class ParticleController : MonoBehaviour
{ // This script is literally just to diable the object
    // call in animation to turn off this particle GameObject
    public void DisableObject()
    {
        gameObject.SetActive(false);
    }
}