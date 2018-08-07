﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "climate", menuName = "Game/Climate")]
public class ClimateData : ScriptableObject {
    public Sprite image;
    [M8.Localize]
    public string titleTextRef;
    [M8.Localize]
    public string descTextRef;

    public ClimateZoneData zone;
}