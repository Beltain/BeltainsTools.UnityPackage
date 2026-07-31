using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace BeltainsTools.Utilities
{
    public static class ReflectionUtilities
    {
        /// <summary>Invokes the provided method on all UnityEngine.Object instances of the method's declaring type. If the method is static, it will be invoked without any instances.</summary>
        public static object[] InvokeOnAllObjectsOrStatic(System.Reflection.MethodInfo methodInfo, object[] parameters = null)
        {
            if (methodInfo.IsStatic)
            {
                // Static methods don't need instances
                return new object[] { methodInfo.Invoke(null, parameters) };
            }

            System.Type declaringType = methodInfo.DeclaringType;

            // For MonoBehaviour-derived types, find all active instances
            List<object> results = new List<object>();
            if (typeof(UnityEngine.Object).IsAssignableFrom(declaringType))
            {
                UnityEngine.Object[] instances = UnityEngine.Object.FindObjectsByType(declaringType);
                foreach (UnityEngine.Object instance in instances)
                    results.Add(methodInfo.Invoke(instance, parameters));
            }
            return results.ToArray();
        }

        public static IEnumerable<Type> GetAllDescendantsOf(Assembly assembly, Type genericTypeDefinition)
        {
            IEnumerable<Type> GetAllAscendants(Type t)
            {
                Type current = t;

                while (current.BaseType != typeof(object))
                {
                    yield return current.BaseType;
                    current = current.BaseType;
                }
            }

            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (genericTypeDefinition == null)
                throw new ArgumentNullException(nameof(genericTypeDefinition));

            if (!genericTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException(
                    "Specified type is not a valid generic type definition.",
                    nameof(genericTypeDefinition));

            return assembly.GetTypes()
                           .Where(t => GetAllAscendants(t).Any(d =>
                               d.IsGenericType &&
                               d.GetGenericTypeDefinition()
                                .Equals(genericTypeDefinition)));
        }
    }
}