using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleAnimatorEnterExit : MonoBehaviour {
    public WeatherType[] weatherTypes;

    public GameObject rootGO;
    public M8.Animator.Animate animator;
    public string takeEnter = "enter";
    public string takeExit = "exit";

    private bool mIsActive = false;

    void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnAnimatorTakeComplete;

        if(GameController.isInstantiated && GameController.instance.weatherCycle) {
            GameController.instance.weatherCycle.weatherBeginCallback -= OnWeatherBegin;
        }
    }

    void Awake() {
        if(rootGO) rootGO.SetActive(false);

        animator.takeCompleteCallback += OnAnimatorTakeComplete;

        GameController.instance.weatherCycle.weatherBeginCallback += OnWeatherBegin;
    }

    void OnWeatherBegin() {
        var curWeather = GameController.instance.weatherCycle.curWeather;

        bool active = false;
        for(int i = 0; i < weatherTypes.Length; i++) {
            if(curWeather.type == weatherTypes[i]) {
                active = true;
                break;
            }
        }

        if(mIsActive != active) {
            mIsActive = active;
            if(mIsActive) {
                if(rootGO) rootGO.SetActive(true);

                animator.Play(takeEnter);
            }
            else {
                animator.Play(takeExit);
            }
        }
    }

    void OnAnimatorTakeComplete(M8.Animator.Animate anim, M8.Animator.Take take) {
        if(take.name == takeExit) {
            if(rootGO) rootGO.SetActive(false);
        }
    }
}
