using BeltainsTools.Pooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeltainsTools.Debugging
{
    public class UI_Telemetry_Panel : MonoBehaviour
    {
        [SerializeField] protected UI_TextOutput_Group m_TextGroupPrefab;
        [SerializeField] protected UI_TextOutput_Element m_TextElementPrefab;
        [SerializeField] protected UI_LineGraphOutput_Element m_LineGraphElementPrefab;

        RectTransform m_RectTransform;

        Pool<UI_TextOutput_Group> m_TextGroupPool;
        Pool<UI_TextOutput_Element> m_TextElementPool;
        Pool<UI_LineGraphOutput_Element> m_LineGraphElementPool;

        Dictionary<string, UI_TextOutput_Group> m_TextGroups = new Dictionary<string, UI_TextOutput_Group>();

        void SetTelemetry(IEnumerable<d.TelemetryMessage> telemetryMessages)
        {
            m_TextElementPool.RecycleAllPooledObjects();
            m_TextGroupPool.RecycleAllPooledObjects();
            m_TextGroups.Clear();
            m_LineGraphElementPool.RecycleAllPooledObjects();

            foreach (d.TelemetryMessage message in telemetryMessages)
            {
                UI_TextOutput_Element entry = m_TextElementPool.SpawnPooledObject(Vector3.zero, Quaternion.identity, transform);
                entry.SetText(message);

                if (!message.Group.IsNullOrEmpty())
                {
                    if (!m_TextGroups.TryGetValue(message.Group, out UI_TextOutput_Group group))
                    {
                        group = m_TextGroupPool.SpawnPooledObject(Vector3.zero, Quaternion.identity, transform);
                        group.SetTitle(message.Group);
                        m_TextGroups.Add(message.Group, group);
                    }

                    m_TextGroups[message.Group].AddEntry(entry);
                }
            }

            //m_LineGraphElementPool.SpawnPooledObject(Vector3.zero, Quaternion.identity, transform).Set("Test", new Vector2[] {
            //    new Vector2(0, 0),
            //    new Vector2(1, 1),
            //    new Vector2(2, 0.5f),
            //    new Vector2(3, 1.5f),
            //    new Vector2(4, 1)
            //});

            foreach (var group in m_TextGroups.Values)
                group.ForceRebuildLayoutImmediate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_RectTransform);
        }



        private void OnPool()
        {
            m_RectTransform = transform as RectTransform;

            m_TextElementPool = new Pool<UI_TextOutput_Element>(m_TextElementPrefab, transform, transform, 16);
            m_TextGroupPool = new Pool<UI_TextOutput_Group>(m_TextGroupPrefab, transform, transform, 8);
            m_LineGraphElementPool = new Pool<UI_LineGraphOutput_Element>(m_LineGraphElementPrefab, transform, transform, 4);
        }

        private void OnSpawn()
        {
            SetTelemetry(d.GetTrackedMessages());
            d.TelemetryMessagesUpdatedEvent.Subscribe(SetTelemetry);
        }

        private void OnRecycle()
        {
            d.TelemetryMessagesUpdatedEvent.Unsubscribe(SetTelemetry);
        }
    }
}
