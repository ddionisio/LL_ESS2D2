﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoLExt {
    public class LoLPlaySoundClick : LoLPlaySound, IPointerClickHandler {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            Play();
        }
    }
}