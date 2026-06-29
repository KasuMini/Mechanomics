using UnityEngine;

// Expands a region's OKLCh base colour into a shade ramp, à la Lancer Tactics' toneramps.
// The base IS the lit/top shade (t = 1); contrast darkens downward toward black (t = 0).
// falloff curves how the darkening is distributed (0 = even); chroma eases off near black;
// hueShift (degrees) swings only the shadows.
public static class Toneramps
{
    public static Color Evaluate(in RegionPalette rp, float t)
    {
        t = Mathf.Clamp01(t);
        // falloff 0 -> linear; >0 darkens faster toward shadow; <0 stays near base longer.
        float shade = Mathf.Pow(t, Mathf.Max(1f + rp.falloff, 0.05f));

        float shadowL = Mathf.Lerp(rp.okL, 0f, rp.contrast);     // contrast 1 -> black floor
        float L = Mathf.Lerp(shadowL, rp.okL, shade);            // base at t=1, darkens downward
        float C = rp.okC * Mathf.SmoothStep(0f, Mathf.Max(rp.okL, 1e-4f), L); // fade chroma toward black
        float h = rp.okH + rp.hueShift * (1f - shade);           // base hue at t=1; shadows shift
        return OkColor.OklchToSrgb(L, C, h);
    }
}
