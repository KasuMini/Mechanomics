using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Mechanomics/EquipmentData", order = 0)]
public class EquipmentData : ScriptableObject
{
    

    [Header("Identity")]
    public string equipmentName;

    [Header("Stat Change")]
    public int statDelta;
    public MechStat changedStat;

}
