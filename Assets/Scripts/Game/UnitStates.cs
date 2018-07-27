using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "unitStates", menuName = "Game/Unit States")]
public class UnitStates : M8.SingletonScriptableObject<UnitStates> {
    [Header("General")]
    public M8.EntityState spawning;
    public M8.EntityState normal;
    public M8.EntityState despawning;
    public M8.EntityState hold; //for flowers

    [Header("Enemy")]
    public M8.EntityState dying;
    public M8.EntityState dead;
    public M8.EntityState flyOff;

    [Header("Flower Grab")]
    public M8.EntityState grab;
    public M8.EntityState leave;
}
