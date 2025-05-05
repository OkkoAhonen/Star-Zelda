using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterUIManager : MonoBehaviour
{
    [Header("Stats UI")]
    [SerializeField] private Transform statContainer;
    [SerializeField] private GameObject statPrefab;

    [Header("Stat Categories")]
    [SerializeField] private Transform bodyStatContainer;
    [SerializeField] private Transform accuracyStatContainer;
    [SerializeField] private Transform magicPowerStatContainer;

    [Header("Remaining Points")]
    [SerializeField] private TMP_Text bodyPointsText;
    [SerializeField] private TMP_Text accuracyPointsText;
    [SerializeField] private TMP_Text magicPointsText;

    [Header("Attributes UI")]
    [SerializeField] private TMP_Text attributePointsText;
    [SerializeField] private TMP_Text bodyAttributeText;
    [SerializeField] private TMP_Text accuracyAttributeText;
    [SerializeField] private TMP_Text magicAttributeText;

    [SerializeField] private Button bodyAttributeButton;
    [SerializeField] private Button accuracyAttributeButton;
    [SerializeField] private Button magicAttributeButton;

    public static CharacterUIManager instance { get; private set; }

    private Dictionary<StatType, TMP_Text> statTexts = new Dictionary<StatType, TMP_Text>();
    private Dictionary<StatType, Button> buttonByStat = new Dictionary<StatType, Button>();

    private PlayerStatsManager playerStatsManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        PlayerEvents playerEvents = GameEventsManager.instance.playerEvents;
        // you could subscribe to onStatChanged here if needed:
        // playerEvents.onStatChange += UpdateStatDisplay;

        playerStatsManager = PlayerStatsManager.instance;

        InitializeStatsUI();

        bodyAttributeButton.onClick.AddListener(() =>
        {
            playerStatsManager.SpendAttributePoint("body");
            RefreshUI();
        });

        accuracyAttributeButton.onClick.AddListener(() =>
        {
            playerStatsManager.SpendAttributePoint("accuracy");
            RefreshUI();
        });

        magicAttributeButton.onClick.AddListener(() =>
        {
            playerStatsManager.SpendAttributePoint("magicpowers");
            RefreshUI();
        });

        RefreshUI();
    }

    private void InitializeStatsUI()
    {
        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            GameObject statEntry = Instantiate(statPrefab);

            TMP_Text statNameText = statEntry.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text statValueText = statEntry.transform.GetChild(1).GetComponent<TMP_Text>();
            Button increaseButton = statEntry.transform.GetChild(2).GetComponent<Button>();

            // Use our StatDefinition SO to get display name and description
            StatDefinition def = StatDefinition.Get(stat);
            if (def != null)
            {
                statNameText.text = def.displayName;
                // If your prefab has a 4th text for description:
                if (statEntry.transform.childCount > 3)
                {
                    TMP_Text statDescText = statEntry.transform.GetChild(3).GetComponent<TMP_Text>();
                    statDescText.text = def.description;
                }
            }
            else
            {
                statNameText.text = stat.ToString();
            }

            statValueText.text = playerStatsManager.GetStat(stat).ToString();

            StatType capturedStat = stat;
            increaseButton.onClick.AddListener(() =>
            {
                playerStatsManager.SpendLevelPointOnStat(capturedStat);
                RefreshUI();
            });

            statTexts[stat] = statValueText;
            buttonByStat[stat] = increaseButton;

            // Parent under correct category
            if (playerStatsManager.IsBodyStat(stat))
            {
                statEntry.transform.SetParent(bodyStatContainer, false);
            }
            else if (playerStatsManager.IsAccuracyStat(stat))
            {
                statEntry.transform.SetParent(accuracyStatContainer, false);
            }
            else if (playerStatsManager.IsMagicStat(stat))
            {
                statEntry.transform.SetParent(magicPowerStatContainer, false);
            }
            else
            {
                statEntry.transform.SetParent(statContainer, false);
            }

            // Can use the same block of code for all SO

            EventTrigger trigger = statEntry.GetComponent<EventTrigger>();
            if (trigger == null) trigger = statEntry.AddComponent<EventTrigger>();

            // Pointer Enter => Show(def)
            var entryEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entryEnter.callback.AddListener((data) =>
                TooltipManager.Instance.Show(def, Input.mousePosition)
            );
            trigger.triggers.Add(entryEnter);

            // Pointer Exit => Hide()
            var entryExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            entryExit.callback.AddListener((data) =>
                TooltipManager.Instance.Hide()
            );
            trigger.triggers.Add(entryExit);
        }
    }

    public void RefreshUI()
    {
        // Update remaining points
        bodyPointsText.text = playerStatsManager.GetRemainingPoints("body").ToString();
        accuracyPointsText.text = playerStatsManager.GetRemainingPoints("accuracy").ToString();
        magicPointsText.text = playerStatsManager.GetRemainingPoints("magicpowers").ToString();

        attributePointsText.text = playerStatsManager.GetRemainingAttributePoints().ToString();

        bodyAttributeText.text = playerStatsManager.GetAttribute("body").ToString();
        accuracyAttributeText.text = playerStatsManager.GetAttribute("accuracy").ToString();
        magicAttributeText.text = playerStatsManager.GetAttribute("magicpowers").ToString();

        bool canSpend = playerStatsManager.GetRemainingAttributePoints() > 0;
        bodyAttributeButton.interactable = canSpend;
        accuracyAttributeButton.interactable = canSpend;
        magicAttributeButton.interactable = canSpend;

        foreach (KeyValuePair<StatType, TMP_Text> entry in statTexts)
        {
            StatType stat = entry.Key;
            TMP_Text valueText = entry.Value;
            valueText.text = playerStatsManager.GetStat(stat).ToString();

            Button btn = buttonByStat[stat];
            bool enabled = false;

            if (playerStatsManager.IsBodyStat(stat))
            {
                enabled = playerStatsManager.GetRemainingPoints("body") > 0;
            }
            else if (playerStatsManager.IsAccuracyStat(stat))
            {
                enabled = playerStatsManager.GetRemainingPoints("accuracy") > 0;
            }
            else if (playerStatsManager.IsMagicStat(stat))
            {
                enabled = playerStatsManager.GetRemainingPoints("magicpowers") > 0;
            }

            btn.interactable = enabled;
        }
    }

    // Optionally, handle external stat-change events:
    private void UpdateStatDisplay(StatType statType, int newValue)
    {
        if (statTexts.TryGetValue(statType, out TMP_Text text))
        {
            text.text = newValue.ToString();
        }
    }
}
