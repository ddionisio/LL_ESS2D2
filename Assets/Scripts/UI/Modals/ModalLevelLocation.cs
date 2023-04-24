using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LoLExt;

public class ModalLevelLocation : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    public enum State {
        Reveal,
        Mismatch,
        Match
    }

    public const string parmLevelLocation = "l";
    public const string parmClimateMatch = "c";

    [System.Serializable]
    public struct ItemMatchData {
        public GameObject climateLockedGO;
        public ClimateWidget climateWidget;

        public GameObject climateZoneLockedGO;
        public ClimateZoneWidget climateZoneWidget;

        public void SetLocked(bool locked) {
            if(climateLockedGO) climateLockedGO.SetActive(locked);
            if(climateWidget) climateWidget.gameObject.SetActive(!locked);

            if(climateZoneLockedGO) climateZoneLockedGO.SetActive(locked);
            if(climateZoneWidget) climateZoneWidget.gameObject.SetActive(!locked);
        }

        public void Apply(ClimateData climateData) {
            if(climateWidget) climateWidget.climate = climateData;
            if(climateZoneWidget) climateZoneWidget.climateZone = climateData.zone;
        }
    }

    [Header("Display")]
    public GameObject revealGO;
    public GameObject matchGO;
    public GameObject mismatchGO;

    public GameObject climateMatchGO;
    public GameObject climateMismatchGO;

    public GameObject climateZoneMatchGO;
    public GameObject climateZoneMismatchGO;

    public Image levelImage;
    public M8.TextMeshPro.LocalizerTextMeshPro levelTitleLocLabel;
    public M8.TextMeshPro.LocalizerTextMeshPro levelDescLocLabel;
    
    [Header("Data")]
    public ItemMatchData climateLevel;
    public ItemMatchData climateMatch;

    [Header("Audio")]
    public string sfxPathMatch;
    public string sfxPathMismatch;

    private State state {
        get { return mState; }
        set {
            if(mState != value) {
                mState = value;
                ApplyCurState();
            }
        }
    }

    private State mState;

    private LevelLocationData mLevelLocationData;
    private ClimateData mClimateMatchData;
    
    public void Reveal() {
        bool isZoneMatch = mLevelLocationData.climate.zone == mClimateMatchData.zone;
        bool isClimateMatch = mLevelLocationData.climate == mClimateMatchData;
        bool isMatch = isZoneMatch && isClimateMatch;

        if(climateMatchGO) climateMatchGO.SetActive(isClimateMatch);
        if(climateMismatchGO) climateMismatchGO.SetActive(!isClimateMatch);

        if(climateZoneMatchGO) climateZoneMatchGO.SetActive(isZoneMatch);
        if(climateZoneMismatchGO) climateZoneMismatchGO.SetActive(!isZoneMatch);

        if(isMatch) {
            //play ding sound
            if(!string.IsNullOrEmpty(sfxPathMatch)) LoLManager.instance.PlaySound(sfxPathMatch, false, false);

            state = State.Match;
        }
        else {
            //play error sound
            if(!string.IsNullOrEmpty(sfxPathMismatch)) LoLManager.instance.PlaySound(sfxPathMismatch, false, false);

            state = State.Mismatch;
        }
    }

    public void LaunchLevel() {        
        GameData.instance.Progress();
    }

    void M8.UIModal.Interface.IPop.Pop() {
        mLevelLocationData = null;
        mClimateMatchData = null;

        if(revealGO) revealGO.SetActive(false);
        if(matchGO) matchGO.SetActive(false);
        if(mismatchGO) mismatchGO.SetActive(false);

        if(climateMatchGO) climateMatchGO.SetActive(false);
        if(climateMismatchGO) climateMismatchGO.SetActive(false);

        if(climateZoneMatchGO) climateZoneMatchGO.SetActive(false);
        if(climateZoneMismatchGO) climateZoneMismatchGO.SetActive(false);
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        mLevelLocationData = parms.GetValue<LevelLocationData>(parmLevelLocation);
        mClimateMatchData = parms.GetValue<ClimateData>(parmClimateMatch);

        //setup displays
        if(levelImage) levelImage.sprite = mLevelLocationData.image;

        if(levelTitleLocLabel) {
            levelTitleLocLabel.key = mLevelLocationData.titleTextRef;
            levelTitleLocLabel.Apply();
        }

        if(levelDescLocLabel) {
            levelDescLocLabel.key = mLevelLocationData.descTextRef;
            levelDescLocLabel.Apply();
        }

        climateLevel.Apply(mLevelLocationData.climate);        
        climateMatch.Apply(mClimateMatchData);

        if(climateMatchGO) climateMatchGO.SetActive(false);
        if(climateMismatchGO) climateMismatchGO.SetActive(false);

        if(climateZoneMatchGO) climateZoneMatchGO.SetActive(false);
        if(climateZoneMismatchGO) climateZoneMismatchGO.SetActive(false);

        mState = State.Reveal;
        ApplyCurState();
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {
            //play speech
            if(!string.IsNullOrEmpty(mLevelLocationData.titleTextRef)) LoLManager.instance.SpeakTextQueue(mLevelLocationData.titleTextRef, mLevelLocationData.titleTextRef, 0);
            if(!string.IsNullOrEmpty(mLevelLocationData.descTextRef)) LoLManager.instance.SpeakTextQueue(mLevelLocationData.descTextRef, mLevelLocationData.titleTextRef, 1);
        }
    }

    private void ApplyCurState() {
        if(revealGO) revealGO.SetActive(mState == State.Reveal);
        if(matchGO) matchGO.SetActive(mState == State.Match);
        if(mismatchGO) mismatchGO.SetActive(mState == State.Mismatch);

        climateLevel.SetLocked(mState == State.Reveal);
    }
}
