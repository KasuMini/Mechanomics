using System.Collections.Generic;
using UnityEngine;

// Builds skill events at runtime. Difficulty (= current day for the demo) sets the primary leg (~one average
// mech per point); each extra stat gate shrinks the legs by the breadth discount so all events of a
// difficulty stay ~equally winnable. Within an event the legs are spread high-to-low for variety.
public class EventGenerator : MonoBehaviour
{
    public LoadText eventText;

    [Header("Difficulty")]
    [Min(1)] public int maxDifficulty = 10;
    [Range(1, 3)] public int maxStats = 3;

    [Header("Tuning")]
    public float poolPerMech = 5f;          // average stat one mech adds to a pool (stats roll 1-9)
    [Range(0.1f, 1f)] public float breadthDiscount = 0.7f;   // gamma: leg shrink per extra gate
    [Range(0f, 1.5f)] public float legVariety = 0.6f;        // log2 spread of legs around the target

    [Header("Rewards")]
    public int cashPerDifficulty = 350;
    public int penaltyPerDifficulty = 100;

    static readonly MechStat[] AllStats = { MechStat.Agility, MechStat.Strength, MechStat.Systems };

    // Geometric-mean leg size for a difficulty-D event spread over `stats` gates: poolPerMech * D * gamma^(stats-1).
    public static int LegSize(int difficulty, int stats, float poolPerMech, float gamma)
    {
        float target = poolPerMech * difficulty * Mathf.Pow(gamma, Mathf.Max(0, stats - 1));
        return Mathf.Max(1, Mathf.RoundToInt(target));
    }

    // Spreads k legs around `target`, holding their geometric mean (weights are log2 offsets; their mean is
    // subtracted so the product of factors stays 1). Single gate -> exactly `target`.
    public static int[] SplitLegs(int target, float[] logWeights)
    {
        int k = logWeights.Length;
        float mean = 0f;
        for (int i = 0; i < k; i++) mean += logWeights[i];
        mean /= Mathf.Max(1, k);

        var legs = new int[k];
        for (int i = 0; i < k; i++)
            legs[i] = Mathf.Max(1, Mathf.RoundToInt(target * Mathf.Pow(2f, logWeights[i] - mean)));
        return legs;
    }

    public EventData GenerateNewEvent(int difficulty)
    {
        difficulty = Mathf.Clamp(difficulty, 1, maxDifficulty);
        int gates = Random.Range(1, Mathf.Clamp(maxStats, 1, 3) + 1);

        int target = LegSize(difficulty, gates, poolPerMech, breadthDiscount);
        int[] legs = SplitLegs(target, RandomLogWeights(gates));
        System.Array.Sort(legs);
        System.Array.Reverse(legs);   // high to low

        var ev = new SkillEvent
        {
            cashReward = cashPerDifficulty * difficulty,
            cashPenalty = penaltyPerDifficulty * difficulty,
            quotaReward = 1,
        };
        int g = 0;
        foreach (MechStat stat in PickStats(gates))
            ev.requirements.Add(new SkillRequirement(stat, legs[g++]));

        return EventData.Create("Help Requested", Description(), difficulty, ev);
    }

    float[] RandomLogWeights(int count)
    {
        var w = new float[count];
        for (int i = 0; i < count; i++) w[i] = Random.Range(-legVariety, legVariety);
        return w;
    }

    // `count` distinct stats, order shuffled (Fisher-Yates over a copy).
    static IEnumerable<MechStat> PickStats(int count)
    {
        var pool = new List<MechStat>(AllStats);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool.GetRange(0, Mathf.Clamp(count, 1, pool.Count));
    }

    string Description()
    {
        if (eventText == null || eventText.keyword1 == null || eventText.keyword2 == null
            || eventText.keyword1.Length == 0 || eventText.keyword2.Length == 0)
            return string.Empty;
        string a = eventText.keyword1[Random.Range(0, eventText.keyword1.Length)];
        string b = eventText.keyword2[Random.Range(0, eventText.keyword2.Length)];
        return $"{a} {b}";
    }
}
