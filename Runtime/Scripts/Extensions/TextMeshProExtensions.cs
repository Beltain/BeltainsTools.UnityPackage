using UnityEngine;
using BeltainsTools.Utilities;

namespace BeltainsTools
{
    public static class TextMeshProExtensions
    {
        /// <inheritdoc cref="TextMeshProUtilities.GetCharacterPosition(TMPro.TMP_InputField, int)"/>
        public static Vector3 GetCharacterPosition(this TMPro.TMP_InputField inputField, int index)
            => TextMeshProUtilities.GetCharacterPosition(inputField, index);
        /// <inheritdoc cref="TextMeshProUtilities.GetCharacterPosition(TMPro.TMP_Text, int)"/>
        public static Vector3 GetCharacterPosition(this TMPro.TMP_Text textElement, int index)
            => TextMeshProUtilities.GetCharacterPosition(textElement, index);
    }
}
