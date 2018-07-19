using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motherbase : MonoBehaviour {
    public const string poolGroupRef = "flowerUnitPool";
    public const int flowerPoolCapacityMultiplier = 3;

    public enum State {
        None,
        Entering,
        SpawnUnit
    }

    [Header("Flower Info")]
    public GameObject flowerPrefab;
    public int flowerSpawnCount;

    [Header("Spawn Info")]
    public float spawnUnitHeightOffsetMin = 2.0f;
    public float spawnUnitHeightOffsetMax = 3.0f;
    public float spawnUnitDelay = 0.35f;
    public Vector2 spawnStart;
    public Rect spawnAreaLeft;
    public Rect spawnAreaRight;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeEnter;
    public string takeSpawnUnit;
        
    public State state { get { return mState; } }

    private State mState = State.None;
    private Coroutine mRout;

    private M8.PoolController mFlowerPool;
    private M8.CacheList<UnitAllyFlower> mFlowers; //active flowers

    private Queue<Unit> mUnitSpawns = new Queue<Unit>();

    void Awake() {
        int flowerCapacity = flowerSpawnCount * flowerPoolCapacityMultiplier;

        //initialize pool
        mFlowerPool = M8.PoolController.CreatePool(poolGroupRef);
        mFlowerPool.AddType(flowerPrefab, flowerCapacity, flowerCapacity);

        mFlowers = new M8.CacheList<UnitAllyFlower>(flowerCapacity);
    }

    public void Enter() {
        if(mState == State.Entering)
            return;

        StopCurrentRout();

        mRout = StartCoroutine(DoEnter());
    }
    
    private void StopCurrentRout() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    IEnumerator DoEnter() {
        mState = State.Entering;

        yield return null;

        mState = State.None;
        mRout = null;
    }

    IEnumerator DoSpawning() {
        mState = State.SpawnUnit;

        while(mUnitSpawns.Count > 0) {
            var unitSpawn = mUnitSpawns.Dequeue();

            if(animator && !string.IsNullOrEmpty(takeSpawnUnit)) {
                animator.Play(takeSpawnUnit);
                while(animator.isPlaying)
                    yield return null;
            }

            //grab spawn path

            //StartCoroutine(DoU)
        }

        mState = State.None;
        mRout = null;
    }

    IEnumerator DoUnitSpawn(Unit unit, Vector2 start, Vector2 end) {
        unit.state = unit.stateSpawning;

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

        unit.state = unit.stateSpawned;
    }

    void OnFlowerRelease(M8.EntityBase ent) {
        mFlowers.Remove((UnitAllyFlower)ent);
    }
}
