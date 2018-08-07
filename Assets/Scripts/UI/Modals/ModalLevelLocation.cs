using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalLevelLocation : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    public const string parmLevelLocation = "l";
    public const string parmClimateMatch = "c";

    [System.Serializable]
    public struct ItemMatchData {
        public ClimateWidget climateWidget;
        public ClimateZoneWidget climateZoneWidget;

        public void Apply(ClimateData climateData) {
            if(climateWidget) climateWidget.climate = climateData;
            if(climateZoneWidget) climateZoneWidget.climateZone = climateData.zone;
        }
    }

    [Header("Display")]
    public GameObject matchGO;
    public GameObject mismatchGO;

    public GameObject climateMatchGO;
    public GameObject climateMismatchGO;

    public GameObject climateZoneMatchGO;
    public GameObject climateZoneMismatchGO;

    public Image levelImage;
    public Text levelTitleLabel;

    public ItemMatchData climateLevel;
    public ItemMatchData climateMatch;

    private LevelLocationData mLevelLocationData;
    private ClimateData mClimateMatchData;

    private LoLSpeakTextClick mTitleLabelSpeakText;

    public void LaunchLevel() {        
        GameData.instance.Progress();
    }

    void M8.UIModal.Interface.IPop.Pop() {
        mLevelLocationData = null;
        mClimateMatchData = null;
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        mLevelLocationData = parms.GetValue<LevelLocationData>(parmLevelLocation);
        mClimateMatchData = parms.GetValue<ClimateData>(parmClimateMatch);

        //setup displays
        if(levelImage) levelImage.sprite = mLevelLocationData.image;

        if(levelTitleLabel) levelTitleLabel.text = M8.Localize.Get(mLevelLocationData.titleTextRef);
        if(mTitleLabelSpeakText) mTitleLabelSpeakText.key = mLevelLocationData.titleTextRef;

        climateLevel.Apply(mLevelLocationData.climate);
        climateMatch.Apply(mClimateMatchData);

        ApplyMatch();
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {
            //play *ding* if match is correct
        }
    }

    void Awake() {
        if(levelTitleLabel) mTitleLabelSpeakText = levelTitleLabel.GetComponent<LoLSpeakTextClick>();
    }

    private void ApplyMatch() {
        bool isZoneMatch = mLevelLocationData.climate.zone == mClimateMatchData.zone;
        bool isClimateMatch = mLevelLocationData.climate == mClimateMatchData;
        bool isMatch = isZoneMatch && isClimateMatch;

        if(matchGO) matchGO.SetActive(isMatch);
        if(mismatchGO) mismatchGO.SetActive(!isMatch);

        if(climateMatchGO) climateMatchGO.SetActive(isClimateMatch);
        if(climateMismatchGO) climateMismatchGO.SetActive(!isClimateMatch);

        if(climateZoneMatchGO) climateZoneMatchGO.SetActive(isZoneMatch);
        if(climateZoneMismatchGO) climateZoneMismatchGO.SetActive(!isZoneMatch);
    }
}
