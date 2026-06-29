using UnityEngine;
using Wacton.Unicolour;

// sRGB <-> OKLCh via the Unicolour library (MIT, zero-dependency). Out-of-gamut colours are
// gamut-mapped with Unicolour's default OKLCh chroma reduction (hue + lightness preserved).
// Hue is in degrees.
public static class OkColor
{
    public static Color OklchToSrgb(float L, float C, float hDeg)
    {
        var (r, g, b) = new Unicolour(ColourSpace.Oklch, L, C, hDeg).MapToRgbGamut().Rgb;
        return new Color((float)r, (float)g, (float)b, 1f);
    }

    public static void SrgbToOklch(Color c, out float L, out float C, out float hDeg)
    {
        var (l, ch, h) = new Unicolour(ColourSpace.Rgb, c.r, c.g, c.b).Oklch;
        L = (float)l;
        C = (float)ch;
        hDeg = float.IsNaN((float)h) ? 0f : (float)h;
    }
}
