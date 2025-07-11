using UnityEngine;
using UnityEngine.AI;

namespace BeltainsTools.Utilities
{
    public static class NavMeshUtilities
    {
        #region NavMeshPath
        public static float GetPathLength(NavMeshPath path)
        {
            float length = 0f;
            for (int i = 1; i < path.corners.Length; i++)
                length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            return length;
        }
        #endregion

        #region NavMeshAgent
        public static bool TryGetPathTo(NavMeshAgent agent, Vector3 targetPos, out NavMeshPath path, float maxDistanceTolerance = -1f, int areaMask = NavMesh.AllAreas)
        {
            path = new NavMeshPath();

            NavMeshQueryFilter filter = new NavMeshQueryFilter()
            {
                agentTypeID = agent.agentTypeID,
                areaMask = areaMask
            };

            if (!NavMesh.CalculatePath(agent.transform.position, targetPos, filter, path))
                return false;

            if (maxDistanceTolerance != -1 && Vector3.Distance(path.corners[path.corners.Length - 1], targetPos) > maxDistanceTolerance)
                return false;

            return true;
        }
        #endregion
    }
}
