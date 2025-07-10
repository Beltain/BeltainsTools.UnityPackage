using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class StringUtilities
    {
        public static bool TryParse(string input, System.Type type, out object result)
        {
            System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(type);

            if (converter.IsValid(input))
            {
                result = converter.ConvertFromString(input);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static class AutoComplete
        {
            public static IEnumerable<string> GetWordCompletionCandidateWords<T>(string partialWord, IEnumerable<T> items, System.Func<T, string> wordSelector) => GetWordCompletionCandidates(partialWord, items, wordSelector).Select(wordSelector);
            public static IEnumerable<string> GetWordCompletionCandidates(string partialWord, IEnumerable<string> items) => GetWordCompletionCandidates(partialWord, items, r => r);
            public static IEnumerable<T> GetWordCompletionCandidates<T>(string partialWord, IEnumerable<T> items, System.Func<T, string> wordSelector)
            {
                d.AssertFormat(!partialWord.Contains(' '), "Tried to get matching item for non-word {0}! Cannot autocomplete sentences!", partialWord);
                if (partialWord.IsEmpty())
                    return items;

                List<T> candidateItems = new List<T>();
                foreach (T item in items)
                {
                    string potentialWord = wordSelector.Invoke(item);

                    int excessLength = potentialWord.Length - partialWord.Length;
                    if (excessLength < 0)
                        continue;

                    string partialPotentialWord = potentialWord.Substring(0, partialWord.Length);
                    if (string.Compare(partialPotentialWord, partialWord, true) != 0)
                        continue;

                    candidateItems.Add(item);
                }

                return candidateItems;
            }
        }
    }
}
