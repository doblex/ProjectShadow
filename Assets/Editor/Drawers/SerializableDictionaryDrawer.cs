using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    private const float Line = 20f;
    private const float Pad = 4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var keys = property.FindPropertyRelative("keys");
        int count = keys.arraySize;
        return (count + 2) * (Line + Pad); // header + items + add button
    }

    public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
    {
        SerializedProperty keys = property.FindPropertyRelative("keys");
        SerializedProperty values = property.FindPropertyRelative("values");

        EditorGUI.BeginProperty(pos, label, property);

        // Draw main label
        Rect r = new Rect(pos.x, pos.y, pos.width, Line);
        EditorGUI.LabelField(r, label);
        r.y += Line + Pad;

        // Draw column headers
        float half = pos.width * 0.45f;
        EditorGUI.LabelField(new Rect(r.x, r.y, half, Line), "Key");
        EditorGUI.LabelField(new Rect(r.x + half + 10, r.y, half, Line), "Value");
        r.y += Line + Pad;

        for (int i = 0; i < keys.arraySize; i++)
        {
            SerializedProperty keyProp = keys.GetArrayElementAtIndex(i);
            SerializedProperty valProp = values.GetArrayElementAtIndex(i);

            Rect keyRect = new Rect(r.x, r.y, half, Line);
            Rect valRect = new Rect(r.x + half + 10, r.y, half, Line);

            GUI.enabled = false;
            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
            GUI.enabled = true;
            EditorGUI.PropertyField(valRect, valProp, GUIContent.none);

            r.y += Line + Pad;
        }

        EditorGUI.EndProperty();
    }
}
