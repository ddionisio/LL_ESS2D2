﻿using UnityEngine;
using System.Collections;

namespace M8 {
    [AddComponentMenu("M8/Sprite/ColorPulse")]
    public class SpriteColorPulse : MonoBehaviour {
        public SpriteRenderer sprite;

        public float pulsePerSecond;
        public Color startColor;
        public Color endColor = Color.white;
        public bool isRealTime;

        private float mCurPulseTime = 0;
        private bool mStarted = false;
        private float mLastTime;

        private Color mDefaultColor;

        void OnEnable() {
            if(mStarted) {
                mCurPulseTime = 0;
                mLastTime = isRealTime ? Time.realtimeSinceStartup : Time.time;
                sprite.color = startColor;
            }
        }

        void OnDisable() {
            if(mStarted) {
                sprite.color = mDefaultColor;
            }
        }

        void Awake() {
            if(sprite == null)
                sprite = GetComponent<SpriteRenderer>();

            mDefaultColor = sprite.color;
        }

        // Use this for initialization
        void Start() {
            mStarted = true;
            sprite.color = startColor;
        }

        // Update is called once per frame
        void Update() {
            float time = isRealTime ? Time.realtimeSinceStartup : Time.time;
            float delta = time - mLastTime;
            mLastTime = time;

            mCurPulseTime += delta;

            float t = Mathf.Sin(Mathf.PI*mCurPulseTime*pulsePerSecond);
            t *= t;

            sprite.color = Color.Lerp(startColor, endColor, t);
        }
    }
}