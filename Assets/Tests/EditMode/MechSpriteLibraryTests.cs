using NUnit.Framework;

public class MechSpriteLibraryTests
{
    [Test]
    public void Index_MapsSizeToColumn_VariantToRow()
    {
        // 3 columns, 6 sprites -> 2 rows
        Assert.AreEqual(0, MechSpriteLibrary.Index(1, 0, 3, 6)); // size1, variant0
        Assert.AreEqual(1, MechSpriteLibrary.Index(2, 0, 3, 6)); // size2
        Assert.AreEqual(2, MechSpriteLibrary.Index(3, 0, 3, 6)); // size3
        Assert.AreEqual(3, MechSpriteLibrary.Index(1, 1, 3, 6)); // size1, variant1
        Assert.AreEqual(5, MechSpriteLibrary.Index(3, 1, 3, 6)); // size3, variant1
    }

    [Test]
    public void Index_WrapsVariantAndClampsSize()
    {
        Assert.AreEqual(0, MechSpriteLibrary.Index(1, 2, 3, 6));  // variant 2 wraps to row 0
        Assert.AreEqual(3, MechSpriteLibrary.Index(1, -1, 3, 6)); // variant -1 wraps to row 1
        Assert.AreEqual(2, MechSpriteLibrary.Index(9, 0, 3, 6));  // size clamps to last column
        Assert.AreEqual(0, MechSpriteLibrary.Index(0, 0, 3, 6));  // size clamps to first column
    }

    [Test]
    public void Index_ReturnsMinusOne_WhenEmptyOrInvalid()
    {
        Assert.AreEqual(-1, MechSpriteLibrary.Index(1, 0, 3, 0));
        Assert.AreEqual(-1, MechSpriteLibrary.Index(1, 0, 0, 6));
    }
}
