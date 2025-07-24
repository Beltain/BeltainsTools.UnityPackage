using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools
{
    [CreateAssetMenu(fileName = "new Project Texture2D Array Order Data", menuName = BTInternal.PackageData.k_PrettyName + "/Texture2DArray/ProjectOrderData")]
    public class ProjectTexture2DArrayOrderData : ScriptableObject
    {
        [SerializeField] public Texture2DArrayOrderData[] AllData = new Texture2DArrayOrderData[0];


        /// <summary>Get the index a texture exists at in its <see cref="Texture2DArray"/></summary>
        /// <returns>Index of element in its array or -1 if it's not found in the project data</returns>
        public int GetIndexFor(Texture2D texture)
        {
            int resultIndex = -1;
            for (int i = 0; i < AllData.Length; i++)
            {
                resultIndex = AllData[i].GetTextureIndex(texture);
                if (resultIndex != -1)
                    return resultIndex;
            }
            return resultIndex;
        }
    }
}
