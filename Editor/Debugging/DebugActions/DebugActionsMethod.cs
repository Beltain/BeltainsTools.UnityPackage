using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor.Debugging.DebugActions
{
    public class DebugActionsMethod
    {
        private const float k_TitlePadding = 50f;

        private readonly DebugActionAttribute m_Attribute;
        private readonly MethodInfo m_MethodInfo;
        private readonly MethodInfo m_ValidatorMethodInfo;
        private readonly DebugActionParam[] m_Parameters;

        private GUIStyle m_MethodStyle;
        private GUIStyle m_MethodFontStyle;
        private GUIStyle m_ParamStyle;

        public DebugActionsMethod(MethodInfo methodInfo, DebugActionAttribute attribute, MethodInfo validatorMethodInfo)
        {
            m_MethodInfo = methodInfo;
            m_Attribute = attribute;
            m_ValidatorMethodInfo = validatorMethodInfo;
            ParameterInfo[] parameterInfos = m_MethodInfo.GetParameters();
            m_Parameters = new DebugActionParam[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
                m_Parameters[i] = DebugActionParam.CreateForParameter(parameterInfos[i]);
        }

        private void InitialiseStyles()
        {
            if (m_MethodStyle == null)
            {
                GUIStyle methodStyle = new GUIStyle(EditorStyles.helpBox);
                methodStyle.alignment = TextAnchor.MiddleLeft;
                methodStyle.margin = new RectOffset(0, 0, 0, 0);
                methodStyle.border = new RectOffset(0, 0, 0, 0);
                methodStyle.padding = new RectOffset(2, 2, 2, 2);
                m_MethodStyle = methodStyle;
            }

            if (m_MethodFontStyle == null)
            {
                GUIStyle methodFontStyle = new GUIStyle(EditorStyles.boldLabel);
                methodFontStyle.alignment = TextAnchor.MiddleCenter;
                m_MethodFontStyle = methodFontStyle;
            }

            if (m_ParamStyle == null)
            {
                GUIStyle paramStyle = new GUIStyle(EditorStyles.helpBox);
                paramStyle.margin = new RectOffset(2, 2, 0, 0);
                paramStyle.border = new RectOffset(0, 0, 0, 0);
                paramStyle.padding = new RectOffset(2, 2, 0, 0);
                m_ParamStyle = paramStyle;
            }
        }

        public void OnGUI()
        {
            InitialiseStyles();
            bool cachedWideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;

            Rect horizontalRect = EditorGUILayout.BeginHorizontal(m_MethodStyle, GUILayout.ExpandWidth(false));

            EditorGUI.BeginDisabledGroup(!Validate());
            if (GUILayout.Button("Execute", GUILayout.Width(100)))
                Execute();
            EditorGUI.EndDisabledGroup();

            GUIContent methodTitleContent = new GUIContent(m_Attribute.GetName(m_MethodInfo) + "()");
            float methodTitleWidth = m_MethodStyle.CalcSize(methodTitleContent).x * 1.25f + k_TitlePadding;
            EditorGUILayout.LabelField(methodTitleContent, m_MethodFontStyle, GUILayout.Width(methodTitleWidth), GUILayout.ExpandWidth(false));

            foreach (DebugActionParam param in m_Parameters)
            {
                EditorGUILayout.BeginHorizontal(m_ParamStyle);
                param.OnGUI(m_ParamStyle);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.wideMode = cachedWideMode;
        }

        private bool Validate()
        {
            return m_ValidatorMethodInfo == null || (bool)m_ValidatorMethodInfo.Invoke(null, null);
        }

        public void Execute()
        {
            m_MethodInfo.InvokeOnAllObjectsOrStatic(m_Parameters.Select(r => r.GetValue()).ToArray());
        }
    }
}
