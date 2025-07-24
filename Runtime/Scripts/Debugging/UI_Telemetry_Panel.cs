using BeltainsTools.Pooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.Debugging
{
    public class UI_Telemetry_Panel : MonoBehaviour
    {
        [SerializeField] protected UI_TextOutput_Element m_TextElementPrefab;
        [SerializeField] protected UI_LineGraphOutput_Element m_LineGraphElementPrefab;

        Pool<UI_TextOutput_Element> m_TextElementPool;
        Pool<UI_LineGraphOutput_Element> m_LineGraphElementPool;


        void SetTelemetry(IEnumerable<string> telemetryMessages)
        {
            m_TextElementPool.RecycleAllPooledObjects();
            m_LineGraphElementPool.RecycleAllPooledObjects();

            foreach (string message in telemetryMessages)
            {
                m_TextElementPool.SpawnPooledObject(Vector3.zero, Quaternion.identity, transform).SetText(message);
            }

            //m_LineGraphElementPool.SpawnPooledObject(Vector3.zero, Quaternion.identity, transform).Set("Test", new Vector2[] {
            //    new Vector2(0, 0),
            //    new Vector2(1, 1),
            //    new Vector2(2, 0.5f),
            //    new Vector2(3, 1.5f),
            //    new Vector2(4, 1)
            //});
        }



        private void OnPool()
        {
            if (m_TextElementPool == null)
                m_TextElementPool = new Pool<UI_TextOutput_Element>(m_TextElementPrefab, transform, transform, 10);
            if (m_LineGraphElementPool == null)
                m_LineGraphElementPool = new Pool<UI_LineGraphOutput_Element>(m_LineGraphElementPrefab, transform, transform, 10);
        }

        private void OnSpawn()
        {
            SetTelemetry(d.GetTrackedMessages());
            d.TrackedObjectsUpdatedEvent.Subscribe(SetTelemetry);
        }

        private void OnRecycle()
        {
            d.TrackedObjectsUpdatedEvent.Unsubscribe(SetTelemetry);
        }
    }
}
