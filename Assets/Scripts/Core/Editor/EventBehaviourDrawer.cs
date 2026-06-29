using System;
using UnityEditor;
using UnityEngine;

// Picks the concrete behaviour for an EventData's [SerializeReference] EventBehaviour field.
// The type list is hardcoded below (no assembly scanning) — add new EventBehaviour subclasses here.
[CustomPropertyDrawer(typeof(EventBehaviour), true)]
public class EventBehaviourDrawer : PropertyDrawer
{
    static readonly Type[] Types =
    {
        typeof(SkillEvent),
        // typeof(CombatEvent),   // restore when combat is un-shelved
    };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        int current = 0;
        string currentName = TypeName(property.managedReferenceFullTypename);
        for (int i = 0; i < Types.Length; i++)
            if (Types[i].Name == currentName) { current = i + 1; break; }

        string[] names = new string[Types.Length + 1];
        names[0] = "(None)";
        for (int i = 0; i < Types.Length; i++) names[i + 1] = Types[i].Name;

        var popup = new Rect(
            position.x + EditorGUIUtility.labelWidth + 2f, position.y,
            position.width - EditorGUIUtility.labelWidth - 2f, EditorGUIUtility.singleLineHeight);

        int selected = EditorGUI.Popup(popup, current, names);
        if (selected != current)
        {
            property.managedReferenceValue = selected == 0 ? null : Activator.CreateInstance(Types[selected - 1]);
            property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, label, true);

    static string TypeName(string managedReferenceFullTypename)
    {
        if (string.IsNullOrEmpty(managedReferenceFullTypename)) return null;
        int space = managedReferenceFullTypename.LastIndexOf(' ');
        string full = space >= 0 ? managedReferenceFullTypename.Substring(space + 1) : managedReferenceFullTypename;
        int dot = full.LastIndexOf('.');
        return dot >= 0 ? full.Substring(dot + 1) : full;
    }
}
