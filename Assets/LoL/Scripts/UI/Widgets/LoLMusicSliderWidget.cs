using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoLExt {
    public class LoLMusicSliderWidget : MonoBehaviour {
        public Slider slider;

        void OnEnable() {
            if(M8.UserSettingAudio.isInstantiated)
                slider.value = M8.UserSettingAudio.instance.musicVolume;
        }

        void Awake() {
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        void OnValueChanged(float val) {
            if(M8.UserSettingAudio.isInstantiated)
                M8.UserSettingAudio.instance.musicVolume = slider.value;
        }
    }
}