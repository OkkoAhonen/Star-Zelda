using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static PlayerStatsManager;

// Character UI manager that shows Aspect levels and Attributes.
// It listens for PlayerStatsManager changes and updates UI accordingly.
public class CharacterUIManager : MonoBehaviour
{
    [Header("Stat entry prefab")]
    [SerializeField] private GameObject statEntryPrefab;

    [Header("Containers")]
    [SerializeField] private Transform bodyContainer;
    [SerializeField] private Transform mindContainer;
    [SerializeField] private Transform soulContainer;

    [Header("Aspect controls")]
    [SerializeField] private TMP_Text bodyPoints;
    [SerializeField] private TMP_Text mindPoints;
    [SerializeField] private TMP_Text soulPoints;
    [SerializeField] private Button bodySpendButton;
    [SerializeField] private Button mindSpendButton;
    [SerializeField] private Button soulSpendButton;

    private PlayerStatsManager stats;

    private readonly Dictionary<AttributeType, TMP_Text> attributeValueTexts = new Dictionary<AttributeType, TMP_Text>();
    private readonly Dictionary<AttributeType, Button> attributeSpendButtons = new Dictionary<AttributeType, Button>();

    private void Awake()
    {
        stats = PlayerStatsManager.instance;
    }

    private void Start()
    {
        BuildUI();
        RefreshUI();

        // subscribe to a stats-changed event if available
        GameEventsManager.instance.playerEvents.onStatsChanged += OnStatsChanged;

        bodySpendButton.onClick.AddListener(() =>
        {
            stats.SpendAspectPoint(AspectType.Body);
            RefreshUI();
        });

        mindSpendButton.onClick.AddListener(() =>
        {
            stats.SpendAspectPoint(AspectType.Mind);
            RefreshUI();
        });

        soulSpendButton.onClick.AddListener(() =>
        {
            stats.SpendAspectPoint(AspectType.Soul);
            RefreshUI();
        });
    }

    private void OnDestroy()
    {
        if (GameEventsManager.instance != null && GameEventsManager.instance.playerEvents != null)
            GameEventsManager.instance.playerEvents.onStatsChanged -= OnStatsChanged;
    }

    private void BuildUI()
    {
        // Clear containers
        foreach (Transform t in bodyContainer) Destroy(t.gameObject);
        foreach (Transform t in mindContainer) Destroy(t.gameObject);
        foreach (Transform t in soulContainer) Destroy(t.gameObject);

        // Hardcoded mapping of Attributes to containers (keep in sync with PlayerStatsManager)
        var bodyAttrs = new AttributeType[] { AttributeType.Strength, AttributeType.Vitality };
        var mindAttrs = new AttributeType[] { AttributeType.Agility, AttributeType.Luck, AttributeType.Ranged };
        var soulAttrs = new AttributeType[] { AttributeType.Magic, AttributeType.Alchemy };

        CreateEntriesForCategory(bodyAttrs, bodyContainer);
        CreateEntriesForCategory(mindAttrs, mindContainer);
        CreateEntriesForCategory(soulAttrs, soulContainer);
    }

    private void CreateEntriesForCategory(AttributeType[] attributes, Transform container)
    {
        foreach (var attr in attributes)
        {
            var go = Instantiate(statEntryPrefab, container);
            var nameText = go.transform.GetChild(0).GetComponent<TMP_Text>();
            var valueText = go.transform.GetChild(1).GetComponent<TMP_Text>();
            var btn = go.transform.GetChild(2).GetComponent<Button>();

            nameText.text = attr.ToString();
            valueText.text = stats.GetAttribute(attr).ToString();

            attributeValueTexts[attr] = valueText;
            attributeSpendButtons[attr] = btn;

            AttributeType captured = attr;
            btn.onClick.AddListener(() =>
            {
                stats.SpendAttributePoint(captured);
                RefreshUI();
            });

            // Tooltip trigger
            var trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((d) => TooltipManager.Instance.Hide());
            trigger.triggers.Add(entryExit);
        }
    }

    public void RefreshUI()
    {
        bodyPoints.text = stats.GetRemainingAspectPoints(AspectType.Body).ToString();
        mindPoints.text = stats.GetRemainingAspectPoints(AspectType.Mind).ToString();
        soulPoints.text = stats.GetRemainingAspectPoints(AspectType.Soul).ToString();

        foreach (var kv in attributeValueTexts)
        {
            kv.Value.text = stats.GetAttribute(kv.Key).ToString();
        }
    }

    private void OnStatsChanged()
    {
        RefreshUI();
    }
}
