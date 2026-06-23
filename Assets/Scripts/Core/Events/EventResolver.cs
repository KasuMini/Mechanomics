using UnityEngine;

// Skill-check math. Used by SkillEvent and covered by EventResolverTests.
public static class EventResolver
{
    // 0 -> -2, 5 -> 0, 10 -> +2
    public static int ReliabilityModifier(int reliability)
    {
        return Mathf.RoundToInt((Mathf.Clamp(reliability, 0, 10) - 5) * 0.4f);
    }

    public static EventOutcome Resolve(SkillEvent ev, int testedStatValue, int reliability, int roll)
    {
        int total = roll + testedStatValue + ReliabilityModifier(reliability);
        bool success = total >= ev.difficulty;

        return new EventOutcome
        {
            Success = success,
            Roll = roll,
            Total = total,
            CashDelta = success ? ev.cashReward : -ev.cashPenalty,
            QuotaDelta = success ? ev.quotaReward : 0,
            MechDisabled = !success && ev.disableMechOnFailure,
            ResultText = success ? ev.successText : ev.failureText,
        };
    }

    public static EventOutcome Resolve(SkillEvent ev, int testedStatValue, int reliability, System.Random rng)
    {
        int sides = Mathf.Max(2, ev.diceSides);
        return Resolve(ev, testedStatValue, reliability, rng.Next(1, sides + 1));
    }
}
