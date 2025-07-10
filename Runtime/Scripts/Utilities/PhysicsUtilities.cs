using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class PhysicsUtilities
    {
        public static int RaycastNonAlloc(Ray ray, ref RaycastHit[] results, float maxDistance, int layerMask) => RaycastNonAlloc(ray, ref results, maxDistance, layerMask, QueryTriggerInteraction.UseGlobal);
        public static int RaycastNonAlloc(Ray ray, ref RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            int raycastHits = UnityEngine.Physics.RaycastNonAlloc(ray, results, maxDistance, layerMask, queryTriggerInteraction);
            if (raycastHits > results.Length)
            {
                while (raycastHits > results.Length)
                    System.Array.Resize(ref results, results.Length * 2);
                return RaycastNonAlloc(ray, ref results, maxDistance, layerMask, queryTriggerInteraction);
            }
            return raycastHits;
        }

        public static int SpherecastNonAlloc(Vector3 origin, float radius, Vector3 direction, ref RaycastHit[] results, float maxDistance, int layerMask) => SpherecastNonAlloc(origin, radius, direction, ref results, maxDistance, layerMask, QueryTriggerInteraction.UseGlobal);
        public static int SpherecastNonAlloc(Vector3 origin, float radius, Vector3 direction, ref RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            int spherecastHits = UnityEngine.Physics.SphereCastNonAlloc(origin, radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
            if (spherecastHits > results.Length)
            {
                while (spherecastHits > results.Length)
                    System.Array.Resize(ref results, results.Length * 2);
                return SpherecastNonAlloc(origin, radius, direction, ref results, maxDistance, layerMask, queryTriggerInteraction);
            }
            return spherecastHits;
        }
    }
}
