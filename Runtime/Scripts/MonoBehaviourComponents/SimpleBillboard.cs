using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools
{
    /// <summary>A simple billboard that looks at the camera</summary>
    public class SimpleBillboard : MonoBehaviour
    {
        private void LateUpdate()
        {
            Vector3 directionToFace = Camera.main.orthographic ? (-Camera.main.transform.forward) : Camera.main.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(directionToFace);
        }
    }
}
