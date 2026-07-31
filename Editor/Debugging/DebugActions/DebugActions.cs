using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BeltainsTools.Editor.Debugging.DebugActions
{
    public static class DebugActions
    {
        private struct ProtoDebugActionMethod
        {
            public MethodInfo Method;
            public DebugActionAttribute Attribute;
            public ProtoDebugActionMethod(MethodInfo methodInfo, DebugActionAttribute attribute)
            {
                Method = methodInfo;
                Attribute = attribute;
            }
        }

        public static List<DebugActionsMethod> DiscoverDebugActionMethods()
        {
            List<ProtoDebugActionMethod> actionMethods = new List<ProtoDebugActionMethod>();
            List<ProtoDebugActionMethod> validatorMethods = new List<ProtoDebugActionMethod>();
            foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (System.Type type in assembly.GetTypes())
                {
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        DebugActionAttribute attribute = method.GetCustomAttribute<DebugActionAttribute>();
                        if (attribute != null && DebugActionParam.ValidateMethod(method, attribute))
                        {
                            if (attribute.IsValidatorMethod)
                                validatorMethods.Add(new ProtoDebugActionMethod(method, attribute));
                            else
                                actionMethods.Add(new ProtoDebugActionMethod(method, attribute));
                        }
                    }
                }
            }

            List<DebugActionsMethod> actionMethodsList = new List<DebugActionsMethod>();
            foreach (ProtoDebugActionMethod actionMethod in actionMethods)
            {
                MethodInfo validatorMethod = validatorMethods.Find(m => string.Compare(m.Attribute.GetName(m.Method), actionMethod.Attribute.GetName(actionMethod.Method)) == 0).Method;
                actionMethodsList.Add(new DebugActionsMethod(actionMethod.Method, actionMethod.Attribute, validatorMethod));
            }

            return actionMethodsList;
        }
    }
}
