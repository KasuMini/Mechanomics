using System.Collections.Generic;
using NUnit.Framework;

public class EventResolverTests
{
    class StubMech : IMechStats
    {
        public int stat;
        public int reliability = 5;
        public int size = 1;
        public int GetStat(MechStat s) => stat;
        public int Reliability => reliability;
        public int Size => size;
    }

    static IReadOnlyList<IMechStats> Pool(params IMechStats[] m) => m;

    // --- pure helpers ---

    [Test]
    public void ReliabilityModifier_MapsAroundMidpoint()
    {
        Assert.AreEqual(-2, EventResolver.ReliabilityModifier(0));
        Assert.AreEqual(0, EventResolver.ReliabilityModifier(5));
        Assert.AreEqual(2, EventResolver.ReliabilityModifier(10));
    }

    [Test]
    public void DieSuccesses_AddsOnePerMarginOverDc()
    {
        Assert.AreEqual(0, EventResolver.DieSuccesses(7, 8, 4));
        Assert.AreEqual(1, EventResolver.DieSuccesses(8, 8, 4));
        Assert.AreEqual(1, EventResolver.DieSuccesses(11, 8, 4));
        Assert.AreEqual(2, EventResolver.DieSuccesses(12, 8, 4));
        Assert.AreEqual(3, EventResolver.DieSuccesses(16, 8, 4));
    }

    [Test]
    public void Degree_PartialOnlyAppliesWhenMultipleRequired()
    {
        Assert.AreEqual(OutcomeDegree.Fail, EventResolver.Degree(0, 1));
        Assert.AreEqual(OutcomeDegree.Success, EventResolver.Degree(1, 1));
        Assert.AreEqual(OutcomeDegree.Fail, EventResolver.Degree(0, 2));
        Assert.AreEqual(OutcomeDegree.Partial, EventResolver.Degree(1, 2));
        Assert.AreEqual(OutcomeDegree.Success, EventResolver.Degree(2, 2));
        Assert.AreEqual(OutcomeDegree.Fail, EventResolver.Degree(1, 3));   // ceil(3/2) = 2
        Assert.AreEqual(OutcomeDegree.Partial, EventResolver.Degree(2, 3));
        Assert.AreEqual(OutcomeDegree.Success, EventResolver.Degree(3, 3));
    }

    // --- SkillEvent (deterministic via extreme stats) ---

    [Test]
    public void SkillEvent_StrongMech_Succeeds_AndPaysReward()
    {
        var ev = new SkillEvent { dc = 8, successesRequired = 1, diceSides = 6, cashReward = 500 };
        var outcome = ev.Resolve(Pool(new StubMech { stat = 100 }), new System.Random(1));
        Assert.AreEqual(OutcomeDegree.Success, outcome.Degree);
        Assert.AreEqual(500, outcome.CashDelta);
    }

    [Test]
    public void SkillEvent_WeakMech_Fails_AndAppliesPenalty()
    {
        var ev = new SkillEvent { dc = 8, successesRequired = 1, diceSides = 6, cashPenalty = 100 };
        // max total = 6 + 0 + relMod(0 -> -2) = 4 < 8 -> 0 successes regardless of roll
        var outcome = ev.Resolve(Pool(new StubMech { stat = 0, reliability = 0 }), new System.Random(1));
        Assert.AreEqual(OutcomeDegree.Fail, outcome.Degree);
        Assert.AreEqual(-100, outcome.CashDelta);
    }

    [Test]
    public void SkillEvent_TwoMechs_PoolSuccesses_ToClearHigherRequirement()
    {
        var ev = new SkillEvent { dc = 8, successesRequired = 2, diceSides = 6 };
        // each strong mech always yields >=1 success; two -> >=2 -> Success
        var outcome = ev.Resolve(Pool(new StubMech { stat = 8 }, new StubMech { stat = 8 }), new System.Random(1));
        Assert.AreEqual(OutcomeDegree.Success, outcome.Degree);
        Assert.GreaterOrEqual(outcome.Successes, 2);
    }

    [Test]
    public void SkillEvent_NoMechs_Fails()
    {
        var ev = new SkillEvent { successesRequired = 1 };
        var outcome = ev.Resolve(Pool(), new System.Random(1));
        Assert.AreEqual(OutcomeDegree.Fail, outcome.Degree);
    }

    // --- CombatEvent ---

    [Test]
    public void CombatEvent_StrongMech_Wins_NoLosses()
    {
        var ev = new CombatEvent { dc = 8, diceSides = 6, enemyHp = 3, enemyPower = 2, enemyHitOn = 4, maxRounds = 4, cashReward = 800 };
        // stat 100 -> first attack deals far more than 3 -> win before the enemy ever acts
        var outcome = ev.Resolve(Pool(new StubMech { stat = 100, size = 1 }), new System.Random(1));
        Assert.AreEqual(OutcomeDegree.Success, outcome.Degree);
        Assert.AreEqual(800, outcome.CashDelta);
        Assert.IsEmpty(outcome.DisabledMechIndices);
    }

    [Test]
    public void CombatEvent_OverwhelmedMech_Loses_AndIsDisabled()
    {
        // mech never damages (total <= 4 < dc 8); enemy always hits (enemyHitOn 1)
        var ev = new CombatEvent { dc = 8, diceSides = 6, enemyHp = 100, enemyPower = 1, enemyHitOn = 1, maxRounds = 1 };
        var outcome = ev.Resolve(Pool(new StubMech { stat = 0, reliability = 0, size = 1 }), new System.Random(1));
        Assert.AreEqual(OutcomeDegree.Fail, outcome.Degree);
        CollectionAssert.Contains(outcome.DisabledMechIndices, 0);
    }

    [Test]
    public void CombatEvent_SizeIsHitPoints()
    {
        // 1 wound/round (enemyHitOn 1, power 1), mech never wins; size-3 soaks 2, disabled on the 3rd
        var twoRounds = new CombatEvent { dc = 8, diceSides = 6, enemyHp = 100, enemyPower = 1, enemyHitOn = 1, maxRounds = 2 };
        var a = twoRounds.Resolve(Pool(new StubMech { stat = 0, reliability = 0, size = 3 }), new System.Random(1));
        Assert.IsEmpty(a.DisabledMechIndices);   // 2 wounds < size 3

        var threeRounds = new CombatEvent { dc = 8, diceSides = 6, enemyHp = 100, enemyPower = 1, enemyHitOn = 1, maxRounds = 3 };
        var b = threeRounds.Resolve(Pool(new StubMech { stat = 0, reliability = 0, size = 3 }), new System.Random(1));
        CollectionAssert.Contains(b.DisabledMechIndices, 0);  // 3 wounds == size 3
    }
}
