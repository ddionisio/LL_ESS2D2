using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "weatherCycles", menuName = "Game/Weather Cycles")]
public class WeatherCycleData : ScriptableObject {
    [System.Serializable]
    public class CycleData {
        [Header("Weather Info")]
        public WeatherData[] weathers;
        public float duration;

        [Header("Growth")]
        public float flowerGrowthMod; //global flower growth based on current cycle

        public float weatherDuration {
            get {
                if(weathers.Length <= 0)
                    return 0f;

                return duration / weathers.Length;
            }
        }
    }

    public CycleData[] cycles;
}
