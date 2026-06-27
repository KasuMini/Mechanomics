using System.Collections.Generic;
using NUnit.Framework;

public class MechInventoryTests
{
    static MechData Mech(int size) => MechData.Create("m", "p", 1, 1, 1, 1, size);

    [Test]
    public void SlotSpan_IsSizePlusOne()
    {
        Assert.AreEqual(2, MechData.SlotSpan(1));
        Assert.AreEqual(3, MechData.SlotSpan(2));
        Assert.AreEqual(4, MechData.SlotSpan(3));
    }

    [Test]
    public void TryAdd_TwelveSize1_FillsExactly_ThirteenthFails()
    {
        var inv = new MechInventory();
        for (int i = 0; i < 12; i++) Assert.IsTrue(inv.TryAdd(Mech(1)));
        Assert.AreEqual(24, inv.UsedSpan);
        Assert.IsFalse(inv.TryAdd(Mech(1)));
    }

    [Test]
    public void TryAdd_SixSize3_FillsExactly_SeventhFails()
    {
        var inv = new MechInventory();
        for (int i = 0; i < 6; i++) Assert.IsTrue(inv.TryAdd(Mech(3)));
        Assert.AreEqual(24, inv.UsedSpan);
        Assert.IsFalse(inv.TryAdd(Mech(3)));
    }

    [Test]
    public void FirstFit_PicksLowestGap()
    {
        var inv = new MechInventory();
        var a = Mech(1); var b = Mech(1);
        inv.TryAdd(a);                 // notches 0-1
        inv.TryAdd(b);                 // notches 2-3
        Assert.AreEqual(0, inv.StartOf(a));
        Assert.AreEqual(2, inv.StartOf(b));
        inv.Remove(a);                 // free 0-1
        var c = Mech(1);
        inv.TryAdd(c);                 // lowest gap is 0-1
        Assert.AreEqual(0, inv.StartOf(c));
    }

    [Test]
    public void TryMove_OntoFreeSpan_Succeeds()
    {
        var inv = new MechInventory();
        var a = Mech(1);
        inv.TryAdd(a);                 // 0-1
        Assert.IsTrue(inv.TryMove(a, 10));
        Assert.AreEqual(10, inv.StartOf(a));
    }

    [Test]
    public void TryMove_NudgeWithinOwnFootprint_Succeeds()
    {
        var inv = new MechInventory();
        var a = Mech(2);               // span 3
        inv.TryAdd(a);                 // 0-2
        Assert.IsTrue(inv.TryMove(a, 1));   // overlaps its own old cells
        Assert.AreEqual(1, inv.StartOf(a));
    }

    [Test]
    public void TryMove_OntoAnotherMech_FailsAndLeavesStateUnchanged()
    {
        var inv = new MechInventory();
        var a = Mech(1); var b = Mech(1);
        inv.TryAdd(a);                 // 0-1
        inv.TryAdd(b);                 // 2-3
        Assert.IsFalse(inv.TryMove(b, 0));  // would collide with a
        Assert.AreEqual(0, inv.StartOf(a));
        Assert.AreEqual(2, inv.StartOf(b));
    }

    [Test]
    public void TryAdd_FragmentedArray_BlocksTooBigEvenWithEnoughFreeCells()
    {
        var inv = new MechInventory();
        var a = Mech(1); var b = Mech(1);
        inv.TryAdd(a);                 // 0-1
        inv.TryAdd(b);                 // 2-3
        inv.Remove(a);                 // free 0-1 (2 scattered cells, plus 4-23 free)
        // Move b to the far end so the only gaps are 0-1 and 4..23 contiguous.
        // Construct true fragmentation: fill so only single-cell gaps remain.
        var inv2 = new MechInventory();
        var fillers = new List<MechData>();
        for (int i = 0; i < 12; i++) { var m = Mech(1); fillers.Add(m); inv2.TryAdd(m); }
        inv2.Remove(fillers[0]);       // free notches 0-1 only
        inv2.Remove(fillers[5]);       // free notches 10-11 only
        // 4 free cells total but no contiguous span of 3.
        Assert.AreEqual(20, inv2.UsedSpan);
        Assert.IsFalse(inv2.TryAdd(Mech(2)));   // needs 3 contiguous
        Assert.IsTrue(inv2.TryAdd(Mech(1)));    // needs 2 contiguous -> fits in 0-1
    }

    [Test]
    public void Remove_FreesCells()
    {
        var inv = new MechInventory();
        var a = Mech(2);
        inv.TryAdd(a);
        Assert.AreEqual(3, inv.UsedSpan);
        Assert.IsTrue(inv.Remove(a));
        Assert.AreEqual(0, inv.UsedSpan);
        Assert.IsFalse(inv.Contains(a));
    }

    [Test]
    public void Mechs_EnumeratesLeftToRight()
    {
        var inv = new MechInventory();
        var a = Mech(1); var b = Mech(2); var c = Mech(1);
        inv.TryAdd(a);                 // 0-1
        inv.TryAdd(b);                 // 2-4
        inv.TryAdd(c);                 // 5-6
        var order = new List<MechData>(inv.Mechs);
        CollectionAssert.AreEqual(new[] { a, b, c }, order);
    }
}
