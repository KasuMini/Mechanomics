using System;
using System.Collections.Generic;
using UnityEngine;

// A dice-pool skill check: each assigned mech rolls and the successes are summed vs successesRequired.
[Serializable]
public class SkillEvent : EventBehaviour
{
    [Header("Challenge")]
    public MechStat testedStat = MechStat.Strength;
    [Min(1)] public int dc = 8;
    [Min(1)] public int successesRequired = 1;
    [Min(2)] public int diceSides = 6;

    [Header("Rewards")]
    public int cashReward = 500;
    public int partialReward = 150;
    public int quotaReward = 1;
    public int cashPenalty = 100;

    [Header("Result text")]
    [TextArea(2, 4)] public string successText;
    [TextArea(2, 4)] public string partialText;
    [TextArea(2, 4)] public string failureText;

    public override string Summary => $"{testedStat} ×{successesRequired} (DC {dc})";

    public override EventOutcome Resolve(IReadOnlyList<IMechStats> mechs, System.Random rng)
    {
        int successes = 0;
        if (mechs != null)
        {
            foreach (IMechStats m in mechs)
            {
                int total = EventResolver.Roll(rng, diceSides)
                          + m.GetStat(testedStat)
                          + EventResolver.ReliabilityModifier(m.Reliability);
                successes += EventResolver.DieSuccesses(total, dc, EventResolver.SUCCESS_MARGIN);
            }
        }

        OutcomeDegree degree = EventResolver.Degree(successes, successesRequired);
        var outcome = new EventOutcome { Degree = degree, Successes = successes, Required = successesRequired };

        switch (degree)
        {
            case OutcomeDegree.Success:
                outcome.CashDelta = cashReward; outcome.QuotaDelta = quotaReward; outcome.ResultText = successText; break;
            case OutcomeDegree.Partial:
                outcome.CashDelta = partialReward; outcome.ResultText = partialText; break;
            default:
                outcome.CashDelta = -cashPenalty; outcome.ResultText = failureText; break;
        }
        return outcome;
    }
}
