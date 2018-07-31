using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handle despawn to release
/// </summary>
public class EntityDespawningRelease : MonoBehaviour {
    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeDespawn;

    private M8.EntityBase mEnt;

    void Awake() {
        mEnt = GetComponent<M8.EntityBase>();
        mEnt.setStateCallback += OnEntityStateChanged;

        if(animator)
            animator.takeCompleteCallback += OnAnimatorTakeComplete;
    }

    void OnEntityStateChanged(M8.EntityBase ent) {
        if(ent.state == UnitStates.instance.despawning) {
            if(animator && !string.IsNullOrEmpty(takeDespawn))
                animator.Play(takeDespawn);
            else
                mEnt.Release();
        }
    }

    void OnAnimatorTakeComplete(M8.Animator.Animate anim, M8.Animator.Take take) {
        if(take.name == takeDespawn) {
            mEnt.Release();
        }
    }
}
