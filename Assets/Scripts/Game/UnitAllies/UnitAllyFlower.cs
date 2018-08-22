using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public class UnitAllyFlower : Unit {
    [System.Serializable]
    public struct FlowerInfo {
        public Sprite sprite;
        public AnimationCurve scaleCurve;
    }

    [Header("Growth")]
    public float growthCycleStemCount = 1; //number of stems expected to fully grow by the end of cycle with no modifications, determines rate.
    public float growthStemValue; //the value representing a full stem growth
    public BoxCollider2D growthCollider; //match height to stem

    [Header("Flower")]
    public FlowerInfo[] flowerLevels;

    [Header("Leaves")]
    public Sprite[] leafSprites;
    public int leafCount = 4;

    [Header("Sway")]
    public float swayAngleMin;
    public float swayAngleMax;
    public float swayDelayMin;
    public float swayDelayMax;
    public float swayWaitDelayMin;
    public float swayWaitDelayMax;

    [Header("Display")]
    public Transform stemRoot;
    public Transform topRoot; //root for bud and blossom
    public GameObject budGO;
    public GameObject blossomGO;
    public SpriteRenderer blossomDisplay;

    [Header("Stem")]
    public GameObject stemTemplate;
    public int stemMaxCount = 3;

    [Header("Power To Motherbase")]
    public Transform powerExtractRoot;
    public float powerExtractTopOfs = 1.5f; //offset relative to topmost stem
    public float powerExtractCurveTopOfs = 3.0f; //offset relative to top extract point for midpoint curve path
    public float powerExtractMoveUpDelay = 0.3f;
    public float powerExtractMoveWait = 1f;
    public float powerExtractMoveDelay = 0.5f;

    public bool isBlossomed { get { return blossomGO.activeSelf; } }
    public float growth { get { return mGrowth; } }
    public float growthMax { get { return mGrowthMax; } }

    public Vector2 topPosition {
        get {
            var curStem = mStems[mCurStemIndex];
            return curStem.topWorldPosition;
        }
    }

    public bool allowFlowerBlossomGrowth { get; set; }

    private FlowerStem[] mStems;
    private int mCurStemIndex;

    private Coroutine mGrowRout;
    private float mGrowthRate; //growth/second
    private float mGrowth;
    private float mGrowthMax;
    private float mStemGrowth;
    private Dictionary<string, float> mGrowthMods = new Dictionary<string, float>();

    private Coroutine mSwayRout;
    private Quaternion[] mSwayStemRotStarts;

    public void ApplyGrowth(float growthDelta) {
        if(growthDelta == 0f)
            return;

        if(!allowFlowerBlossomGrowth && isBlossomed && growthDelta > 0f) //don't allow growth for blossomed flower
            return;

        var curStem = mStems[mCurStemIndex];

        mGrowth = Mathf.Clamp(mGrowth + growthDelta, 0f, mGrowthMax);

        mStemGrowth += growthDelta;

        float stemVal = Mathf.Clamp(mStemGrowth / growthStemValue, 0f, curStem.maxGrowth);

        curStem.growth = stemVal;

        var lastStemIndex = mCurStemIndex;

        if(mStemGrowth >= growthStemValue) {
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
            curStem.gameObject.SetActive(false);

            if(mCurStemIndex > 0) {
                mStemGrowth += growthStemValue;

                mCurStemIndex--;

                curStem = mStems[mCurStemIndex];

                curStem.growth = mStemGrowth / growthStemValue;
            }
            else {
                mStemGrowth = 0f;

                curStem = mStems[mCurStemIndex];
                curStem.growth = 0f;
            }
        }

        if(mCurStemIndex != lastStemIndex) {
            topRoot.SetParent(curStem.transform);
        }

        topRoot.localPosition = curStem.topLocalPosition;

        if(growthCollider) {
            var topPos = curStem.topWorldPosition;

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

    public bool ContainsGrowthMod(string id) {
        return mGrowthMods.ContainsKey(id);
    }

    public float GetGrowthMod(string id) {
        if(mGrowthMods != null && mGrowthMods.ContainsKey(id))
            return mGrowthMods[id];

        return 0f;
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

    public void SetBlossom(bool blossom) {
        if(isBlossomed == blossom)
            return;

        if(blossom) {
            budGO.SetActive(false);
            blossomGO.SetActive(true);

            ApplyBlossomValue();
        }
        else {
            budGO.SetActive(true);
            blossomGO.SetActive(false);
        }
    }

    public void PowerExtractDisplay(Vector2 destination, System.Action endCallback) {
        StartCoroutine(DoPowerExtract(destination, endCallback));
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(prevState == UnitStates.instance.grow) {
            StopGrowRoutine();
        }

        if(state == UnitStates.instance.grow) {
            isPhysicsActive = true;

            SetSwayActive(true);

            //start growth
            mGrowRout = StartCoroutine(DoGrow());
        }
        else if(state == UnitStates.instance.despawning) {
            SetSwayActive(false);
        }
        else if(state == UnitStates.instance.blowOff) {
            SetSwayActive(false);
        }
    }

    protected override void OnSpawned(GenericParams parms) {
        base.OnSpawned(parms);
                
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

        SetSwayActive(false);

        budGO.SetActive(false);
        blossomGO.SetActive(false);

        if(powerExtractRoot) powerExtractRoot.gameObject.SetActive(false);

        mGrowthMods.Clear();

        mGrowth = 0f;
        mStemGrowth = 0f;

        allowFlowerBlossomGrowth = false;

        for(int i = 0; i < stemMaxCount; i++) {
            mStems[i].growth = 0f;
            mStems[i].gameObject.SetActive(false);
        }
    }

    protected override void Awake() {
        base.Awake();

        mGrowthMax = growthStemValue * stemMaxCount;

        //initialize stems
        mStems = new FlowerStem[stemMaxCount];
        mSwayStemRotStarts = new Quaternion[stemMaxCount];

        var curStemRoot = stemRoot;
        var curStemPos = Vector3.zero;

        var leafSprite = leafSprites[Random.Range(0, leafSprites.Length)];
        bool leafFlip = Random.Range(0, 2) == 0;

        for(int i = 0; i < stemMaxCount; i++) {
            var newGO = Instantiate(stemTemplate, curStemRoot);
            
            mStems[i] = newGO.GetComponent<FlowerStem>();

            mStems[i].Init(leafSprite, leafCount, leafFlip, i < stemMaxCount - 1);

            newGO.transform.localPosition = curStemPos;
            newGO.SetActive(false);
                        
            curStemRoot = newGO.transform;
            curStemPos = mStems[i].topLocalMaxPosition;

            if(leafCount % 2 != 0)
                leafFlip = !leafFlip;
        }

        stemTemplate.SetActive(false);

        budGO.SetActive(false);
        blossomGO.SetActive(false);

        topRoot.SetParent(mStems[0].transform);
        topRoot.localPosition = mStems[0].topLocalPosition;

        if(powerExtractRoot) powerExtractRoot.gameObject.SetActive(false);

        mCurStemIndex = 0;
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
        
    private void ApplyBlossomValue() {
        float t = mGrowth / mGrowthMax;

        float fIndex = flowerLevels.Length * t;
        int index = Mathf.FloorToInt(flowerLevels.Length * t);
        if(index >= flowerLevels.Length)
            index = flowerLevels.Length - 1;

        var flowerInfo = flowerLevels[index];

        //float scaleUnit = mGrowthMax / flowerLevels.Length;
        float scaleT = fIndex - index;
        float scaleVal = flowerInfo.scaleCurve.Evaluate(scaleT);

        blossomDisplay.sprite = flowerInfo.sprite;
        blossomDisplay.transform.localScale = new Vector3(scaleVal, scaleVal, 1.0f);
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

        state = UnitStates.instance.idle;

        //apply flower blossom
        SetBlossom(true);
    }

    private void SetSwayActive(bool active) {
        if(active) {
            mSwayRout = StartCoroutine(DoSway());
        }
        else if(mSwayRout != null) {
            StopCoroutine(mSwayRout);
            mSwayRout = null;
        }
    }

    IEnumerator DoSway() {
        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InOutSine);
        float angleSign = Random.Range(0, 2) == 0 ? 1f : -1f;

        while(true) {
            //initialize stem rots
            for(int i = 0; i < stemMaxCount; i++)
                mSwayStemRotStarts[i] = mStems[i].transform.localRotation;

            var targetRot = Quaternion.Euler(0f, 0f, angleSign * Random.Range(swayAngleMin, swayAngleMax));

            float curTime = 0f;
            float delay = Random.Range(swayDelayMin, swayDelayMax);
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                float t = easeFunc(curTime, delay, 0f, 0f);

                for(int i = 0; i <= mCurStemIndex; i++) {
                    var stemT = mStems[i].transform;
                    stemT.localRotation = Quaternion.Lerp(mSwayStemRotStarts[i], targetRot, t);
                }
            }

            yield return new WaitForSeconds(Random.Range(swayWaitDelayMin, swayWaitDelayMax));

            angleSign *= -1f;

            yield return null;
        }
    }

    IEnumerator DoPowerExtract(Vector2 destination, System.Action endCallback) {
        powerExtractRoot.gameObject.SetActive(true);

        //move up
        Vector2 startPos = topRoot.position;
        Vector2 endPos = new Vector2(startPos.x, startPos.y + powerExtractTopOfs);

        powerExtractRoot.position = startPos;

        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);

        float curTime = 0f;
        while(curTime < powerExtractMoveUpDelay) {
            yield return null;

            curTime += Time.deltaTime;
            float t = easeFunc(curTime, powerExtractMoveUpDelay, 0f, 0f);
            powerExtractRoot.position = Vector2.Lerp(startPos, endPos, t);
        }

        yield return new WaitForSeconds(powerExtractMoveWait);

        startPos = endPos;
        endPos = destination;

        Vector2 midPos = new Vector2(Mathf.Lerp(startPos.x, endPos.x, 0.5f), startPos.y + powerExtractCurveTopOfs);

        easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InOutSine);

        curTime = 0f;
        while(curTime < powerExtractMoveDelay) {
            yield return null;

            curTime += Time.deltaTime;
            float t = easeFunc(curTime, powerExtractMoveDelay, 0f, 0f);
            powerExtractRoot.position = M8.MathUtil.Bezier(startPos, midPos, endPos, t);
        }

        powerExtractRoot.gameObject.SetActive(false);

        if(endCallback != null)
            endCallback();
    }
}
