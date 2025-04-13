using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewTrait", menuName = "Game Data/Trait")]
public class Trait : ScriptableObject
{
    [SerializeField] private string _traitName;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private int _requiredKills = 1;

    public string traitName => _traitName;
    public string description => _description;
    public Sprite icon => _icon;
    public int requiredKills => _requiredKills;
}
