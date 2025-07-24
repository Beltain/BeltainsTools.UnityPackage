using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools
{
    public class Texture2DArrayOrderData : ScriptableObject
    {
        public Texture2DArray Array;
        public Texture2D[] OrderedElements;

        /// <summary>Get the index a texture exists at in this <see cref="Texture2DArray"/></summary>
        /// <returns>Index of element in its array or -1 if it's not found</returns>
        public int GetTextureIndex(Texture2D texture)
        {
            for (int i = 0; i < OrderedElements.Length; i++)
            {
                if (texture == OrderedElements[i])
                    return i;
            }
            return -1;
        }
    }
}