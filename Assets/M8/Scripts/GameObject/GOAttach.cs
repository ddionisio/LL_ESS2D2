using UnityEngine;
using System.Collections;

namespace M8 {
    /// <summary>
    /// Make sure this is on an object with a rigidbody!
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("M8/Game Object/Attach")]
    public class GOAttach : MonoBehaviour {
        public Transform target;
        public Vector3 offset;

        void Update() {
            if(target != null) {
                transform.position = target.position + target.rotation * offset;

                transform.rotation = target.rotation;
            }
        }
    }
}