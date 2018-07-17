using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "weather", menuName = "Game/Weather")]
public class WeatherData : ScriptableObject {
    [Header("Display")]
    public Sprite icon;

    [M8.Localize]
    public string titleRef; //short detail, ex: Mostly Sunny, T-Storms
    [M8.Localize]
    public string detailRef; //description of the weather

    [Header("Data")]
    public int uvLevel;
    public int temperature;
    public Vector2 windVector; // mph
    public int humidityPercent;
}
