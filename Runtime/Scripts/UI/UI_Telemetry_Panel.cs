using BeltainsTools.Pooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.Debugging
{
    public class UI_Telemetry_Panel : MonoBehaviour
    {
        [SerializeField] protected UI_TextOutput_Element m_ElementPrefab;

        Pool<UI_TextOutput_Element> m_ElementPool;


        void SetTelemetry(IEnumerable<string> telemetryMessages)
        {
            m_ElementPool.RecycleAllPooledObjects();

            foreach (string message in telemetryMessages)
            {
                m_ElementPool.SpawnPooledObject(Vector3.zero, Quaternion.identity, transform).SetText(message);
            }
        }



        private void OnPool()
        {
            if(m_ElementPool == null)
                m_ElementPool = new Pool<UI_TextOutput_Element>(m_ElementPrefab, transform, transform, 10);
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
