using NUnit.Framework;

public class EventGeneratorTests
{
    const float Pool = 4.5f;
    const float Gamma = 0.7f;

    [Test]
    public void LegSize_PrimaryGate_IsPoolTimesDifficulty()
    {
        Assert.AreEqual(9, EventGenerator.LegSize(2, 1, Pool, Gamma));   // 4.5 * 2
        Assert.AreEqual(14, EventGenerator.LegSize(3, 1, Pool, Gamma));  // 4.5 * 3 = 13.5 -> 14
    }

    [Test]
    public void LegSize_EachExtraGate_AppliesBreadthDiscount()
    {
        Assert.AreEqual(9, EventGenerator.LegSize(2, 1, Pool, Gamma));   // 9
        Assert.AreEqual(6, EventGenerator.LegSize(2, 2, Pool, Gamma));   // 9 * 0.7  = 6.3 -> 6
        Assert.AreEqual(4, EventGenerator.LegSize(2, 3, Pool, Gamma));   // 9 * 0.49 = 4.41 -> 4
    }

    [Test]
    public void LegSize_MoreGates_NeverLarger()
    {
        int one = EventGenerator.LegSize(3, 1, Pool, Gamma);
        int two = EventGenerator.LegSize(3, 2, Pool, Gamma);
        int three = EventGenerator.LegSize(3, 3, Pool, Gamma);
        Assert.GreaterOrEqual(one, two);
        Assert.GreaterOrEqual(two, three);
    }

    [Test]
    public void LegSize_FloorsAtOne()
    {
        Assert.AreEqual(1, EventGenerator.LegSize(1, 3, Pool, 0.1f));
    }

    // --- SplitLegs (high-to-low variety, geometric mean held on target) ---

    [Test]
    public void SplitLegs_ZeroWeights_AllEqualTarget()
    {
        CollectionAssert.AreEqual(new[] { 9, 9, 9 }, EventGenerator.SplitLegs(9, new[] { 0f, 0f, 0f }));
    }

    [Test]
    public void SplitLegs_SpreadsAroundTarget_HoldingGeometricMean()
    {
        // mean(weights)=0, factors 2^1=2 and 2^-1=0.5 -> 20 and 5; geometric mean sqrt(100)=10 == target.
        CollectionAssert.AreEqual(new[] { 20, 5 }, EventGenerator.SplitLegs(10, new[] { 1f, -1f }));
    }

    [Test]
    public void SplitLegs_SingleGate_IsExactlyTarget()
    {
        CollectionAssert.AreEqual(new[] { 8 }, EventGenerator.SplitLegs(8, new[] { 0.5f }));
    }
}
