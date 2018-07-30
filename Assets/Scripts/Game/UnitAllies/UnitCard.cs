using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Units that are spawned via card
/// </summary>
public class UnitCard : Unit {
    [Header("Data")]
    public float despawnDelay = 2.0f;

    [Header("Animator Card Takes")]
    public string takeDespawn;

    public Vector2 targetPosition { get; protected set; }

    protected CardDeckController.CardItem mCardItem;

    private CardDeployTargetDisplay mTargetDisplay;

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

        if(state == UnitStates.instance.despawning) {
            if(animator != null && !string.IsNullOrEmpty(takeDespawn))
                mRout = StartCoroutine(DoDespawning());
            else
                Release(); //no despawn animation, release right away
        }
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        if(parms != null) {
            mCardItem = parms.GetValue<CardDeckController.CardItem>(UnitSpawnParams.card);
            targetPosition = parms.GetValue<Vector2>(UnitSpawnParams.target);
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
        
    }

    IEnumerator DoDespawning() {
        animator.Play(takeDespawn);

        while(animator.isPlaying)
            yield return null;

        mRout = null;

        Release();
    }
}
