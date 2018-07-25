using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : M8.EntityBase {
    [Header("States")]
    public M8.EntityState stateSpawning;
    public M8.EntityState stateNormal;
    public M8.EntityState stateDespawning;

    [Header("Display")]
    public GameObject displayRootGO;
    public GameObject spawnRootGO;
    
    protected Coroutine mRout;

    public virtual void SetDisplayActive(bool active) {
        if(displayRootGO)
            displayRootGO.SetActive(active);
    }

    protected void StopRoutine() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    protected override void StateChanged() {
        StopRoutine();

        //spawn states are set via Motherbase or EnemyController
        if(state == stateSpawning) {
            if(spawnRootGO)
                spawnRootGO.SetActive(true);
        }
        else if(state == stateNormal) {
            if(spawnRootGO)
                spawnRootGO.SetActive(false);

            SetDisplayActive(true);
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        SetDisplayActive(false);

        if(spawnRootGO)
            spawnRootGO.SetActive(false);
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        //start ai, player control, etc
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        SetDisplayActive(false);

        if(spawnRootGO)
            spawnRootGO.SetActive(false);
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
}
