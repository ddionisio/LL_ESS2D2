using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "card", menuName = "Game/Card")]
public class CardData : ScriptableObject {
    [Header("Data")]
    public GameObject unitPrefab;
    public float cooldownDuration;

    [Header("Display")]
    public Sprite image;

    [M8.Localize]
    public string nameRef;
    [M8.Localize]
    public string descriptionRef;
}
