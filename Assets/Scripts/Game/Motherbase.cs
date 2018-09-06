using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Motherbase : MonoBehaviour {
    public const string poolGroupRef = "flowerUnitPool";
    
    public enum State {
        None,
        Entering,
        SpawnUnit,
        Victory
    }

    public struct SpawnInfo {
        public Unit unit;
        public Vector2 start;
        public Vector2 end;
    }

    [Header("Flower Info")]
    public GameObject flowerPrefab;
    public int flowerCapacity = 12;
    public int flowerCycleSpawnCount = 4;
    
    [Header("Spawn Info")]
    public float spawnUnitHeightOffsetMin = 2.0f;
    public float spawnUnitHeightOffsetMax = 3.0f;
    public float spawnUnitDelay = 0.35f; //delay from motherbase to ground
    public float spawnUnitDisplayDelay = 2f; //display purpose, spawner shown until delay and play takeSpawnUnitExit
    public Vector2 spawnStart;
    public Rect spawnAreaLeft; //for flowers
    public Rect spawnAreaRight; //for flowers
    public float spawnAreaSectionRandScale = 0.25f; //for flowers
    public Rect spawnAreaCenter; //for units

    [Header("Grow")]
    public GameObject[] growStems;
    public float growStemDelay = 0.5f;
    public float growPowerExtractDelay = 0.3f;
    public SpriteRenderer[] growBlossomDisplays;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeEnter;
    public string takeSpawnUnitEnter;
    public string takeSpawnUnitExit;
    public string takeSpawnUnit;
    public string takeVictory;
                
    public State state { get { return mState; } }

    public float flowerTotalGrowth {
        get {
            float growth = 0f;
            for(int i = 0; i < mFlowers.Count; i++)
                growth += mFlowers[i].growth;
            return growth;
        }
    }

    public float flowerTotalGrowthMax {
        get {
            if(mFlowers.Count > 0)
                return mFlowers[0].growthMax * mFlowers.Count;
            
            return 0f;
        }
    }

    private State mState = State.None;
    private Coroutine mRout;

    private M8.PoolController mFlowerPool;
    private M8.CacheList<UnitAllyFlower> mFlowers; //active flowers
    private M8.CacheList<UnitAllyFlower> mFlowerQuery; //used for grabbing flowers based on filter

    private Queue<SpawnInfo> mUnitsToSpawn = new Queue<SpawnInfo>();

    private Vector2[] mSpawnPointLeft;
    private Vector2[] mSpawnPointRight;

    private int mSpawnPointLeftCounter;
    private int mSpawnPointRightCounter;

    private bool mIsFlowerSpawnLeft;
    private int mFlowerSpawnLeftCounter;
    private int mFlowerSpawnRightCounter;

    public void ShakeCamera(float duration, float strength, int vibrato) {
        var gameCam = M8.Camera2D.main;

        gameCam.unityCamera.DOShakePosition(duration, strength, vibrato).SetAutoKill(true).Play();
    }

    /// <summary>
    /// Returns true if there are any active flowers spawning
    /// </summary>
    public bool CheckFlowersSpawning() {
        for(int i = 0; i < mFlowers.Count; i++) {
            var flower = mFlowers[i];
            if(flower.state == UnitStates.instance.spawning)
                return true;
        }

        return false;
    }

    public UnitAllyFlower GetRandomFlower(bool checkMarked) {
        if(checkMarked) {
            mFlowerQuery.Clear();

            for(int i = 0; i < mFlowers.Count; i++) {
                var flower = mFlowers[i];
                if(checkMarked && flower.isMarked)
                    continue;

                mFlowerQuery.Add(flower);
            }

            return mFlowerQuery[Random.Range(0, mFlowerQuery.Count)];
        }
        else {
            return mFlowers[Random.Range(0, mFlowers.Count)];
        }
    }

    public UnitAllyFlower GetNearestFlower(float x, bool checkMarked) {
        UnitAllyFlower flower = null;
        float nearestDist = float.MaxValue;

        for(int i = 0; i < mFlowers.Count; i++) {
            var _flower = mFlowers[i];

            if(checkMarked && _flower.isMarked)
                continue;

            float dist = Mathf.Abs(_flower.position.x - x);
            if(dist < nearestDist) {
                flower = _flower;
                nearestDist = dist;
            }
        }

        return flower;
    }

    public UnitAllyFlower GetNearestFlowerBloomed(float x, bool checkMarked) {
        UnitAllyFlower flower = null;
        float nearestDist = float.MaxValue;

        for(int i = 0; i < mFlowers.Count; i++) {
            var _flower = mFlowers[i];

            if(!_flower.isBlossomed)
                continue;

            if(checkMarked && _flower.isMarked)
                continue;

            float dist = Mathf.Abs(_flower.position.x - x);
            if(dist < nearestDist) {
                flower = _flower;
                nearestDist = dist;
            }
        }

        return flower;
    }

    /// <summary>
    /// note: cachelist is shared
    /// </summary>
    public M8.CacheList<UnitAllyFlower> GetFlowersBudding(bool unmarkExclusive) {
        mFlowerQuery.Clear();

        for(int i = 0; i < mFlowers.Count; i++) {
            var flower = mFlowers[i];
            if(unmarkExclusive && flower.isMarked)
                continue;
            if(flower.isBlossomed)
                continue;

            mFlowerQuery.Add(flower);
        }

        return mFlowerQuery;
    }

    public M8.CacheList<UnitAllyFlower> GetFlowersExcept(UnitAllyFlower excludeFlower, bool unmarkExclusive) {
        mFlowerQuery.Clear();

        for(int i = 0; i < mFlowers.Count; i++) {
            var flower = mFlowers[i];
            if(unmarkExclusive && flower.isMarked)
                continue;
            if(flower == excludeFlower)
                continue;

            mFlowerQuery.Add(flower);
        }

        return mFlowerQuery;
    }

    public UnitAllyFlower GetFlowerLowestGrowth(bool isBlossomed) {
        UnitAllyFlower retFlower = null;
        float lowestGrowth = float.MaxValue;

        for(int i = 0; i < mFlowers.Count; i++) {
            var flower = mFlowers[i];
            if(flower.isBlossomed != isBlossomed)
                continue;

            if(flower.growth < lowestGrowth) {
                retFlower = flower;
                lowestGrowth = flower.growth;
            }
        }

        return retFlower;
    }

    public UnitAllyFlower GetFlowerLowestGrowthNear(float x, bool isBlossomed) {
        UnitAllyFlower retFlower = null;
        float lowestGrowth = float.MaxValue;
        float lowestNearDist = float.MaxValue;

        for(int i = 0; i < mFlowers.Count; i++) {
            var flower = mFlowers[i];
            if(flower.isBlossomed != isBlossomed)
                continue;

            var dist = Mathf.Abs(x - flower.position.x);

            if(flower.growth < lowestGrowth || (flower.growth == lowestGrowth && dist < lowestNearDist)) {
                retFlower = flower;
                lowestGrowth = flower.growth;
                lowestNearDist = dist;
            }
        }

        return retFlower;
    }

    /// <summary>
    /// Check if given flower is the lowest growth from the other flowers
    /// </summary>
    public bool IsFlowerLowestGrowth(UnitAllyFlower flower) {
        for(int i = 0; i < mFlowers.Count; i++) {
            if(mFlowers[i] != flower && mFlowers[i].growth < flower.growth)
                return false;
        }

        return true;
    }

    public void Enter() {
        if(mState == State.Entering)
            return;

        StopCurrentRout();

        mRout = StartCoroutine(DoEnter());
    }

    public void SpawnFlower() {
        //grab point
        Vector2 spawnDest;

        if(mIsFlowerSpawnLeft) {
            spawnDest = mSpawnPointLeft[mFlowerSpawnLeftCounter];
            mFlowerSpawnLeftCounter = (mFlowerSpawnLeftCounter + 1) % mSpawnPointLeft.Length;
        }
        else {
            spawnDest = mSpawnPointRight[mFlowerSpawnRightCounter];
            mFlowerSpawnRightCounter = (mFlowerSpawnRightCounter + 1) % mSpawnPointRight.Length;
        }

        mIsFlowerSpawnLeft = !mIsFlowerSpawnLeft;

        //spawn flower
        var flower = mFlowerPool.Spawn<UnitAllyFlower>(flowerPrefab.name, "", null, null);
        flower.releaseCallback += OnFlowerRelease;
        mFlowers.Add(flower);

        AddSpawn(flower, spawnStart + (Vector2)transform.position, spawnDest, false);
    }

    /// <summary>
    /// Add unit to spawn queue to initiate spawning for unit. Given point is usu. the unit's destination (via card drag to play area)
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="point"></param>
    public void SpawnQueueUnit(Unit unit, Vector2 point) {
        //grab ground position
        var worldPos = transform.position;
        float minX = spawnAreaCenter.xMin + worldPos.x;
        float maxX = spawnAreaCenter.xMax + worldPos.x;

        Vector2 spawnDest = new Vector2(Mathf.Clamp(point.x, minX, maxX), point.y);

        UnitPoint groundPoint;
        if(UnitPoint.GetGroundPoint(spawnDest, out groundPoint))
            spawnDest = groundPoint.position;

        AddSpawn(unit, spawnStart + (Vector2)transform.position, spawnDest, true);
    }

    public void Victory() {
        if(mState != State.Victory) {
            StartCoroutine(DoVictory());
        }
    }

    void Awake() {
        //initialize pool
        mFlowerPool = M8.PoolController.CreatePool(poolGroupRef);
        mFlowerPool.AddType(flowerPrefab, flowerCapacity, flowerCapacity);

        mFlowers = new M8.CacheList<UnitAllyFlower>(flowerCapacity);
        mFlowerQuery = new M8.CacheList<UnitAllyFlower>(flowerCapacity);

        //Generate flower spawn points
        int flowerRegionDivisible = flowerCapacity / 2;

        float flowerRegionWidthLeft = (spawnAreaLeft.width / flowerRegionDivisible);
        float flowerRegionWidthRight = (spawnAreaRight.width / flowerRegionDivisible);

        var flowerSpawnLeftXs = new float[flowerRegionDivisible];
        var flowerSpawnRightXs = new float[flowerRegionDivisible];

        var worldPos = transform.position;

        for(int i = 0; i < flowerRegionDivisible; i++) {
            var leftOfs = Random.Range(-flowerRegionWidthLeft * spawnAreaSectionRandScale, flowerRegionWidthLeft * spawnAreaSectionRandScale);
            flowerSpawnLeftXs[i] = worldPos.x + spawnAreaLeft.xMin + (flowerRegionWidthLeft * i) + flowerRegionWidthLeft * 0.5f + leftOfs;

            var rightOfs = Random.Range(-flowerRegionWidthRight * spawnAreaSectionRandScale, flowerRegionWidthRight * spawnAreaSectionRandScale);
            flowerSpawnRightXs[i] = worldPos.x + spawnAreaRight.xMin + (flowerRegionWidthRight * i) + flowerRegionWidthRight * 0.5f + rightOfs;
        }

        M8.ArrayUtil.Shuffle(flowerSpawnLeftXs);
        M8.ArrayUtil.Shuffle(flowerSpawnRightXs);

        mSpawnPointLeft = new Vector2[flowerRegionDivisible];
        mSpawnPointRight = new Vector2[flowerRegionDivisible];

        var worldSpawnAreaLeft = new Rect(spawnAreaLeft.min + (Vector2)worldPos, spawnAreaLeft.size);
        var worldSpawnAreaRight = new Rect(spawnAreaRight.min + (Vector2)worldPos, spawnAreaRight.size);

        for(int i = 0; i < flowerRegionDivisible; i++) {
            UnitPoint leftGroundPt;
            UnitPoint.GetGroundPoint(new Vector2(flowerSpawnLeftXs[i], worldSpawnAreaLeft.yMax), out leftGroundPt);
            mSpawnPointLeft[i] = leftGroundPt.position;

            UnitPoint rightGroundPt;
            UnitPoint.GetGroundPoint(new Vector2(flowerSpawnRightXs[i], worldSpawnAreaRight.yMax), out rightGroundPt);
            mSpawnPointRight[i] = rightGroundPt.position;
        }

        if(animator && !string.IsNullOrEmpty(takeEnter)) {
            animator.ResetTake(takeEnter);
        }
    }
            
    private void StopCurrentRout() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    private void AddSpawn(Unit unit, Vector2 start, Vector2 end, bool isSpawnShowDelay) {
        mUnitsToSpawn.Enqueue(new SpawnInfo() { unit=unit, start=start, end=end });

        if(mState != State.SpawnUnit) {
            StopCurrentRout();
            mRout = StartCoroutine(DoSpawning(isSpawnShowDelay));
        }
    }

    IEnumerator DoEnter() {
        mState = State.Entering;

        if(animator && !string.IsNullOrEmpty(takeEnter)) {
            animator.Play(takeEnter);
            while(animator.isPlaying)
                yield return null;
        }

        mState = State.None;
        mRout = null;
    }

    IEnumerator DoSpawning(bool isSpawnShowDelay) {
        mState = State.SpawnUnit;

        bool doShow = true;

        while(true) {
            //show spawner display
            if(doShow && animator && !string.IsNullOrEmpty(takeSpawnUnitEnter)) {
                animator.Play(takeSpawnUnitEnter);
                while(animator.isPlaying)
                    yield return null;

                doShow = false;
            }

            while(mUnitsToSpawn.Count > 0) {
                var unitSpawnInfo = mUnitsToSpawn.Dequeue();

                if(animator && !string.IsNullOrEmpty(takeSpawnUnit)) {
                    animator.Play(takeSpawnUnit);
                    while(animator.isPlaying)
                        yield return null;
                }

                StartCoroutine(DoUnitSpawn(unitSpawnInfo.unit, unitSpawnInfo.start, unitSpawnInfo.end));
            }

            //delay before exiting spawner display
            if(isSpawnShowDelay) {
                float lastTime = Time.time;
                while(Time.time - lastTime < spawnUnitDisplayDelay && mUnitsToSpawn.Count == 0)
                    yield return null;
            }

            if(mUnitsToSpawn.Count == 0) {
                //hide spawner display
                if(animator && !string.IsNullOrEmpty(takeSpawnUnitExit)) {
                    animator.Play(takeSpawnUnitExit);
                    while(animator.isPlaying)
                        yield return null;

                    if(mUnitsToSpawn.Count > 0) {
                        //got more units to spawn, show again and continue spawning
                        doShow = true;
                        continue;
                    }
                }

                break;
            }
        }

        mState = State.None;
        mRout = null;
    }

    IEnumerator DoUnitSpawn(Unit unit, Vector2 start, Vector2 end) {
        var unitTrans = unit.transform;

        var topY = Mathf.Max(start.y, end.y);
        var midPoint = new Vector2(Mathf.Lerp(start.x, end.x, 0.5f), topY + Random.Range(spawnUnitHeightOffsetMin, spawnUnitHeightOffsetMax));

        unitTrans.position = start;

        unit.state = UnitStates.instance.spawning;

        float curTime = 0f;
        while(curTime < spawnUnitDelay) {
            yield return null;

            //fail-safe, unit has been released, cancel spawning
            if(!unit || unit.isReleased) {
                yield break;
            }

            curTime += Time.deltaTime;

            float t = Mathf.Clamp01(curTime / spawnUnitDelay);

            unitTrans.position = M8.MathUtil.Bezier(start, midPoint, end, t);            
        }

        unit.MotherbaseSpawnFinish();
    }

    IEnumerator DoVictory() {
        mState = State.Victory;

        //wait for other rout to finish
        while(mRout != null)
            yield return null;

        yield return new WaitForSeconds(0.5f);

        //play a little victory
        if(animator && !string.IsNullOrEmpty(takeVictory)) {
            animator.Play(takeVictory);
            while(animator.isPlaying)
                yield return null;
        }

        //extract power from active flowers
        int powerExtractFinishCount = 0;

        var dest = spawnStart + (Vector2)transform.position;

        //start power extract, apply flower value
        for(int i = 0; i < mFlowers.Count; i++) {
            mFlowers[i].PowerExtractDisplay(dest, () => powerExtractFinishCount++);

            if(i < growBlossomDisplays.Length - 1) {
                growBlossomDisplays[i].sprite = mFlowers[i].blossomDisplay.sprite;
                growBlossomDisplays[i].transform.localScale = mFlowers[i].blossomDisplay.transform.localScale;
            }

            yield return new WaitForSeconds(growPowerExtractDelay); //wait a bit to cascade extraction
        }

        while(powerExtractFinishCount < mFlowers.Count)
            yield return null;
                
        var waitStem = new WaitForSeconds(growStemDelay);
                
        int blossomIndex = 0;
        int blossomCount = Mathf.Min(mFlowers.Count, growBlossomDisplays.Length);

        int stemCount = Mathf.Min(Mathf.CeilToInt(mFlowers.Count * 0.5f), growStems.Length);
        for(int i = 0; i < stemCount; i++) {
            growStems[i].SetActive(true);

            yield return waitStem;

            //show blossoms
            if(blossomIndex < blossomCount) {
                for(int j = 0; j < 2; j++) {
                    growBlossomDisplays[blossomIndex].gameObject.SetActive(true);

                    blossomIndex++;
                    if(blossomIndex >= blossomCount)
                        break;
                }
            }
        }

        mState = State.None;
        mRout = null;
    }

    void OnFlowerRelease(M8.EntityBase ent) {
        ent.releaseCallback -= OnFlowerRelease;
        mFlowers.Remove((UnitAllyFlower)ent);
    }
    
    void OnDrawGizmos() {
        var worldPos = transform.position;

        Gizmos.color = new Color(0.75f, 0.75f, 0f, 0.8f);
        Gizmos.DrawSphere((Vector3)spawnStart + worldPos, 0.35f);

        Gizmos.color = new Color(0.75f, 0.75f, 0f, 0.8f);
        Gizmos.DrawWireCube((Vector3)spawnAreaLeft.center + worldPos, spawnAreaLeft.size);
        Gizmos.DrawWireCube((Vector3)spawnAreaRight.center + worldPos, spawnAreaRight.size);

        Gizmos.color = new Color(1.00f, 0.75f, 0f, 1f);
        Gizmos.DrawWireCube((Vector3)spawnAreaCenter.center + worldPos, spawnAreaCenter.size);
    }
}
