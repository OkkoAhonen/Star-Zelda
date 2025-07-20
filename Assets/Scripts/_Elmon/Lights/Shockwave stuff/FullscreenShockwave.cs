using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FullscreenShockwave : MonoBehaviour
{
    [SerializeField] private Material shockwaveMat; // The material using the shader graph
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private float strength = 0.05f;
    [SerializeField] private float radius = 0.5f;

    private float waveTime = 0f;

    public void Trigger(Vector2 screenPosition)
    {
        waveTime = 0f;
        shockwaveMat.SetVector("_Center", screenPosition);
    }

    void Update()
    {
        waveTime += Time.deltaTime * waveSpeed;

        shockwaveMat.SetFloat("_Time", waveTime);
        shockwaveMat.SetFloat("_DistortionStrength", strength);
        shockwaveMat.SetFloat("_Radius", radius);
    }
}
