using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BeltainsTools.Pooling;
using BeltainsTools.EventHandling;
using UnityEngine.EventSystems;
using System.Linq;
using BeltainsTools.Utilities;
using UnityEngine.UI;

namespace BeltainsTools.Debugging
{
    public class UI_Terminal_Panel : MonoBehaviour
    {
        [SerializeField] protected Color m_HighlightColor = Color.red;
        [SerializeField] protected UI_TextOutput_Element m_HistoryOutputElementPrefab;
        [SerializeField] protected RectTransform m_HistoryContainer;
        [SerializeField] protected TextMeshProUGUI m_AutofillText;
        [SerializeField] protected UI_TextOutput_Element m_AutofillSuggestionElementPrefab;
        [SerializeField] protected RectTransform m_AutofillSuggestionsBox;
        [SerializeField] protected RectTransform m_AutofillOverflowElement;

        static UI_Terminal_Panel s_ActiveInstance;



        TMP_InputField m_InputField;

        BEvent<UI_Terminal_Panel> CloseRequestedEvent;
        BEvent<bool> InputFieldSelectedEvent;

        static char k_TokenSeparatorChar = ' '; //Character that defines how tokens are recognised and separated

        string m_CurText = string.Empty;
        string[] m_CurTextTokens = new string[0];
        string m_CommandToken = string.Empty;

        string[] m_CommandGuideTokens = new string[0];
        List<string[]> m_CommandGuideAutofillSuggestions = new List<string[]>();

        Pool<UI_TextOutput_Element> m_HistoryElementsPool;
        Queue<HistoryItem> m_History = new Queue<HistoryItem>();
        HistoryItem[] m_HistoryArray = null; //Searchable version of m_History, set from m_History
        const int k_HistoryMaxInputsCount = 20;
        int m_InputHistorySearchIndex = -1; //The index we're currently searching through in the history. -1 = not cycling history
        bool m_IsMidHistorySearch = false;

        static readonly char[] s_AutofillBreakChars = new char[] { '.', '_'}; //Complete array of all chars that breaks up tokens, besdies space, as space is considered the actual token separator for this system.

        Pool<UI_TextOutput_Element> m_AutofillSuggestionElementsPool;
        List<UI_TextOutput_Element> m_ActiveAutofillSuggestionElements = new List<UI_TextOutput_Element>();
        string[] m_AutofillSuggestions = new string[0];
        const int k_AutofillMaxSuggestionsCount = 10;
        int m_AutofillSelectionIndex = 0;
        string m_SelectedAutofillString = "";
        bool m_DoesAutofillOverflow = false; //Have we gone over the max number of suggestions?
        bool m_HasAutofillSuggestions = false; //Do we have any suggestions based on the current token context
        bool m_RequestedCloseAutofill = false; //User has ended autofill feature for the current token context



        bool AutofillActive => m_HasAutofillSuggestions && !m_RequestedCloseAutofill && !m_IsMidHistorySearch;




        struct HistoryItem
        {
            public string Input; //Value input by the player
            public string Output; //Value returned from our attempted running of the input as a debug command
        }





        [DebugCommand("Terminal.Clear", "Clear terminal history")]
        public static void ClearActiveTerminalHistory()
        {
            d.Assert(s_ActiveInstance != null, "What?");
            s_ActiveInstance.ClearHistory();
        }

        [DebugCommand("Terminal.Help", "Draw a list of all available commands")]
        public static string PrintCommands()
        {
            return string.Join("\n", DebugCommands.s_Commands.Select(command => command.HelpLine));
        }



        public void SetCallbacks(System.Action<UI_Terminal_Panel> onCloseCallback, System.Action<bool> onInputFieldSelectedCallback)
        {
            if (onCloseCallback != null)
                CloseRequestedEvent.Subscribe(onCloseCallback);
            if (onInputFieldSelectedCallback != null)
                InputFieldSelectedEvent.Subscribe(onInputFieldSelectedCallback);
        }


        public void ClearHistory()
        {
            ClearHistory_Internal();
        }




        void AttemptDebugCommand(string input)
        {
            if (input != "")
            {
                string commandRunLog = DebugCommands.ExecuteCommandString(input);
                AddToHistory(input, commandRunLog);
            }


            ResetInputField();
            FocusInput();
        }

        void ClosePanel()
        {
            CloseRequestedEvent.Invoke(this);
        }

        void ResetInputField()
        {
            m_InputField.text = "";
            ResetHistorySearchIndex();
        }

        void FocusInput()
        {
            Coroutines.FrameDelayedAction.Execute(() =>
            {
                m_InputField.Select();
                m_InputField.ActivateInputField();
            }, this);
        }

