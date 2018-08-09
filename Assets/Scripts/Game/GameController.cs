﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : GameModeController<GameController> {
    [Header("Controls")]
    public CardDeckController cardDeck;
    public WeatherCycleController weatherCycle;

    public Motherbase motherbase;

    public CardDeployReticle[] cardDeployReticles;

    [Header("Weather Forecast")]
    public string modalWeatherForecast = "weatherForecast";

    [Header("Level Info")]
    public GameBounds2D levelBounds;

    /// <summary>
    /// Called before current cycle starts (initialize info here)
    /// </summary>
    public event System.Action prepareCycleCallback;

    /// <summary>
    /// Called before cycle starts (play animation here)
    /// </summary>
    public event System.Action startCycleCallback;

    /// <summary>
    /// Called after all cycles have finished
    /// </summary>
    public event System.Action endCallback;

    private M8.GenericParams mWeatherForecastParms = new M8.GenericParams();
    
    public CardDeployReticle GetCardDeployReticle(string reticleName) {
        for(int i = 0; i < cardDeployReticles.Length; i++) {
            if(cardDeployReticles[i].name == reticleName)
                return cardDeployReticles[i];
        }

        return null;
    }

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        if(weatherCycle)
            weatherCycle.cycleEndCallback -= OnCycleEnd;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        //hide deploy reticles
        for(int i = 0; i < cardDeployReticles.Length; i++) {
            if(cardDeployReticles[i])
                cardDeployReticles[i].gameObject.SetActive(false);
        }

        weatherCycle.cycleEndCallback += OnCycleEnd;
    }

    protected override IEnumerator Start() {
        yield return base.Start();
                
        motherbase.Enter();
        while(motherbase.state == Motherbase.State.Entering)
            yield return null;

        //start cycle
        StartCoroutine(DoNewCycle());
    }

    IEnumerator DoNewCycle() {
        //prep display (hud, etc.)
        if(prepareCycleCallback != null)
            prepareCycleCallback();

        //open weather forecast
        mWeatherForecastParms[ModalWeatherForecast.parmWeatherCycleIndex] = weatherCycle.curCycleIndex;
        mWeatherForecastParms[ModalWeatherForecast.parmWeatherCycle] = weatherCycle.curCycleData;

        var uiMgr = M8.UIModal.Manager.instance;

        uiMgr.ModalOpen(modalWeatherForecast, mWeatherForecastParms);

        //wait for modals closed
        while(uiMgr.isBusy || uiMgr.activeCount > 0)
            yield return null;
        //

        if(startCycleCallback != null)
            startCycleCallback();

        yield return new WaitForSeconds(GameData.instance.levelCycleStartDelay);
                
        //spawn flowers from mother base
        for(int i = 0; i < motherbase.flowerCycleSpawnCount; i++)
            motherbase.SpawnFlower();

        while(motherbase.state == Motherbase.State.SpawnUnit || motherbase.CheckFlowersSpawning())
            yield return null;

        weatherCycle.StartCurCycle();
    }

    IEnumerator DoEndCycle() {
        yield return new WaitForSeconds(GameData.instance.levelCycleEndDelay);

        if(weatherCycle.NextCycle()) {
            StartCoroutine(DoNewCycle());
        }
        else {
            //end
            if(endCallback != null)
                endCallback();
        }
    }

    void OnCycleEnd() {
        StartCoroutine(DoEndCycle());
    }
}
