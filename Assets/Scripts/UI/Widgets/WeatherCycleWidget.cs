using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherCycleWidget : MonoBehaviour {
    [System.Serializable]
    public class WeatherData {
        public Image panelImage;
        public Image iconImage;

        private Color mPanelImageDefaultColor;
        private Color mIconImageDefaultColor;

        public void Init() {
            mPanelImageDefaultColor = panelImage.color;
            mIconImageDefaultColor = iconImage.color;
        }

        public void Active() {
            panelImage.color = mPanelImageDefaultColor;
            iconImage.color = mIconImageDefaultColor;
        }

        public void Inactive(Color panelColor, Color iconColor) {
            panelImage.color = panelColor;
            iconImage.color = iconColor;
        }
    }

    public WeatherData[] weatherSlots;

    [Header("Display State")]
    public Text weatherTitleLabel;
    public Text temperatureLabel;
    public Color panelColorInactive;
    public Color iconColorInactive;

    [Header("Pointer")]
    public Transform pointerRoot;
    public float pointerAngleStart = -45;
    public float pointerAngleEnd = -315;

    [Header("Progress")]
    public Image progressImage;

    private bool mIsCyclePlaying;

    void OnDisable() {
        if(GameController.isInstantiated) {
            GameController.instance.prepareCycleCallback -= OnPrepareCycle;

            var weatherCycle = GameController.instance.weatherCycle;
            if(weatherCycle) {
                weatherCycle.cycleBeginCallback -= OnCycleBegin;
                weatherCycle.cycleEndCallback -= OnCycleEnd;
                weatherCycle.weatherBeginCallback -= OnCycleWeatherBegin;
                weatherCycle.weatherEndCallback -= OnCycleWeatherEnd;
            }
        }
    }

    void OnEnable() {
        GameController.instance.prepareCycleCallback += OnPrepareCycle;

        var weatherCycle = GameController.instance.weatherCycle;
        weatherCycle.cycleBeginCallback += OnCycleBegin;
        weatherCycle.cycleEndCallback += OnCycleEnd;
        weatherCycle.weatherBeginCallback += OnCycleWeatherBegin;
        weatherCycle.weatherEndCallback += OnCycleWeatherEnd;
    }

    void Awake() {
        for(int i = 0; i < weatherSlots.Length; i++)
            weatherSlots[i].Init();
    }

    void Update() {
        if(mIsCyclePlaying) {
            float cycleProgress = GameController.instance.weatherCycle.curCycleProgress;

            float pointerAngle = Mathf.Lerp(pointerAngleStart, pointerAngleEnd, cycleProgress);
            SetPointerAngle(pointerAngle);

            progressImage.fillAmount = cycleProgress;
        }
    }

    private void SetAllInactive() {
        SetPointerAngle(pointerAngleStart);

        progressImage.fillAmount = 0f;

        for(int i = 0; i < weatherSlots.Length; i++)
            weatherSlots[i].Inactive(panelColorInactive, iconColorInactive);

        weatherTitleLabel.text = "";
        temperatureLabel.text = "";
    }

    private void SetPointerAngle(float angle) {
        var curRot = pointerRoot.eulerAngles;
        curRot.z = angle;
        pointerRoot.eulerAngles = curRot;
    }

    void OnPrepareCycle() {
        //apply weather icons from current cycle
        var curCycle = GameController.instance.weatherCycle.curCycleData;

        for(int i = 0; i < weatherSlots.Length; i++) {
            weatherSlots[i].iconImage.sprite = curCycle.weathers[i].type.icon;
        }

        SetAllInactive();
    }

    void OnCycleBegin() {
        mIsCyclePlaying = true;
    }

    void OnCycleEnd() {
        mIsCyclePlaying = false;
    }

    void OnCycleWeatherBegin() {
        var curWeather = GameController.instance.weatherCycle.curWeather;
        var curWeatherInd = GameController.instance.weatherCycle.curWeatherIndex;

        weatherTitleLabel.text = M8.Localize.Get(curWeather.type.titleRef);
        temperatureLabel.text = curWeather.temperatureText;

        weatherSlots[curWeatherInd].Active();
    }

    void OnCycleWeatherEnd() {
        int curWeatherInd = GameController.instance.weatherCycle.curWeatherIndex;

        weatherSlots[curWeatherInd].Inactive(panelColorInactive, iconColorInactive);
    }
}
