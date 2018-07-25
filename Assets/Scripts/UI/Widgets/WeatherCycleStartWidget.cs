﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherCycleStartWidget : MonoBehaviour {
    public Text cycleLabel;
    [M8.Localize]
    public string cycleTextRef;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeCycleStart;

    void OnDestroy() {
        if(GameController.isInstantiated)
            GameController.instance.prepareCycleCallback -= OnCyclePrepare;
    }

    void Awake() {
        //ensure display is 'hidden'
        if(animator && !string.IsNullOrEmpty(takeCycleStart))
            animator.ResetTake(takeCycleStart);

        GameController.instance.prepareCycleCallback += OnCyclePrepare;
    }

    void OnCyclePrepare() {
        int cycleNum = GameController.instance.weatherCycle.curCycleIndex + 1;
        cycleLabel.text = string.Format(M8.Localize.Get(cycleTextRef), cycleNum);

        if(animator && !string.IsNullOrEmpty(takeCycleStart))
            animator.Play(takeCycleStart);
    }
}
