using System.Collections.Generic;
using NUnit.Framework;

public class EventResolverTests
{
    class StubMech : IMechStats
    {
        public int agility, strength, systems;
        public int reliability = 5;
        public int size = 1;
        public int GetStat(MechStat s) =>
            s == MechStat.Agility ? agility : s == MechStat.Strength ? strength : systems;
        public int Reliability => reliability;
        public int Size => size;
    }

    static IReadOnlyList<IMechStats> Pool(params IMechStats[] m) => m;
    static List<SkillRequirement> Reqs(params SkillRequirement[] r) => new List<SkillRequirement>(r);
    static SkillRequirement Req(MechStat s, int amount) => new SkillRequirement(s, amount);

    // --- Coverage ---

    [Test]
    public void Coverage_IsRatioUpToOne()
    {
        Assert.AreEqual(0.7f, EventResolver.Coverage(7, 10), 1e-4f);
        Assert.AreEqual(1f, EventResolver.Coverage(12, 10), 1e-4f);   // excess ignored
        Assert.AreEqual(0f, EventResolver.Coverage(0, 10), 1e-4f);
    }

    // --- SuccessChance ---

    [Test]
    public void SuccessChance_SingleRequirement_IsThatCoverage()
    {
        var mechs = Pool(new StubMech { agility = 7 });
        Assert.AreEqual(0.7f, EventResolver.SuccessChance(Reqs(Req(MechStat.Agility, 10)), mechs), 1e-4f);
    }

    [Test]
    public void SuccessChance_MultiplesEveryRequirement()
    {
        var mechs = Pool(new StubMech { agility = 7, strength = 6 });
        // 7/10 * 6/8 = 0.7 * 0.75
        float expected = 0.7f * 0.75f;
        Assert.AreEqual(expected, EventResolver.SuccessChance(
            Reqs(Req(MechStat.Agility, 10), Req(MechStat.Strength, 8)), mechs), 1e-4f);
    }

    [Test]
    public void SuccessChance_AnyZeroRequirement_TanksToZero()
    {
        var mechs = Pool(new StubMech { agility = 10, strength = 0 });
        Assert.AreEqual(0f, EventResolver.SuccessChance(
            Reqs(Req(MechStat.Agility, 10), Req(MechStat.Strength, 5)), mechs), 1e-4f);
    }

    [Test]
    public void SuccessChance_NoRequirements_IsOne()
    {
        Assert.AreEqual(1f, EventResolver.SuccessChance(Reqs(), Pool()), 1e-4f);
    }

    [Test]
    public void SuccessChance_ExtraGate_IsStrictlyHarder()
    {
        // Same team, same Agi gate; adding a second (unmet) gate must lower the chance.
        var mechs = Pool(new StubMech { agility = 9, strength = 5 });
        float oneGate = EventResolver.SuccessChance(Reqs(Req(MechStat.Agility, 9)), mechs);
        float twoGates = EventResolver.SuccessChance(Reqs(Req(MechStat.Agility, 9), Req(MechStat.Strength, 8)), mechs);
        Assert.Greater(oneGate, twoGates);
    }

    [Test]
    public void SumStat_AddsAcrossSelectedMechs()
    {
        var mechs = Pool(new StubMech { agility = 3 }, new StubMech { agility = 4 });
        Assert.AreEqual(7, EventResolver.SumStat(mechs, MechStat.Agility));
    }

    // --- SkillEvent.Resolve (chance 0/1 are deterministic for any seed) ---

    [Test]
    public void SkillEvent_FullyCovered_AlwaysSucceeds_AndPays()
    {
        var ev = new SkillEvent { cashReward = 500, quotaReward = 1 };
        ev.requirements.Add(Req(MechStat.Agility, 5));
        for (int seed = 0; seed < 20; seed++)
        {
            var outcome = ev.Resolve(Pool(new StubMech { agility = 5 }), new System.Random(seed));
            Assert.AreEqual(OutcomeDegree.Success, outcome.Degree);
            Assert.AreEqual(500, outcome.CashDelta);
            Assert.AreEqual(1, outcome.QuotaDelta);
        }
    }

    [Test]
    public void SkillEvent_ZeroCovered_AlwaysFails_AndPenalizes()
    {
        var ev = new SkillEvent { cashPenalty = 100 };
        ev.requirements.Add(Req(MechStat.Strength, 5));
        for (int seed = 0; seed < 20; seed++)
        {
            var outcome = ev.Resolve(Pool(new StubMech { strength = 0 }), new System.Random(seed));
            Assert.AreEqual(OutcomeDegree.Fail, outcome.Degree);
            Assert.AreEqual(-100, outcome.CashDelta);
        }
    }
}
