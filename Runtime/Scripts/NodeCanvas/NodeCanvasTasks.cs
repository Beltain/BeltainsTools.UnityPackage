using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace BeltainsTools.NodeCanvas
{
    [Category("BeltainsTools/Timing")]
    public class Delay : ActionTask
    {
        public BBParameter<float> m_Timeout = 1;

        float m_TimeoutCompletion;

        protected override void OnUpdate()
        {
            if(m_TimeoutCompletion <= Time.time)
            {
                EndAction();
            }
        }

        protected override void OnExecute()
        {
            m_TimeoutCompletion = Time.time + m_Timeout.value;
        }

        protected override string OnInit()
        {
            return base.OnInit();
        }
    }

    [Category("BeltainsTools/Collections/Dictionaries")]
    public class DictionaryIsEmpty : ConditionTask
    {
        [RequiredField]
        [BlackboardOnly]
        public BBParameter<IDictionary> m_Dictionary;

        protected override string info => $"{m_Dictionary} is empty";

        protected override bool OnCheck()
        {
            return m_Dictionary.value.Count == 0;
        }
    }

    [Category("BeltainsTools/Collections/Dictionaries")]
    public class RemoveElementFromDictionary<TKey, TValue> : ConditionTask
    {
        [RequiredField]
        [BlackboardOnly]
        public BBParameter<Dictionary<TKey, TValue>> m_Dictionary;
        [RequiredField]
        public BBParameter<TKey> m_KeyToRemove;

        protected override string info => $"Remove {m_KeyToRemove} from {m_Dictionary}";

        protected override bool OnCheck()
        {
            return m_Dictionary.value.Remove(m_KeyToRemove.value);
        }
    }

    //[Category("BeltainsTools")]
    //public class Example : ConditionTask
    //{
    //    protected override bool OnCheck()
    //    {
    //        return false;
    //    }

    //    protected override string OnInit()
    //    {
    //        return base.OnInit();
    //    }
    //}
}
