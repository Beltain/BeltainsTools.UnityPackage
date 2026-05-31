using UnityEngine;

namespace BeltainsTools.PlayerInteraction
{
    [RequireComponent(typeof(InputRouter))]
    /// <summary>Implementer base component for <see cref="InputRouter.IRaycastStrategy"/>. Provides default implementations for all methods, so you can just override the ones you need.</summary>
    public abstract class RaycastStrategyComponentBase : MonoBehaviour, InputRouter.IRaycastStrategy
    {
        protected InputRouter m_InputRouter;

        protected virtual bool GetInvalidateTarget(GameObject target) => false;
        bool InputRouter.IRaycastStrategy.GetInvalidateTarget(GameObject target)
        {
            return GetInvalidateTarget(target);
        }

        protected virtual bool GetValidateTarget(GameObject target) => true;
        bool InputRouter.IRaycastStrategy.GetValidateTarget(GameObject target)
        {
            return GetValidateTarget(target);
        }

        protected virtual LayerMask GetLayerMaskBlacklist() => 0;
        LayerMask InputRouter.IRaycastStrategy.GetLayerMaskBlacklist()
        {
            return GetLayerMaskBlacklist();
        }

        protected virtual LayerMask GetLayerMaskWhitelist() => ~0;
        LayerMask InputRouter.IRaycastStrategy.GetLayerMaskWhitelist()
        {
            return GetLayerMaskWhitelist();
        }


        virtual protected void Awake()
        {
            m_InputRouter = GetComponent<InputRouter>();
            m_InputRouter.RegisterRaycastStrategy(this);
        }

        virtual protected void OnDestroy()
        {
            m_InputRouter.DeregisterRaycastStrategy(this);
        }
    }

    public class BasicRaycastStrategyComponent : RaycastStrategyComponentBase
    {
        [SerializeField]
        private LayerMask m_LayerMaskBlacklist = 0;
        [SerializeField]
        private LayerMask m_LayerMaskWhitelist = ~0;

        protected override LayerMask GetLayerMaskBlacklist() => m_LayerMaskBlacklist;
        protected override LayerMask GetLayerMaskWhitelist() => m_LayerMaskWhitelist;
    }
}