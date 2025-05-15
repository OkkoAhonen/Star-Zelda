using UnityEngine;
using UnityEngine.UI;

public class GainExperienceButton : MonoBehaviour
{
    [Header("Button Settings")]
    public Button experienceButton;
    public float experienceAmount = 10f;

    private void Start()
    {
        if (experienceButton != null)
        {
            experienceButton.onClick.AddListener(() => GainExperience(experienceAmount));
        }
    }

    public void GainExperience(float experience)
    {
        GameEventsManager.instance.playerEvents.GainExperience(experience);
    }
}
