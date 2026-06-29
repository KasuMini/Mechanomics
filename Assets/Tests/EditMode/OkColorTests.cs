using NUnit.Framework;
using UnityEngine;

public class OkColorTests
{
    static void RoundTrips(Color c)
    {
        OkColor.SrgbToOklch(c, out float l, out float ch, out float h);
        Color back = OkColor.OklchToSrgb(l, ch, h);
        Assert.That(back.r, Is.EqualTo(c.r).Within(0.01f));
        Assert.That(back.g, Is.EqualTo(c.g).Within(0.01f));
        Assert.That(back.b, Is.EqualTo(c.b).Within(0.01f));
    }

    [Test]
    public void Srgb_Oklch_RoundTrips()
    {
        RoundTrips(new Color(0.8f, 0.2f, 0.2f));
        RoundTrips(new Color(0.2f, 0.6f, 0.3f));
        RoundTrips(new Color(0.1f, 0.3f, 0.9f));
        RoundTrips(new Color(0.5f, 0.5f, 0.5f));
    }

    [Test]
    public void OutOfGamut_ReducesChroma_PreservesHue()
    {
        // High-chroma red-orange, far outside sRGB.
        var c = OkColor.OklchToSrgb(0.712f, 0.4f, 44.9f);
        Assert.That(c.r, Is.InRange(0f, 1f));
        Assert.That(c.g, Is.InRange(0f, 1f));
        Assert.That(c.b, Is.InRange(0f, 1f));

        OkColor.SrgbToOklch(c, out _, out float ch, out float h);
        Assert.That(h, Is.EqualTo(44.9f).Within(2f));   // hue unchanged
        Assert.Less(ch, 0.4f);                           // chroma pulled into gamut
    }

    [Test]
    public void Lightness_Ordered_BlackToWhite()
    {
        OkColor.SrgbToOklch(Color.black, out float lo, out _, out _);
        OkColor.SrgbToOklch(Color.white, out float hi, out _, out _);
        Assert.Less(lo, hi);
    }
}
