using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using BeltainsTools.Utilities;
using BeltainsTools;
using BeltainsTools.Debugging;

[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
public class DebugCommandAttribute : System.Attribute 
{
    public string m_Name;
    public string m_Description;
    public DebugCommands.AccessLevelTypes m_AccessLevel;


    static HashSet<System.Type> s_SupportedTypes = new HashSet<System.Type> //Arbitrary list but can be expanded as needed
    {
        typeof(bool),
        typeof(byte),
        typeof(short),
        typeof(int),
        typeof(float),
        typeof(long),
        typeof(double),
        typeof(string),
    };

    static readonly Dictionary<System.Type, string[]> s_ParameterAutofillSuggestions = new Dictionary<System.Type, string[]>
    {
        { typeof(bool), new string[] { "true", "false" } },
    };



    public bool GetHasAccess()
    {
        return m_AccessLevel >= DebugCommands.s_CurrentAccessLevel;
    }

    public static bool TryParseStringToParameter(string paramString, ParameterInfo paramInfo, out object parsedParam)
    {
        System.Type paramType = paramInfo.ParameterType;
        if (paramType.IsEnum)
        {
            string[] enumNames = System.Enum.GetNames(paramType);
            System.Array enumValues = System.Enum.GetValues(paramType);
            for (long i = 0; i < enumNames.Length; i++)
            {
                if (string.Compare(enumNames[i], paramString, true) != 0)
                    continue;

                //The input paramString already matches the name of one of the values of the param Enum Type, so just return it's corresponding value
                parsedParam = enumValues.GetValue(i);
                return true;
            }

            //We haven't resolved the value yet so just return the base type and we'll try parse that later
            paramType = System.Enum.GetUnderlyingType(paramType);
        }

        return StringUtilities.TryParse(paramString, paramType, out parsedParam);
    }

    public static string[] GetAutofillSuggestionsFor(System.Type type)
    {
        if (type.IsEnum)
            return System.Enum.GetNames(type);

        if (s_ParameterAutofillSuggestions.ContainsKey(type))
            return s_ParameterAutofillSuggestions[type];

        return new string[0];
    }




    public DebugCommandAttribute(string name = "", string description = "", DebugCommands.AccessLevelTypes accessLevel = DebugCommands.AccessLevelTypes.Dev)
    {
        m_Name = name;
        m_Description = description;
        m_AccessLevel = accessLevel;
    }


    public void Validate(MethodInfo method)
    {
        if (!m_Name.IsEmpty() && m_Name.Contains(' '))
            throw new System.Exception($"ERROR: DebugCommandAttribute name contains a space! This is not allowed!");

        if (!method.IsPublic || (method.ReturnType != typeof(void) && method.ReturnType != typeof(string)))
            throw new System.Exception($"ERROR: DebugCommandAttribute assigned on incorrectly configured method '{method.Name}'! Method must be 'public void or string'!");

        if (!method.IsStatic && !typeof(UnityEngine.Object).IsAssignableFrom(method.DeclaringType))
            throw new System.Exception($"ERROR: DebugCommandAttribute assigned on incorrectly configured method '{method.Name}'! Method must be 'static' or belong to a UnityEngine.Object-derived type!");

        foreach (ParameterInfo param in method.GetParameters())
        {
            if (param.IsIn || param.IsOut || param.ParameterType.IsByRef)
                throw new System.Exception($"ERROR: DebugCommandAttribute assigned on {method.Name} with incorrectly configured parameters! Params cannot be 'in', 'out', or 'ref'!");

            System.Type paramType = param.ParameterType;
            if (param.ParameterType.IsEnum)
                paramType = System.Enum.GetUnderlyingType(paramType);

            if (!s_SupportedTypes.Contains(paramType))
                throw new System.Exception($"ERROR: DebugCommandAttribute assigned on {method.Name} with unsupported parameter types! Type must be one of the following: [{string.Join(", ", s_SupportedTypes)}]");
        }
    }
}