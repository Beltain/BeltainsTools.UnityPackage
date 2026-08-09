using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools
{
    public interface IAssetRegisterable<TKey>
    {
        public TKey GetIdentifierKey();
    }

    public abstract class AssetRegisterObject : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public string m_RebuildPath = "Assets/";

        public abstract void SetAssets(IEnumerable<object> assets);
        public abstract System.Type GetAssetType();
    }

    public abstract class AssetRegisterObject<TKey, TAsset> : AssetRegisterObject, IEnumerable<TAsset> where TAsset : UnityEngine.Object, IAssetRegisterable<TKey>
    {
        [SerializeField]
        protected TAsset[] m_Assets = new TAsset[0];

        protected Dictionary<TKey, TAsset> m_LookupTable = null;

        public TAsset this[TKey key]
        {
            get => Get(key);
        }

        public override System.Type GetAssetType() => typeof(TAsset);
        public override void SetAssets(IEnumerable<object> assets)
        {
            m_Assets = assets.Cast<TAsset>().ToArray();
        }

        public IEnumerator<TAsset> GetEnumerator()
        {
            return ((IEnumerable<TAsset>)m_Assets).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Assets.GetEnumerator();
        }

        public bool Contains(TKey key)
        {
            if (m_LookupTable == null)
                InitLookup();

            return m_LookupTable.ContainsKey(key);
        }

        public TAsset Get(TKey key)
        {
            if (m_LookupTable == null)
                InitLookup();

            return m_LookupTable.ContainsKey(key) ? m_LookupTable[key] : null;
        }

        private void InitLookup()
        {
            bool warnOfNulls = false;
            m_LookupTable = new Dictionary<TKey, TAsset>();
            foreach (TAsset asset in m_Assets)
            {
                if (asset == null)
                {
                    warnOfNulls = true;
                    continue;
                }

                m_LookupTable.Add(asset.GetIdentifierKey(), asset);
            }

            if (warnOfNulls)
                d.LogWarning($"Empty entries detected in {name} asset register object! Please remove all empty entries from the assets array!");
        }
    }
}
