using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// A multi-skill check: each requirement's coverage (capped at 1, excess ignored) multiplies into a
// success chance the dispatch rolls against.
[Serializable]
public class SkillEvent : EventBehaviour
{
    [Header("Requirements")]
    public List<SkillRequirement> requirements = new List<SkillRequirement>();

    [Header("Rewards")]
    public int cashReward = 500;
    public int quotaReward = 1;
    public int cashPenalty = 100;

    [Header("Result text")]
    [TextArea(2, 4)] public string successText;
    [TextArea(2, 4)] public string failureText;

    public override string Summary
    {
        get
        {
            if (requirements == null || requirements.Count == 0) return "(no requirements)";
            var parts = new string[requirements.Count];
            for (int i = 0; i < requirements.Count; i++)
                parts[i] = $"{requirements[i].stat} {requirements[i].amount}";
            return string.Join(", ", parts);
        }
    }

    public override EventOutcome Resolve(IReadOnlyList<IMechStats> mechs, System.Random rng)
    {
        float chance = EventResolver.SuccessChance(requirements, mechs);
        bool ok = rng.NextDouble() < chance;

        // TODO(reliability): per-unreliable-mech malfunction -> Partial + malus goes here.
        var outcome = new EventOutcome { Chance = chance };
        if (ok)
        {
            outcome.Degree = OutcomeDegree.Success;
            outcome.CashDelta = cashReward;
            outcome.QuotaDelta = quotaReward;
            outcome.ResultText = successText;
        }
        else
        {
            outcome.Degree = OutcomeDegree.Fail;
            outcome.CashDelta = -cashPenalty;
            outcome.ResultText = failureText;
        }
        return outcome;
    }

    public override string Preview(IReadOnlyList<IMechStats> selected)
    {
        var sb = new StringBuilder();
        if (requirements != null)
        {
            for (int i = 0; i < requirements.Count; i++)
            {
                SkillRequirement r = requirements[i];
                int sum = EventResolver.SumStat(selected, r.stat);
                sb.AppendLine($"{r.stat} {sum}/{r.amount}");
            }
        }
        float chance = EventResolver.SuccessChance(requirements, selected);
        sb.Append($"Success: {chance:P0}");
        return sb.ToString();
    }
}
