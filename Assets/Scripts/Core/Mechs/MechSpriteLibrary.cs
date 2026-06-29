using UnityEngine;

// Maps a mech's (size, variant) to its sprite cell. Sprites are stored row-major:
// columns are sizes (col = size-1), rows are cosmetic variants.
[CreateAssetMenu(fileName = "MechSpriteLibrary", menuName = "Mechanomics/MechSpriteLibrary", order = 1)]
public class MechSpriteLibrary : ScriptableObject
{
    public int columns = 3;
    public Sprite[] sprites;

    public int Rows => columns > 0 ? sprites.Length / columns : 0;

    // Flat index for a cell, or -1 if out of range. Variant wraps within available rows.
    public static int Index(int size, int variant, int columns, int count)
    {
        if (columns <= 0 || count <= 0) return -1;
        int rows = count / columns;
        if (rows <= 0) return -1;
        int col = Mathf.Clamp(size - 1, 0, columns - 1);
        int row = ((variant % rows) + rows) % rows;
        int i = row * columns + col;
        return i < count ? i : -1;
    }

    public Sprite Get(int size, int variant)
    {
        if (sprites == null) return null;
        int i = Index(size, variant, columns, sprites.Length);
        return i >= 0 ? sprites[i] : null;
    }
}
