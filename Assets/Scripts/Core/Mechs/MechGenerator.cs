using UnityEngine;

public class MechGenerator : MonoBehaviour
{
    public MechSpriteLibrary spriteLibrary;
    public LoadText lists;

    // Size scales average stats: size1 x1.0, size2 x1.5, size3 x2.0.
    public static float SizeFactor(int size) => size == 1 ? 1f : size == 2 ? 1.5f : 2f;

    public static int ScaleStat(int baseVal, int size, int cap)
        => Mathf.Clamp(Mathf.RoundToInt(baseVal * SizeFactor(size)), 0, cap);

    // Generates new MechData for the market: rolls a size 1-3, then scales each stat by size.
    public MechData GenerateNewData()
    {
        string name = lists.mechNames[Random.Range(0, lists.mechNames.Length)];
        string pilot = lists.pilotNames[Random.Range(0, lists.pilotNames.Length)];
        int size = Random.Range(1, 4);
        int rows = spriteLibrary != null ? spriteLibrary.Rows : 1;
        int variant = rows > 0 ? Random.Range(0, rows) : 0;
        int agility     = ScaleStat(Random.Range(1, 11), size, MechData.StatCapPrimary);
        int strength    = ScaleStat(Random.Range(1, 11), size, MechData.StatCapPrimary);
        int reliability = ScaleStat(Random.Range(1, 11), size, MechData.StatCapPrimary);
        int systems     = ScaleStat(Random.Range(1, 5),  size, MechData.StatCapSystems);
        return MechData.Create(name, pilot, agility, strength, systems, reliability, size, variant);
    }
}
