using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LoLExt;
using TMPro;

public class WeatherCycleStartWidget : MonoBehaviour {
    public TMP_Text cycleLabel;
    [M8.Localize]
    public string cycleTextRef;

    [Header("Speech")]
    public string speechGroup;
    [M8.Localize]
    public string[] speechTextRefs;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeCycleStart;

    void OnDisable() {
        if(GameController.isInstantiated)
            GameController.instance.startCycleCallback -= OnCycleStart;
    }

    void OnEnable() {
        //ensure display is 'hidden'
        if(animator && !string.IsNullOrEmpty(takeCycleStart))
            animator.ResetTake(takeCycleStart);

        GameController.instance.startCycleCallback += OnCycleStart;
    }

    void OnCycleStart() {
        int cycleNum = GameController.instance.weatherCycle.curCycleIndex + 1;
        cycleLabel.text = string.Format(M8.Localize.Get(cycleTextRef), cycleNum);

        if(animator && !string.IsNullOrEmpty(takeCycleStart))
            animator.Play(takeCycleStart);

        //play speeches
        for(int i = 0; i < speechTextRefs.Length; i++) {
            if(!string.IsNullOrEmpty(speechTextRefs[i]))
                LoLManager.instance.SpeakTextQueue(speechTextRefs[i], speechGroup, i);
        }
    }
}
