using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "card", menuName = "Game/Card")]
public class CardData : ScriptableObject {
    [Header("Data")]
    public GameObject unitPrefab;
    public float cooldownDuration;

    [Header("Target Info")]
    public string targetReticleName;
    public GameObject targetDisplayPrefab;
    [M8.TagSelector]
    public string[] targetTagFilters;

    [Header("Display")]
    public Sprite icon; //show when dragging to world
    public Sprite image; //for card/description
    public Sprite illustration; //for description

    [M8.Localize]
    public string titleRef;
    [M8.Localize]
    public string descriptionRef;

    public string title { get { return M8.Localize.Get(titleRef); } }
    public string description { get { return M8.Localize.Get(descriptionRef); } }

    public bool IsTargetValid(GameObject go) {
        if(targetTagFilters == null || targetTagFilters.Length == 0)
            return true;

        for(int i = 0; i < targetTagFilters.Length; i++) {
            if(go.CompareTag(targetTagFilters[i]))
                return true;
        }

        return false;
    }
}
