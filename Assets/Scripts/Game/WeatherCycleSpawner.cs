using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleSpawner : MonoBehaviour {
    public const string poolGroup = "cycleSpawner";

    [System.Serializable]
    public class SpawnInfo {
        [Header("Edit")]
        public string label;
        public Color color = Color.white;

        [Header("Info")]
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
        Dictionary<GameObject, int> poolCountLookup = new Dictionary<GameObject, int>();

        for(int i = 0; i < cycles.Length; i++) {
            var curCycle = cycles[i];

            for(int j = 0; j < curCycle.spawns.Length; j++) {
                var spawn = curCycle.spawns[j];

                if(poolCountLookup.ContainsKey(spawn.prefab))
                    poolCountLookup[spawn.prefab]++;
                else
                    poolCountLookup.Add(spawn.prefab, 1);
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
            var spawn = curCycle.spawns[i];

            yield return new WaitForSeconds(spawn.delay);

            var parms = new M8.GenericParams();
            parms[UnitSpawnParams.position] = spawn.position;
            parms[UnitSpawnParams.dir] = spawn.dir;

            mPool.Spawn(spawn.prefab.name, "", null, parms);
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

                var pointColor = new Color(spawn.color.r, spawn.color.g, spawn.color.b, spawn.color.a * 0.75f);
                var arrowColor = spawn.color;

                Gizmos.color = pointColor;
                Gizmos.DrawSphere(spawn.position, radius);

                var end = spawn.position + spawn.dir * arrowLen;

                Gizmos.color = arrowColor;
                M8.Gizmo.ArrowLine2D(spawn.position, end);
            }
        }
    }
}
