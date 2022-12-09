using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class ModalVictory : M8.UIModal.Controller, M8.UIModal.Interface.IPush {
        public string sfxPath;

        public override void SetActive(bool aActive) {
            base.SetActive(aActive);

            if(aActive) {
                if(!string.IsNullOrEmpty(sfxPath))
                    LoLManager.instance.PlaySound(sfxPath, false, false);
            }
        }

        void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {

        }
    }
}