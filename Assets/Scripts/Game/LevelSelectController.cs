using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectController : GameModeController<LevelSelectController> {
    [Header("Data")]
    public string modalLevelSelect = "levelSelect";

    [Header("Signal")]
    public SignalBoolean signalActive;
    public SignalLevelLocationData signalLevelLocation;

    private M8.GenericParams mLevelLocationParms = new M8.GenericParams();

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        if(signalLevelLocation)
            signalLevelLocation.callback -= OnLevelLocationClicked;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        if(signalLevelLocation)
            signalLevelLocation.callback += OnLevelLocationClicked;
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        //tutorial at beginning

        //active selection
        if(signalActive)
            signalActive.Invoke(true);
    }

    void OnLevelLocationClicked(LevelLocationData levelData) {
        mLevelLocationParms[ModalLevelLocation.parmLevelLocation] = levelData;
        mLevelLocationParms[ModalLevelLocation.parmClimateMatch] = GameData.instance.curLevelData.climateMatch;

        M8.UIModal.Manager.instance.ModalOpen(modalLevelSelect, mLevelLocationParms);
    }
}
