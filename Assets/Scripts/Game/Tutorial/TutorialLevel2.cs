using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevel2 : MonoBehaviour {
    [Header("Intro")]
    public AnimatorEnterExit introClimateIllustration;
    public LoLExt.ModalDialogController introClimateDialog;
    public AnimatorEnterExit introMicroClimateIllustration;
    public LoLExt.ModalDialogController introMicroClimateDialog;

    [Header("Enemy Intro")]
    public LoLExt.ModalDialogController introMushroomDialog01;
    public AnimatorEnterExit introMushroomIronFrogCard;
    public LoLExt.ModalDialogController introMushroomDialog02;

    [Header("Unit Templates")]
    public GameObject mushroomPrefab;

    [Header("Signal")]
    public SignalUnit signalCycleSpawnerSpawned;

    private Unit mCycleUnitSpawned;

    void OnDestroy() {
        if(signalCycleSpawnerSpawned) signalCycleSpawnerSpawned.callback -= OnCycleSpawn;

        if(GameController.isInstantiated) {
            GameController.instance.prepareCycleCallback -= OnPrepareCycle;
            GameController.instance.weatherCycle.weatherBeginCallback -= OnWeatherBegin;
        }
    }

    void Awake() {
        if(signalCycleSpawnerSpawned) signalCycleSpawnerSpawned.callback += OnCycleSpawn;

        GameController.instance.prepareCycleCallback += OnPrepareCycle;
        GameController.instance.weatherCycle.weatherBeginCallback += OnWeatherBegin;
    }

    void OnPrepareCycle() {
        //show intro
        if(GameController.instance.weatherCycle.curCycleIndex == 0)
            StartCoroutine(DoIntro());
    }

    void OnWeatherBegin() {
        var curCycleInd = GameController.instance.weatherCycle.curCycleIndex;
        var curWeatherInd = GameController.instance.weatherCycle.curWeatherIndex;

        StopAllCoroutines();

        mCycleUnitSpawned = null;

        if(curCycleInd == 0) {
            if(curWeatherInd == 1) {
                StartCoroutine(DoMushroom());
            }
        }
    }

    void OnCycleSpawn(Unit unit) {
        mCycleUnitSpawned = unit;
    }

    IEnumerator DoIntro() {
        GameController.instance.pause = true;

        yield return new WaitForSeconds(0.5f);

        if(introClimateIllustration) {
            introClimateIllustration.gameObject.SetActive(true);
            yield return introClimateIllustration.PlayEnterWait();
        }

        if(introClimateDialog) {
            introClimateDialog.Play();
            while(introClimateDialog.isPlaying)
                yield return null;
        }

        if(introClimateIllustration) {
            yield return introClimateIllustration.PlayExitWait();
            introClimateIllustration.gameObject.SetActive(false);
        }

        if(introMicroClimateIllustration) {
            introMicroClimateIllustration.gameObject.SetActive(true);
            yield return introMicroClimateIllustration.PlayEnterWait();
        }

        if(introMicroClimateDialog) {
            introMicroClimateDialog.Play();
            while(introMicroClimateDialog.isPlaying)
                yield return null;
        }

        if(introMicroClimateIllustration) {
            yield return introMicroClimateIllustration.PlayExitWait();
            introMicroClimateIllustration.gameObject.SetActive(false);
        }


        GameController.instance.pause = false;
    }

    IEnumerator DoMushroom() {
        //wait for weed to spawn
        while(!mCycleUnitSpawned || mCycleUnitSpawned.spawnType != mushroomPrefab.name)
            yield return null;

        yield return new WaitForSeconds(1f); //wait a bit

        M8.SceneManager.instance.Pause();

        //show dialog
        if(introMushroomDialog01) {
            introMushroomDialog01.Play();
            while(introMushroomDialog01.isPlaying)
                yield return null;
        }

        if(introMushroomIronFrogCard) {
            introMushroomIronFrogCard.gameObject.SetActive(true);
            yield return introMushroomIronFrogCard.PlayEnterWait();
        }

        if(introMushroomDialog02) {
            introMushroomDialog02.Play();
            while(introMushroomDialog02.isPlaying)
                yield return null;
        }

        if(introMushroomIronFrogCard) {            
            yield return introMushroomIronFrogCard.PlayExitWait();
            introMushroomIronFrogCard.gameObject.SetActive(false);
        }

        M8.SceneManager.instance.Resume();
    }
}
