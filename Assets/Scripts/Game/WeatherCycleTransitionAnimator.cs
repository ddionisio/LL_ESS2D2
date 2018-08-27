using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleTransitionAnimator : MonoBehaviour {
    public M8.Animator.Animate animator;
    public string takeCycleEnter;
    public string takeCycleExit;

    void OnDisable() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle) {
            GameController.instance.weatherCycle.cycleBeginCallback -= OnCycleBegin;
            GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;
        }
    }

    void OnEnable() {
        if(GameController.isInstantiated) {
            GameController.instance.weatherCycle.cycleBeginCallback += OnCycleBegin;
            GameController.instance.weatherCycle.cycleEndCallback += OnCycleEnd;
        }

        if(animator && !string.IsNullOrEmpty(takeCycleEnter))
            animator.ResetTake(takeCycleEnter);
    }

    void OnCycleBegin() {
        if(animator && !string.IsNullOrEmpty(takeCycleEnter))
            animator.Play(takeCycleEnter);
    }

    void OnCycleEnd() {
        if(animator && !string.IsNullOrEmpty(takeCycleExit))
            animator.Play(takeCycleExit);
    }
}
