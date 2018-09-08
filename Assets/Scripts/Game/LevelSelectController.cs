using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectController : GameModeController<LevelSelectController> {
    [Header("Data")]
    public float startWaitDelay = 0.5f;
    public string modalLevelSelect = "levelSelect";
    public GameObject[] levelGroupGOs;

    [Header("Intro")]
    public CutsceneController introCutscene;
    public GameObject introRootGO;

    [Header("Tutorial")]
    public string tutorialModalDialog;
    [M8.Localize]
    public string[] tutorialDialogTexts;

    [Header("Signal")]
    public M8.Signal signalShowLevelMatch;
    public SignalLevelLocationData signalLevelLocation;

    private M8.GenericParams mLevelLocationParms = new M8.GenericParams();

    private bool mIsIntroFinish;
        
    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        if(introCutscene)
            introCutscene.endCallback -= OnIntroEnd;

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

        if(introCutscene)
            introCutscene.endCallback += OnIntroEnd;

        if(signalLevelLocation)
            signalLevelLocation.callback += OnLevelLocationClicked;
    }

    protected override IEnumerator Start() {
        var curIndex = GameData.instance.curLevelIndex;

        if(introRootGO) introRootGO.SetActive(curIndex == 0);

        do {
            yield return null;
        } while(M8.SceneManager.instance.isLoading);
                
        //tutorial at beginning        
        if(curIndex == 0) {
            if(introCutscene) {
                mIsIntroFinish = false;
                introCutscene.Play();
                while(!mIsIntroFinish)
                    yield return null;
            }

            if(introRootGO) introRootGO.SetActive(false);
        }

        if(signalModeChanged)
            signalModeChanged.Invoke(mode);

        yield return new WaitForSeconds(startWaitDelay);

        for(int i = 0; i < tutorialDialogTexts.Length; i++) {
            bool isNext = false;
            ModalDialog.Open(tutorialModalDialog, "", tutorialDialogTexts[i], () => isNext = true);
            while(!isNext)
                yield return null;
        }

        M8.UIModal.Manager.instance.ModalCloseUpTo(tutorialModalDialog, true);

        while(M8.UIModal.Manager.instance.isBusy)
            yield return null;

        if(signalShowLevelMatch)
            signalShowLevelMatch.Invoke();
    }

    void OnLevelLocationClicked(LevelLocationData levelData) {
        mLevelLocationParms[ModalLevelLocation.parmLevelLocation] = levelData;
        mLevelLocationParms[ModalLevelLocation.parmClimateMatch] = GameData.instance.curLevelData.climateMatch;

        M8.UIModal.Manager.instance.ModalOpen(modalLevelSelect, mLevelLocationParms);
    }

    void OnIntroEnd() {
        mIsIntroFinish = true;
    }
}
