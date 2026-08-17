using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace BeltainsTools
{
    [DisallowMultipleComponent]
    public class SceneGuid : MonoBehaviour
    {
        [SerializeField] SerialisableGuid m_Guid;

        public SerialisableGuid Guid => m_Guid;

#if UNITY_EDITOR
        void OnValidate()
        {
            // Defer: OnValidate can run during import; delayCall keeps us out of
            // illegal SerializedObject/asset-write windows.
            EditorApplication.delayCall -= EnforceContext;
            EditorApplication.delayCall += EnforceContext;
        }

        void EnforceContext()
        {
            if (this == null)
                return; // destroyed before delayCall fired

            if (ShouldBeBlank())
            {
                if (m_Guid.IsInitialized())
                {
                    m_Guid = default;
                    EditorUtility.SetDirty(this);
                }
            }
            else if (!m_Guid.IsInitialized())
            {
                m_Guid = SerialisableGuid.NewGuid();
                EditorUtility.SetDirty(this);
            }
        }

        bool ShouldBeBlank()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(this))
                return true; // In a prefab asset on disk or open in Prefab Mode

            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.IsPartOfPrefabContents(gameObject))
                return true;

            return false;
        }
#endif
    }
}
