using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
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

        if (!method.IsPublic || !method.IsStatic || (method.ReturnType != typeof(void) && method.ReturnType != typeof(string)))
            throw new System.Exception($"ERROR: DebugCommandAttribute assigned on incorrectly configured method '{method.Name}'! Method must be 'public static void or string'!");

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

namespace BeltainsTools.Debugging
{
    public static class DebugCommands
    {
        public static Command[] s_Commands { get; private set; } = new Command[0];
        public static AccessLevelTypes s_CurrentAccessLevel = AccessLevelTypes.Unset;

        /// <summary>Lower levels should have higher access (enum integer value lower)</summary>
        public enum AccessLevelTypes : byte
        {
            Dev = 0, //all
            Cheater = 100,
            Player = 200, //least
            Unset = 255,
        }

        public class CommandException : System.ApplicationException 
        {
            public CommandException() : base() {}
            public CommandException(string message) : base(message) {}
        }

        public class Command
        {
            MethodInfo m_Method;
            ParameterInfo[] m_Parameters;

            public string HelpLine { get; private set; }
            public string[] GuideTokens { get; private set; }

            public string Name { get; private set; }
            public string Description { get; private set; }
            public AccessLevelTypes AccessLevel { get; private set; }
            public bool ReturnsOutput { get; private set; }


            public Command(MethodInfo method, DebugCommandAttribute attribute)
            {
                m_Method = method;

                attribute.Validate(method);

                m_Parameters = m_Method.GetParameters();

                Name = attribute.m_Name == string.Empty ?
                    $"{method.DeclaringType.Name}.{method.Name}" :
                    attribute.m_Name;

                Description = attribute.m_Description;
                AccessLevel = attribute.m_AccessLevel;

                ReturnsOutput = method.ReturnType == typeof(string);

                List<string> guideTokensList = new List<string>{ Name };
                for (int i = 0; i < m_Parameters.Length; i++)
                {
                    guideTokensList.Add($"{m_Parameters[i].Name}<{m_Parameters[i].ParameterType.Name}>" + (m_Parameters[i].HasDefaultValue ? $": {m_Parameters[i].DefaultValue}" : ""));
                }

                GuideTokens = guideTokensList.ToArray();
                HelpLine = $"{string.Join("  ", GuideTokens)}  //{(Description == string.Empty ? " - " : Description)}";
            }


            public string Execute(params string[] parameterStrings)
            {
                if (parameterStrings.Length > m_Parameters.Length)
                    throw new CommandException($"Debug command {Name} expected {m_Parameters.Length} params but got too many ({parameterStrings.Length})!");

                object[] parsedParams = new object[m_Parameters.Length];

                for (int i = 0; i < parsedParams.Length; i++)
                {
                    object paramObject;
                    if(i < parameterStrings.Length)
                    {
                        if (!DebugCommandAttribute.TryParseStringToParameter(parameterStrings[i], m_Parameters[i], out paramObject))
                            throw new CommandException($"Debug command {Name} input '{parameterStrings[i]}' not recognised for {m_Parameters[i].Name}");
                    }
                    else if (m_Parameters[i].HasDefaultValue)
                    {
                        paramObject = m_Parameters[i].DefaultValue;
                    }
                    else
                    {
                        throw new CommandException($"Debug command {Name} expected {m_Parameters.Length} params but got too few ({parameterStrings.Length})!");
                    }
                    parsedParams[i] = paramObject;
                }

                return Execute(parsedParams);
            }

            string Execute(params object[] parameters)
            {
                if(!ReturnsOutput)
                {
                    m_Method.Invoke(null, parameters);
                    return string.Empty;
                }
                else
                {
                    return (string)m_Method.Invoke(null, parameters);
                }
            }

            public List<string[]> GetAutoFillSuggestionsForParams()
            {
                List<string[]> result = new List<string[]>();
                for (int i = 0; i < m_Parameters.Length; i++)
                {
                    result.Add(DebugCommandAttribute.GetAutofillSuggestionsFor(m_Parameters[i].ParameterType));
                }
                return result;
            }
        }

        /// <summary>The managing layer for Internal_ExecuteCommandString.</summary>
        /// <returns>Return log on the status of the requested command</returns>
        public static string ExecuteCommandString(string commandString) //
        {
            string output = string.Empty;
            try
            {
                output = Internal_ExecuteCommandString(commandString);
            }
            catch (CommandException e)
            {
                string exceptionMessage = e.Message;
                return $"Command Failed: {(exceptionMessage.IsNullOrEmpty() ? "Unrecognised Error" : exceptionMessage)}";
            }

            return $"Command Executed: {commandString}" + (output == string.Empty ? "" : $"\n{output}");
        }
        static string Internal_ExecuteCommandString(string commandString)
        {
            if (commandString.IsEmpty())
                throw new CommandException("No command given!");

            string[] commandTokens = commandString.Split(' ');
            string commandName = commandTokens[0];
            string[] commandParams = commandTokens.Skip(1).ToArray();

            Command matchingCommand = s_Commands.Where(r => string.Compare(r.Name, commandName, true) == 0).FirstOrDefault();
            if (matchingCommand == null)
                throw new CommandException($"Command {commandName} not recognised!");

            return matchingCommand.Execute(commandParams);
        }


        [DebugCommand("Debug.RebuildCommands", "Gather all commands from the current app assemblies")]
        public static void RebuildDebugCommands()
        {
            if (s_CurrentAccessLevel == AccessLevelTypes.Unset)
                throw new System.Exception("Trying to rebuild debug commands when no access level has been set. Please assign an access level with the DebugCommands.SetAccessLevel method first");

            List<Command> commands = new List<Command>();
            foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (System.Type type in assembly.GetTypes())
                {
                    foreach(MethodInfo method in type.GetMethods())
                    {
                        DebugCommandAttribute debugCommandAttribute = (DebugCommandAttribute)method.GetCustomAttribute(typeof(DebugCommandAttribute), false);
                        if (debugCommandAttribute == null)
                            continue;

                        if (!debugCommandAttribute.GetHasAccess())
                            continue;

                        //method has a debug command attribute, so register it
                        commands.Add(new Command(method, debugCommandAttribute));
                    }
                }
            }

            s_Commands = commands.OrderBy(r => r.Name).ToArray();
        }

        public static void SetAccessLevel(AccessLevelTypes accessLevel)
        {
            s_CurrentAccessLevel = accessLevel;
        }
    }
}