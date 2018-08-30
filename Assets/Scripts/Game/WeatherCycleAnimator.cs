using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleAnimator : MonoBehaviour {    
    [Header("Data")]
    public WeatherType[] weatherTypes;
    public GameObject rootGO;
    public M8.Animator.Animate animator;
    public string takePlay;

    void OnDestroy() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle) {
            GameController.instance.weatherCycle.weatherBeginCallback -= OnWeatherBegin;
        }
    }

    void Awake() {
        if(rootGO)
            rootGO.SetActive(false);

        GameController.instance.weatherCycle.weatherBeginCallback += OnWeatherBegin;
    }

    void OnWeatherBegin() {
        var curWeather = GameController.instance.weatherCycle.curWeather;

        bool isWeatherMatch = false;
        for(int i = 0; i < weatherTypes.Length; i++) {
            if(curWeather.type == weatherTypes[i]) {
                isWeatherMatch = true;
                break;
            }
        }

        if(isWeatherMatch) {
            if(rootGO) rootGO.SetActive(true);

            if(animator && !string.IsNullOrEmpty(takePlay)) {
                //set animation time to sync with weather duration
                float totalTime = animator.GetTakeTotalTime(takePlay);
                float scale = totalTime / GameController.instance.weatherCycle.curCycleData.weatherDuration;

                animator.animScale = scale;

                animator.Play(takePlay);
            }
        }
        else {
            if(rootGO) rootGO.SetActive(false);
        }
    }
}
