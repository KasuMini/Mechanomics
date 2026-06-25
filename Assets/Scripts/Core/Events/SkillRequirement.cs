using UnityEngine;

// One skill gate on an event: a stat and how much of it the committed mechs must total.
[System.Serializable]
public struct SkillRequirement
{
    public MechStat stat;
    [Min(1)] public int amount;

    public SkillRequirement(MechStat stat, int amount)
    {
        this.stat = stat;
        this.amount = amount;
    }
}
