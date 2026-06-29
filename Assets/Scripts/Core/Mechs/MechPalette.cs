using UnityEngine;

// One region's colour: an OKLCh base colour expanded into a shaded ramp by contrast + hue shift.
[System.Serializable]
public struct RegionPalette
{
    [Range(0f, 1f)] public float okL;      // perceptual lightness
    [Range(0f, 0.4f)] public float okC;    // chroma
    [Range(0f, 360f)] public float okH;    // hue (degrees)
    [Range(0f, 1f)] public float contrast;
    [Range(-0.9f, 3f)] public float falloff;    // darkening curve, 0 = even
    [Range(-40f, 40f)] public float hueShift;   // shadow hue swing, degrees

    public Color BaseColor => OkColor.OklchToSrgb(okL, okC, okH);

    public RegionPalette(Color baseColor, float contrast, float hueShift)
    {
        OkColor.SrgbToOklch(baseColor, out okL, out okC, out okH);
        this.contrast = contrast;
        this.hueShift = hueShift;
        falloff = 0f;
    }
}

// A palette swap for the 3-ramp mech art: each region (R/G/B) is a base colour + toneramp.
// Bakes a 3-row LUT and a shared material; editing recolours live consumers.
[CreateAssetMenu(fileName = "MechPalette", menuName = "Mechanomics/MechPalette", order = 2)]
public class MechPalette : ScriptableObject
{
    public RegionPalette primary;   // red region
    public RegionPalette secondary; // green region
    public RegionPalette glow;      // blue region

    const int LutWidth = 64;
    const string ShaderName = "Mechanomics/MechPaletteSwap";

    Texture2D lut;
    Material mat;
    bool dirty = true;

    public RegionPalette GetRegion(int i) => i == 0 ? primary : i == 1 ? secondary : glow;

    public void SetRegion(int i, RegionPalette r)
    {
        if (i == 0) primary = r; else if (i == 1) secondary = r; else glow = r;
        Invalidate();
    }

    public Material Material
    {
        get
        {
            if (mat == null)
                mat = new Material(Shader.Find(ShaderName)) { name = name + "_mat", hideFlags = HideFlags.DontSave };
            mat.SetTexture("_PaletteTex", Lut());
            return mat;
        }
    }

    Texture2D Lut()
    {
        if (lut == null)
        {
            lut = new Texture2D(LutWidth, 3, TextureFormat.RGBA32, false)
            {
                name = name + "_lut",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };
            dirty = true;
        }
        if (dirty)
        {
            Bake(primary, 0);
            Bake(secondary, 1);
            Bake(glow, 2);
            lut.Apply();
            dirty = false;
        }
        return lut;
    }

    void Bake(RegionPalette rp, int row)
    {
        float invRange = 1f / (LutWidth - 1);
        for (int x = 0; x < LutWidth; x++)
            lut.SetPixel(x, row, Toneramps.Evaluate(rp, x * invRange));
    }

    // Mark the LUT dirty and rebuild so live consumers (sharing this material) recolour.
    public void Invalidate()
    {
        dirty = true;
        if (mat != null) mat.SetTexture("_PaletteTex", Lut());
    }

    void OnValidate() => Invalidate();

    void Reset()
    {
        primary = new RegionPalette(new Color(0.78f, 0.20f, 0.18f), 0.55f, 0f);
        secondary = new RegionPalette(new Color(0.18f, 0.70f, 0.40f), 0.55f, 0f);
        glow = new RegionPalette(new Color(0.95f, 0.78f, 0.25f), 0.55f, 20f);
    }
}
