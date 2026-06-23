using System;
using System.Collections.Generic;
using UnityEngine;

// Round-by-round combat: mechs attack to deplete the enemy's HP; the enemy attacks back and
// wounds mechs (size = hits to disable). Win = enemy HP gone; Loss = all mechs disabled or rounds run out.
[Serializable]
public class CombatEvent : EventBehaviour
{
    [Header("Attack")]
    public MechStat testedStat = MechStat.Strength;
    [Min(1)] public int dc = 8;
    [Min(2)] public int diceSides = 6;

    [Header("Enemy")]
    [Min(1)] public int enemyHp = 3;
    [Min(1)] public int enemyPower = 2;     // enemy attack dice per round
    [Min(1)] public int enemyHitOn = 4;     // an enemy die >= this lands a wound
    [Min(1)] public int maxRounds = 4;

    [Header("Rewards")]
    public int cashReward = 800;
    public int quotaReward = 1;
    public int cashPenalty = 200;

    [Header("Result text")]
    [TextArea(2, 4)] public string successText;
    [TextArea(2, 4)] public string failureText;

    public override string Summary => $"Combat: HP {enemyHp}, {enemyPower} atk (DC {dc})";

    public override EventOutcome Resolve(IReadOnlyList<IMechStats> mechs, System.Random rng)
    {
        var outcome = new EventOutcome { Required = enemyHp };
        int n = mechs?.Count ?? 0;
        if (n == 0)
        {
            outcome.Degree = OutcomeDegree.Fail;
            outcome.CashDelta = -cashPenalty;
            outcome.ResultText = failureText;
            return outcome;
        }

        var wounds = new int[n];
        var disabled = new bool[n];
        int hp = enemyHp;
        bool won = false;

        for (int round = 0; round < maxRounds && hp > 0; round++)
        {
            for (int i = 0; i < n; i++)
            {
                if (disabled[i]) continue;
                int total = EventResolver.Roll(rng, diceSides)
                          + mechs[i].GetStat(testedStat)
                          + EventResolver.ReliabilityModifier(mechs[i].Reliability);
                hp -= EventResolver.DieSuccesses(total, dc, EventResolver.SUCCESS_MARGIN);
            }
            if (hp <= 0) { won = true; break; }

            for (int a = 0; a < enemyPower; a++)
            {
                int target = LowestWoundActive(wounds, disabled);
                if (target < 0) break;
                if (EventResolver.Roll(rng, diceSides) >= enemyHitOn)
                {
                    wounds[target]++;
                    if (wounds[target] >= mechs[target].Size)
                    {
                        disabled[target] = true;
                        outcome.DisabledMechIndices.Add(target);
                    }
                }
            }
            if (LowestWoundActive(wounds, disabled) < 0) break;
        }

        outcome.Successes = enemyHp - Mathf.Max(0, hp);
        if (won)
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

    static int LowestWoundActive(int[] wounds, bool[] disabled)
    {
        int best = -1;
        for (int i = 0; i < wounds.Length; i++)
        {
            if (disabled[i]) continue;
            if (best < 0 || wounds[i] < wounds[best]) best = i;
        }
        return best;
    }
}
