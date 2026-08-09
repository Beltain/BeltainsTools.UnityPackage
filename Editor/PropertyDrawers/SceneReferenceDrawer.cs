using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Retreive the scene asset at the current stored path
            SerializedProperty scenePathProperty = property.FindPropertyRelative("m_ScenePath");
            Object sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathProperty.stringValue);

            // Update the scene path if a new scene asset is assigned
            Object newSceneAsset = EditorGUI.ObjectField(position, label, sceneAsset, typeof(SceneAsset), false);
            if (newSceneAsset != sceneAsset)
                scenePathProperty.stringValue = newSceneAsset is SceneAsset ? AssetDatabase.GetAssetPath(newSceneAsset) : string.Empty;

            EditorGUI.EndProperty();
        }
    }
}
