using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectMatchWidget : MonoBehaviour {
    [Header("Display")]
    public GameObject rootGO;
    public ClimateWidget climateWidget;
    public ClimateZoneWidget climateZoneWidget;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeEnter = "enter";
    public string takeSide = "side";

    [Header("Signals")]
    public M8.Signal signalActivate; //start upon receiving this signal
    public SignalBoolean signalLocationActive;

    public void Proceed() {
        //move widget to the side
        if(animator && !string.IsNullOrEmpty(takeSide))
            animator.Play(takeSide);
    }

    void OnDisable() {
        if(signalActivate)
            signalActivate.callback -= OnSignalActivate;

        if(rootGO) rootGO.SetActive(false);
    }

    void OnEnable() {
        //grab current info via GameData
        var climateMatch = GameData.instance.curLevelData.climateMatch;

        climateWidget.climate = climateMatch;
        climateZoneWidget.climateZone = climateMatch.zone;

        if(signalActivate)
            signalActivate.callback += OnSignalActivate;
    }

    void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnAnimatorTakeComplete;
    }

    void Awake() {
        if(rootGO) rootGO.SetActive(false);

        if(animator)
            animator.takeCompleteCallback += OnAnimatorTakeComplete;
    }

    void OnSignalActivate() {
        if(rootGO) rootGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeEnter))
            animator.Play(takeEnter);
    }

    void OnAnimatorTakeComplete(M8.Animator.Animate anim, M8.Animator.Take take) {
        if(take.name == takeSide) {
            //active selection
            if(signalLocationActive)
                signalLocationActive.Invoke(true);
        }
    }
}
