using UnityEngine;

namespace BeltainsTools.Juice
{
    public partial class SpringyTargetFollower
    {
        public static class FollowStrategies
        {
            [System.Serializable]
            public abstract class FollowStrategyBase
            {
                protected virtual bool TryGetWorldPosition(TargetStrategies.TargetStrategyBase target, out Vector3 position) { position = Vector3.zero; return false; }
                protected virtual bool TryGetWorldRotation(TargetStrategies.TargetStrategyBase target, out Quaternion rotation) { rotation = Quaternion.identity; return false; }
                protected virtual bool TryGetLocalScale(TargetStrategies.TargetStrategyBase target, out Vector3 localScale) { localScale = Vector3.one; return false; }

                public virtual void Update(SpringyTransform transform, TargetStrategies.TargetStrategyBase target)
                {
                    if (TryGetWorldPosition(target, out Vector3 position))
                        transform.position = position;
                    if (TryGetWorldRotation(target, out Quaternion rotation))
                        transform.rotation = rotation;
                    if (TryGetLocalScale(target, out Vector3 localScale))
                        transform.localScale = localScale;
                }
            }

            [System.Serializable]
            public class FollowSimple : FollowStrategyBase
            {
                public FollowTypes m_FollowTypes = ~(FollowTypes)0;

                [System.Flags]
                public enum FollowTypes { Position = 1 << 0, Rotation = 1 << 1, Scale = 1 << 2 }


                public FollowSimple() : this(~(FollowTypes)0) { }
                public FollowSimple(FollowTypes followTypes)
                {
                    m_FollowTypes = followTypes;
                }

                protected override bool TryGetWorldPosition(TargetStrategies.TargetStrategyBase target, out Vector3 position)
                {
                    if ((m_FollowTypes & FollowTypes.Position) == 0)
                        return base.TryGetWorldPosition(target, out position);
                    position = target.GetWorldPosition();
                    return true;
                }

                protected override bool TryGetWorldRotation(TargetStrategies.TargetStrategyBase target, out Quaternion rotation)
                {
                    if ((m_FollowTypes & FollowTypes.Rotation) == 0)
                        return base.TryGetWorldRotation(target, out rotation);
                    rotation = target.GetWorldRotation();
                    return true;
                }

                protected override bool TryGetLocalScale(TargetStrategies.TargetStrategyBase target, out Vector3 localScale)
                {
                    if ((m_FollowTypes & FollowTypes.Scale) == 0)
                        return base.TryGetLocalScale(target, out localScale);
                    localScale = target.GetLocalScale();
                    return true;
                }
            }

            [System.Serializable]
            public class FollowTiltToTarget : FollowStrategyBase
            {
                public Vector3 m_RotationAxis;
                public Vector3 m_RestingUp;
                public TiltSettings m_TiltSettings;

                private Vector3 m_CurrentPosition;

                [System.Serializable]
                public class TiltSettings
                {
                    [Tooltip("The maximum tilt angle in either direction")]
                    public float AngleMax = 10f;
                    [Tooltip("Distance sensitivity scale. How far our target needs to be from us in order to reach our max tilt angle. Use lower value for more exaggerated tilting.\n" +
                        "0 = near instant tilt every move. 100 = when our target is 100 units away we will reach full tilt")]
                    public float MaxDeltaFactor = 10f;
                }

                public FollowTiltToTarget() : this(new TiltSettings(), Vector3.forward, Vector3.up) { }
                public FollowTiltToTarget(TiltSettings tiltSettings, Vector3 rotationAxis, Vector3 restingUp)
                {
                    m_RotationAxis = rotationAxis;
                    m_RestingUp = restingUp;
                    m_TiltSettings = tiltSettings;
                }

                public override void Update(SpringyTransform transform, TargetStrategies.TargetStrategyBase target)
                {
                    m_CurrentPosition = transform.transform.position; // actual current position of the springy follower, used for tilt calculation

                    base.Update(transform, target);
                }

                protected override bool TryGetWorldPosition(TargetStrategies.TargetStrategyBase target, out Vector3 position)
                {
                    position = target.GetWorldPosition();
                    return true;
                }

                protected override bool TryGetWorldRotation(TargetStrategies.TargetStrategyBase target, out Quaternion rotation)
                {
                    rotation = CalculateTiltTowards(m_TiltSettings, m_CurrentPosition, target.GetWorldPosition(), m_RotationAxis, m_RestingUp);
                    return true;
                }


                private static Quaternion CalculateTiltTowards(TiltSettings tiltSettings, Vector3 currentPos, Vector3 targetPos, Vector3 rotationAxis, Vector3 restingUp)
                {
                    // shitty tilt logic from nippets
                    Quaternion newTarget = Quaternion.LookRotation(rotationAxis, restingUp);
                    Vector3 targetDelta = targetPos - currentPos;
                    float targetMagnitude = targetDelta.magnitude;

                    if (!targetMagnitude.Approximately(0f))
                    {
                        float angle = Vector2.SignedAngle(Vector2.up, targetDelta.normalized);
                        angle *= tiltSettings.MaxDeltaFactor <= 0f ? 1f : (targetMagnitude / tiltSettings.MaxDeltaFactor);
                        float maxAngle = Mathf.Min(Mathf.Abs(angle), tiltSettings.AngleMax) * Mathf.Sign(angle);
                        newTarget *= Quaternion.AngleAxis(maxAngle, rotationAxis);
                    }

                    return newTarget;
                }
            }
        }
    }
}
