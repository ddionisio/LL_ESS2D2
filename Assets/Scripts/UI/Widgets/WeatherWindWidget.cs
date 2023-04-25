using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeatherWindWidget : MonoBehaviour {
    [Header("Display")]
    public TMP_Text infoLabel;
    public Transform pointerRoot;

    [Header("Data")]
    public float changeDelay = 0.3f;

    private bool mIsCyclePlaying;
    private Vector2 mCurPointerVel;

    private float mCurSpeed;
    private float mCurSpeedValVel;

    void OnDisable() {
        if(GameController.isInstantiated) {
            GameController.instance.prepareCycleCallback -= OnPrepareCycle;

            var weatherCycle = GameController.instance.weatherCycle;
            if(weatherCycle) {
                weatherCycle.cycleBeginCallback -= OnCycleBegin;
                weatherCycle.cycleEndCallback -= OnCycleEnd;
                weatherCycle.weatherBeginCallback -= OnCycleWeatherBegin;
            }
        }
    }

    void OnEnable() {
        GameController.instance.prepareCycleCallback += OnPrepareCycle;

        var weatherCycle = GameController.instance.weatherCycle;
        weatherCycle.cycleBeginCallback += OnCycleBegin;
        weatherCycle.cycleEndCallback += OnCycleEnd;
        weatherCycle.weatherBeginCallback += OnCycleWeatherBegin;
    }

    void Update() {
        if(mIsCyclePlaying) {
            var curWeather = GameController.instance.weatherCycle.curWeather;

            Vector2 curDir = pointerRoot.up;
            Vector2 toDir = curWeather.windDir;
            if(curDir != toDir) {
                pointerRoot.up = Vector2.SmoothDamp(curDir, toDir, ref mCurPointerVel, changeDelay);
            }

            float speed = curWeather.windSpeed;
            if(mCurSpeed != speed) {
                mCurSpeed = Mathf.SmoothDamp(mCurSpeed, speed, ref mCurSpeedValVel, changeDelay);
                ApplyCurrentSpeedDisplay();
            }
        }
    }

    private void ApplyCurrentSpeedDisplay() {
        int val = Mathf.RoundToInt(mCurSpeed);
        infoLabel.text = val.ToString() + "\nMPH";
    }

    void OnPrepareCycle() {
        pointerRoot.up = Vector2.up;

        mCurSpeed = 0f;
        ApplyCurrentSpeedDisplay();
    }

    void OnCycleBegin() {
        mIsCyclePlaying = true;
    }

    void OnCycleEnd() {
        mIsCyclePlaying = false;
    }

    void OnCycleWeatherBegin() {
        mCurPointerVel = Vector2.zero;
        mCurSpeedValVel = 0f;
    }
}
