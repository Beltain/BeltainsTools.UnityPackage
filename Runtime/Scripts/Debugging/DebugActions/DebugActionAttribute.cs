using BeltainsTools;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Attribute to mark a method as a DebugAction Method</summary>
[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
public class DebugActionAttribute : System.Attribute
{
    protected string m_Name;
    protected bool m_IsValidatorMethod;

    public bool IsValidatorMethod => m_IsValidatorMethod;

    public DebugActionAttribute(string name = null, bool isValidatorMethod = false)
    {
        m_Name = name;
        m_IsValidatorMethod = isValidatorMethod;
    }

    public string GetName(System.Reflection.MethodInfo method)
    {
        return m_Name.IsNullOrEmpty() ? method.Name : m_Name;
    }
}