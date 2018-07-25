using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public class UnitAllyFlower : Unit {
    [Header("Growth")]
    public int growthCycleStemCount = 1; //number of stems expected to fully grow by the end of cycle with no modifications, determines rate.
    public float growthStemValue; //the value representing a full stem growth

    [Header("Flower")]
    public float flowerMinScale;
    public float flowerMaxScale;

    [Header("Display")]
    public Transform stemRoot;

    [Header("Stem")]
    public GameObject stemTemplate;
    public int stemMaxCount = 3;

    private FlowerStem[] mStems;

    private Coroutine mGrowRout;
    private float mGrowthRate; //growth/second
    private Dictionary<int, float> mGrowthMods = new Dictionary<int, float>();

    protected override void StateChanged() {
        base.StateChanged();

        StopGrowRoutine();

        if(state == stateNormal) {
            //start growth
            mGrowRout = StartCoroutine(DoGrow());
        }
    }

    protected override void OnSpawned(GenericParams parms) {
        base.OnSpawned(parms);

        //determine growth rate
        float growthScale = (growthStemValue * stemMaxCount) / growthCycleStemCount;
        mGrowthRate = growthScale / GameController.instance.weatherCycle.curCycleData.duration;

        GameController.instance.weatherCycle.cycleEndCallback += OnCycleEnd;
    }

    protected override void OnDespawned() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle)
            GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;

        StopGrowRoutine();

        mGrowthMods.Clear();
    }

    protected override void Awake() {
        base.Awake();
                
        //initialize stems
        mStems = new FlowerStem[stemMaxCount];

        for(int i = 0; i < stemMaxCount; i++) {

        }

        stemTemplate.SetActive(false);
    }

    private void StopGrowRoutine() {
        if(mGrowRout != null) {
            StopCoroutine(mGrowRout);
            mGrowRout = null;
        }
    }

    private float GetGrowthRate() {
        float rate = mGrowthRate;

        foreach(var pair in mGrowthMods)
            rate += mGrowthRate*pair.Value;

        if(rate < 0f)
            rate = 0f;

        return rate;
    }

    IEnumerator DoGrow() {
        yield return null;

        mGrowRout = null;

        //apply flower blossom
    }

    void OnCycleEnd() {
        GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;
                
        if(state != stateNormal) //only blossom if we are in a normal state
            return;

        if(mGrowRout != null) {
            StopGrowRoutine();

            //apply flower blossom
        }
    }
}
