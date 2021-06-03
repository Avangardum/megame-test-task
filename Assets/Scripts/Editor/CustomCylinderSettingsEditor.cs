using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;

namespace MegameTestTask.Editor
{
    [CustomEditor(typeof(CustomCylinderSettings))]
    public class CustomCylinderSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var convertedTarget = (CustomCylinderSettings) target;
            
            EditorGUILayout.LabelField("Visible mesh", EditorStyles.boldLabel);
            convertedTarget.VisibleMeshHeight = EditorGUILayout.FloatField("Height", convertedTarget.VisibleMeshHeight);
            convertedTarget.VisibleMeshRadius = EditorGUILayout.FloatField("Radius", convertedTarget.VisibleMeshRadius);

            EditorGUILayout.LabelField("Collider", EditorStyles.boldLabel);
            convertedTarget.ColliderHeight = EditorGUILayout.FloatField("Height", convertedTarget.ColliderHeight);
            convertedTarget.ColliderRadius = EditorGUILayout.FloatField("Radius", convertedTarget.ColliderRadius);
        }
    }
}