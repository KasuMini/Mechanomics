using UnityEngine;

[CreateAssetMenu(fileName = "Event", menuName = "Mechanomics/Event", order = 0)]
public class EventData : ScriptableObject
{
    [Header("Identity")]
    public string title;
    [TextArea(2, 5)] public string description;

    [Header("Challenge")]
    public MechStat testedStat = MechStat.Strength;
    [Range(1, 25)] public int difficulty = 10;
    [Min(2)] public int diceSides = 10;

    [Header("Logistics")]
    [Range(1, 4)] public int suggestedSize = 1;

    [Header("On success")]
    public int cashReward = 500;
    public int quotaReward = 1;
    [TextArea(2, 5)] public string successText;

    [Header("On failure")]
    public int cashPenalty = 0;
    public bool disableMechOnFailure = false;
    [TextArea(2, 5)] public string failureText;
}