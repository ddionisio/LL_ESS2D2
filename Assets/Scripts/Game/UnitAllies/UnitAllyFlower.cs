using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public class UnitAllyFlower : Unit {
    [Header("Growth")]
    public float growthCycleStemCount = 1; //number of stems expected to fully grow by the end of cycle with no modifications, determines rate.
    public float growthStemValue; //the value representing a full stem growth
    public BoxCollider2D growthCollider; //match height to stem

    [Header("Flower")]
    public float flowerMinScale;
    public float flowerMaxScale;

    [Header("Display")]
    public Transform stemRoot;
    public Transform topRoot; //root for bud and blossom
    public GameObject budGO;
    public GameObject blossomGO;

    [Header("Stem")]
    public GameObject stemTemplate;
    public int stemMaxCount = 3;

    public bool isBlossomed { get { return blossomGO.activeSelf; } }
    public float growth { get { return mGrowth; } }
    public float growthMax { get { return mGrowthMax; } }

    public Vector2 topPosition {
        get {
            var curStem = mStems[mCurStemIndex];
            return curStem.topWorldPosition;
        }
    }

    private FlowerStem[] mStems;
    private int mCurStemIndex;

    private Coroutine mGrowRout;
    private float mGrowthRate; //growth/second
    private float mGrowth;
    private float mGrowthMax;
    private float mStemGrowth;
    private Dictionary<string, float> mGrowthMods = new Dictionary<string, float>();

    public void ApplyGrowth(float growthDelta) {
        if(isBlossomed && growthDelta > 0f) //don't allow growth for blossomed flower
            return;

        var curStem = mStems[mCurStemIndex];

        mGrowth = Mathf.Clamp(mGrowth + growthDelta, 0f, mGrowthMax);

        mStemGrowth += growthDelta;

        float stemVal = Mathf.Clamp(mStemGrowth / growthStemValue, 0f, curStem.maxGrowth);

        curStem.growth = stemVal;

        if(mStemGrowth >= growthStemValue) {
            curStem.ShowLeaves();

            if(mCurStemIndex < mStems.Length - 1) {
                mCurStemIndex++;

                curStem = mStems[mCurStemIndex];
                curStem.gameObject.SetActive(true);

                mStemGrowth -= growthStemValue;
            }
            else {
                mStemGrowth = growthStemValue;
            }
        }
        else if(mStemGrowth < 0f) {
            curStem.HideLeaves();
            curStem.gameObject.SetActive(false);

            if(mCurStemIndex > 0) {
                mStemGrowth += growthStemValue;

                mCurStemIndex--;

                curStem = mStems[mCurStemIndex];
                curStem.HideLeaves();

                curStem.growth = Mathf.Clamp(mStemGrowth / growthStemValue, 0f, curStem.maxGrowth);
            }
            else {
                mStemGrowth = 0f;

                curStem = mStems[mCurStemIndex];
                curStem.growth = 0f;
            }
        }

        var topPos = curStem.topWorldPosition;

        topRoot.position = topPos;

        if(growthCollider) {
            float deltaHeight = topPos.y - position.y;

            //assume pivot is bottom
            var ofs = growthCollider.offset;
            ofs.y = deltaHeight * 0.5f;

            growthCollider.offset = ofs;

            var size = growthCollider.size;
            size.y = deltaHeight;

            growthCollider.size = size;
        }

        if(isBlossomed)
            ApplyBlossomValue();
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

    public override void MotherbaseSpawnFinish() {
        state = UnitStates.instance.grow;
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(prevState == UnitStates.instance.grow) {
            StopGrowRoutine();
        }

        if(state == UnitStates.instance.grow) {
            isPhysicsActive = true;

            //start growth
            mGrowRout = StartCoroutine(DoGrow());
        }
    }

    protected override void OnSpawned(GenericParams parms) {
        base.OnSpawned(parms);

        budGO.SetActive(false);
        blossomGO.SetActive(false);

        topRoot.localPosition = Vector3.zero;

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
        mStemGrowth = 0f;

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

        ApplyBlossomValue();
    }

    private void ApplyBlossomValue() {
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
        
        StopGrowRoutine();

        //apply flower blossom
        if(!isBlossomed) //only blossom if we haven't
            Blossom();
    }
}
