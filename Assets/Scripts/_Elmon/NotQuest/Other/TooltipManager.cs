using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private Image tooltipIcon;
    [SerializeField] private TMP_Text tooltipTitle;
    [SerializeField] private TMP_Text tooltipBody;

    private bool isFollowingMouse = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Hide();
    }

    private void Update()
    {
        if (isFollowingMouse)
        {
            Vector2 pos = Input.mousePosition;

            // Optional: Offset so tooltip doesn't cover cursor
            pos += new Vector2(15f, -15f);

            tooltipRoot.transform.position = pos;
        }
    }

    public void Show(ScriptableObject so, Vector2 screenPos)
    {
        // Use reflection to fetch name, description, icon (same as before)

        string title = so.name;
        MemberInfo titleM = GetMember(so, "displayName", "perkName");
        if (titleM != null) title = GetStringValue(so, titleM);

        string body = null;
        MemberInfo bodyM = GetMember(so, "description", "_description");
        if (bodyM != null) body = GetStringValue(so, bodyM);

        Sprite icon = null;
        MemberInfo iconM = GetMember(so, "icon", "_icon");
        if (iconM != null) icon = GetSpriteValue(so, iconM);

        tooltipTitle.text = title;

        tooltipBody.gameObject.SetActive(!string.IsNullOrEmpty(body));
        if (tooltipBody.gameObject.activeSelf)
            tooltipBody.text = body;

        tooltipIcon.gameObject.SetActive(icon != null);
        if (tooltipIcon.gameObject.activeSelf)
            tooltipIcon.sprite = icon;

        tooltipRoot.SetActive(true);
        tooltipRoot.transform.position = screenPos;
        isFollowingMouse = true;
    }

    public void Hide()
    {
        tooltipRoot.SetActive(false);
        isFollowingMouse = false;
    }

    // --- Utility Reflection Methods ---
    private MemberInfo GetMember(ScriptableObject so, params string[] names)
    {
        return so.GetType()
            .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(m =>
                names.Contains(m.Name, System.StringComparer.OrdinalIgnoreCase) &&
                (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property));
    }

    private string GetStringValue(ScriptableObject so, MemberInfo m)
    {
        return m is FieldInfo f ? (string)f.GetValue(so) : (string)((PropertyInfo)m).GetValue(so);
    }

    private Sprite GetSpriteValue(ScriptableObject so, MemberInfo m)
    {
        return m is FieldInfo f ? (Sprite)f.GetValue(so) : (Sprite)((PropertyInfo)m).GetValue(so);
    }
}