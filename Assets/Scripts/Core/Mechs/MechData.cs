using UnityEngine;

[CreateAssetMenu(fileName = "MechData", menuName = "Mechanomics/MechData", order = 0)]
public class MechData : ScriptableObject, IMechStats
{
    [Header("Identity")]
    public string mechName;
    public string pilotName;

    public const int StatCapPrimary = 20;   // agility / strength / reliability ceiling
    public const int StatCapSystems = 8;

    [Header("Stat")]
    [Range(0, StatCapPrimary)] public int agilityStat;
    [Range(0, StatCapPrimary)] public int strengthStat;
    [Range(0, StatCapSystems)] public int systemsStat;
    [Range(0, StatCapPrimary)] public int reliabilityStat;
    [Range(1, 3)]              public int size = 1;
    public int variant;        // cosmetic sprite row

    [Header("Equipment")]
    public EquipmentData innateEquipment;

    // (agility + strength + systems + reliability) * x, floored
    public int cost { get { return Mathf.FloorToInt(agilityStat + strengthStat + systemsStat + reliabilityStat) * 100; } }

    public int Reliability => reliabilityStat;
    public int Size => size;

    // Notch span on the bottom inventory bar: size1=2, size2=3, size3=4.
    public const int TrackNotches = MechInventory.Capacity;
    public static int SlotSpan(int size) => size + 1;
    public int Span => SlotSpan(size);

    public int GetStat(MechStat stat)
    {
        switch (stat)
        {
            case MechStat.Agility: return agilityStat;
            case MechStat.Strength: return strengthStat;
            case MechStat.Systems: return systemsStat;
            default: return 0;
        }
    }


    public static MechData Create(string name, string pilot, int agility, int strength, int systems, int reliability, int size, int variant = 0)
    {
        var data = CreateInstance<MechData>();
        data.name = name;               // Object name -> what inspector reference fields show
        data.mechName = name;
        data.pilotName = pilot;
        data.agilityStat = agility;
        data.strengthStat = strength;
        data.systemsStat = systems;
        data.reliabilityStat = reliability;
        data.size = size;
        data.variant = variant;
        return data;
    }
}
