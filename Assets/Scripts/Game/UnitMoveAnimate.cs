using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General unit move animation
/// </summary>
public class UnitMoveAnimate : MonoBehaviour {
    [Header("Display")]
    public Transform displayRoot; //use to change 'facing' based on dir x

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeIdle;
    public string takeMove;
    
    void Awake() {
        var unit = GetComponent<Unit>();

        unit.dirChangedCallback += OnUnitDirChanged;
        unit.spawnCallback += OnEntSpawned;
        unit.setStateCallback += OnEntStateChanged;
    }

    void OnUnitDirChanged(Vector2 dir) {
        if(displayRoot) {
            float dirXSign = Mathf.Sign(dir.x);

            var s = displayRoot.localScale;
            s.x = dirXSign * Mathf.Abs(s.x);

            displayRoot.localScale = s;
        }
    }

    void OnEntSpawned(M8.EntityBase ent) {
        var unit = (Unit)ent;

        //update dir
        OnUnitDirChanged(unit.curDir);
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
    }
}
