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
    public void TryAdd_TenSize1_FillsExactly_EleventhFails()
    {
        var inv = new MechInventory();
        for (int i = 0; i < 10; i++) Assert.IsTrue(inv.TryAdd(Mech(1)));   // 10 x span 2 = 20
        Assert.AreEqual(20, inv.UsedSpan);
        Assert.IsFalse(inv.TryAdd(Mech(1)));
    }

    [Test]
    public void TryAdd_FiveSize3_FillsExactly_SixthFails()
    {
        var inv = new MechInventory();
        for (int i = 0; i < 5; i++) Assert.IsTrue(inv.TryAdd(Mech(3)));    // 5 x span 4 = 20
        Assert.AreEqual(20, inv.UsedSpan);
        Assert.IsFalse(inv.TryAdd(Mech(3)));
    }

    [Test]
    public void CanAdd_RejectsNullDuplicateAndOverflow()
    {
        var inv = new MechInventory();
        Assert.IsFalse(inv.CanAdd(null));
        var a = Mech(1);
        inv.TryAdd(a);
        Assert.IsFalse(inv.CanAdd(a));                  // already owned
        for (int i = 0; i < 9; i++) inv.TryAdd(Mech(1)); // now 10 x span 2 = 20, full
        Assert.IsFalse(inv.CanAdd(Mech(1)));            // no room left
    }

    [Test]
    public void Remove_ShrinksUsedSpanAndDropsMech()
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
    public void Mechs_PreservesInsertionOrder()
    {
        var inv = new MechInventory();
        var a = Mech(1); var b = Mech(2); var c = Mech(1);
        inv.TryAdd(a); inv.TryAdd(b); inv.TryAdd(c);
        CollectionAssert.AreEqual(new[] { a, b, c }, new List<MechData>(inv.Mechs));
    }

    [Test]
    public void CenterOf_CentresRow_Gapless()
    {
        var inv = new MechInventory();
        var a = Mech(1); var b = Mech(1);    // span 2 each
        inv.TryAdd(a);
        Assert.AreEqual(10f, inv.CenterOf(a), 1e-4f);  // lone mech sits at the bar centre (Capacity/2)
        inv.TryAdd(b);
        // Two mechs straddle the centre, packed shoulder-to-shoulder (no gap between centres).
        Assert.AreEqual(10f, (inv.CenterOf(a) + inv.CenterOf(b)) * 0.5f, 1e-4f);
        Assert.AreEqual(2f, inv.CenterOf(b) - inv.CenterOf(a), 1e-4f);   // a.Span/2 + b.Span/2
    }

    [Test]
    public void Reorder_MovesDraggedToNearestIndex_AndBack()
    {
        var inv = new MechInventory();
        var a = Mech(1); var b = Mech(1); var c = Mech(1);
        inv.TryAdd(a); inv.TryAdd(b); inv.TryAdd(c);    // order a, b, c
        inv.Reorder(a, inv.CenterOf(c) + 1f);           // drag a past c
        CollectionAssert.AreEqual(new[] { b, c, a }, new List<MechData>(inv.Mechs));
        inv.Reorder(a, 0f);                             // drag a back to the front
        CollectionAssert.AreEqual(new[] { a, b, c }, new List<MechData>(inv.Mechs));
    }
}
