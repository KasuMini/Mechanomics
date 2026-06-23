using System;
using UnityEngine;

// A single-stat skill check, resolved by EventResolver.
[Serializable]
public class SkillEvent : EventBehaviour
{
    [Header("Challenge")]
    public MechStat testedStat = MechStat.Strength;
    [Range(1, 25)] public int difficulty = 10;
    [Min(2)] public int diceSides = 10;

    [Header("On success")]
    public int cashReward = 500;
    public int quotaReward = 1;
    [TextArea(2, 5)] public string successText;

    [Header("On failure")]
    public int cashPenalty = 0;
    public bool disableMechOnFailure = false;
    [TextArea(2, 5)] public string failureText;

    public override string Summary => $"{testedStat} check (DC {difficulty})";

    public override EventOutcome Resolve(IMechStats mech, System.Random rng)
    {
        int statValue = mech.GetStat(testedStat);
        return EventResolver.Resolve(this, statValue, mech.Reliability, rng);
    }
}
