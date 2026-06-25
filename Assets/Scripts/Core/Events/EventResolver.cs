using System.Collections.Generic;
using UnityEngine;

// Pure resolution helpers. Covered by EventResolverTests.
public static class EventResolver
{
    public static int SumStat(IReadOnlyList<IMechStats> mechs, MechStat stat)
    {
        if (mechs == null) return 0;
        int sum = 0;
        foreach (IMechStats m in mechs) sum += m.GetStat(stat);
        return sum;
    }

    // min(sum, amount) / amount — excess over a requirement is ignored.
    public static float Coverage(int sum, int amount)
    {
        return amount <= 0 ? 1f : Mathf.Clamp01((float)sum / amount);
    }

    // Product of every requirement's coverage; no requirements -> 1.
    public static float SuccessChance(IReadOnlyList<SkillRequirement> reqs, IReadOnlyList<IMechStats> mechs)
    {
        if (reqs == null || reqs.Count == 0) return 1f;
        float chance = 1f;
        for (int i = 0; i < reqs.Count; i++)
            chance *= Coverage(SumStat(mechs, reqs[i].stat), reqs[i].amount);
        return chance;
    }

    // 0 -> -2, 5 -> 0, 10 -> +2. Kept for the future reliability stub.
    public static int ReliabilityModifier(int reliability)
    {
        return Mathf.RoundToInt((Mathf.Clamp(reliability, 0, 10) - 5) * 0.4f);
    }
}