        void PlaceCaretAtEndAndClearSelection()
        {
            m_InputField.ReleaseSelection();
            m_InputField.caretPosition = m_InputField.text.Length;
            m_InputField.stringPosition = m_InputField.text.Length;
        }



        void ResetHistorySearchIndex() => SetHistorySearchIndex(-1);
        void CycleSearchHistory(int depth) => SetHistorySearchIndex(m_InputHistorySearchIndex + depth, true);
        void SetHistorySearchIndex(int index, bool setTextEmptyIfReset = false)
        {
            index = Mathf.Clamp(index, -1, m_History.Count - 1);

            if (m_InputHistorySearchIndex == index)
                return;
            m_InputHistorySearchIndex = index;

            if (index < 0)
            {
                m_IsMidHistorySearch = false;
                if (setTextEmptyIfReset)
                    m_InputField.text = "";
            }
            else
            {
                m_IsMidHistorySearch = true;
                m_InputField.text = m_HistoryArray[m_InputHistorySearchIndex].Input;
            }
            PlaceCaretAtEndAndClearSelection();
        }

        void AddToHistory(string inputString, string outputString)
        {
            if (m_History.Count >= k_HistoryMaxInputsCount)
                m_History.Dequeue();

            m_History.Enqueue(new HistoryItem() { Input = inputString, Output = outputString });
            m_HistoryArray = m_History.Reverse().ToArray();
            RedrawHistoryElements();
        }

        void ClearHistory_Internal()
        {
            m_History.Clear();
            m_HistoryArray = new HistoryItem[0];
            m_IsMidHistorySearch = false;
            RedrawHistoryElements();
        }

        void RedrawHistoryElements()
        {
            m_HistoryElementsPool.RecycleAllPooledObjects();

            foreach (HistoryItem historyItem in m_History)
            {
                m_HistoryElementsPool.SpawnPooledObject(Vector3.zero, Quaternion.identity, m_HistoryContainer).SetText(historyItem.Output);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_HistoryContainer);
        }





        void TryApplyCurrentAutofill()
        {
            if (!AutofillActive)
                return;

            List<string> textTokensAfterAutoComplete = m_CurTextTokens.SkipLast(1).ToList();
            string autofillStringToken = m_SelectedAutofillString;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                int endOfCurTextToken = m_CurTextTokens[m_CurTextTokens.Length - 1].Length;
                for (int i = endOfCurTextToken; i < autofillStringToken.Length; i++)
                {
                    if (s_AutofillBreakChars.Contains(autofillStringToken[i]))
                    {
                        autofillStringToken = autofillStringToken.Substring(0, i + 1);
                        break;
                    }
                }
            }
            textTokensAfterAutoComplete.Add(autofillStringToken);

            m_InputField.text = string.Join(k_TokenSeparatorChar, textTokensAfterAutoComplete);
            PlaceCaretAtEndAndClearSelection();
        }

        bool TryCloseAutofill()
        {
            if (!AutofillActive)
                return false;

            m_RequestedCloseAutofill = true;
            ClearAutofillOptions();
            return true;
        }

        void UpdateAutoFillSuggestions()
        {
            if(m_CurText.IsEmpty())
            {
                ClearAutofillOptions();
                return;
            }

            if (m_CurTextTokens.Length == 1)
            {
                //Display command auto fill options
                SetAutofillOptions(m_CommandToken, DebugCommands.s_Commands, r => r.Name);
            }
            else if (m_CurTextTokens.Length <= m_CommandGuideTokens.Length)
            {
                //Display param auto fill options
                string currentToken = m_CurTextTokens[m_CurTextTokens.Length - 1];
                string[] paramAutoCompleteList = m_CommandGuideAutofillSuggestions[m_CurTextTokens.Length - 2];
                SetAutofillOptions(currentToken, paramAutoCompleteList);
            }
            else
            {
                ClearAutofillOptions();
            }
        }

        void SetAutofillOptions<T>(string partialWord, IEnumerable<T> items, System.Func<T, string> wordSelector) => SetAutofillOptions(StringUtilities.AutoComplete.GetWordCompletionCandidateWords(partialWord, items, wordSelector).ToArray());
        void SetAutofillOptions(string partialWord, IEnumerable<string> items) => SetAutofillOptions(StringUtilities.AutoComplete.GetWordCompletionCandidates(partialWord, items).ToArray());
        void ClearAutofillOptions() => SetAutofillOptions(new string[0]);
        void SetAutofillOptions(string[] suggestions)
        {
            m_DoesAutofillOverflow = suggestions.Length > k_AutofillMaxSuggestionsCount;
            if (m_DoesAutofillOverflow)
                suggestions = suggestions.SkipLast(suggestions.Length - k_AutofillMaxSuggestionsCount).ToArray();

            m_AutofillSuggestions = suggestions;
            m_HasAutofillSuggestions = m_AutofillSuggestions.Length > 0;

            TryReselectPreviousAutofillSelection();
            RedrawAutofillSuggestionElements();
        }

