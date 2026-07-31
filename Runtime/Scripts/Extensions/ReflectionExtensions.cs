using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class ReflectionExtensions
    {
        /// <inheritdoc cref="ReflectionUtilities.GetAllDescendantsOf(Assembly, Type)"/>
        public static IEnumerable<Type> GetAllDescendantsOf(this Assembly assembly, Type genericTypeDefinition)
            => ReflectionUtilities.GetAllDescendantsOf(assembly, genericTypeDefinition);

        /// <inheritdoc cref="ReflectionUtilities.InvokeOnAllObjectsOrStatic(MethodInfo, object[])"/>
        public static object[] InvokeOnAllObjectsOrStatic(this MethodInfo methodInfo, object[] parameters = null)
            => ReflectionUtilities.InvokeOnAllObjectsOrStatic(methodInfo, parameters);
    }
}
