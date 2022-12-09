using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class LoLSaveSettingsOnButtonClick : MonoBehaviour {
        public Button[] buttons;

        void Start() {
            for(int i = 0; i < buttons.Length; i++) {
                var btn = buttons[i];
                if(btn)
                    btn.onClick.AddListener(OnPress);
            }
        }

        private void OnPress() {
            LoLManager.instance.settingsData.Save();
        }
    }
}