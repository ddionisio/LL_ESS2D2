using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "unitStates", menuName = "Game/Unit States")]
public class UnitStates : M8.SingletonScriptableObject<UnitStates> {
    [Header("General")]
    public M8.EntityState spawning;
    public M8.EntityState idle;
    public M8.EntityState move;
    public M8.EntityState act;
    public M8.EntityState despawning;

    [Header("Flower")]
    public M8.EntityState grow;
    public M8.EntityState hold;

    [Header("Enemy")]
    public M8.EntityState dying;
    public M8.EntityState dead;
    public M8.EntityState flyOff;
}
