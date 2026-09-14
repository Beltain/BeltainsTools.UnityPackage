using UnityEngine;

namespace BeltainsTools
{
    /// <summary>
    /// A reusable unique identifier field for assets.
    /// Resolves to a user-provided override when set, otherwise falls back to the owning asset's name.
    /// </summary>
    [System.Serializable]
    public struct AssetUID
    {
        [SerializeField]
        private string m_Override;

        public bool HasOverride => !string.IsNullOrEmpty(m_Override);

        /// <summary>Resolve the UID for the given owning asset.</summary>
        /// <returns>The override if one is set, otherwise the asset's name</returns>
        public string Resolve(Object owningAsset)
        {
            return Resolve(m_Override, owningAsset);
        }

        public static string Resolve(string overrideValue, Object owningAsset)
        {
            string resolvedUID = !string.IsNullOrEmpty(overrideValue) ? overrideValue :
                owningAsset != null ? owningAsset.name :
                string.Empty;

            return Utilities.StringUtilities.ToBigUpper(resolvedUID);
        }

        public static implicit operator string(AssetUID uid) => uid.m_Override;
    }
}
