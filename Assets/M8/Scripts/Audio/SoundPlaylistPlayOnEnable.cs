﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M8 {
    [AddComponentMenu("M8/Audio/Sound Playlist Play On Enable")]
    public class SoundPlaylistPlayOnEnable : MonoBehaviour {
        public enum Mode {
            TwoD,
            ThreeD,
            ThreeDFollow
        }

        [SoundPlaylist]
        public string sound;
        public Mode mode = Mode.TwoD;

        void OnEnable() {
            switch(mode) {
                case Mode.TwoD:
                    SoundPlaylist.instance.Play(sound, false);
                    break;
                case Mode.ThreeD:
                    SoundPlaylist.instance.Play(sound, transform.position, false);
                    break;
                case Mode.ThreeDFollow:
                    SoundPlaylist.instance.Play(sound, transform, false);
                    break;
            }
        }
    }
}