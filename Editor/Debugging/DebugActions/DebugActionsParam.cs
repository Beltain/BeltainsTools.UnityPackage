using UnityEditor;
using UnityEngine;

namespace BeltainsTools.Editor.Debugging.DebugActions
{
    public abstract class DebugActionParam
    {
        public string Title { get; private set; }

        private GUILayoutOption[] m_ControlLayoutOptions;

        protected DebugActionParam(string title)
        {
            Title = title;
        }

        protected GUILayoutOption[] GetControlLayoutOptions()
        {
            if (m_ControlLayoutOptions == null)
            {
                m_ControlLayoutOptions = new GUILayoutOption[]
                {
                    GUILayout.Width(GetControlWidth()),
                    GUILayout.ExpandWidth(false)
                };
            }

            return m_ControlLayoutOptions;
        }

        public abstract object GetValue();

        public float GetGUIWidth() => GetLabelWidth() + GetControlWidth();
        private float GetLabelWidth() => EditorStyles.label.CalcSize(new GUIContent(Title)).x;
        protected abstract float GetControlWidth();

        public virtual void OnGUI()
        {
            EditorGUILayout.LabelField(Title, GUILayout.Width(GetLabelWidth()), GUILayout.ExpandWidth(false));
        }

        public static bool ValidateMethod(System.Reflection.MethodInfo methodInfo, DebugActionAttribute attribute)
        {
            if (!methodInfo.IsStatic && !typeof(UnityEngine.Object).IsAssignableFrom(methodInfo.DeclaringType))
                throw new System.Exception($"ERROR: DebugActionAttribute assigned on incorrectly configured method '{methodInfo.Name}'! Method must be 'static' or belong to a UnityEngine.Object-derived type!");

            return attribute.IsValidatorMethod ? ValidateValidatorMethod(methodInfo) : ValidateActionMathod(methodInfo);
        }

        private static bool ValidateActionMathod(System.Reflection.MethodInfo methodInfo)
        {
            foreach (System.Reflection.ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                if (!GetIsParameterSupported(parameterInfo))
                {
                    d.LogErrorFormat("Parameter {0} of type {1} is not supported for DebugAction methods.", parameterInfo.Name, parameterInfo.ParameterType);
                    return false;
                }
            }
            return true;
        }

        private static bool ValidateValidatorMethod(System.Reflection.MethodInfo methodInfo)
        {
            System.Reflection.ParameterInfo[] parameters = methodInfo.GetParameters();
            if (methodInfo.GetParameters().Length != 0)
            {
                d.LogErrorFormat("Debug Action Validator method {0} must not have any parameters.", methodInfo.Name);
                return false;
            }

            if (methodInfo.ReturnType != typeof(bool))
            {
                d.LogErrorFormat("Debug Action Validator method {0} must return a boolean value.", methodInfo.Name);
                return false;
            }
            return true;
        }

        public static bool GetIsParameterSupported(System.Reflection.ParameterInfo parameterInfo)
        {
            return GetSupportedDebugActionTypeForParameter(parameterInfo) != null;
        }

        public static DebugActionParam CreateForParameter(System.Reflection.ParameterInfo parameterInfo)
        {
            System.Type debugActionParamType = GetSupportedDebugActionTypeForParameter(parameterInfo);
            d.AssertFormat(debugActionParamType != null, "No supported DebugActionParam type found for parameter {0} of type {1}", parameterInfo.Name, parameterInfo.ParameterType);

            if (parameterInfo.ParameterType.IsEnum)
            {
                return parameterInfo.HasDefaultValue
                    ? new DebugActionParam_Enum(parameterInfo.Name, parameterInfo.DefaultValue)
                    : new DebugActionParam_Enum(parameterInfo.Name, parameterInfo.ParameterType);
            }

            return parameterInfo.HasDefaultValue ? 
                (DebugActionParam)System.Activator.CreateInstance(debugActionParamType, parameterInfo.Name, parameterInfo.DefaultValue) :
                (DebugActionParam)System.Activator.CreateInstance(debugActionParamType, parameterInfo.Name);
        }

        private static System.Type GetSupportedDebugActionTypeForParameter(System.Reflection.ParameterInfo parameterInfo)
        {
            System.Type paramType = parameterInfo.ParameterType;

            if (paramType.IsEnum)
                return typeof(DebugActionParam_Enum);

            foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (System.Type type in assembly.GetTypes())
                    {
                        if (!type.IsAbstract &&
                            typeof(DebugActionParam).IsAssignableFrom(type) &&
                            SupportsParameterType(type, paramType))
                        {
                            return type;
                        }
                    }
                }
                catch (System.Reflection.ReflectionTypeLoadException) { continue; }
            }

