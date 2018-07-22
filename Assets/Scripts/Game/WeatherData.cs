using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeatherData {
    public WeatherType weather;

    [Header("Data")]
    public int uvLevel;
    public int temperature;
    public Vector2 windVector; // mph
    public int humidityPercent;
}
