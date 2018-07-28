using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "card", menuName = "Game/Card")]
public class CardData : ScriptableObject {
    [Header("Data")]
    public GameObject unitPrefab;
    public float cooldownDuration;

    [Header("Display")]
    public Sprite icon; //show when dragging to world
    public Sprite image; //for card/description

    [M8.Localize]
    public string titleRef;
    [M8.Localize]
    public string descriptionRef;

    public string title { get { return M8.Localize.Get(titleRef); } }
    public string description { get { return M8.Localize.Get(descriptionRef); } }
}
