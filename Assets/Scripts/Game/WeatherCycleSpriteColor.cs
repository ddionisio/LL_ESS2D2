using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleSpriteColor : MonoBehaviour {
    [System.Serializable]
    public class Data {
        public WeatherType weather;
        public Color[] colors;
    }

    [Header("Data")]
    public Data[] weathers;

    [Header("Display")]
    public SpriteRenderer target;
    public AnimationCurve curve;
    public Color defaultColor;

    private Data mCurData = null;
    private float mCurTime;
    private float mCurDelay;

    private Dictionary<WeatherType, Data> mWeatherLookup;

    void OnDestroy() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle) {
            GameController.instance.weatherCycle.weatherBeginCallback -= WeatherBegin;
        }
    }

    void Awake() {
        mWeatherLookup = new Dictionary<WeatherType, Data>(weathers.Length);
        for(int i = 0; i < weathers.Length; i++)
            mWeatherLookup.Add(weathers[i].weather, weathers[i]);

        //initialize color
        target.color = defaultColor;

        GameController.instance.weatherCycle.weatherBeginCallback += WeatherBegin;
    }

    void Update() {
        if(mCurData != null && mCurTime < mCurDelay) {
            mCurTime += Time.deltaTime;

            float t = Mathf.Clamp01(mCurTime / mCurDelay);
            t = Mathf.Clamp01(curve.Evaluate(t));

            int lastInd = mCurData.colors.Length - 1;

            int beginInd = Mathf.FloorToInt(t * lastInd);
            int endInd = Mathf.CeilToInt(t * lastInd);

            var clrStart = mCurData.colors[beginInd];
            var clrEnd = mCurData.colors[endInd];

            if(clrStart != clrEnd) {
                var clrT = t * lastInd - beginInd;

                target.color = Color.Lerp(clrStart, clrEnd, clrT);
            }
            else
                target.color = clrStart;
        }
    }

    void WeatherBegin() {
        var curWeather = GameController.instance.weatherCycle.curWeather;

        mWeatherLookup.TryGetValue(curWeather.type, out mCurData);

        mCurTime = 0f;
        mCurDelay = GameController.instance.weatherCycle.curCycleData.weatherDuration;
    }
}
