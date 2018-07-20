﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : GameModeController<GameController> {
    [Header("Controls")]
    public CardDeck cardDeck;
    public WeatherCycle weatherCycle;
    public Motherbase motherbase;

    [Header("Level Info")]
    public GameBounds2D levelBounds;
    public LayerMask levelGroundLayerMask;
    public float levelCycleStartDelay = 1f;

    /// <summary>
    /// Called before starting current cycle
    /// </summary>
    public event System.Action prepareCycleCallback;

    /// <summary>
    /// Called after all cycles have finished
    /// </summary>
    public event System.Action endCallback;
    
    public LevelGroundPosition GetGroundPosition(Vector2 pos) {
        return LevelGroundPosition.FromPointInBounds(levelBounds.rect, pos, levelGroundLayerMask);
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();


    }

    IEnumerator Start() {
        while(M8.SceneManager.instance.isLoading)
            yield return null;

        motherbase.Enter();
        while(motherbase.state == Motherbase.State.Entering)
            yield return null;

        //start cycle
        StartCoroutine(DoNewCycle());
    }

    IEnumerator DoNewCycle() {
        if(prepareCycleCallback != null)
            prepareCycleCallback();

        yield return new WaitForSeconds(levelCycleStartDelay);

        weatherCycle.StartCurCycle();
    }

    void OnWeatherCycleEnd() {
        if(weatherCycle.NextCycle()) {
            StartCoroutine(DoNewCycle());
            return;
        }

        //end
        if(endCallback != null)
            endCallback();
    }
}
