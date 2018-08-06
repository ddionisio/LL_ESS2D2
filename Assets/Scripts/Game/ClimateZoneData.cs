using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "climateZone", menuName = "Game/Climate Zone")]
public class ClimateZoneData : ScriptableObject {
    public Sprite image;
    [M8.Localize]
    public string titleTextRef;
    [M8.Localize]
    public string descTextRef;
}
