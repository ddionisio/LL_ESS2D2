using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePostController : GameModeController<GamePostController> {
    [Header("Data")]
    public string modalQuiz = "quizClimateZoneMatch";

    [Header("Cutscene")]
    public CutsceneController cutscene;

    [Header("Audio")]
    public string musicPath;

    private bool mIsCutsceneFinish;

    protected override void OnInstanceDeinit() {
        if(cutscene)
            cutscene.endCallback -= OnCutsceneEnd;

        base.OnInstanceDeinit();
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        if(cutscene)
            cutscene.endCallback += OnCutsceneEnd;
    }

    protected override IEnumerator Start() {
        if(!string.IsNullOrEmpty(musicPath) && LoLManager.instance.lastSoundBackgroundPath != musicPath)
            LoLManager.instance.PlaySound(musicPath, true, true);

        //wait for scene
        yield return base.Start();

        //check progress to see if we are doing review or post review
        if(GameData.instance.isGameStarted) {
            int index = LoLManager.instance.progressMax - LoLManager.instance.curProgress;
            if(index == 1)
                StartCoroutine(DoPostReview());
            else
                StartCoroutine(DoReview());
        }
        else
            StartCoroutine(DoReview());
    }

    IEnumerator DoReview() {
        //do cutscene
        if(cutscene) {
            mIsCutsceneFinish = false;
            cutscene.Play();
            while(!mIsCutsceneFinish)
                yield return null;
        }

        M8.UIModal.Manager.instance.ModalOpen(modalQuiz);

        //wait for modals to clear
        while(M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.activeCount > 0)
            yield return null;

        LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);

        //post review
        StartCoroutine(DoPostReview());
    }

    IEnumerator DoPostReview() {
        //TODO: something
        //yield return new WaitForSeconds(2f);
        yield return new WaitForSeconds(1f);

        if(GameData.instance.isGameStarted)
            GameData.instance.Progress();
        else
            GameData.instance.endScene.Load(); //for debug purpose
    }

    void OnCutsceneEnd() {
        mIsCutsceneFinish = true;
    }
}
