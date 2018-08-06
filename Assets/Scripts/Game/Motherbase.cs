using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motherbase : MonoBehaviour {
    public const string poolGroupRef = "flowerUnitPool";
    
    public enum State {
        None,
        Entering,
        SpawnUnit
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
    public float flowerCycleSpawnDelay = 0.3f;
    
    [Header("Spawn Info")]
    public float spawnUnitHeightOffsetMin = 2.0f;
    public float spawnUnitHeightOffsetMax = 3.0f;
    public float spawnUnitDelay = 0.35f;
    public Vector2 spawnStart;
    public Rect spawnAreaLeft; //for flowers
    public Rect spawnAreaRight; //for flowers
    public float spawnAreaSectionRandScale = 0.25f; //for flowers
    public Rect spawnAreaCenter; //for units

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeEnter;
    public string takeSpawnUnit;
        
    public State state { get { return mState; } }

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

    public UnitAllyFlower GetFlowerLowestGrowth() {
        UnitAllyFlower retFlower = null;
        float lowestGrowth = float.MaxValue;

        for(int i = 0; i < mFlowers.Count; i++) {
            var flower = mFlowers[i];
            if(flower.growth < lowestGrowth) {
                retFlower = flower;
                lowestGrowth = flower.growth;
            }
        }

        return retFlower;
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

        AddSpawn(flower, spawnStart + (Vector2)transform.position, spawnDest);
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

        AddSpawn(unit, spawnStart + (Vector2)transform.position, spawnDest);
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
            flowerSpawnLeftXs[i] = worldPos.x + spawnAreaLeft.xMin + (flowerRegionWidthLeft * i) + flowerRegionWidthLeft*0.5f + leftOfs;

            var rightOfs = Random.Range(-flowerRegionWidthRight * spawnAreaSectionRandScale, flowerRegionWidthRight * spawnAreaSectionRandScale);
            flowerSpawnRightXs[i] = worldPos.x + spawnAreaRight.xMin + (flowerRegionWidthRight * i) + flowerRegionWidthRight*0.5f + rightOfs;
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
    }
            
    private void StopCurrentRout() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    private void AddSpawn(Unit unit, Vector2 start, Vector2 end) {
        mUnitsToSpawn.Enqueue(new SpawnInfo() { unit=unit, start=start, end=end });

        if(mState != State.SpawnUnit) {
            StopCurrentRout();
            mRout = StartCoroutine(DoSpawning());
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

    IEnumerator DoSpawning() {
        mState = State.SpawnUnit;

        while(mUnitsToSpawn.Count > 0) {
            var unitSpawnInfo = mUnitsToSpawn.Dequeue();

            if(animator && !string.IsNullOrEmpty(takeSpawnUnit)) {
                animator.Play(takeSpawnUnit);
                while(animator.isPlaying)
                    yield return null;
            }

            StartCoroutine(DoUnitSpawn(unitSpawnInfo.unit, unitSpawnInfo.start, unitSpawnInfo.end));
        }

        mState = State.None;
        mRout = null;
    }

    IEnumerator DoUnitSpawn(Unit unit, Vector2 start, Vector2 end) {
        unit.state = UnitStates.instance.spawning;

        var unitTrans = unit.transform;

        var topY = Mathf.Max(start.y, end.y);
        var midPoint = new Vector2(Mathf.Lerp(start.x, end.x, 0.5f), topY + Random.Range(spawnUnitHeightOffsetMin, spawnUnitHeightOffsetMax));

        unitTrans.position = start;

        float curTime = 0f;
        while(curTime < spawnUnitDelay) {
            yield return null;

            curTime += Time.deltaTime;

            float t = Mathf.Clamp01(curTime / spawnUnitDelay);

            unitTrans.position = M8.MathUtil.Bezier(start, midPoint, end, t);            
        }

        unit.MotherbaseSpawnFinish();
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
