using UnityEngine;

public static class EventResolver
{
    // 0 -> -2, 5 -> 0, 10 -> +2
    public static int ReliabilityModifier(int reliability)
    {
        return Mathf.RoundToInt((Mathf.Clamp(reliability, 0, 10) - 5) * 0.4f);
    }

    public static EventOutcome Resolve(EventData job, int testedStatValue, int reliability, int roll)
    {
        int total = roll + testedStatValue + ReliabilityModifier(reliability);
        bool success = total >= job.difficulty;

        return new EventOutcome
        {
            Success = success,
            Roll = roll,
            Total = total,
            CashDelta = success ? job.cashReward : -job.cashPenalty,
            QuotaDelta = success ? job.quotaReward : 0,
            MechDisabled = !success && job.disableMechOnFailure,
            ResultText = success ? job.successText : job.failureText,
        };
    }

    public static EventOutcome Resolve(EventData job, int testedStatValue, int reliability, System.Random rng)
    {
        int sides = Mathf.Max(2, job.diceSides);
        return Resolve(job, testedStatValue, reliability, rng.Next(1, sides + 1));
    }
}
