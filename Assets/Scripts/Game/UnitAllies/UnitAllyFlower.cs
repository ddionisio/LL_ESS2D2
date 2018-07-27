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

    public float growth { get { return mGrowth; } }

    private FlowerStem[] mStems;
    private int mCurStemIndex;

    private Coroutine mGrowRout;
    private float mGrowthRate; //growth/second
    private float mGrowth;
    private float mGrowthMax;
    private Dictionary<string, float> mGrowthMods = new Dictionary<string, float>();

    public void ApplyGrowth(float growthDelta) {
        var curStem = mStems[mCurStemIndex];
                
        float stemGrowth = mGrowth % growthStemValue;

        mGrowth = Mathf.Clamp(mGrowth + growthDelta, 0f, mGrowthMax);

        stemGrowth += growthDelta;

        float stemVal = Mathf.Clamp(stemGrowth / growthStemValue, 0f, curStem.maxGrowth);

        curStem.growth = stemVal;

        if(stemGrowth >= growthStemValue) {
            stemGrowth = stemGrowth - growthStemValue;

            if(mCurStemIndex < mStems.Length - 1) {
                curStem.ShowLeaves();

                mCurStemIndex++;
                                
                curStem = mStems[mCurStemIndex];
                curStem.gameObject.SetActive(true);
            }
        }
        else if(stemGrowth < 0f) {
            curStem.HideLeaves();
            curStem.gameObject.SetActive(false);

            if(mCurStemIndex > 0) {
                mCurStemIndex--;

                curStem = mStems[mCurStemIndex];
                curStem.HideLeaves();

                curStem.growth = Mathf.Clamp((growthStemValue - stemGrowth) / growthStemValue, 0f, curStem.maxGrowth);
            }
        }

        budGO.transform.position = curStem.topWorldPosition;
    }

    public void ApplyGrowthMod(string id, float mod) {
        if(mGrowthMods.ContainsKey(id))
            mGrowthMods[id] = mod;
        else
            mGrowthMods.Add(id, mod);
    }

    public void RemoveGrowthMod(string id) {
        if(mGrowthMods.ContainsKey(id))
            mGrowthMods.Remove(id);
    }

    public float GetGrowthRate(float mod) {
        return mGrowthRate * mod;
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(prevState == UnitStates.instance.normal) {
            StopGrowRoutine();
        }

        if(state == UnitStates.instance.normal) {
            //start growth
            mGrowRout = StartCoroutine(DoGrow());
        }
    }

    protected override void OnSpawned(GenericParams parms) {
        base.OnSpawned(parms);

        //determine growth rate
        float growthMaxCycle = growthStemValue * growthCycleStemCount;
        mGrowthRate = growthMaxCycle / GameController.instance.weatherCycle.curCycleData.duration;

        GameController.instance.weatherCycle.cycleEndCallback += OnCycleEnd;
    }

    protected override void OnDespawned() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle)
            GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;

        StopGrowRoutine();

        mGrowthMods.Clear();

        mGrowth = 0f;

        for(int i = 0; i < stemMaxCount; i++) {
            mStems[i].growth = 0f;
            mStems[i].HideLeaves();
            mStems[i].gameObject.SetActive(false);
        }
    }

    protected override void Awake() {
        base.Awake();

        mGrowthMax = growthStemValue * stemMaxCount;

        //initialize stems
        mStems = new FlowerStem[stemMaxCount];

        float stemY = 0f;

        for(int i = 0; i < stemMaxCount; i++) {
            var newGO = Instantiate(stemTemplate, stemRoot);
            newGO.transform.localPosition = new Vector3(0f, stemY, 0f);
            mStems[i] = newGO.GetComponent<FlowerStem>();

            newGO.SetActive(false);

            stemY += mStems[i].topOfsY;

            mStems[i].growth = 0f;
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

        //apply global rate from cycle and weather
        var curCycle = GameController.instance.weatherCycle.curCycleData;
        var curWeather = GameController.instance.weatherCycle.curWeather;

        rate += (mGrowthRate * curCycle.flowerGrowthMod) + (mGrowthRate * curWeather.flowerGrowthMod);

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

        float blossomT = Mathf.Clamp01(mGrowth / mGrowthMax);
        float blossomScale = Mathf.Lerp(flowerMinScale, flowerMaxScale, blossomT);

        blossomGO.transform.localScale = new Vector3(blossomScale, blossomScale, 1.0f);
    }

    IEnumerator DoGrow() {
        mCurStemIndex = 0;

        budGO.SetActive(true);

        var curStem = mStems[mCurStemIndex];
        curStem.gameObject.SetActive(true);

        while(mGrowth < mGrowthMax) {
            yield return null;

            float growthRate = GetGrowthRate();
            float growthDelta = growthRate * Time.deltaTime;

            ApplyGrowth(growthDelta);
        }

        mGrowRout = null;
    }

    void OnCycleEnd() {
        GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;
                
        if(state != UnitStates.instance.normal) //only blossom if we are in a normal state
            return;

        if(mGrowRout != null) {
            StopGrowRoutine();

            //apply flower blossom
            Blossom();
        }
    }
}
