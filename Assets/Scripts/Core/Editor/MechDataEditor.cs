using UnityEditor;

[CustomEditor(typeof(MechData))]
public class MechDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Cost", ((MechData)target).cost.ToString());
    }
}
