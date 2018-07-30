using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAllyMallet : UnitCard {
    [Header("Data")]
    public float moveSpeed;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeStrike;

    private Unit mTarget;

    /// <summary>
    /// Call through animation during attack
    /// </summary>
    public void StrikeTarget() {
        if(mTarget && !mTarget.isReleased) {            
            mTarget.Hit(this);
        }
    }

    public override void MotherbaseSpawnFinish() {
        state = UnitStates.instance.move;
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        ClearTarget();
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(prevState == UnitStates.instance.act) {
            ClearTarget();
        }
        else if(prevState == UnitStates.instance.move) {
            RemoveTargetDisplay();
        }

        bool bodySimulate = false;

        if(state == UnitStates.instance.idle) {
            bodySimulate = true;
        }
        else if(state == UnitStates.instance.move) {
            bodySimulate = true;

            //determine dir
            var dpos = targetPosition - position;
            curDir = new Vector2(Mathf.Sign(dpos.x), 0f);
        }
        else if(state == UnitStates.instance.act) {
            isDespawnOnCycleEnd = false;
            mRout = StartCoroutine(DoStrike());
        }

        body.simulated = bodySimulate;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        //can't interact if not moving or idle
        if(!(state == UnitStates.instance.idle || state == UnitStates.instance.move))
            return;

        //check if viable target
        var go = collision.gameObject;
        if(mCardItem.card.IsTargetValid(go)) {
            var unit = go.GetComponent<Unit>();
            if(unit) {
                //make sure it's not marked
                if(!unit.isMarked) {
                    mTarget = unit;
                    mTarget.state = UnitStates.instance.idle; //let it stand, ready to receive pounding
                    mTarget.SetMark(true);

                    //ensure we are facing it
                    var dposX = mTarget.position.x - position.x;
                    curDir = new Vector2(Mathf.Sign(dposX), 0f);

                    state = UnitStates.instance.act; //strike
                }
            }
        }
    }

    void FixedUpdate() {
        if(state == UnitStates.instance.move) {
            //move
            var nextPos = position + curDir * moveSpeed * Time.fixedDeltaTime;

            //check if we are at destination (slightly passed)
            float dirSign = Mathf.Sign(curDir.x);
            if(nextPos.x == targetPosition.x || (dirSign < 0f && nextPos.x < targetPosition.x) || (dirSign > 0f && nextPos.x > targetPosition.x)) {
                nextPos = targetPosition;

                state = UnitStates.instance.idle;
            }

            UpdatePosition(nextPos);
        }
    }

    IEnumerator DoStrike() {
        if(animator && !string.IsNullOrEmpty(takeStrike)) {
            animator.Play(takeStrike);
            while(animator.isPlaying)
                yield return null;
        }

        Release();
    }

    private void ClearTarget() {
        if(mTarget && !mTarget.isReleased) {
            mTarget.SetMark(false);
            mTarget = null;
        }
    }

    private void UpdatePosition(Vector2 toPos) {
        UnitPoint point;
        if(UnitPoint.GetGroundPoint(toPos, out point)) {
            ApplyUnitPoint(point);

            float dirSign = Mathf.Sign(curDir.x);

            curDir = M8.MathUtil.Rotate(Vector2.up, dirSign * M8.MathUtil.HalfPI);
        }
    }
}