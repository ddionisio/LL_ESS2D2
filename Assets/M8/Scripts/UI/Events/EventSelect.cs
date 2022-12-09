﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace M8.UI.Events {
    [AddComponentMenu("M8/UI/Events/Select")]
    public class EventSelect : MonoBehaviour, ISelectHandler {
        public UnityEvent selectEvent;
        public Signal selectSignal;

        void ISelectHandler.OnSelect(BaseEventData eventData) {
            selectEvent.Invoke();

            if(selectSignal)
                selectSignal.Invoke();
        }
    }
}