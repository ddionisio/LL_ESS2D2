﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace M8 {
    [AddComponentMenu("M8/Entity/Signal Release")]
    public class EntitySignalRelease : MonoBehaviour {
        [SerializeField]
        EntityBase _target;
        [SerializeField]
        SignalEntity _signal;
        [SerializeField]
        EntityUnityEvent _event;

        void OnDestroy() {
            if(_target)
                _target.releaseCallback -= OnEntityRelease;
        }

        void Awake() {
            if(!_target) _target = GetComponent<EntityBase>();

            if(_target)
                _target.releaseCallback += OnEntityRelease;
        }

        void OnEntityRelease(EntityBase ent) {
            if(_signal)
                _signal.Invoke(ent);

            _event.Invoke(ent);
        }
    }
}