using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScoreWidget : MonoBehaviour {
    public Text scoreLabel;
    public float delay;
    public DG.Tweening.Ease ease;

    private float mCurTime;
    DG.Tweening.EaseFunction mEaseFunc;
    private float mToScore;
    private bool mIsPlaying = false;

    public void Play(int score) {
        mToScore = score;
        mCurTime = 0f;
        mIsPlaying = true;
    }

    void OnEnable() {
        mEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(ease);

        scoreLabel.text = "0";
    }

    void Update() {
        if(mIsPlaying) {
            mCurTime += Time.deltaTime;

            float t = mEaseFunc(mCurTime, delay, 0f, 0f);

            int score = Mathf.RoundToInt(mToScore * t);

            scoreLabel.text = score.ToString();

            if(mCurTime >= delay)
                mIsPlaying = false;
        }
    }
}
