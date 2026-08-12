using UnityEngine;

namespace BeltainsTools
{
    [System.Serializable]
    public class SceneReference
    {
        [SerializeField]
        private string m_ScenePath;

        public string ScenePath => m_ScenePath;
        public string SceneName => string.IsNullOrEmpty(m_ScenePath) ? string.Empty : System.IO.Path.GetFileNameWithoutExtension(m_ScenePath);

#if UNITY_EDITOR
        public bool TryValidate()
        {
            return !string.IsNullOrEmpty(m_ScenePath) &&
                UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(m_ScenePath) != null;
        }
#endif
    }
}
