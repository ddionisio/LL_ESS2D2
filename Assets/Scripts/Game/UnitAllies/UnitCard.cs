﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Units that are spawned via card
/// </summary>
public class UnitCard : Unit {
    [Header("UnitCard Data")]
    public float despawnDelay = 2.0f;
    
    public Vector2 targetPosition { get; protected set; }

    protected CardDeckController.CardItem mCardItem;

    private CardDeployTargetDisplay mTargetDisplay;

    private float mDespawnCurTime;

    protected void AddTargetDisplay() {
        if(!mTargetDisplay) {
            if(mCardItem != null && mCardItem.card.targetDisplayPrefab) {
                mTargetDisplay = CardDeployTargetDisplay.Spawn(mCardItem.card.targetDisplayPrefab, targetPosition);
            }
        }
    }

    protected void RemoveTargetDisplay() {
        if(mTargetDisplay) {
            mTargetDisplay.Release();
            mTargetDisplay = null;
        }
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(state == UnitStates.instance.idle) {
            RemoveTargetDisplay();

            //set despawnTimer
            mDespawnCurTime = 0f;
        }
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        if(parms != null) {
            mCardItem = parms.GetValue<CardDeckController.CardItem>(UnitSpawnParams.card);
            targetPosition = parms.GetValue<Vector2>(UnitSpawnParams.target);

            AddTargetDisplay();
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        if(mCardItem != null) {
            mCardItem.IncrementPendingCount();
            mCardItem = null;
        }

        RemoveTargetDisplay();
    }

    protected virtual void Update() {
        if(state == UnitStates.instance.idle) {
            mDespawnCurTime += Time.deltaTime;
            if(mDespawnCurTime >= despawnDelay)
                state = UnitStates.instance.despawning;
        }
    }
}