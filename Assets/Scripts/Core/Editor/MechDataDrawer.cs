using UnityEditor;
using UnityEngine;

// Expandable MechData reference (market/owned arrays) with the computed Cost appended, mirroring MechDataEditor.
[CustomPropertyDrawer(typeof(MechData))]
public class MechDataDrawer : ExpandableScriptableObjectDrawer
{
    protected override float ExtraHeight => EditorGUIUtility.singleLineHeight;

    protected override void DrawExtra(Rect rect, Object target)
        => EditorGUI.LabelField(rect, "Cost", ((MechData)target).cost.ToString());
}
