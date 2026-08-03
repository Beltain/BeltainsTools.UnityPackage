using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor.Debugging.DebugActions
{
    public class DebugActionsMethod
    {
        private const float k_MethodGroupHeight = 22f;

        private readonly DebugActionAttribute m_Attribute;
        private readonly MethodInfo m_MethodInfo;
        private readonly MethodInfo m_ValidatorMethodInfo;
        private readonly DebugActionParam[] m_Parameters;

        private bool m_StylesInitialised;
        private GUIStyle m_MethodGroupStyle;
        private GUIStyle m_MethodButtonStyle;
        private GUIStyle m_MethodSubtextStyle;
        private GUIStyle m_ParamGroupStyle;

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
            if (m_StylesInitialised) 
                return;
            m_StylesInitialised = true;

            GUIStyle methodGroupStyle = new GUIStyle(EditorStyles.helpBox);
            methodGroupStyle.alignment = TextAnchor.MiddleLeft;
            methodGroupStyle.margin = new RectOffset(0, 0, 0, 0);
            methodGroupStyle.border = new RectOffset(0, 0, 0, 0);
            methodGroupStyle.padding = new RectOffset(2, 2, 2, 2);
            m_MethodGroupStyle = methodGroupStyle;

            GUIStyle methodButtonStyle = new GUIStyle(EditorStyles.miniButton);
            methodButtonStyle.alignment = TextAnchor.MiddleCenter;
            methodButtonStyle.font = EditorStyles.boldLabel.font;
            methodButtonStyle.padding = new RectOffset(12, 12, 0, 0);
            methodButtonStyle.margin = new RectOffset(0, 20, 0, 0);
            methodButtonStyle.fixedHeight = k_MethodGroupHeight;
            methodButtonStyle.stretchWidth = false;
            m_MethodButtonStyle = methodButtonStyle;

            GUIStyle methodSubtextStyle = new GUIStyle(EditorStyles.miniLabel);
            methodSubtextStyle.alignment = TextAnchor.MiddleCenter;
            methodSubtextStyle.fontStyle = FontStyle.Italic;
            methodSubtextStyle.normal.textColor = EditorGUIUtility.isProSkin 
                ? new Color(0.5f, 0.5f, 0.5f) 
                : new Color(0.4f, 0.4f, 0.4f);
            methodSubtextStyle.padding = new RectOffset(12, 12, 0, 0);
            methodSubtextStyle.margin = new RectOffset(20, 0, 0, 0);
            methodSubtextStyle.fixedHeight = k_MethodGroupHeight;
            methodSubtextStyle.stretchWidth = false;
            m_MethodSubtextStyle = methodSubtextStyle;

            GUIStyle paramStyle = new GUIStyle(EditorStyles.helpBox);
            paramStyle.margin = new RectOffset(4, 4, 0, 0);
            paramStyle.border = new RectOffset(0, 0, 0, 0);
            paramStyle.padding = new RectOffset(2, 2, 0, 0);
            m_ParamGroupStyle = paramStyle;
        }

        [DebugAction]
        public static void Test(float egg, int legg)
        {

        }

        public void OnGUI()
        {
            InitialiseStyles();
            bool cachedWideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;

            Rect horizontalRect = EditorGUILayout.BeginHorizontal(m_MethodGroupStyle, GUILayout.Height(k_MethodGroupHeight), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));

            EditorGUI.BeginDisabledGroup(!Validate());
            GUIContent methodButtonContent = new GUIContent(m_Attribute.GetName(m_MethodInfo));
            float methodButtonWidth = m_MethodButtonStyle.CalcSize(methodButtonContent).x;
            if (GUILayout.Button(methodButtonContent, m_MethodButtonStyle, GUILayout.Width(methodButtonWidth)))
                Execute();
            EditorGUI.EndDisabledGroup();

            foreach (DebugActionParam param in m_Parameters)
            {
                EditorGUILayout.BeginHorizontal(m_ParamGroupStyle, GUILayout.Width(param.GetGUIWidth()), GUILayout.ExpandWidth(false));
                param.OnGUI();
                EditorGUILayout.EndHorizontal();
            }

            GUIContent methodSubtextContent = new GUIContent($"{m_MethodInfo.DeclaringType}.{m_MethodInfo.Name}({(m_Parameters.Length > 0 ? "..." : "")})");
            float methodSubtextWidth = m_MethodSubtextStyle.CalcSize(methodSubtextContent).x;
            EditorGUILayout.LabelField(methodSubtextContent, m_MethodSubtextStyle, GUILayout.Width(methodSubtextWidth));

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
