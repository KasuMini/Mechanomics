using UnityEditor;
using UnityEngine;

// Shared expandable foldout for ScriptableObject references: the object field plus, when opened, the
// referenced object's fields drawn inline. Subclass + [CustomPropertyDrawer(typeof(T))] to enable per type.
public abstract class ExpandableScriptableObjectDrawer : PropertyDrawer
{
    const float Pad = 2f;

    // Optional trailing line (e.g. a computed value); 0 = none.
    protected virtual float ExtraHeight => 0f;
    protected virtual void DrawExtra(Rect rect, Object target) { }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float line = EditorGUIUtility.singleLineHeight;
        var header = new Rect(position.x, position.y, position.width, line);
        EditorGUI.PropertyField(header, property, label);

        var target = property.objectReferenceValue;
        if (target != null)
        {
            // foldout arrow overlaid on the label area (object field already drew the name)
            var foldRect = new Rect(header.x, header.y, EditorGUIUtility.labelWidth, line);
            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, GUIContent.none, true);
        }
        else
        {
            property.isExpanded = false;
        }

        if (property.isExpanded && target != null)
        {
            EditorGUI.indentLevel++;
            var so = new SerializedObject(target);
            so.Update();

            float y = position.y + line + EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty p = so.GetIterator();
            bool enter = true;
            while (p.NextVisible(enter))
            {
                enter = false;
                if (p.propertyPath == "m_Script") continue;
                float h = EditorGUI.GetPropertyHeight(p, true);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), p, true);
                y += h + EditorGUIUtility.standardVerticalSpacing;
            }
            if (ExtraHeight > 0f) DrawExtra(new Rect(position.x, y, position.width, line), target);

            if (so.ApplyModifiedProperties()) EditorUtility.SetDirty(target);
            so.Dispose();
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        var target = property.objectReferenceValue;
        if (!property.isExpanded || target == null) return line;

        float h = line + EditorGUIUtility.standardVerticalSpacing;
        var so = new SerializedObject(target);
        SerializedProperty p = so.GetIterator();
        bool enter = true;
        while (p.NextVisible(enter))
        {
            enter = false;
            if (p.propertyPath == "m_Script") continue;
            h += EditorGUI.GetPropertyHeight(p, true) + EditorGUIUtility.standardVerticalSpacing;
        }
        so.Dispose();
        return h + ExtraHeight + Pad;
    }
}

[CustomPropertyDrawer(typeof(EventData))]
public class EventDataDrawer : ExpandableScriptableObjectDrawer { }

[CustomPropertyDrawer(typeof(EquipmentData))]
public class EquipmentDataDrawer : ExpandableScriptableObjectDrawer { }
