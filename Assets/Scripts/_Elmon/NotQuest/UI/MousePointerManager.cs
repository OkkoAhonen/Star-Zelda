using UnityEngine;

public class MousePointerManager : MonoBehaviour
{
    public static MousePointerManager Instance { get; private set; }

    [SerializeField] private float defaultSpeed = 1f;
    [SerializeField] private float defaultSize = 1f;
    [SerializeField] private Sprite[] pointerIcons;

    private CursorMode cursorMode = CursorMode.Auto;
    private Vector2 hotspot = Vector2.zero;

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        // load saved settings
        float speed = PlayerPrefs.GetFloat("MouseSpeed", defaultSpeed);
        float size = PlayerPrefs.GetFloat("PointerSize", defaultSize);
        int iconIdx = PlayerPrefs.GetInt("PointerIcon", 0);
        SetSpeed(speed);
        SetSize(size);
        SetIcon(iconIdx);
    }

    public void SetSpeed(float value)
    {
        PlayerPrefs.SetFloat("MouseSpeed", value);
        // Implementation depends on input system; for example:
        // UnityEngine.InputSystem.Mouse.current.velocitySensitivity = value;
    }

    public void SetSize(float value)
    {
        PlayerPrefs.SetFloat("PointerSize", value);
        // scale the hardware cursor via OS settings is not possible; you may need a custom cursor mesh.
        Vector2 scaledHotspot = hotspot * value;
        // re-apply icon to update size
        Sprite current = pointerIcons[PlayerPrefs.GetInt("PointerIcon", 0)];
        Cursor.SetCursor(current.texture, scaledHotspot, cursorMode);
    }

    public void SetIcon(int idx)
    {
        if (idx < 0 || idx >= pointerIcons.Length) return;
        PlayerPrefs.SetInt("PointerIcon", idx);
        Sprite icon = pointerIcons[idx];
        Cursor.SetCursor(icon.texture, hotspot, cursorMode);
    }

    public Sprite GetIcon(int idx)
    {
        if (idx < 0 || idx >= pointerIcons.Length) return null;
        return pointerIcons[idx];
    }
}