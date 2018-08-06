using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "locationData", menuName = "Game/Location Data")]
public class LocationData : ScriptableObject {
    public Sprite image;
    [M8.Localize]
    public string titleTextRef;

    public ClimateZone climateZone;
    public ClimateData climate;
}
