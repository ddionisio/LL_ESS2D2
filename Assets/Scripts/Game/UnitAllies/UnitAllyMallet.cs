using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public class UnitAllyMallet : UnitCard {
    [Header("Data")]
    public float moveSpeed;
    public LayerMask attackCheckLayerMask;
    public float attackCheckDelay = 0.333f;
    public bool isTargetOffscreen; //if true, set target destination offscreen
    public float targetOffscreenOfs; //for isTargetOffscreen, use this as offset outside the screen

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeStrike;

    private Unit mTarget;

    private Collider2D[] mAttackCheckColls = new Collider2D[4];

    private bool mIsStrikeAction;
    
    public override void MotherbaseSpawnFinish() {
        state = UnitStates.instance.move;
    }

    public void StrikeAction() {
        mIsStrikeAction = true;
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        mIsStrikeAction = false;

        ClearTarget();
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(prevState == UnitStates.instance.idle || prevState == UnitStates.instance.move) {
            ShowReticleIndicator(false);
        }
        else if(prevState == UnitStates.instance.act) {
            ClearTarget();
        }

        if(state == UnitStates.instance.spawning) {
            //change target position to off-screen
            if(isTargetOffscreen) {
                var dpos = targetPosition - position;

                float x;
                if(dpos.x < 0f)
                    x = GameController.instance.levelBounds.rect.xMin - targetOffscreenOfs;
                else
                    x = GameController.instance.levelBounds.rect.xMax + targetOffscreenOfs;

                targetPosition = new Vector2(x, targetPosition.y);
            }
        }
        else if(state == UnitStates.instance.idle) {
            ShowReticleIndicator(true);

            mRout = StartCoroutine(DoAttackCheck());
        }
        else if(state == UnitStates.instance.move) {
            ShowReticleIndicator(true);

            //determine dir
            var dpos = targetPosition - position;
            curDir = new Vector2(Mathf.Sign(dpos.x), 0f);

            mRout = StartCoroutine(DoAttackCheck());
        }
        else if(state == UnitStates.instance.act) {
            isPhysicsActive = false;

            RemoveTargetDisplay();
            
            mRout = StartCoroutine(DoStrike());
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

            if(!UpdatePosition(nextPos))
                state = UnitStates.instance.idle;
        }
    }

    IEnumerator DoAttackCheck() {
        while(true) {
            yield return new WaitForSeconds(attackCheckDelay);

            Collider2D nearestColl = null;
            float nearestCollDistSqr = float.MaxValue;

            var checkPos = position;

            int collCount = Physics2D.OverlapCircleNonAlloc(checkPos, mCardItem.card.indicatorRadius, mAttackCheckColls, attackCheckLayerMask);
            for(int i = 0; i < collCount; i++) {
                var coll = mAttackCheckColls[i];
                if(!mCardItem.card.IsTargetValid(coll.gameObject))
                    continue;

                float distSqr = ((Vector2)coll.transform.position - checkPos).sqrMagnitude;
                if(distSqr < nearestCollDistSqr) {
                    nearestColl = coll;
                    nearestCollDistSqr = distSqr;
                }
            }

            if(nearestColl) {
                var unit = nearestColl.GetComponent<Unit>();
                if(unit && !unit.isMarked) {
                    mTarget = unit;
                    mTarget.state = UnitStates.instance.idle; //let it stand, ready to receive pounding
                    mTarget.SetMark(true);

                    //ensure we are facing it
                    var dposX = mTarget.position.x - position.x;
                    curDir = new Vector2(Mathf.Sign(dposX), 0f);

                    mRout = null;
                    state = UnitStates.instance.act; //strike
                    break;
                }
            }
        }
    }

    IEnumerator DoStrike() {
        mIsStrikeAction = false;

        if(animator && !string.IsNullOrEmpty(takeStrike)) {
            animator.Play(takeStrike);
        }
        else
            mIsStrikeAction = true;

        //wait for animation to do strike
        while(!mIsStrikeAction)
            yield return null;

        mTarget.state = UnitStates.instance.dead;
        mTarget = null;

        //wait for animation to finish
        if(animator) {
            while(animator.isPlaying)
                yield return null;
        }

        state = prevState;
    }

    private void ClearTarget() {
        if(mTarget && !mTarget.isReleased) {
            mTarget.SetMark(false);
            mTarget = null;
        }
    }

    private bool UpdatePosition(Vector2 toPos) {
        UnitPoint point;
        bool ret = UnitPoint.GetGroundPoint(toPos, out point);
        if(ret) {
            ApplyUnitPoint(point);

            float dirSign = Mathf.Sign(curDir.x);

            curDir = M8.MathUtil.Rotate(Vector2.up, dirSign * M8.MathUtil.HalfPI);
        }

        return ret;
    }

    void OnDrawGizmos() {
        if(mCardItem != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, mCardItem.card.indicatorRadius);
        }
    }
}