using NUnit.Framework;
using UnityEngine;

public class MechPaletteSetTests
{
    [Test]
    public void Random_ReturnsNull_WhenEmpty()
    {
        var set = ScriptableObject.CreateInstance<MechPaletteSet>();
        Assert.IsNull(set.Random());
        set.palettes = new MechPalette[0];
        Assert.IsNull(set.Random());
    }

    [Test]
    public void Random_ReturnsAMember_WhenPopulated()
    {
        var set = ScriptableObject.CreateInstance<MechPaletteSet>();
        var p = ScriptableObject.CreateInstance<MechPalette>();
        set.palettes = new[] { p };
        Assert.AreSame(p, set.Random());
    }
}
