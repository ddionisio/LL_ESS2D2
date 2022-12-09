using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "weather", menuName = "Game/Weather")]
public class WeatherType : ScriptableObject {
    [Header("Display")]
    public Sprite icon;
    public Sprite image; //higher quality for forecast and description

    [M8.Localize]
    public string titleRef; //short detail, ex: Mostly Sunny, T-Storms
    [M8.Localize]
    public string detailRef; //description of the weather
}
