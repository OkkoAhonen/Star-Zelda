using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{ // Tää menis sille objectille jolla on ScrollRect LOL
    public float scrollMultiplier = 30f;
    private ScrollRect scrollRect;

    void Awake() => scrollRect = GetComponent<ScrollRect>();

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
            scrollRect.verticalNormalizedPosition += scroll * scrollMultiplier * Time.deltaTime;
    }
}