        void TryReselectPreviousAutofillSelection()
        {
            for (int i = 0; i < m_AutofillSuggestions.Length; i++)
            {
                if (string.Compare(m_AutofillSuggestions[i], m_SelectedAutofillString) == 0)
                {
                    SetAutofillSelection(i);
                    return;
                }
            }

            ResetAutofillSelection();
        }

        void ResetAutofillSelection(bool updateUI = false) => SetAutofillSelection(0, updateUI);
        void NavigateAutofillSelection(int dir) => SetAutofillSelection(m_AutofillSelectionIndex + dir, true);
        void SetAutofillSelection(int index, bool updateUI = false)
        {
            if (!AutofillActive)
                index = -1;
            else
                index = Mathf.Clamp(index, 0, m_AutofillSuggestions.Length - 1);

            m_AutofillSelectionIndex = index;
            m_SelectedAutofillString = index == -1 ? string.Empty : m_AutofillSuggestions[m_AutofillSelectionIndex];

            if(updateUI)
                RedrawAutofillSelection();
        }

        void RedrawAutofillSuggestionElements()
        {
            m_AutofillSuggestionElementsPool.RecycleAllPooledObjects();
            m_ActiveAutofillSuggestionElements.Clear();

            m_AutofillSuggestionsBox.gameObject.SetActive(AutofillActive);

            if (AutofillActive)
            {
                for (int i = 0; i < m_AutofillSuggestions.Length; i++)
                {
                    UI_TextOutput_Element suggestionElement = m_AutofillSuggestionElementsPool.SpawnPooledObject(Vector3.zero, Quaternion.identity, m_AutofillSuggestionsBox);
                    m_ActiveAutofillSuggestionElements.Add(suggestionElement);
                    suggestionElement.SetText(m_AutofillSuggestions[i]);
                }

                m_AutofillOverflowElement.SetAsLastSibling();
                m_AutofillOverflowElement.gameObject.SetActive(m_DoesAutofillOverflow);
            }

            RedrawAutofillSelection();
        }

        void RedrawAutofillSelection()
        {
            //Select ui element
            for (int i = 0; i < m_ActiveAutofillSuggestionElements.Count; i++)
            {
                if (m_AutofillSelectionIndex == i)
                    m_ActiveAutofillSuggestionElements[i].SetBackgroundColor(m_HighlightColor);
                else
                    m_ActiveAutofillSuggestionElements[i].ResetBackgroundColor();
            }

            UpdateAutofillText();
        }

        void UpdateAutofillText()
        {
            string autofillToken = m_SelectedAutofillString;
            string lastCurTextToken = m_CurTextTokens[m_CurTextTokens.Length - 1]; //we only support autofill on the final token of the current text, so here we just get the final token to work with

            //overlay the current Text so there's no mismatching cases breaking up the positioning of the autofill text
            autofillToken = lastCurTextToken + (autofillToken.Length > lastCurTextToken.Length ? autofillToken.Substring(lastCurTextToken.Length) : "");

            //Assemble current text. Should layer tokens like: [command guide tokens] < overlay with [current text tokens] < overlay with [cur token autofill token]
            List<string> autoFilledCurrentTokens = m_CurTextTokens.SkipLast(1).ToList();
            if(!autofillToken.IsEmpty())
                autoFilledCurrentTokens.Add(autofillToken);
            List<string> finalTokens = m_CommandGuideTokens.ToList();
            for (int i = 0; i < autoFilledCurrentTokens.Count; i++)
            {
                if (i < finalTokens.Count)
                    finalTokens[i] = autoFilledCurrentTokens[i];
                else
                    finalTokens.Add(autoFilledCurrentTokens[i]);
            }

            //set
            m_AutofillText.text = string.Join(k_TokenSeparatorChar, finalTokens);
        }

