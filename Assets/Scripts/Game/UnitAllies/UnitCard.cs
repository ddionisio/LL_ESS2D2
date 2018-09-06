using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Units that are spawned via card
/// </summary>
public class UnitCard : Unit {
    [Header("UnitCard Data")]
    public float despawnDelay = 2.0f; // set to -1 to not despawn
    public float despawnFadeDelay = 1.0f; //delay within the end of despawnDelay
    public float despawnFadePulsePerSecond;
    public float despawnAlpha;
    public M8.SpriteColorGroup despawnFadeColorGroup;
    public ReticleIndicator reticleIndicator;
    
    public Vector2 targetPosition { get; protected set; }

    protected CardDeckController.CardItem mCardItem;

    private CardDeployTargetDisplay mTargetDisplay;

    private Coroutine mDespawnRout;
    private bool mReticleIndicatorIsActive;

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

        if(prevState == UnitStates.instance.idle) {
            StopDespawn();
        }
        else if(prevState == UnitStates.instance.move) {
            RemoveTargetDisplay();
        }

        if(state == UnitStates.instance.move) {
            AddTargetDisplay();

            isPhysicsActive = true;
        }
        else if(state == UnitStates.instance.idle) {
            if(despawnDelay > 0f) {
                mDespawnRout = StartCoroutine(DoDespawn());
                isPhysicsActive = true;
            }
            else
                state = UnitStates.instance.despawning;
        }
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        if(parms != null) {
            parms.TryGetValue(UnitSpawnParams.card, out mCardItem);

            Vector2 targetPt;
            parms.TryGetValue(UnitSpawnParams.target, out targetPt);
            targetPosition = targetPt;
        }

        if(mCardItem != null && reticleIndicator) {
            reticleIndicator.radius = mCardItem.card.indicatorRadius;
            reticleIndicator.color = mCardItem.card.indicatorColor;
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        StopDespawn();

        if(mCardItem != null) {
            mCardItem.IncrementPendingCount();
            mCardItem = null;
        }

        RemoveTargetDisplay();

        ShowReticleIndicator(false);
    }

    protected override void Awake() {
        base.Awake();

        if(reticleIndicator) reticleIndicator.gameObject.SetActive(false);
        mReticleIndicatorIsActive = false;
    }

    protected void ShowReticleIndicator(bool show) {
        if(mReticleIndicatorIsActive != show) {
            mReticleIndicatorIsActive = show;

            if(reticleIndicator) {
                reticleIndicator.gameObject.SetActive(mReticleIndicatorIsActive && reticleIndicator.radius > 0f);
            }
        }
    }

    IEnumerator DoDespawn() {
        if(despawnFadeColorGroup) {
            float despawnWaitDelay = despawnDelay - despawnFadeDelay;
            if(despawnWaitDelay > 0f) {
                yield return new WaitForSeconds(despawnWaitDelay);
            }

            float delay = Mathf.Min(despawnFadeDelay, despawnDelay);
            //float fadeDelay = delay 

            float curTime = 0f;
            while(curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;

                float t = Mathf.Sin(Mathf.PI * curTime * despawnFadePulsePerSecond);
                t *= t;

                var clr = despawnFadeColorGroup.color;
                clr.a = Mathf.Lerp(1.0f, despawnAlpha, t);
                despawnFadeColorGroup.color = clr;
            }
        }
        else {
            yield return new WaitForSeconds(despawnDelay);
        }
        
        mDespawnRout = null;

        state = UnitStates.instance.despawning;
    }

    void StopDespawn() {
        if(mDespawnRout != null) {
            StopCoroutine(mDespawnRout);
            mDespawnRout = null;
        }

        if(despawnFadeColorGroup)
            despawnFadeColorGroup.Revert();
    }
}
