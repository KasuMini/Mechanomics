using UnityEngine;

[CreateAssetMenu(fileName = "MechData", menuName = "Mechanomics/MechData", order = 0)]
public class MechData : ScriptableObject
{
    [Header("Identity")]
    public string mechName;
    public string pilotName;

    [Header("Stat")]
    [Range(0, 10)]public int agilityStat;
    [Range(0, 10)]public int strengthStat;
    [Range(0, 4)]public int systemsStat;
    [Range(0, 10)]public int reliabilityStat;
    [Range(1, 3)] public int size = 1;

    // Converts all stats to floats, multiple by value, then floor to int to get cost of mech
    public int cost { get { return Mathf.FloorToInt(((float)agilityStat + (float)strengthStat + (float)systemsStat) * reliabilityStat); } }

    // Constructor
    public MechData(string newName, string newPilot, int newAgility, int newStrength, int newSystems, int newReliability, int newSize)
    {
        this.mechName = newName;
        this.pilotName = newPilot;
        this.agilityStat = newAgility;
        this.strengthStat = newStrength;
        this.systemsStat = newSystems;
        this.reliabilityStat = newReliability;
        this.size = newSize;
    }
}