        void UpdateCommandToken()
        {
            if(m_CurTextTokens.Length == 0)
            {
                m_CommandToken = string.Empty;
                m_CommandGuideTokens = new string[0];
                m_CommandGuideAutofillSuggestions.Clear();
                return;
            }

            if (string.Compare(m_CommandToken, m_CurTextTokens[0], true) == 0)
                return;
            m_CommandToken = m_CurTextTokens[0];

            DebugCommands.Command curTextCommand = DebugCommands.s_Commands.Where(r => string.Compare(r.Name, m_CommandToken, true) == 0).FirstOrDefault();
            if(curTextCommand == null)
            {
                m_CommandGuideTokens = new string[0];
                m_CommandGuideAutofillSuggestions.Clear();
                return;
            }

            m_CommandGuideTokens = curTextCommand.GuideTokens;
            m_CommandGuideAutofillSuggestions = curTextCommand.GetAutoFillSuggestionsForParams();
        }

        void UpdateAutofillBoxPosition()
        {
            if (m_CurText.IsEmpty())
                return;

            //Move autofill box to the location of the last cur text token.
            int anchorCharIndex = Mathf.Clamp(m_CurText.Length - m_CurTextTokens.Last().Length, 0, m_CurText.Length - 1);
            m_AutofillSuggestionsBox.position = new Vector3(m_InputField.GetCharacterPosition(anchorCharIndex).x, m_AutofillSuggestionsBox.position.y, m_AutofillSuggestionsBox.position.z);
        }





        void OnSelected(string curValueWhenSelected)
        {
            ResetHistorySearchIndex();
            EventSystem.current.sendNavigationEvents = false;
            InputFieldSelectedEvent.Invoke(true);
        }

        void OnDeselected(string curValueWhenDeselected)
        {
            EventSystem.current.sendNavigationEvents = true;
            InputFieldSelectedEvent.Invoke(false);
        }

        char OnValidateInput(string text, int charIndex, char addedChar)
        {
            if (addedChar == '`')
            {
                return '\0';
            }

            if (addedChar == k_TokenSeparatorChar)
            {
                m_RequestedCloseAutofill = false;
            }

            return addedChar;
        }

        void OnValueChanged(string newValue)
        {
            m_CurText = m_InputField.text;
            m_CurTextTokens = m_CurText.Split(k_TokenSeparatorChar);
            UpdateCommandToken();

            if (m_CurText.IsEmpty())
                m_RequestedCloseAutofill = false;

            if (m_IsMidHistorySearch && (m_InputHistorySearchIndex == -1 || newValue != m_HistoryArray[m_InputHistorySearchIndex].Input))
                ResetHistorySearchIndex(); //break the history search as we've changed the text some other way

            UpdateAutoFillSuggestions();
            UpdateAutofillBoxPosition();
        }

        void OnEndEdit(string endValue)
        {
            if(Input.GetKeyDown(KeyCode.Return))
                AttemptDebugCommand(endValue);
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (TryCloseAutofill())
                {
                    FocusInput();
                    PlaceCaretAtEndAndClearSelection();
                }
            }
        }


        void OnPool()
        {
            m_HistoryElementsPool = new Pool<UI_TextOutput_Element>(m_HistoryOutputElementPrefab, m_HistoryContainer, m_HistoryContainer, k_HistoryMaxInputsCount);
            m_AutofillSuggestionElementsPool = new Pool<UI_TextOutput_Element>(m_AutofillSuggestionElementPrefab, m_AutofillSuggestionsBox, m_AutofillSuggestionsBox, k_AutofillMaxSuggestionsCount);

            m_InputField = GetComponentInChildren<TMP_InputField>();

            m_InputField.onSelect.AddListener(OnSelected);
            m_InputField.onDeselect.AddListener(OnDeselected);
            m_InputField.onValidateInput += OnValidateInput;
            m_InputField.onValueChanged.AddListener(OnValueChanged);
            m_InputField.onEndEdit.AddListener(OnEndEdit);

            m_InputField.caretColor = m_HighlightColor;
            m_InputField.selectionColor = m_HighlightColor;
        }

        void OnSpawn()
        {
            d.Assert(s_ActiveInstance == null, "More than one instance of UI_Terminal_Panel active! No bueno!");
            s_ActiveInstance = this;

            ResetInputField();
        }

        void OnRecycle()
        {
            s_ActiveInstance = null;

            if (m_InputField.gameObject == EventSystem.current.currentSelectedGameObject)
                EventSystem.current.SetSelectedGameObject(null);

            CloseRequestedEvent.Clear();
            InputFieldSelectedEvent.Clear();
        }


        private void OnEnable()
        {
            FocusInput();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (AutofillActive)
                    NavigateAutofillSelection(-1);
                else
                    CycleSearchHistory(1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (AutofillActive)
                    NavigateAutofillSelection(1);
                else
                    CycleSearchHistory(-1);
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                TryApplyCurrentAutofill();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                TryCloseAutofill();
            }
        }
    }
}
