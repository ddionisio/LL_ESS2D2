using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleSpawner : MonoBehaviour {
    public const string poolGroup = "cycleSpawner";

    [System.Serializable]
    public class CycleInfo {
        public GameObject cycleRoot;
        public GameObject[] weatherRoots;

        public WeatherCycleSpawnerItem[] cycleSpawnItems { get; private set; }
        public WeatherCycleSpawnerItem[][] weatherSpawnItems { get; private set; }

        public void Init() {
            if(cycleRoot && cycleRoot.activeSelf) {
                cycleSpawnItems = cycleRoot.GetComponentsInChildren<WeatherCycleSpawnerItem>(false);

                for(int i = 0; i < cycleSpawnItems.Length; i++)
                    cycleSpawnItems[i].cycleEndType = Unit.DespawnCycleType.Cycle;
            }

            weatherSpawnItems = new WeatherCycleSpawnerItem[weatherRoots.Length][];

            for(int i = 0; i < weatherRoots.Length; i++) {
                var weatherRoot = weatherRoots[i];
                if(weatherRoot && weatherRoot.activeSelf) {
                    weatherSpawnItems[i] = weatherRoot.GetComponentsInChildren<WeatherCycleSpawnerItem>(false);
                    for(int j = 0; j < weatherSpawnItems[i].Length; j++)
                        weatherSpawnItems[i][j].cycleEndType = Unit.DespawnCycleType.Weather;
                }
            }
        }
    }

    public CycleInfo[] cycles;

    [Header("Signals")]
    public SignalUnit signalUnitSpawned;

    private M8.PoolController mPool;

    private Coroutine mCycleRout;
    private Coroutine mWeatherRout;
    
    void OnDestroy() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle) {
            GameController.instance.weatherCycle.cycleBeginCallback -= OnCycleBegin;
            GameController.instance.weatherCycle.cycleEndCallback -= OnCycleEnd;
            GameController.instance.weatherCycle.weatherBeginCallback -= OnWeatherBegin;
            GameController.instance.weatherCycle.weatherEndCallback -= OnWeatherEnd;
        }
    }

    void Awake() {
        //initialize spawn items
        for(int i = 0; i < cycles.Length; i++)
            cycles[i].Init();

        //grab pool objects and determine its spawn count
        var poolCountLookup = new Dictionary<GameObject, int>();

        for(int i = 0; i < cycles.Length; i++) {
            var curCycle = cycles[i];

            var poolCycleCountLookup = new Dictionary<GameObject, int>();

            //grab total counts for each spawn on this cycle
            if(curCycle.cycleSpawnItems != null && curCycle.cycleSpawnItems.Length > 0) {
                for(int j = 0; j < curCycle.cycleSpawnItems.Length; j++) {
                    var spawn = curCycle.cycleSpawnItems[j];

                    if(poolCycleCountLookup.ContainsKey(spawn.prefab))
                        poolCycleCountLookup[spawn.prefab]++;
                    else
                        poolCycleCountLookup.Add(spawn.prefab, 1);
                }
            }
            
            //grab total counts for each weather on this cycle
            if(curCycle.weatherSpawnItems != null && curCycle.weatherSpawnItems.Length > 0) {
                for(int j = 0; j < curCycle.weatherSpawnItems.Length; j++) {
                    if(curCycle.weatherSpawnItems[j] == null || curCycle.weatherSpawnItems[j].Length == 0)
                        continue;

                    for(int k = 0; k < curCycle.weatherSpawnItems[j].Length; k++) {
                        var spawn = curCycle.weatherSpawnItems[j][k];

                        if(poolCycleCountLookup.ContainsKey(spawn.prefab))
                            poolCycleCountLookup[spawn.prefab]++;
                        else
                            poolCycleCountLookup.Add(spawn.prefab, 1);
                    }
                }
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
        GameController.instance.weatherCycle.weatherBeginCallback += OnWeatherBegin;
        GameController.instance.weatherCycle.weatherEndCallback += OnWeatherEnd;
    }

    IEnumerator DoCycle() {
        var cycleInd = GameController.instance.weatherCycle.curCycleIndex;
        if(cycleInd >= cycles.Length) {
            mCycleRout = null;
            yield break;
        }

        var curCycle = cycles[cycleInd];
        if(curCycle.cycleSpawnItems != null) {
            for(int i = 0; i < curCycle.cycleSpawnItems.Length; i++) {
                var spawnInfo = curCycle.cycleSpawnItems[i];
                
                yield return new WaitForSeconds(spawnInfo.delay);

                var unit = spawnInfo.Spawn(mPool);
                if(unit && signalUnitSpawned)
                    signalUnitSpawned.Invoke(unit);
            }
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
    }

    IEnumerator DoWeather() {
        var cycleInd = GameController.instance.weatherCycle.curCycleIndex;
        if(cycleInd >= cycles.Length) {
            mWeatherRout = null;
            yield break;
        }

        var curCycle = cycles[cycleInd];

        var weatherInd = GameController.instance.weatherCycle.curWeatherIndex;
        if(weatherInd >= curCycle.weatherSpawnItems.Length) {
            mWeatherRout = null;
            yield break;
        }

        var weatherSpawnItems = curCycle.weatherSpawnItems[weatherInd];
        if(weatherSpawnItems != null) {
            for(int i = 0; i < weatherSpawnItems.Length; i++) {
                var spawnInfo = weatherSpawnItems[i];

                yield return new WaitForSeconds(spawnInfo.delay);

                var unit = spawnInfo.Spawn(mPool);
                if(unit && signalUnitSpawned)
                    signalUnitSpawned.Invoke(unit);
            }
        }

        mWeatherRout = null;
    }

    void OnWeatherBegin() {
        mWeatherRout = StartCoroutine(DoWeather());
    }

    void OnWeatherEnd() {
        if(mWeatherRout != null) {
            StopCoroutine(mWeatherRout);
            mWeatherRout = null;
        }
    }
}
