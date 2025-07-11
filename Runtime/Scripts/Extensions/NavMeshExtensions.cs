using UnityEngine;
using UnityEngine.AI;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class NavMeshExtensions
    {
        #region NavMeshPath
        /// <inheritdoc cref="NavMeshUtilities.GetPathLength(NavMeshPath)"/>
        public static float GetPathLength(this NavMeshPath path)
            => NavMeshUtilities.GetPathLength(path);
        #endregion

        #region NavMeshAgent
        /// <inheritdoc cref="NavMeshUtilities.TryGetPathTo(NavMeshAgent, Vector3, out NavMeshPath, float, int)"/>
        public static bool TryGetPathTo(this NavMeshAgent agent, Vector3 targetPos, out NavMeshPath path, float maxDistanceTolerance = -1f, int areaMask = NavMesh.AllAreas)
            => NavMeshUtilities.TryGetPathTo(agent, targetPos, out path, maxDistanceTolerance, areaMask);
        #endregion
    }
}
