using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace M8.UI.Graphics {
    [AddComponentMenu("M8/UI/Graphics/SpriteAnimation")]
    public class SpriteAnimation : MonoBehaviour {
        public Image frameHolder;

        public Sprite[] frames;
        public float framesPerSecond;

        public bool resetOnStop = false;
        public bool autoPlay = true;
        public bool autoSize = true;
        public bool shuffleOnPlay = false;
        public bool isRealtime = false;

        private int mCurFrame;
        private float mFrameCounter;
        private float mLastActiveTime;
        private bool mStarted;
        private bool mPlaying;

        public void Play() {
            if(!mPlaying) {
                mPlaying = true;

                if(shuffleOnPlay)
                    M8.ArrayUtil.Shuffle(frames);

                mLastActiveTime = isRealtime ? Time.realtimeSinceStartup : Time.time;
            }
        }

        public void Pause() {
            mPlaying = false;
        }

        public void Stop() {
            if(resetOnStop) {
                mCurFrame = 0;
                mFrameCounter = 0;
                SetToCurrentFrame();
            }

            mPlaying = false;
        }

        void Awake() {
            if(frameHolder == null)
                frameHolder = GetComponent<Image>();
        }

        void OnEnable() {
            if(mStarted && autoPlay)
                Play();
        }

        void OnDisable() {
            Stop();
        }

        // Use this for initialization
        void Start() {
            mCurFrame = 0;
            mFrameCounter = 0;
            SetToCurrentFrame();

            mStarted = true;

            if(autoPlay)
                Play();
        }

        // Update is called once per frame
        void Update() {
            if(mPlaying) {
                var curTime = isRealtime ? Time.realtimeSinceStartup : Time.time;

                mFrameCounter += (curTime - mLastActiveTime) * framesPerSecond;
                int newFrame = Mathf.RoundToInt(mFrameCounter);
                if(mCurFrame != newFrame) {
                    mCurFrame = newFrame;
                    if(mCurFrame >= frames.Length) {
                        mCurFrame = 0;
                        mFrameCounter -= (float)frames.Length;
                    }

                    SetToCurrentFrame();
                }

                mLastActiveTime = curTime;
            }
        }

        void SetToCurrentFrame() {
            frameHolder.sprite = frames[mCurFrame];
            if(autoSize)
                frameHolder.SetNativeSize();
        }
    }
}