using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleAnimatorActive : MonoBehaviour {
    public WeatherType weather;
    public GameObject rootGO;
    public M8.Animator.Animate animator;
    public string take;

    private bool mIsActive = false;

    void OnDisable() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle) {
            GameController.instance.weatherCycle.weatherBeginCallback -= OnWeatherBegin;
            GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;
        }
    }

    void OnEnable() {
        GameController.instance.weatherCycle.weatherBeginCallback += OnWeatherBegin;
        GameController.instance.weatherCycle.cycleEndCallback += OnCycleEnd;

        if(rootGO) rootGO.SetActive(false);

        mIsActive = false;
    }

    void OnWeatherBegin() {
        var curWeather = GameController.instance.weatherCycle.curWeather;

        bool active = curWeather.type == weather;
        if(mIsActive != active) {
            mIsActive = active;

            if(rootGO) rootGO.SetActive(mIsActive);

            if(mIsActive)
                animator.Play(take);
        }
    }

    void OnCycleEnd() {
        if(mIsActive) {
            mIsActive = false;
            if(rootGO) rootGO.SetActive(false);
        }
    }
}
