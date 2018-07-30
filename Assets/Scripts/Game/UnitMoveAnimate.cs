using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General unit move animation
/// </summary>
public class UnitMoveAnimate : MonoBehaviour {
    [Header("Data")]
    public bool releaseAfterDespawn = true; //release unit at end of despawn animation?

    [Header("Display")]
    public Transform displayRoot; //use to change 'facing' based on dir x

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeIdle;
    public string takeMove;
    public string takeDespawn;
    
    private Unit mUnit;

    void Awake() {
        mUnit = GetComponent<Unit>();

        mUnit.dirChangedCallback += OnDirChanged;
        mUnit.spawnCallback += OnEntSpawned;
        mUnit.setStateCallback += OnEntStateChanged;

        if(animator)
            animator.takeCompleteCallback += OnAnimatorTakeComplete;
    }

    void OnDirChanged() {
        if(displayRoot) {
            float dirXSign = Mathf.Sign(mUnit.curDir.x);

            var s = displayRoot.localScale;
            s.x = dirXSign * Mathf.Abs(s.x);

            displayRoot.localScale = s;
        }
    }

    void OnEntSpawned(M8.EntityBase ent) {
        //update dir
        OnDirChanged();
    }

    void OnEntStateChanged(M8.EntityBase ent) {
        if(!animator)
            return;

        if(ent.state == UnitStates.instance.idle) {
            if(!string.IsNullOrEmpty(takeIdle))
                animator.Play(takeIdle);
        }
        else if(ent.state == UnitStates.instance.move) {
            if(!string.IsNullOrEmpty(takeMove))
                animator.Play(takeMove);
        }
        else if(ent.state == UnitStates.instance.despawning) {
            if(!string.IsNullOrEmpty(takeDespawn))
                animator.Play(takeDespawn);
        }
    }

    void OnAnimatorTakeComplete(M8.Animator.Animate anim, M8.Animator.Take take) {
        if(take.name == takeDespawn) {
            if(releaseAfterDespawn)
                mUnit.Release();
        }
    }
}
