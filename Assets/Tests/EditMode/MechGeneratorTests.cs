using NUnit.Framework;

public class MechGeneratorTests
{
    [Test]
    public void SizeFactor_Is_1_15_2()
    {
        Assert.AreEqual(1f, MechGenerator.SizeFactor(1), 1e-4f);
        Assert.AreEqual(1.5f, MechGenerator.SizeFactor(2), 1e-4f);
        Assert.AreEqual(2f, MechGenerator.SizeFactor(3), 1e-4f);
    }

    [Test]
    public void ScaleStat_RoundsBaseTimesFactor()
    {
        Assert.AreEqual(6, MechGenerator.ScaleStat(6, 1, 20));   // x1.0
        Assert.AreEqual(9, MechGenerator.ScaleStat(6, 2, 20));   // x1.5
        Assert.AreEqual(12, MechGenerator.ScaleStat(6, 3, 20));  // x2.0
        Assert.AreEqual(4, MechGenerator.ScaleStat(3, 2, 20));   // 4.5 -> round-half-to-even -> 4
        Assert.AreEqual(8, MechGenerator.ScaleStat(5, 2, 20));   // 7.5 -> round-half-to-even -> 8
    }

    [Test]
    public void ScaleStat_ClampsToCap()
    {
        Assert.AreEqual(8, MechGenerator.ScaleStat(4, 3, 8));    // 8 ok, at cap
        Assert.AreEqual(20, MechGenerator.ScaleStat(14, 2, 20)); // 21 -> clamp 20
        Assert.AreEqual(8, MechGenerator.ScaleStat(5, 3, 8));    // 10 -> clamp 8
    }
}
