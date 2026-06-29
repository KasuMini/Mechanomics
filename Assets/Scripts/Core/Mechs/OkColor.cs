using UnityEngine;

// OKLab / OKLCh <-> sRGB (Björn Ottosson's OKLab). OKLCh = perceptual Lightness, Chroma, Hue.
// Hue is in degrees. Out-of-gamut results are clamped to [0,1].
public static class OkColor
{
    static float ToLinear(float c) => c <= 0.04045f ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);
    static float ToSrgb(float c) => c <= 0.0031308f ? 12.92f * c : 1.055f * Mathf.Pow(c, 1f / 2.4f) - 0.055f;
    static float Cbrt(float x) => x < 0f ? -Mathf.Pow(-x, 1f / 3f) : Mathf.Pow(x, 1f / 3f);

    public static Color OklchToSrgb(float L, float C, float hDeg)
    {
        float h = hDeg * Mathf.Deg2Rad;
        float a = C * Mathf.Cos(h);
        float b = C * Mathf.Sin(h);

        float l_ = L + 0.3963377774f * a + 0.2158037573f * b;
        float m_ = L - 0.1055613458f * a - 0.0638541728f * b;
        float s_ = L - 0.0894841775f * a - 1.2914855480f * b;
        float l = l_ * l_ * l_, m = m_ * m_ * m_, s = s_ * s_ * s_;

        float r = +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
        float g = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
        float bb = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

        return new Color(
            Mathf.Clamp01(ToSrgb(r)),
            Mathf.Clamp01(ToSrgb(g)),
            Mathf.Clamp01(ToSrgb(bb)), 1f);
    }

    public static void SrgbToOklch(Color c, out float L, out float C, out float hDeg)
    {
        float r = ToLinear(c.r), g = ToLinear(c.g), b = ToLinear(c.b);

        float l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
        float m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
        float s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;
        float l_ = Cbrt(l), m_ = Cbrt(m), s_ = Cbrt(s);

        L = 0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_;
        float a = 1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_;
        float bb = 0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_;

        C = Mathf.Sqrt(a * a + bb * bb);
        hDeg = Mathf.Atan2(bb, a) * Mathf.Rad2Deg;
        if (hDeg < 0f) hDeg += 360f;
    }
}
