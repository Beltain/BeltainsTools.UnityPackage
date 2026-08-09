using UnityEngine;

namespace BeltainsTools
{
    [System.Serializable]
    public class SceneReference
    {
        [SerializeField]
        private string m_ScenePath;

        public string ScenePath => m_ScenePath;
        public string SceneName => m_ScenePath.IsNullOrEmpty() ? string.Empty : System.IO.Path.GetFileNameWithoutExtension(m_ScenePath);

#if UNITY_EDITOR
        public bool TryValidate()
        {
            return !m_ScenePath.IsNullOrEmpty() &&
                UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(m_ScenePath) != null;
        }
#endif
    }
}
