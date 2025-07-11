using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class TextMeshProUtilities
    {
        /// <inheritdoc cref="GetCharacterPosition(TMPro.TMP_Text, int)"/>
        public static Vector3 GetCharacterPosition(TMPro.TMP_InputField inputField, int index) => GetCharacterPosition(inputField.textComponent, index);
        /// <summary>Get the world position of the TMP character at position <paramref name="index"/></summary>
        public static Vector3 GetCharacterPosition(TMPro.TMP_Text textElement, int index)
        {
            TMPro.TMP_TextInfo textInfo = textElement.GetTextInfo(textElement.text);
            return textElement.transform.position + textInfo.characterInfo[index].bottomLeft;
        }
    }
}
