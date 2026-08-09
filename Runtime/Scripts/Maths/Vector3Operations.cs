using UnityEngine;

namespace BeltainsTools.Maths
{
    public static class Vector3Operations
    {
        /// <summary>Set the value of the input vector's directional component to be the provided magnitude. (ie. Setting the Vector3.up component to 0 clears y value)</summary>
        public static Vector3 SetComponent(Vector3 inVector, Vector3 componentDirection, float componentMagnitude)
        {
            componentDirection.Normalize();
            Vector3 alongDirection = Vector3.Dot(inVector, componentDirection) * componentDirection;
            Vector3 withoutComponent = inVector - alongDirection;
            return withoutComponent + componentDirection * componentMagnitude;
        }
    }
}
