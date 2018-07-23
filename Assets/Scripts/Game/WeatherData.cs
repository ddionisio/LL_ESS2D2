using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeatherData {
    public WeatherType type;

    [Header("Data")]
    public int uvLevel;
    public int temperature;
    public float windAngle; //0 = up vector
    public float windSpeed;
    public int humidityPercent;
    public int precipitationPercent;

    public Vector2 windDir {
        get {
            if(!mIsWindInfoSet)
                ApplyWindInfo();

            return mWindDir;
        }
    }

    public Vector2 windVelocity {
        get {
            if(!mIsWindInfoSet)
                ApplyWindInfo();

            return mWindVelocity;
        }
    }

    private Vector2 mWindDir;
    private Vector2 mWindVelocity;
    private bool mIsWindInfoSet;

    private void ApplyWindInfo() {
        mWindDir = M8.MathUtil.RotateAngle(Vector2.up, windAngle);
        mWindVelocity = mWindDir * windSpeed;

        mIsWindInfoSet = true;
    }
}
