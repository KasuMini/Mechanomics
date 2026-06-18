using UnityEngine;

[System.Serializable]
public class MechData
{
    [SerializeField] public string mechName;
    [SerializeField] public string pilotName;
    [SerializeField] public int agilityStat;
    [SerializeField] public int strengthStat;
    [SerializeField] public int systemsStat;
    [SerializeField] public float reliabilityStat;
    // Converts all stats to floats, multiple by value, then floor to int to get cost of mech 
    public int cost { get { return Mathf.FloorToInt(((float)agilityStat + (float)strengthStat + (float)systemsStat) * reliabilityStat); } }

    // Constructor
    public MechData(string newName, string newPilot, int newAgility, int newStrength, int newSystems, int newReliability)
    {
        this.mechName = newName;
        this.pilotName = newPilot;
        this.agilityStat = newAgility;
        this.strengthStat = newStrength;
        this.reliabilityStat = newReliability;
    }
}
