using UnityEngine;

public class MechData
{
    public string mechName;
    public string pilotName;
    public int agilityStat;
    public int strengthStat;
    public int reliabilityStat;
    // Converts all stats to floats, multiple by value, then floor to int to get cost of mech 
    public int cost { get { return Mathf.FloorToInt(((float)agilityStat + (float)strengthStat + (float)reliabilityStat) * value); } }
    public float value = 10;

    // Constructor
    public MechData(string newName, string newPilot, int newAgility, int newStrength, int newReliability, float newValue)
    {
        this.mechName = newName;
        this.pilotName = newPilot;
        this.agilityStat = newAgility;
        this.strengthStat = newStrength;
        this.reliabilityStat = newReliability;
        this.value = newValue;
    }
}
