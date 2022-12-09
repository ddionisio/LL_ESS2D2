using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour {
    public const string poolGroup = "unitSpawner";

    [Header("Data")]
    public GameObject spawnTemplate;
    public int spawnMaxCount = 3;
    public int spawnPoolCapacity = 6;
    public Transform[] spawnPoints;
    public float spawnStartDelay;
    public float spawnDelay = 2f;
    public bool spawnFromMotherbase;

    public M8.SignalEntity signalUnitSpawnerSpawned;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeSpawn;

    private M8.PoolController mPool;
    private M8.CacheList<M8.EntityBase> mSpawns;

    private int mCurSpawnPointIndex = 0;
    private M8.GenericParams mSpawnParms;

    void OnDestroy() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle) {
            GameController.instance.weatherCycle.cycleBeginCallback -= OnCycleBegin;
            GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;
        }

        ClearCachedSpawns();
    }

    void Awake() {
        mSpawns = new M8.CacheList<M8.EntityBase>(spawnMaxCount);

        mPool = M8.PoolController.CreatePool(poolGroup);
        mPool.AddType(spawnTemplate, spawnPoolCapacity, spawnPoolCapacity);

        mSpawnParms = new M8.GenericParams();
        mSpawnParms[UnitSpawnParams.despawnCycleType] = Unit.DespawnCycleType.Cycle;

        GameController.instance.weatherCycle.cycleBeginCallback += OnCycleBegin;
        GameController.instance.weatherCycle.cycleEndCallback += OnCycleEnd;

        M8.ArrayUtil.Shuffle(spawnPoints);
    }

    void OnCycleBegin() {
        StartCoroutine(DoSpawning());
    }

    void OnCycleEnd() {
        StopAllCoroutines();

        //assume they were released on their own (via despawn on cycle end)
        ClearCachedSpawns();

        mCurSpawnPointIndex = 0;
    }

    IEnumerator DoSpawning() {
        if(spawnStartDelay > 0f)
            yield return new WaitForSeconds(spawnStartDelay);

        var wait = new WaitForSeconds(spawnDelay);

        while(true) {
            //wait for spawns
            while(mSpawns.Count == spawnMaxCount)
                yield return null;

            //animate and spawn to a random point
            if(animator && !string.IsNullOrEmpty(takeSpawn))
                animator.Play(takeSpawn);

            Spawn();

            yield return wait;
        }
    }
        
    void OnEntityRelease(M8.EntityBase ent) {
        ent.releaseCallback -= OnEntityRelease;

        for(int i = 0; i < mSpawns.Count; i++) {
            if(mSpawns[i] == ent) {                
                mSpawns.RemoveAt(i);
                break;
            }
        }
    }

    private void ClearCachedSpawns() {
        for(int i = 0; i < mSpawns.Count; i++) {
            if(mSpawns[i])
                mSpawns[i].releaseCallback -= OnEntityRelease;
        }

        mSpawns.Clear();
    }

    private void Spawn() {
        var spawnPt = spawnPoints[mCurSpawnPointIndex].position;

        mCurSpawnPointIndex++;
        if(mCurSpawnPointIndex == spawnPoints.Length)
            mCurSpawnPointIndex = 0;

        M8.EntityBase ent;

        if(spawnFromMotherbase) {
            var unit = mPool.Spawn<Unit>(spawnTemplate.name, "", null, mSpawnParms);

            GameController.instance.motherbase.SpawnQueueUnit(unit, spawnPt);

            ent = unit;
        }
        else {
            ent = mPool.Spawn<M8.EntityBase>(spawnTemplate.name, "", null, spawnPt, mSpawnParms);
        }

        ent.releaseCallback += OnEntityRelease;
        mSpawns.Add(ent);

        if(signalUnitSpawnerSpawned)
            signalUnitSpawnerSpawned.Invoke(ent);
    }
}
