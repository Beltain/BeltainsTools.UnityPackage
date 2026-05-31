using UnityEngine;

namespace BeltainsTools.PlayerInteraction
{
    public partial class InputRouter
    {
        public interface IRaycastStrategy
        {
            /// <returns>Layers that should be blacklisted for checking using this strategy</returns>
            /// <remarks>Use this if you want to exclude certain Layers across all strategies</remarks>
            public LayerMask GetLayerMaskBlacklist();
            /// <returns>Layers that should be whitelisted for checking using this strategy</returns>
            /// <remarks>Use this if you want to include certain Layers for this strategy, that could potentially be excluded by other strategies</remarks>
            public LayerMask GetLayerMaskWhitelist();
            /// <returns>If this target shouldn't be added to the raycast 'short list'</returns>
            /// <remarks>Use this if you want to exclude certain targets across all strategies</remarks>
            public bool GetInvalidateTarget(GameObject target);
            /// <return>If this target should be added to the raycast 'long list'</return>
            /// <remarks>Use this if you want to include certain targets for this strategy, that could potentially be excluded by other strategies</remarks>
            public bool GetValidateTarget(GameObject target);
        }

        [System.Serializable]
        public abstract class RaycastStrategyBase : IRaycastStrategy
        {
            /// <inheritdoc cref="IRaycastStrategy.GetLayerMaskBlacklist"/>
            protected virtual LayerMask GetLayerMaskBlacklist() => 0;
            /// <inheritdoc cref="IRaycastStrategy.GetLayerMaskWhitelist"/>
            protected virtual LayerMask GetLayerMaskWhitelist() => ~0;
            /// <inheritdoc cref="IRaycastStrategy.GetInvalidateTarget(GameObject)"/>
            protected virtual bool GetInvalidateTarget(GameObject target) => false;
            /// <inheritdoc cref="IRaycastStrategy.GetValidateTarget(GameObject)"/>
            protected virtual bool GetValidateTarget(GameObject target) => true;


            LayerMask IRaycastStrategy.GetLayerMaskBlacklist() => GetLayerMaskBlacklist();
            LayerMask IRaycastStrategy.GetLayerMaskWhitelist() => GetLayerMaskWhitelist();
            bool IRaycastStrategy.GetInvalidateTarget(GameObject target) => GetInvalidateTarget(target);
            bool IRaycastStrategy.GetValidateTarget(GameObject target) => GetValidateTarget(target);
        }

        [System.Serializable]
        public class RaycastStrategy : RaycastStrategyBase
        {
            [SerializeField]
            private LayerMask m_LayerMaskBlacklist = 0;
            [SerializeField]
            private LayerMask m_LayerMaskWhitelist = ~0;

            protected override LayerMask GetLayerMaskBlacklist() => m_LayerMaskBlacklist;
            protected override LayerMask GetLayerMaskWhitelist() => m_LayerMaskWhitelist;
        }
    }   
}
