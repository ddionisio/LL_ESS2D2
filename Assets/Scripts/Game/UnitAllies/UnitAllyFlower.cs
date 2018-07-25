using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public class UnitAllyFlower : Unit {
    [Header("Growth")]
    public float growthCycleStemCount = 1; //number of stems expected to fully grow by the end of cycle with no modifications, determines rate.
    public float growthStemValue; //the value representing a full stem growth

    [Header("Flower")]
    public float flowerMinScale;
    public float flowerMaxScale;

    [Header("Display")]
    public Transform stemRoot;
    public GameObject budGO;
    public GameObject blossomGO;

    [Header("Stem")]
    public GameObject stemTemplate;
    public int stemMaxCount = 3;

    private FlowerStem[] mStems;
    private int mCurStemIndex;

    private Coroutine mGrowRout;
    private float mGrowthRate; //growth/second
    private float mGrowth;
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

        mGrowth = 0f;
    }

    protected override void Awake() {
        base.Awake();
                
        //initialize stems
        mStems = new FlowerStem[stemMaxCount];

        float stemY = 0f;

        for(int i = 0; i < stemMaxCount; i++) {
            var newGO = Instantiate(stemTemplate, stemRoot);
            newGO.transform.localPosition = new Vector3(0f, stemY, 0f);
            mStems[i] = newGO.GetComponent<FlowerStem>();

            newGO.SetActive(false);

            stemY += mStems[i].topOfsY;
        }

        stemTemplate.SetActive(false);

        budGO.SetActive(false);
        blossomGO.SetActive(false);
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

    private void Blossom() {
        mStems[mCurStemIndex].ShowLeaves(); //show if it hasn't already

        budGO.SetActive(false);

        blossomGO.SetActive(true);
        blossomGO.transform.position = mStems[mCurStemIndex].topWorldPosition;
    }

    IEnumerator DoGrow() {
        mCurStemIndex = 0;

        float maxGrowth = growthStemValue * stemMaxCount;
        float stemGrowth = 0f;

        budGO.SetActive(true);

        var budT = budGO.transform;

        FlowerStem curStem = null;

        while(mGrowth < maxGrowth) {
            if(!curStem) {
                curStem = mStems[mCurStemIndex];
                curStem.gameObject.SetActive(true);

                curStem.growth = 0f;
            }

            budT.position = curStem.topWorldPosition;

            yield return null;

            float growthRate = GetGrowthRate();

            float growthDelta = growthRate * Time.deltaTime;

            mGrowth += growthDelta;

            stemGrowth += growthDelta;

            float stemVal = Mathf.Clamp(stemGrowth / growthStemValue, 0f, curStem.maxGrowth);

            curStem.growth = stemVal;

            if(stemGrowth >= growthStemValue) {
                stemGrowth = stemGrowth - growthStemValue;

                if(mCurStemIndex < mStems.Length - 1)
                    mCurStemIndex++;

                curStem.ShowLeaves();

                curStem = null;
            }
        }

        mGrowRout = null;
    }

    void OnCycleEnd() {
        GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;
                
        if(state != stateNormal) //only blossom if we are in a normal state
            return;

        if(mGrowRout != null) {
            StopGrowRoutine();

            //apply flower blossom
            Blossom();
        }
    }
}
