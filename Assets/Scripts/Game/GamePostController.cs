using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePostController : GameModeController<GamePostController> {
    [Header("Data")]
    public string modalQuiz = "quizClimateZoneMatch";

    protected override void OnInstanceInit() {
        base.OnInstanceInit();
    }

    protected override IEnumerator Start() {
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
        yield return new WaitForSeconds(2f);

        GameData.instance.Progress();
    }
}
