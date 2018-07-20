using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycle : MonoBehaviour {
    [System.Serializable]
    public class CycleData {
        public WeatherData[] weathers;
        public float duration;

        public float weatherDuration {
            get {
                if(weathers.Length <= 0)
                    return 0f;

                return duration / weathers.Length;
            }
        }
    }

    [Header("Data")]
    public CycleData[] cycles;
        
    public int curCycleIndex { get; private set; }
    public int curWeatherIndex { get; private set; }

    /// <summary>
    /// Current progress of cycle [0, 1]
    /// </summary>
    public float curCycleProgress {
        get {
            return Mathf.Clamp01((Time.time - mStartTimeCycle) / cycles[curCycleIndex].weatherDuration);
        }
    }

    /// <summary>
    /// Current progress of weather within the current cycle [0, 1]
    /// </summary>
    public float curWeatherProgress {
        get {
            return Mathf.Clamp01((Time.time - mStartTimeWeather) / cycles[curCycleIndex].duration);
        }
    }

    public bool isCycleRunning { get { return mCycleRout != null; } }

    public event System.Action cycleBeginCallback;
    public event System.Action cycleNextCallback;
    public event System.Action cycleEndCallback;
    public event System.Action weatherBeginCallback;
    public event System.Action weatherEndCallback;

    private float mTotalDuration;
    private float mStartTimeCycle;
    private float mStartTimeWeather;

    private Coroutine mCycleRout;

    public void StartCurCycle() {
        if(mCycleRout != null)
            StopCoroutine(mCycleRout);

        mCycleRout = StartCoroutine(DoCurCycle());
    }

    /// <summary>
    /// Returns true if next cycle is available, false otherwise (all cycles completed)
    /// </summary>
    /// <returns></returns>
    public bool NextCycle() {
        if(curCycleIndex >= cycles.Length - 1)
            return false;

        curCycleIndex++;

        if(cycleNextCallback != null)
            cycleNextCallback();

        return true;
    }

    void OnDisable() {
        if(mCycleRout != null) {
            StopCoroutine(mCycleRout);
            mCycleRout = null;
        }
    }

    IEnumerator DoCurCycle() {
        mStartTimeCycle = Time.time;

        var curCycle = cycles[curCycleIndex];

        if(cycleBeginCallback != null)
            cycleBeginCallback();

        var weatherWait = new WaitForSeconds(curCycle.weatherDuration);

        for(curWeatherIndex = 0; curWeatherIndex < curCycle.weathers.Length; curWeatherIndex++) {
            mStartTimeWeather = Time.time;

            if(weatherBeginCallback != null)
                weatherBeginCallback();

            yield return weatherWait;

            if(weatherEndCallback != null)
                weatherEndCallback();
        }

        mCycleRout = null;

        if(cycleEndCallback != null)
            cycleEndCallback();
    }
}
