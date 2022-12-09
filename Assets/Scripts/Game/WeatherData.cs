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

    [Header("Growth")]
    public float flowerGrowthMod; //global flower growth based on current weather

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

    public string temperatureText {
        get {
            return temperature.ToString() + "° F";
        }
    }

    public string precipitationPercentText {
        get {
            return precipitationPercent.ToString() + '%';
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
