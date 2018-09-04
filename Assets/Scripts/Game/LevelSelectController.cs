using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectController : GameModeController<LevelSelectController> {
    [Header("Data")]
    public float startWaitDelay = 0.5f;
    public string modalLevelSelect = "levelSelect";
    public GameObject[] levelGroupGOs;

    [Header("Tutorial")]
    public string tutorialModalDialog;
    [M8.Localize]
    public string[] tutorialDialogTexts;
    public float tutorialEndWaitDelay = 1f;

    [Header("Signal")]
    public M8.Signal signalShowLevelMatch;
    public SignalLevelLocationData signalLevelLocation;

    private M8.GenericParams mLevelLocationParms = new M8.GenericParams();
        
    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        if(signalLevelLocation)
            signalLevelLocation.callback -= OnLevelLocationClicked;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        //show group of level selects
#if UNITY_EDITOR
        if(!GameData.instance.isGameStarted) {
            GameData.instance.OverrideLevelIndex(DebugControl.instance.levelIndex);
        }
#endif

        var curIndex = GameData.instance.curLevelIndex;

        for(int i = 0; i < levelGroupGOs.Length; i++) {
            if(levelGroupGOs[i])
                levelGroupGOs[i].SetActive(i == curIndex);
        }
        //

        if(signalLevelLocation)
            signalLevelLocation.callback += OnLevelLocationClicked;
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        yield return new WaitForSeconds(startWaitDelay);

        //tutorial at beginning
        var curIndex = GameData.instance.curLevelIndex;
        if(curIndex == 0) {
            M8.GenericParams dlgParms = new M8.GenericParams();

            for(int i = 0; i < tutorialDialogTexts.Length; i++) {
                bool isNext = false;
                ModalDialog.Open(tutorialModalDialog, "", tutorialDialogTexts[i], () => isNext = true);
                while(!isNext)
                    yield return null;
            }

            M8.UIModal.Manager.instance.ModalCloseUpTo(tutorialModalDialog, true);

            while(M8.UIModal.Manager.instance.isBusy)
                yield return null;

            yield return new WaitForSeconds(tutorialEndWaitDelay);
        }
                
        if(signalShowLevelMatch)
            signalShowLevelMatch.Invoke();
    }

    void OnLevelLocationClicked(LevelLocationData levelData) {
        mLevelLocationParms[ModalLevelLocation.parmLevelLocation] = levelData;
        mLevelLocationParms[ModalLevelLocation.parmClimateMatch] = GameData.instance.curLevelData.climateMatch;

        M8.UIModal.Manager.instance.ModalOpen(modalLevelSelect, mLevelLocationParms);
    }
}
