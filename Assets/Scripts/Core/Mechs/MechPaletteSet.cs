using UnityEngine;

// A curated list of palettes to draw random mech colours from.
[CreateAssetMenu(fileName = "MechPaletteSet", menuName = "Mechanomics/MechPaletteSet", order = 3)]
public class MechPaletteSet : ScriptableObject
{
    public MechPalette[] palettes;

    public MechPalette Random()
    {
        if (palettes == null || palettes.Length == 0) return null;
        return palettes[UnityEngine.Random.Range(0, palettes.Length)];
    }
}
