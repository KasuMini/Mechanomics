using NUnit.Framework;
using UnityEngine;
using Mechanomics;

public class EventResolverTests
{
    private static EventData MakeJob(
        int difficulty,
        int cashReward = 500,
        int cashPenalty = 100,
        MechStat stat = MechStat.Strength)
    {
        var job = ScriptableObject.CreateInstance<EventData>();
        job.difficulty = difficulty;
        job.diceSides = 10;
        job.testedStat = stat;
        job.cashReward = cashReward;
        job.cashPenalty = cashPenalty;
        job.quotaReward = 1;
        return job;
    }

    [Test]
    public void ReliabilityModifier_MapsAroundMidpoint()
    {
        Assert.AreEqual(-2, EventResolver.ReliabilityModifier(0));
        Assert.AreEqual(0, EventResolver.ReliabilityModifier(5));
        Assert.AreEqual(2, EventResolver.ReliabilityModifier(10));
    }

    [Test]
    public void StrongResult_Succeeds_AndPaysReward()
    {
        var job = MakeJob(difficulty: 10);
        var outcome = EventResolver.Resolve(job, testedStatValue: 5, reliability: 5, roll: 8);

        Assert.IsTrue(outcome.Success);
        Assert.AreEqual(13, outcome.Total);
        Assert.AreEqual(500, outcome.CashDelta);
        Assert.AreEqual(1, outcome.QuotaDelta);
    }

    [Test]
    public void WeakResult_Fails_AndAppliesPenalty()
    {
        var job = MakeJob(difficulty: 15);
        var outcome = EventResolver.Resolve(job, testedStatValue: 3, reliability: 5, roll: 2);

        Assert.IsFalse(outcome.Success);
        Assert.AreEqual(-100, outcome.CashDelta);
        Assert.AreEqual(0, outcome.QuotaDelta);
    }

    [Test]
    public void Reliability_CanSwingTheOutcome()
    {
        var job = MakeJob(difficulty: 12);
        var reliableWin = EventResolver.Resolve(job, testedStatValue: 5, reliability: 10, roll: 6);
        var unreliableLoss = EventResolver.Resolve(job, testedStatValue: 5, reliability: 0, roll: 6);

        Assert.IsTrue(reliableWin.Success);
        Assert.IsFalse(unreliableLoss.Success);
    }

    [Test]
    public void DisableMechOnFailure_OnlyFlagsWhenFailing()
    {
        var hardJob = MakeJob(difficulty: 20);
        hardJob.disableMechOnFailure = true;
        var failed = EventResolver.Resolve(hardJob, testedStatValue: 1, reliability: 0, roll: 1);
        Assert.IsTrue(failed.MechDisabled);

        var easyJob = MakeJob(difficulty: 2);
        easyJob.disableMechOnFailure = true;
        var passed = EventResolver.Resolve(easyJob, testedStatValue: 5, reliability: 5, roll: 9);
        Assert.IsFalse(passed.MechDisabled);
    }
}
