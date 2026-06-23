using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Draws a dropdown of concrete subclasses for a [SerializeReference, SubclassSelector] field.
[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        List<Type> types = GetCandidateTypes();
        string[] names = new[] { "(None)" }.Concat(types.Select(t => t.Name)).ToArray();

        int current = 0;
        string currentName = TypeName(property.managedReferenceFullTypename);
        for (int i = 0; i < types.Count; i++)
            if (types[i].Name == currentName) { current = i + 1; break; }

        Rect popup = new Rect(
            position.x + EditorGUIUtility.labelWidth + 2f, position.y,
            position.width - EditorGUIUtility.labelWidth - 2f, EditorGUIUtility.singleLineHeight);

        int selected = EditorGUI.Popup(popup, current, names);
        if (selected != current)
        {
            property.managedReferenceValue = selected == 0 ? null : Activator.CreateInstance(types[selected - 1]);
            property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, label, true);

    List<Type> GetCandidateTypes()
    {
        Type baseType = ElementType(fieldInfo.FieldType);
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(SafeGetTypes)
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType)
            .OrderBy(t => t.Name)
            .ToList();
    }

    static Type ElementType(Type t)
    {
        if (t.IsArray) return t.GetElementType();
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) return t.GetGenericArguments()[0];
        return t;
    }

    static IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly a)
    {
        try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
    }

    static string TypeName(string managedReferenceFullTypename)
    {
        if (string.IsNullOrEmpty(managedReferenceFullTypename)) return null;
        int space = managedReferenceFullTypename.LastIndexOf(' ');
        string full = space >= 0 ? managedReferenceFullTypename.Substring(space + 1) : managedReferenceFullTypename;
        int dot = full.LastIndexOf('.');
        return dot >= 0 ? full.Substring(dot + 1) : full;
    }
}
