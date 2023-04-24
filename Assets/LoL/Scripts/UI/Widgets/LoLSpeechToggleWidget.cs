using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace LoLExt {
    public class LoLSpeechToggleWidget : MonoBehaviour {
        [Header("Display")]
        public Button button;
        public TMP_Text toggleLabel;
        [M8.Localize]
        public string onStringRef;
        [M8.Localize]
        public string offStringRef;

        void OnEnable() {
            UpdateToggleStates();
        }

        void Awake() {
            button.onClick.AddListener(ToggleSpeech);
        }

        void ToggleSpeech() {
            LoLManager.instance.ApplySpeechMute(!LoLManager.instance.isSpeechMute);

            UpdateToggleStates();
        }

        private void UpdateToggleStates() {
            if(toggleLabel) {
                string txt = LoLManager.instance.isSpeechMute ? M8.Localize.Get(offStringRef) : M8.Localize.Get(onStringRef);
                toggleLabel.text = txt;
            }
        }
    }
}