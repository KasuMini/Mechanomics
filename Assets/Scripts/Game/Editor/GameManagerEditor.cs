using System.Linq;
using UnityEditor;
using UnityEngine;

// Adds a play-mode debug panel: drop mechs into the live inventory and audition palettes per mech.
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    MechData mechToAdd;

    public override bool RequiresConstantRepaint() => Application.isPlaying;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var rs = ((GameManager)target).runState;
        if (rs == null) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Inventory (debug)", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play mode to drop mechs into the live inventory and audition palettes.\n" +
                "To preload mechs at run start, use the RunConfig's Starting Mechs list above.", MessageType.Info);
            return;
        }

        var list = Object.FindFirstObjectByType<OwnedMechList>();

        using (new EditorGUILayout.HorizontalScope())
        {
            mechToAdd = (MechData)EditorGUILayout.ObjectField("Add Mech", mechToAdd, typeof(MechData), false);
            using (new EditorGUI.DisabledScope(mechToAdd == null))
                if (GUILayout.Button("Add", GUILayout.Width(48)))
                {
                    if (rs.TryAddMech(mechToAdd)) list?.Sync();
                    else Debug.LogWarning($"No contiguous room on the bar for {mechToAdd.name}.");
                }
        }

        EditorGUILayout.Space();
        var mechs = rs.OwnedMechs.ToList();
        if (mechs.Count == 0) { EditorGUILayout.LabelField("(empty)"); return; }

        MechData toRemove = null;
        foreach (var m in mechs)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{m.mechName} {m.agilityStat}/{m.strengthStat}/{m.systemsStat}/{m.reliabilityStat} Sz{m.size}",
                    GUILayout.Width(200));
                EditorGUI.BeginChangeCheck();
                var pal = (MechPalette)EditorGUILayout.ObjectField(m.palette, typeof(MechPalette), false);
                if (EditorGUI.EndChangeCheck()) { m.palette = pal; list?.Sync(); }
                if (GUILayout.Button("X", GUILayout.Width(22))) toRemove = m;
            }
        }
        if (toRemove != null) { rs.RemoveMech(toRemove); list?.Sync(); }
    }
}
