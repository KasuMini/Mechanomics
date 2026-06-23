using UnityEngine;

// Pure dice-pool helpers. Used by the event behaviours and covered by EventResolverTests.
public static class EventResolver
{
    public const int SUCCESS_MARGIN = 4;

    // 0 -> -2, 5 -> 0, 10 -> +2
    public static int ReliabilityModifier(int reliability)
    {
        return Mathf.RoundToInt((Mathf.Clamp(reliability, 0, 10) - 5) * 0.4f);
    }

    public static int Roll(System.Random rng, int sides)
    {
        return rng.Next(1, Mathf.Max(2, sides) + 1);
    }

    // One die's contribution: clears the DC for 1, plus 1 more per `margin` over it.
    public static int DieSuccesses(int total, int dc, int margin)
    {
        return total < dc ? 0 : 1 + (total - dc) / margin;
    }

    public static OutcomeDegree Degree(int successes, int required)
    {
        if (successes >= required) return OutcomeDegree.Success;
        if (required >= 2 && successes >= (required + 1) / 2) return OutcomeDegree.Partial;
        return OutcomeDegree.Fail;
    }
}
