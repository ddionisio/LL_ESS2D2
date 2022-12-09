using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class LoLPlaySound : MonoBehaviour {
        public string soundPath;
        public bool isBackground;
        public bool isLoop;

        public void Play() {
            if(LoLManager.isInstantiated)
                LoLManager.instance.PlaySound(soundPath, isBackground, isLoop);
        }
    }
}