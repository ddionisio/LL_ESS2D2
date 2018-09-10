using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalWeatherForecast : M8.UIModal.Controller, M8.UIModal.Interface.IPush {
    public const string parmWeatherCycleIndex = "i";
    public const string parmWeatherCycle = "w";

    [Header("Weather Info")]
    public GameObject weatherInfoTemplate;
    public int weatherInfoCount = 4;
    public Transform weatherInfoRoot;

    [Header("Text Speech")]    
    public string textSpeechGroup; //use for weather infos
    [M8.Localize]
    public string textSpeechTitleRef;

    //private int mWeatherCycleIndex;
    private WeatherInfoWidget[] mWeatherInfos;

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        //mWeatherCycleIndex = parms.GetValue<int>(parmWeatherCycleIndex);

        var weatherCycle = parms.GetValue<WeatherCycleData.CycleData>(parmWeatherCycle);
        var weathers = weatherCycle.weathers;

        for(int i = 0; i < mWeatherInfos.Length; i++) {
            //hide excess weather info if cycle is short
            if(i >= weathers.Length) {
                mWeatherInfos[i].gameObject.SetActive(false);
                continue;
            }

            mWeatherInfos[i].gameObject.SetActive(true);

            mWeatherInfos[i].Apply(weathers[i]);
        }
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {
            if(!string.IsNullOrEmpty(textSpeechTitleRef))
                LoLManager.instance.SpeakTextQueue(textSpeechTitleRef, textSpeechGroup, 0);

            /*for(int i = 0; i < mWeatherInfos.Length; i++) {
                var textRef = mWeatherInfos[i].titleTextRef;
                if(!string.IsNullOrEmpty(textRef))
                    LoLManager.instance.SpeakTextQueue(textRef, textSpeechGroup, i + 1);
            }*/
        }
    }

    void Awake() {
        mWeatherInfos = new WeatherInfoWidget[weatherInfoCount];

        for(int i = 0; i < weatherInfoCount; i++) {
            var newGO = Instantiate(weatherInfoTemplate);
            newGO.transform.SetParent(weatherInfoRoot);
            mWeatherInfos[i] = newGO.GetComponent<WeatherInfoWidget>();
        }

        weatherInfoTemplate.SetActive(false);
    }
}
