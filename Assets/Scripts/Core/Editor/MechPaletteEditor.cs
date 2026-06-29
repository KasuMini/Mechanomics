using UnityEditor;
using UnityEngine;

// Palette picker: pick each region's base colour with OKLCh sliders (Lightness/Chroma/Hue),
// then shape the ramp with Contrast / Hue Shift sliders (previewed live).
[CustomEditor(typeof(MechPalette))]
public class MechPaletteEditor : Editor
{
    static readonly string[] RegionNames = { "Primary", "Secondary", "Glow" };
    static GUIStyle chipStyle;
    int region;

    const int PreviewHeight = 120;
    MechSpriteLibrary lib;
    RenderTexture previewRt;

    public override void OnInspectorGUI()
    {
        var p = (MechPalette)target;

        DrawMechPreview(p);

        region = DrawRegionChips(p, region);
        var rp = p.GetRegion(region);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Base Colour (OKLCh)", EditorStyles.miniBoldLabel);
        var swatch = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(swatch, rp.BaseColor);

        EditorGUI.BeginChangeCheck();
        float okL = EditorGUILayout.Slider("Lightness", rp.okL, 0f, 1f);
        float okC = EditorGUILayout.Slider("Chroma", rp.okC, 0f, 0.4f);
        float okH = EditorGUILayout.Slider("Hue", rp.okH, 0f, 360f);
        if (EditorGUI.EndChangeCheck()) { rp.okL = okL; rp.okC = okC; rp.okH = okH; Apply(p, region, rp); }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Shading", EditorStyles.miniBoldLabel);
        var preview = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
        DrawRamp(preview, rp);

        EditorGUI.BeginChangeCheck();
        float contrast = EditorGUILayout.Slider("Contrast", rp.contrast, 0f, 1f);
        float falloff = EditorGUILayout.Slider("Contrast Falloff", rp.falloff, -0.9f, 3f);
        float hueShift = EditorGUILayout.Slider("Hue Shift", rp.hueShift, -40f, 40f);
        if (EditorGUI.EndChangeCheck()) { rp.contrast = contrast; rp.falloff = falloff; rp.hueShift = hueShift; Apply(p, region, rp); }
    }

    // Live preview: blit real mech sprites through the palette's own swap material, so the
    // preview is pixel-identical to the game and recolours as the sliders move. Shows as many
    // variants side by side as the inspector width allows.
    void DrawMechPreview(MechPalette p)
    {
        var sprites = PreviewSprites();
        var rect = GUILayoutUtility.GetRect(0, PreviewHeight, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, Color.white);
        if (sprites.Count == 0)
        {
            EditorGUI.HelpBox(rect, "No MechSpriteLibrary / sprite found to preview.", MessageType.Info);
            return;
        }

        const float pad = 6f;

        // Greedily fill the width, cycling through the distinct sprites and repeating if room remains.
        var draw = new System.Collections.Generic.List<Sprite>();
        float used = 0f;
        for (int i = 0; i < 512; i++)
        {
            var s = sprites[i % sprites.Count];
            float w = s.textureRect.width * IntScale(s, rect.height);
            float add = draw.Count == 0 ? w : pad + w;
            if (used + add > rect.width && draw.Count > 0) break;
            draw.Add(s);
            used += add;
        }

        float x = rect.x + (rect.width - used) * 0.5f;
        foreach (var sprite in draw)
        {
            var tex = sprite.texture;
            EnsureRt(tex);
            Graphics.Blit(tex, previewRt, p.Material);
            RenderTexture.active = null; // Blit leaves the RT bound; clear so Release() doesn't warn.

            var sc = sprite.textureRect;
            int scale = IntScale(sprite, rect.height);
            float w = sc.width * scale, h = sc.height * scale;
            var dest = new Rect(Mathf.Round(x), Mathf.Round(rect.y + (rect.height - h) * 0.5f), w, h);
            var uv = new Rect(sc.x / tex.width, sc.y / tex.height, sc.width / tex.width, sc.height / tex.height);
            GUI.DrawTextureWithTexCoords(dest, previewRt, uv);
            x += w + pad;
        }
    }

    // Integer pixel scale to fit the row height (avoids mixels).
    static int IntScale(Sprite s, float rowHeight) => Mathf.Max(1, Mathf.FloorToInt(rowHeight / s.textureRect.height));

    void EnsureRt(Texture tex)
    {
        if (previewRt != null && previewRt.width == tex.width && previewRt.height == tex.height) return;
        if (previewRt != null) previewRt.Release();
        previewRt = new RenderTexture(tex.width, tex.height, 0) { filterMode = FilterMode.Point, hideFlags = HideFlags.DontSave };
    }

    // Every distinct, non-null sprite in the library (all sizes and variants).
    System.Collections.Generic.List<Sprite> PreviewSprites()
    {
        var list = new System.Collections.Generic.List<Sprite>();
        if (lib == null)
        {
            var guids = AssetDatabase.FindAssets("t:MechSpriteLibrary");
            if (guids.Length > 0)
                lib = AssetDatabase.LoadAssetAtPath<MechSpriteLibrary>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        if (lib == null || lib.sprites == null) return list;

        foreach (var sp in lib.sprites)
            if (sp != null && sp.texture != null && !list.Contains(sp)) list.Add(sp);
        return list;
    }

    void OnDisable()
    {
        if (previewRt != null) { previewRt.Release(); previewRt = null; }
        lib = null;
    }

    void Apply(MechPalette p, int i, RegionPalette rp)
    {
        Undo.RecordObject(p, "Edit Mech Palette");
        p.SetRegion(i, rp);
        EditorUtility.SetDirty(p);
        Repaint();
    }

    int DrawRegionChips(MechPalette p, int current)
    {
        var row = GUILayoutUtility.GetRect(0, 28, GUILayout.ExpandWidth(true));
        float w = row.width / 3f;
        if (chipStyle == null) chipStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        var style = chipStyle;
        for (int i = 0; i < 3; i++)
        {
            var r = new Rect(row.x + i * w + 2, row.y, w - 4, row.height);
            var rp = p.GetRegion(i);
            EditorGUI.DrawRect(r, Toneramps.Evaluate(rp, 0.85f));
            style.normal.textColor = Brightness(rp.BaseColor) > 0.5f ? Color.black : Color.white;
            GUI.Label(r, RegionNames[i], style);
            if (i == current) DrawBorder(r, Color.white, 2);
            if (Clicked(r)) current = i;
        }
        return current;
    }

    static void DrawRamp(Rect rect, RegionPalette rp)
    {
        const int n = 32;
        float seg = rect.width / n;
        for (int i = 0; i < n; i++)
            EditorGUI.DrawRect(new Rect(rect.x + i * seg, rect.y, seg + 1, rect.height),
                Toneramps.Evaluate(rp, i / (float)(n - 1)));
    }

    static void DrawBorder(Rect r, Color c, float t)
    {
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, t), c);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - t, r.width, t), c);
        EditorGUI.DrawRect(new Rect(r.x, r.y, t, r.height), c);
        EditorGUI.DrawRect(new Rect(r.xMax - t, r.y, t, r.height), c);
    }

    static bool Clicked(Rect r)
    {
        var e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && r.Contains(e.mousePosition)) { e.Use(); return true; }
        return false;
    }

    static float Brightness(Color c) => Mathf.Max(c.r, Mathf.Max(c.g, c.b));
}
