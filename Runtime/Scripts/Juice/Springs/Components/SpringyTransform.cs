using UnityEngine;

namespace BeltainsTools.Juice
{
    /// <summary>A juice component that allows for fluid, springy control of a transform's position, rotation, and local scale. Each of these can be controlled independently</summary>
    public class SpringyTransform : MonoBehaviour
    {
        [SerializeField]
        private TransformControlTypes m_ControlTypes = 0;
        [SerializeField]
        private DampedSpring.Vector3 m_PositionSpring = new DampedSpring.Vector3(100f, 1f);
        [SerializeField]
        private DampedSpring.Quaternion m_RotationSpring = new DampedSpring.Quaternion(100f, 1f, 0f);
        [SerializeField]
        private DampedSpring.Vector3 m_LocalScaleSpring = new DampedSpring.Vector3(100f, 1f);
        [SerializeField]
        private float m_LocalScaleMinValue = 0.00001f;

        private bool ControlPosition => (m_ControlTypes & TransformControlTypes.Position) != 0;
        private bool ControlRotation => (m_ControlTypes & TransformControlTypes.Rotation) != 0;
        private bool ControlLocalScale => (m_ControlTypes & TransformControlTypes.LocalScale) != 0;

        /// <summary>The target position of the position spring.</summary>
        public Vector3 position
        {
            get => m_PositionSpring.Target;
            set => SetPosition(value, false);
        }

        /// <summary>The target rotation of the rotation spring.</summary>
        public Quaternion rotation
        {
            get => m_RotationSpring.Target;
            set => SetRotation(value, false);
        }

        /// <summary>The target local scale of the scale spring.</summary>
        public Vector3 localScale
        {
            get => m_LocalScaleSpring.Target;
            set => SetLocalScale(value, false);
        }


        [System.Flags]
        public enum TransformControlTypes : byte
        {
            Position = 1 << 0,
            Rotation = 1 << 1,
            LocalScale = 1 << 2
        }


        public void SetPosition(Vector3 targetPos, bool instantly)
        {
            m_PositionSpring.SetTarget(targetPos);
            if (instantly)
                m_PositionSpring.SetCurrent(targetPos);
        }

        public void SetRotation(Quaternion targetRot, bool instantly)
        {
            m_RotationSpring.SetTarget(targetRot);
            if (instantly)
                m_RotationSpring.SetCurrent(targetRot);
        }

        public void SetLocalScale(Vector3 targetScale, bool instantly)
        {
            m_LocalScaleSpring.SetTarget(targetScale);
            if (instantly)
                m_LocalScaleSpring.SetCurrent(targetScale);
        }


        public void SetControlsActive(TransformControlTypes controls, bool isActive)
        {
            DeinitialiseControlSprings();

            TransformControlTypes prevControls = m_ControlTypes;

            if (isActive)
                m_ControlTypes |= controls;
            else
                m_ControlTypes &= ~controls;

            // set initial control values for all controls we have just enabled
            if (ControlPosition && (prevControls & TransformControlTypes.Position) == 0)
                SetPosition(transform.position, true); // set initial control value since we may have just enabled it
            if (ControlRotation && (prevControls & TransformControlTypes.Rotation) == 0)
                SetRotation(transform.rotation, true); // set initial control value since we may have just enabled it
            if (ControlLocalScale && (prevControls & TransformControlTypes.LocalScale) == 0)
                SetLocalScale(transform.localScale, true); // set initial control value since we may have just enabled it

            InitialiseControlSprings();
        }


        private void InitialiseControlSprings()
        {
            if (ControlPosition)
                m_PositionSpring.Init(m_PositionSpring.Target, m_PositionSpring.Current, OnPositionSpringUpdate);
            if (ControlRotation)
                m_RotationSpring.Init(m_RotationSpring.Target, m_RotationSpring.Current, OnRotationSpringUpdate);
            if (ControlLocalScale)
                m_LocalScaleSpring.Init(m_LocalScaleSpring.Target, m_LocalScaleSpring.Current, OnLocalScaleSpringUpdate);
        }

        private void DeinitialiseControlSprings()
        {
            m_PositionSpring.DeInit();
            m_RotationSpring.DeInit();
            m_LocalScaleSpring.DeInit();
        }


        private void OnPositionSpringUpdate(Vector3 newPos)
        {
            transform.position = newPos;
        }

        private void OnRotationSpringUpdate(Quaternion newRot)
        {
            transform.rotation = newRot;
        }

        private void OnLocalScaleSpringUpdate(Vector3 newScale)
        {
            newScale.x = Mathf.Max(newScale.x, m_LocalScaleMinValue);
            newScale.y = Mathf.Max(newScale.y, m_LocalScaleMinValue);
            newScale.z = Mathf.Max(newScale.z, m_LocalScaleMinValue);
            transform.localScale = newScale;
        }


        private void Awake()
        {
            // fully initialise controls
            TransformControlTypes defaultControlTypes = m_ControlTypes;
            SetControlsActive(~(TransformControlTypes)0, false);
            SetControlsActive(defaultControlTypes, true);
        }

        private void Update()
        {
            if (ControlPosition)
                m_PositionSpring.Update();
            if (ControlRotation)
                m_RotationSpring.Update();
            if (ControlLocalScale)
                m_LocalScaleSpring.Update();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SpringyTransform))]
    public class SpringyTransformEditor : UnityEditor.Editor
    {
        UnityEditor.SerializedProperty prop_Control = null;
        UnityEditor.SerializedProperty prop_PositionSpring = null;
        UnityEditor.SerializedProperty prop_RotationSpring = null;
        UnityEditor.SerializedProperty prop_ScaleSpring = null;
        UnityEditor.SerializedProperty prop_LocalScaleMinValue = null;

        public override void OnInspectorGUI()
        {
            SpringyTransform springyTransform = (SpringyTransform)target;

            serializedObject.Update();

            SpringyTransform.TransformControlTypes controlTypes = (SpringyTransform.TransformControlTypes)prop_Control.enumValueFlag;

            UnityEditor.EditorGUILayout.PropertyField(prop_Control);

            serializedObject.ApplyModifiedProperties();

            if ((controlTypes & SpringyTransform.TransformControlTypes.Position) != 0)
                UnityEditor.EditorGUILayout.PropertyField(prop_PositionSpring);
            if ((controlTypes & SpringyTransform.TransformControlTypes.Rotation) != 0)
                UnityEditor.EditorGUILayout.PropertyField(prop_RotationSpring);
            if ((controlTypes & SpringyTransform.TransformControlTypes.LocalScale) != 0)
            {
                UnityEditor.EditorGUILayout.PropertyField(prop_ScaleSpring);
                UnityEditor.EditorGUILayout.PropertyField(prop_LocalScaleMinValue);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            prop_Control = serializedObject.FindProperty("m_ControlTypes");
            prop_PositionSpring = serializedObject.FindProperty("m_PositionSpring");
            prop_RotationSpring = serializedObject.FindProperty("m_RotationSpring");
            prop_ScaleSpring = serializedObject.FindProperty("m_LocalScaleSpring");
            prop_LocalScaleMinValue = serializedObject.FindProperty("m_LocalScaleMinValue");
        }
    }
#endif
}
