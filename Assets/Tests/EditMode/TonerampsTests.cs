using NUnit.Framework;
using UnityEngine;

public class TonerampsTests
{
    static float Bright(Color c) => Mathf.Max(c.r, Mathf.Max(c.g, c.b));
    static RegionPalette Rp(float contrast, float hueShift)
        => new RegionPalette(new Color(0.8f, 0.3f, 0.2f), contrast, hueShift);

    [Test]
    public void ZeroContrast_IsFlat()
    {
        var rp = Rp(0f, 0f);
        Assert.That(Bright(Toneramps.Evaluate(rp, 0f)),
            Is.EqualTo(Bright(Toneramps.Evaluate(rp, 1f))).Within(1e-3f));
    }

    [Test]
    public void MoreContrast_DarkensShadowEnd()
    {
        Assert.Less(Bright(Toneramps.Evaluate(Rp(0.9f, 0f), 0f)),
                    Bright(Toneramps.Evaluate(Rp(0.2f, 0f), 0f)));
    }

    [Test]
    public void TopShade_IsBase_RegardlessOfContrast()
    {
        var a = Toneramps.Evaluate(Rp(0.2f, 0f), 1f);
        var b = Toneramps.Evaluate(Rp(0.9f, 0f), 1f);
        Assert.That(a.r, Is.EqualTo(b.r).Within(1e-3f));
        Assert.That(a.g, Is.EqualTo(b.g).Within(1e-3f));
        Assert.That(a.b, Is.EqualTo(b.b).Within(1e-3f));
    }

    [Test]
    public void PositiveFalloff_DarkensMidShades()
    {
        var linear = Rp(0.8f, 0f);
        var curved = linear; curved.falloff = 2f;
        Assert.Less(Bright(Toneramps.Evaluate(curved, 0.5f)),
                    Bright(Toneramps.Evaluate(linear, 0.5f)));
    }

    [Test]
    public void HueShift_MovesShadows_NotBase()
    {
        // Base (t=1) is unchanged by hue shift.
        var topA = Toneramps.Evaluate(Rp(0.6f, 0f), 1f);
        var topB = Toneramps.Evaluate(Rp(0.6f, 30f), 1f);
        Assert.That(topB.r, Is.EqualTo(topA.r).Within(1e-3f));
        Assert.That(topB.g, Is.EqualTo(topA.g).Within(1e-3f));
        Assert.That(topB.b, Is.EqualTo(topA.b).Within(1e-3f));

        // Shadow (t<1) hue moves with the shift.
        OkColor.SrgbToOklch(Toneramps.Evaluate(Rp(0.6f, 0f), 0.3f), out _, out _, out float h0);
        OkColor.SrgbToOklch(Toneramps.Evaluate(Rp(0.6f, 30f), 0.3f), out _, out _, out float h1);
        Assert.AreNotEqual(h0, h1);
    }
}
