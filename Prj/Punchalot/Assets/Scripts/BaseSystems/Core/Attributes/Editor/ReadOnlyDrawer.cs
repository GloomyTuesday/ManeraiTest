#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Scripts.BaseSystems
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Uneditable))]
    public class UneditableDrawer: PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool wasEnabled = GUI.enabled;
            GUI.enabled = false; 

            EditorGUI.PropertyField(position, property, label, true);

            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
#endif
}

