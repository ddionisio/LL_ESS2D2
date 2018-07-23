using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherHumidityWidget : MonoBehaviour {
    [Header("Display")]
    public Text percentLabel;
    public Image fillImage;

    [Header("Data")]
    public float fillDelay = 0.3f;

    private bool mIsCyclePlaying;
    private float mFillVel;

    void OnDestroy() {
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

    void Awake() {
        GameController.instance.prepareCycleCallback += OnPrepareCycle;

        var weatherCycle = GameController.instance.weatherCycle;
        weatherCycle.cycleBeginCallback += OnCycleBegin;
        weatherCycle.cycleEndCallback += OnCycleEnd;
        weatherCycle.weatherBeginCallback += OnCycleWeatherBegin;
    }

    void Update() {
        if(mIsCyclePlaying) {
            var curWeather = GameController.instance.weatherCycle.curWeather;

            float curVal = fillImage.fillAmount;
            float toVal = curWeather.humidityPercent / 100f;
            if(curVal != toVal) {
                float val = Mathf.SmoothDamp(curVal, toVal, ref mFillVel, fillDelay);
                ApplyFillValue(val);
            }
        }
    }

    private void ApplyFillValue(float val) {
        int percent = Mathf.RoundToInt(val * 100.0f);
        percentLabel.text = percent.ToString() + "%";

        fillImage.fillAmount = val;
    }

    void OnPrepareCycle() {
        ApplyFillValue(0f);
    }

    void OnCycleBegin() {
        mIsCyclePlaying = true;
    }

    void OnCycleEnd() {
        mIsCyclePlaying = false;
    }

    void OnCycleWeatherBegin() {
        mFillVel = 0f;
    }
}
