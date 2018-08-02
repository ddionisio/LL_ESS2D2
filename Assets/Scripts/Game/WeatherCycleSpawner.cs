using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleSpawner : MonoBehaviour {
    public const string poolGroup = "cycleSpawner";

    public enum SpawnType {
        Location,
        FlowerBudding, //spawn under budding flower, sets unitTarget in params
        FlowerBuddingUnmarked, //spawn under unmarked budding flower, sets unitTarget in params (will not spawn if all flowers are marked)

        TargetFlowerUnmarked, //target nearest flower unmarked relative to position, if no target found, skip spawn
        TargetFlowerBloomedUnmarked, //target nearest bloomed flower unmarked relative to position, if no target found, skip spawn

        FlowerRandom, //any active flower, also sets it as unitTarget
    }

    [System.Serializable]
    public class SpawnInfo {
        [Header("Edit")]
        public string label;
        public Color color = Color.white;
        public bool disabled;

        [Header("Info")]
        public SpawnType type;
        public GameObject prefab;
        public float delay; //delay relative to the last spawn, or at the start

        [Header("Telemetry")]
        public Vector2 position;
        public float dirAngle;

        public Vector2 dir {
            get {
                return M8.MathUtil.RotateAngle(Vector2.up, dirAngle);
            }
        }

        public void Spawn(M8.PoolController pool) {
            var parms = new M8.GenericParams();

            parms[UnitSpawnParams.despawnOnCycleEnd] = true;

            switch(type) {
                case SpawnType.Location:
                    parms[UnitSpawnParams.position] = position;
                    parms[UnitSpawnParams.dir] = dir;
                    break;

                case SpawnType.FlowerBudding:
                case SpawnType.FlowerBuddingUnmarked: {
                        var flowersQuery = GameController.instance.motherbase.GetFlowersBudding(type == SpawnType.FlowerBuddingUnmarked);
                        if(flowersQuery.Count == 0)
                            return;

                        var flower = flowersQuery[Random.Range(0, flowersQuery.Count)];
                        parms[UnitSpawnParams.position] = flower.position;
                        parms[UnitSpawnParams.dir] = flower.up;
                        parms[UnitSpawnParams.unitTarget] = flower;
                    }
                    break;

                case SpawnType.TargetFlowerUnmarked: {
                        var flower = GameController.instance.motherbase.GetNearestFlower(position.x, true);
                        if(!flower)
                            return;

                        parms[UnitSpawnParams.position] = position;
                        parms[UnitSpawnParams.dir] = dir;
                        parms[UnitSpawnParams.unitTarget] = flower;
                    }
                    break;

                case SpawnType.TargetFlowerBloomedUnmarked: {
                        var flower = GameController.instance.motherbase.GetNearestFlowerBloomed(position.x, true);
                        if(!flower)
                            return;

                        parms[UnitSpawnParams.position] = position;
                        parms[UnitSpawnParams.dir] = dir;
                        parms[UnitSpawnParams.unitTarget] = flower;
                    }
                    break;

                case SpawnType.FlowerRandom: {
                        var flower = GameController.instance.motherbase.GetRandomFlower(false);
                        if(!flower)
                            return;

                        parms[UnitSpawnParams.position] = flower.position;
                        parms[UnitSpawnParams.dir] = dir;
                        parms[UnitSpawnParams.unitTarget] = flower;
                    }
                    break;
            }            
            
            pool.Spawn(prefab.name, "", null, parms);
        }
    }

    [System.Serializable]
    public class CycleInfo {
        public SpawnInfo[] spawns;
    }

    public CycleInfo[] cycles;

    //editor
    public int editCycleIndex;

    private M8.PoolController mPool;

    private Coroutine mCycleRout;

    private int mCurCycleInd = 0;

    void OnDestroy() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle) {
            GameController.instance.weatherCycle.cycleBeginCallback -= OnCycleBegin;
            GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;
        }
    }

    void Awake() {
        //grab pool objects and determine its spawn count
        var poolCountLookup = new Dictionary<GameObject, int>();

        for(int i = 0; i < cycles.Length; i++) {
            var curCycle = cycles[i];

            //grab total counts for each spawn on this cycle
            var poolCycleCountLookup = new Dictionary<GameObject, int>();

            for(int j = 0; j < curCycle.spawns.Length; j++) {
                var spawn = curCycle.spawns[j];

                if(poolCycleCountLookup.ContainsKey(spawn.prefab))
                    poolCycleCountLookup[spawn.prefab]++;
                else
                    poolCycleCountLookup.Add(spawn.prefab, 1);
            }

            //apply max count for poolCountLookup
            foreach(var pair in poolCycleCountLookup) {
                if(poolCountLookup.ContainsKey(pair.Key)) {
                    if(poolCountLookup[pair.Key] < pair.Value)
                        poolCountLookup[pair.Key] = pair.Value;
                }
                else
                    poolCountLookup.Add(pair.Key, pair.Value);
            }
        }

        mPool = M8.PoolController.CreatePool(poolGroup);

        foreach(var pair in poolCountLookup) {
            var template = pair.Key;
            var count = pair.Value;

            mPool.AddType(template, count, count);
        }
        //

        GameController.instance.weatherCycle.cycleBeginCallback += OnCycleBegin;
        GameController.instance.weatherCycle.cycleEndCallback += OnCycleEnd;
    }

    IEnumerator DoCycle() {
        var curCycle = cycles[mCurCycleInd];

        for(int i = 0; i < curCycle.spawns.Length; i++) {
            var spawnInfo = curCycle.spawns[i];
            if(spawnInfo.disabled)
                continue;

            yield return new WaitForSeconds(spawnInfo.delay);

            spawnInfo.Spawn(mPool);
        }

        mCycleRout = null;
    }

    void OnCycleBegin() {
        mCycleRout = StartCoroutine(DoCycle());
    }

    void OnCycleEnd() {
        if(mCycleRout != null) {
            StopCoroutine(mCycleRout);
            mCycleRout = null;
        }

        if(mCurCycleInd < cycles.Length - 1)
            mCurCycleInd++;
    }

    void OnDrawGizmos() {
        if(cycles != null && editCycleIndex >= 0 && editCycleIndex < cycles.Length) {
            const float radius = 0.33f;
            const float arrowLen = 1.0f;
                                                
            //display cycle
            var cycle = cycles[editCycleIndex];

            for(int i = 0; i < cycle.spawns.Length; i++) {
                var spawn = cycle.spawns[i];
                if(spawn.disabled)
                    continue;

                var pointColor = new Color(spawn.color.r, spawn.color.g, spawn.color.b, spawn.color.a * 0.75f);
                var arrowColor = spawn.color;

                Gizmos.color = pointColor;
                Gizmos.DrawSphere(spawn.position, radius);

                var end = spawn.position + spawn.dir * arrowLen;

                //display arrow for certain spawn types
                switch(spawn.type) {
                    case SpawnType.Location:
                    case SpawnType.FlowerRandom:
                        Gizmos.color = arrowColor;
                        M8.Gizmo.ArrowLine2D(spawn.position, end);
                        break;
                }
            }
        }
    }
}
