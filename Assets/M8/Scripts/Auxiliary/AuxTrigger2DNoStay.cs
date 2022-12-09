﻿using UnityEngine;

namespace M8.Auxiliary {
    [AddComponentMenu("M8/Auxiliary/Trigger2D No Stay")]
    public class AuxTrigger2DNoStay : MonoBehaviour {
        public delegate void Callback(Collider2D other);

        public event Callback enterCallback;
        public event Callback exitCallback;

        void OnTriggerEnter2D(Collider2D other) {
            if(enterCallback != null)
                enterCallback(other);
        }
        
        void OnTriggerExit2D(Collider2D other) {
            if(exitCallback != null)
                exitCallback(other);
        }
    }
}