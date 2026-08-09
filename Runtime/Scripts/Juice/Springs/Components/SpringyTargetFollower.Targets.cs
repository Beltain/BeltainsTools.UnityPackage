using System;
using UnityEngine;

namespace BeltainsTools.Juice
{
    public partial class SpringyTargetFollower
    {
        public static class TargetStrategies
        {
            [System.Serializable]
            public abstract class TargetStrategyBase
            {
                public virtual Vector3 GetWorldPosition() => Vector3.zero;
                public virtual Quaternion GetWorldRotation() => Quaternion.identity;
                public virtual Vector3 GetLocalScale() => Vector3.one;
            }

            [System.Serializable]
            public class TargetTransform : TargetStrategyBase
            {
                [SerializeField] protected Transform m_Transform;

                public TargetTransform() : this(null) { }
                public TargetTransform(Transform transform)
                {
                    m_Transform = transform;
                }

                public override Vector3 GetWorldPosition() => m_Transform.position;
                public override Quaternion GetWorldRotation() => m_Transform.rotation;
                public override Vector3 GetLocalScale() => m_Transform.localScale;
            }

            [System.Serializable]
            public class TargetTransformWithLocalOffset : TargetTransform
            {
                [SerializeField] private Vector3 m_PositionOffset;
                [SerializeField] private Quaternion m_RotationOffset = Quaternion.identity;
                [SerializeField] private Vector3 m_ScaleMultiplier = Vector3.one;

                public TargetTransformWithLocalOffset() : this(null, Vector3.zero, Quaternion.identity, Vector3.one) { }
                public TargetTransformWithLocalOffset(Transform transform, Vector3 positionOffset, Quaternion rotationOffset, Vector3 scaleMultiplier) : base(transform)
                {
                    m_PositionOffset = positionOffset;
                    m_RotationOffset = rotationOffset;
                    m_ScaleMultiplier = scaleMultiplier;
                }


                public override Vector3 GetWorldPosition() => m_Transform.TransformPoint(m_PositionOffset);
                public override Quaternion GetWorldRotation() => m_Transform.rotation * m_RotationOffset;
                public override Vector3 GetLocalScale() => Vector3.Scale(m_Transform.localScale, m_ScaleMultiplier);
            }

            [System.Serializable]
            public class TargetWorldPoint : TargetStrategyBase
            {
                [SerializeField] private Vector3 m_Position;
                [SerializeField] private Quaternion m_Rotation;
                [SerializeField] private Vector3 m_Scale = Vector3.one;

                public TargetWorldPoint() : this(Vector3.zero, Quaternion.identity, Vector3.one) { }
                public TargetWorldPoint(Vector3 position, Quaternion rotation, Vector3 scale)
                {
                    m_Position = position;
                    m_Rotation = rotation;
                    m_Scale = scale;
                }

                public override Vector3 GetWorldPosition() => m_Position;
                public override Quaternion GetWorldRotation() => m_Rotation;
                public override Vector3 GetLocalScale() => m_Scale;
            }

            public class TargetFuncResolver : TargetStrategyBase
            {
                private readonly Func<Vector3> m_PositionFunc;
                private readonly Func<Quaternion> m_RotationFunc;
                private readonly Func<Vector3> m_ScaleFunc;

                public TargetFuncResolver() : this(null, null, null) { }
                public TargetFuncResolver(
                    Func<Vector3> positionFunc = null, 
                    Func<Quaternion> rotationFunc = null, 
                    Func<Vector3> scaleFunc = null)
                {
                    m_PositionFunc = positionFunc;
                    m_RotationFunc = rotationFunc;
                    m_ScaleFunc = scaleFunc;
                }

                public override Vector3 GetWorldPosition() => m_PositionFunc != null ? m_PositionFunc() : base.GetWorldPosition();
                public override Quaternion GetWorldRotation() => m_RotationFunc != null ? m_RotationFunc() : base.GetWorldRotation();
                public override Vector3 GetLocalScale() => m_ScaleFunc != null ? m_ScaleFunc() : base.GetLocalScale();
            }
        }
    }
}
