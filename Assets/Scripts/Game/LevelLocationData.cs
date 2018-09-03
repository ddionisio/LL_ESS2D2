using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "locationData", menuName = "Game/Location Data")]
public class LevelLocationData : ScriptableObject {
    public Sprite image;
    [M8.Localize]
    public string titleTextRef;
    [M8.Localize]
    public string descTextRef;

    public ClimateData climate;
}
