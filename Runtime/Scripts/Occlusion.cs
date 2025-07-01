using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.Occlusion
{
    /// <summary>A class that handles fading/disabling of objects between a given camera and a target</summary>
    public abstract class TargetOcclusionMasker<TOccluder, TTarget> where TOccluder : new()
    {
        protected Camera m_Cam;
        protected TTarget[] m_Targets;

        protected HashSet<TOccluder> m_Occluders = new HashSet<TOccluder>(128);

#if UNITY_EDITOR
        protected static bool s_Debugging = true;
#endif

        protected bool HasTargets => m_Targets != null && m_Targets.Length > 0;

        protected Func<TOccluder, bool> OccluderFilterDelegate;
        protected Action<TOccluder> AddToOccluderListDelegate;
        protected Action<TOccluder> RemoveFromOccluderListDelegate;


        public TargetOcclusionMasker(
            Camera cam, 
            Func<TOccluder, bool> occluderFilterDelegate, 
            Action<TOccluder> addToOccludingListDelegate, 
            Action<TOccluder> removeFromOccludingListDelegate
            )
        {
            SetCamera(cam);
            m_Targets = null;
            OccluderFilterDelegate = occluderFilterDelegate;
            AddToOccluderListDelegate = addToOccludingListDelegate;
            RemoveFromOccluderListDelegate = removeFromOccludingListDelegate;
        }



        public void SetTarget(TTarget target) => SetTargets(target);
        public void SetTargets(params TTarget[] targets)
        {
            m_Targets = targets;
        }

        public void SetCamera(Camera cam)
        {
            m_Cam = cam;
        }



#if UNITY_EDITOR
        protected Vector3[] _debug_LastFrameClosestHits = new Vector3[0];
#endif
        /// <summary>Given the current camera and target data, rebuild the occluding list</summary>
        public abstract HashSet<TOccluder> RebuildOccluderList();

        public void ResetOccluderList()
        {
            foreach (TOccluder occluder in m_Occluders)
                RemoveFromOccluderListDelegate.Invoke(occluder);
            m_Occluders.Clear();
        }

        public virtual void DrawGizmos()
        {
#if UNITY_EDITOR

#endif
        }
    }

    public class RayVolumeTargetOcclusionMasker<TOccluder> : TargetOcclusionMasker<TOccluder, BoxCollider> where TOccluder : new()
    {
        protected ProbeConfig m_ProbeConfig;
        protected Func<Collider, TOccluder> GetOccluderFromColliderDelegate;

        public struct ProbeConfig
        {
            public int VolumeLayerMask;
            public int OccluderLayerMask;
            public int Frequency;
            public float EdgePadding;
            public float RayLength;
            public float MaxCameraOrthoSize;
        }


        public RayVolumeTargetOcclusionMasker(
            Camera cam, 
            ProbeConfig probeSettings, 
            Func<Collider, TOccluder> getOccluderFromColliderDelegate, 
            Func<TOccluder, bool> occluderFilterDelegate, 
            Action<TOccluder> addToOccludingListDelegate, 
            Action<TOccluder> removeFromOccludingListDelegate
            )
            : base(cam, occluderFilterDelegate, addToOccludingListDelegate, removeFromOccludingListDelegate) 
        {
            m_ProbeConfig = probeSettings;
            GetOccluderFromColliderDelegate = getOccluderFromColliderDelegate;
        }


#if UNITY_EDITOR
        protected static Ray[] _debug_LastFrameRays = new Ray[0];
#endif
        static Bounds _s_TargetScreenBounds = new Bounds();
        static RaycastHit[] _s_RayHitCache = new RaycastHit[16];
        static HashSet<TOccluder> _s_NewOccluders = new HashSet<TOccluder>();
        public override HashSet<TOccluder> RebuildOccluderList()
        {
            if (!HasTargets || m_Cam.orthographicSize > m_ProbeConfig.MaxCameraOrthoSize)
            {
                ResetOccluderList();
                return m_Occluders;
            }

#if UNITY_EDITOR
            if (s_Debugging)
            {
                int probeCount = m_ProbeConfig.Frequency * m_ProbeConfig.Frequency;
                if (_debug_LastFrameRays.Length != probeCount)
                    Array.Resize(ref _debug_LastFrameRays, probeCount);
                if (_debug_LastFrameClosestHits.Length != probeCount)
                    Array.Resize(ref _debug_LastFrameClosestHits, probeCount);
            }
#endif

            for (int i = 0; i < m_Targets.Length; i++)
            {
                m_Cam.GetScreenBoundsForWorldBounds(m_Targets[i].bounds, ref _s_TargetScreenBounds);
                float rayRowSpacing = (_s_TargetScreenBounds.size.y * (1f - m_ProbeConfig.EdgePadding)) / (m_ProbeConfig.Frequency - 1);
                float rayColSpacing = (_s_TargetScreenBounds.size.x * (1f - m_ProbeConfig.EdgePadding)) / (m_ProbeConfig.Frequency - 1);

                int xyIndex = 0;
                for (int x = 0; x < m_ProbeConfig.Frequency; x++)
                {
                    for (int y = 0; y < m_ProbeConfig.Frequency; y++)
                    {
                        Ray ray = m_Cam.ScreenPointToRay(_s_TargetScreenBounds.min + new Vector3(x * rayColSpacing, y * rayRowSpacing));
#if UNITY_EDITOR
                        if (s_Debugging)
                            _debug_LastFrameRays[xyIndex] = ray;
#endif
                        int hitCount = Physics.RaycastNonAlloc(ray, _s_RayHitCache, m_ProbeConfig.RayLength, m_ProbeConfig.OccluderLayerMask | m_ProbeConfig.VolumeLayerMask);
                        float closestFocusAreaDist = -1;
                        for (int j = 0; j < hitCount; j++)
                        {
                            int hitLayerMask = 1 << _s_RayHitCache[j].collider.gameObject.layer;
                            if (((hitLayerMask & m_ProbeConfig.VolumeLayerMask) != 0) && (closestFocusAreaDist == -1 || closestFocusAreaDist > _s_RayHitCache[j].distance))
                                closestFocusAreaDist = _s_RayHitCache[j].distance;
                        }

#if UNITY_EDITOR
                        if (s_Debugging)
                            _debug_LastFrameClosestHits[xyIndex] = ray.origin + (ray.direction * (closestFocusAreaDist != -1 ? closestFocusAreaDist : m_ProbeConfig.RayLength));
#endif

                        if (closestFocusAreaDist != -1)
                        {
                            for (int j = 0; j < hitCount; j++)
                            {
                                int hitLayerMask = 1 << _s_RayHitCache[j].collider.gameObject.layer;
                                if (((hitLayerMask & m_ProbeConfig.OccluderLayerMask) != 0) && closestFocusAreaDist >= _s_RayHitCache[j].distance)
                                {
                                    //Finally set objects occluding
                                    TOccluder occluder = GetOccluderFromColliderDelegate.Invoke(_s_RayHitCache[j].collider);
                                    if (occluder != null && OccluderFilterDelegate.Invoke(occluder) && _s_NewOccluders.Add(occluder) && !m_Occluders.Contains(occluder))
                                    {
                                        AddToOccluderListDelegate.Invoke(occluder);
                                    }
                                }
                            }
                        }

                        xyIndex++;
                    }
                }
            }

            //This is really heavy but necessary to ensure the remove delegate is called only when there's absolutely been a change in the occluder's status in the list
            foreach (TOccluder occluder in m_Occluders)
            {
                if(!_s_NewOccluders.Contains(occluder))
                {
                    RemoveFromOccluderListDelegate.Invoke(occluder);
                }
            }

            m_Occluders.Clear();
            foreach (TOccluder newOccluder in _s_NewOccluders)
                m_Occluders.Add(newOccluder);

            _s_NewOccluders.Clear();
            return m_Occluders;
        }


        public override void DrawGizmos()
        {
#if UNITY_EDITOR
            if (!s_Debugging)
                return;

            for (int i = 0; i < _debug_LastFrameRays.Length; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_debug_LastFrameRays[i].origin, _debug_LastFrameClosestHits[i]);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_debug_LastFrameClosestHits[i], _debug_LastFrameRays[i].origin + (_debug_LastFrameRays[i].direction * m_ProbeConfig.RayLength));
            }
#endif
        }
    }
}