            return null;
        }

        private static bool SupportsParameterType(System.Type debugActionParamType, System.Type paramType)
        {
            System.Type baseType = debugActionParamType.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType &&
                    baseType.GetGenericTypeDefinition() == typeof(DebugActionParam<>))
                {
                    System.Type genericArg = baseType.GetGenericArguments()[0];
                    return genericArg.IsAssignableFrom(paramType);
                }
                baseType = baseType.BaseType;
            }
            return false;
        }
    }

    public abstract class DebugActionParam<T> : DebugActionParam
    {
        public T Value { get; protected set; }
        protected DebugActionParam(string title) : base(title) { Value = default(T); }
        protected DebugActionParam(string title, object value) : base(title) { Value = (T)value; }
        public override object GetValue() => Value;
    }

    public class DebugActionParam_Bool : DebugActionParam<bool>
    {
        public DebugActionParam_Bool(string title) : base(title) { }
        public DebugActionParam_Bool(string title, object value) : base(title, value) { }

        protected override float GetControlWidth() => 15;
        public override void OnGUI()
        {
            base.OnGUI();
            Value = UnityEditor.EditorGUILayout.Toggle(string.Empty, Value, GetControlLayoutOptions());
        }

    }

    public class DebugActionParam_Int : DebugActionParam<int>
    {
        public DebugActionParam_Int(string title) : base(title) { }
        public DebugActionParam_Int(string title, object value) : base(title, value) { }

        protected override float GetControlWidth() => 50;
        public override void OnGUI()
        {
            base.OnGUI();
            Value = EditorGUILayout.IntField(string.Empty, Value, GetControlLayoutOptions());
        }
    }

    public class DebugActionParam_Float : DebugActionParam<float>
    {
        public DebugActionParam_Float(string title) : base(title) { }
        public DebugActionParam_Float(string title, object value) : base(title, value) { }

        protected override float GetControlWidth() => 50;
        public override void OnGUI()
        {
            base.OnGUI();
            Value = EditorGUILayout.FloatField(string.Empty, Value, GetControlLayoutOptions());
        }
    }

    public class DebugActionParam_String : DebugActionParam<string>
    {
        public DebugActionParam_String(string title) : base(title) { }
        public DebugActionParam_String(string title, object value) : base(title, value) { }

        protected override float GetControlWidth() => 150;
        public override void OnGUI()
        {
            base.OnGUI();
            Value = UnityEditor.EditorGUILayout.TextField(string.Empty, Value, GetControlLayoutOptions());
        }
    }

    public class DebugActionParam_Vector2 : DebugActionParam<Vector2>
    {
        public DebugActionParam_Vector2(string title) : base(title) { }
        public DebugActionParam_Vector2(string title, object value) : base(title, value) { }

        protected override float GetControlWidth() => 100;
        public override void OnGUI()
        {
            base.OnGUI();
            Value = EditorGUILayout.Vector2Field(string.Empty, Value, GetControlLayoutOptions());
        }
    }

    public class DebugActionParam_Vector3 : DebugActionParam<Vector3>
    {
        public DebugActionParam_Vector3(string title) : base(title) { }
        public DebugActionParam_Vector3(string title, object value) : base(title, value) { }

        protected override float GetControlWidth() => 150;
        public override void OnGUI()
        {
            base.OnGUI();
            Value = EditorGUILayout.Vector3Field(string.Empty, Value, GetControlLayoutOptions());
        }
    }

    public class DebugActionParam_Enum : DebugActionParam<System.Enum>
    {
        private readonly System.Type m_EnumType;

        public DebugActionParam_Enum(string title, System.Type enumType) : base(title)
        {
            m_EnumType = enumType;
            Value = (System.Enum)System.Enum.GetValues(enumType).GetValue(0);
        }

        public DebugActionParam_Enum(string title, object value) : base(title, value)
        {
            m_EnumType = value.GetType();
        }

        protected override float GetControlWidth() => 120;
        public override void OnGUI()
        {
            base.OnGUI();
            Value = EditorGUILayout.EnumPopup(string.Empty, Value, GetControlLayoutOptions());
        }
    }
}