using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCycleController : MonoBehaviour {
    
    public WeatherCycleData data;
        
    public int curCycleIndex { get; private set; }
    public int curWeatherIndex { get; private set; }

    public WeatherCycleData.CycleData curCycleData { get { return data.cycles[curCycleIndex]; } }
    public WeatherData curWeather { get { return data.cycles[curCycleIndex].weathers[curWeatherIndex]; } }

    /// <summary>
    /// Current progress of cycle [0, 1]
    /// </summary>
    public float curCycleProgress {
        get {
            return Mathf.Clamp01((Time.time - mStartTimeCycle) / data.cycles[curCycleIndex].duration);
        }
    }

    /// <summary>
    /// Current progress of weather within the current cycle [0, 1]
    /// </summary>
    public float curWeatherProgress {
        get {
            return Mathf.Clamp01((Time.time - mStartTimeWeather) / data.cycles[curCycleIndex].weatherDuration);
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
        if(curCycleIndex >= data.cycles.Length - 1)
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

        var curCycle = data.cycles[curCycleIndex];

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
