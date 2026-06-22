using UnityEngine;

[CreateAssetMenu(fileName = "MechData", menuName = "Mechanomics/MechData", order = 0)]
public class MechData : ScriptableObject
{
    [Header("Identity")]
    public string mechName;
    public string pilotName;

    [Header("Stat")]
    [Range(0, 10)] public int agilityStat;
    [Range(0, 10)] public int strengthStat;
    [Range(0, 4)]  public int systemsStat;
    [Range(0, 10)] public int reliabilityStat;
    [Range(1, 3)]  public int size = 1;

    // (agility + strength + systems) * reliability, floored
    public int cost { get { return Mathf.FloorToInt(((float)agilityStat + strengthStat + systemsStat) * reliabilityStat); } }

    public static MechData Create(string name, string pilot, int agility, int strength, int systems, int reliability, int size)
    {
        var data = CreateInstance<MechData>();
        data.mechName = name;
        data.pilotName = pilot;
        data.agilityStat = agility;
        data.strengthStat = strength;
        data.systemsStat = systems;
        data.reliabilityStat = reliability;
        data.size = size;
        return data;
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(MechData))]
public class MechDataEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        UnityEditor.EditorGUILayout.LabelField("Cost", ((MechData)target).cost.ToString());
    }
}
#endif
