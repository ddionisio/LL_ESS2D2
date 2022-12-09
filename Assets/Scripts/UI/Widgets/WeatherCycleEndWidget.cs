using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleEndWidget : MonoBehaviour {
    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeCycleStart;

    [Header("Speech")]
    public string speechGroup;
    [M8.Localize]
    public string speechTextRef;

    void OnDisable() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle)
            GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;
    }

    void OnEnable() {
        //ensure display is 'hidden'
        if(animator && !string.IsNullOrEmpty(takeCycleStart))
            animator.ResetTake(takeCycleStart);

        GameController.instance.weatherCycle.cycleEndCallback += OnCycleEnd;
    }

    void OnCycleEnd() {
        if(animator && !string.IsNullOrEmpty(takeCycleStart))
            animator.Play(takeCycleStart);

        if(!string.IsNullOrEmpty(speechTextRef))
            LoLManager.instance.SpeakTextQueue(speechTextRef, speechGroup, 0);
    }
}
