using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : GameModeController<GameController> {
    [Header("Controls")]
    public CardDeckController cardDeck;
    public WeatherCycleController weatherCycle;

    public Motherbase motherbase;

    public CardDeployReticle[] cardDeployReticles;

    [Header("Level Info")]
    public GameBounds2D levelBounds;
    public LayerMask levelGroundLayerMask;

    [Header("Flower")]
    public int flowerCycleSpawnCount = 4;
    public float flowerCycleSpawnDelay = 0.3f;

    /// <summary>
    /// Called before starting current cycle
    /// </summary>
    public event System.Action prepareCycleCallback;

    /// <summary>
    /// Called after all cycles have finished
    /// </summary>
    public event System.Action endCallback;
    
    public CardDeployReticle GetCardDeployReticle(string reticleName) {
        for(int i = 0; i < cardDeployReticles.Length; i++) {
            if(cardDeployReticles[i].name == reticleName)
                return cardDeployReticles[i];
        }

        return null;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        //hide deploy reticles
        for(int i = 0; i < cardDeployReticles.Length; i++) {
            if(cardDeployReticles[i])
                cardDeployReticles[i].gameObject.SetActive(false);
        }
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
        if(prepareCycleCallback != null)
            prepareCycleCallback();

        yield return new WaitForSeconds(GameData.instance.levelCycleStartDelay);

        weatherCycle.StartCurCycle();

        //spawn flowers from mother base

        yield return new WaitForSeconds(flowerCycleSpawnDelay);

        for(int i = 0; i < flowerCycleSpawnCount; i++)
            motherbase.SpawnFlower();
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
