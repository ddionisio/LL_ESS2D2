using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAllyFlyingAttacker : UnitCard {
    [Header("Data")]
    public float moveSpeed;
    public float moveAttackSpeed;
    public float moveAttackDistance; //distance from left or right of target

    public float attackCheckRadius;
    public LayerMask attackCheckLayerMask;
    public float attackCheckDelay = 0.333f;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeAttackMove;
    public string takeAttackStrike;
    
    private Unit mAttackTarget;
    private DG.Tweening.EaseFunction mMoveEaseFunc;

    private Vector2 mMoveStartPos;
    private float mMoveCurTime;
    private float mMoveDelay;

    private Collider2D[] mAttackCheckColls = new Collider2D[4];

    public override void MotherbaseSpawnFinish() {
        state = UnitStates.instance.move;
    }

    protected override void StateChanged() {
        base.StateChanged();
                
        if(state == UnitStates.instance.move) {
            mMoveStartPos = position;

            var dpos = targetPosition - mMoveStartPos;

            float dist = dpos.magnitude;

            mMoveCurTime = 0f;
            mMoveDelay = dist / moveSpeed;

            curDir = new Vector2(Mathf.Sign(dpos.x), 0f);

            mRout = StartCoroutine(DoAttackCheck());
        }
        else if(state == UnitStates.instance.idle) {
            mRout = StartCoroutine(DoAttackCheck());
        }
        else if(state == UnitStates.instance.act) {
            isPhysicsActive = false;

            mRout = StartCoroutine(DoAttack());
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        ClearAttackTarget();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);
    }

    protected override void Awake() {
        base.Awake();

        mMoveEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InOutSine);
    }

    void FixedUpdate() {
        if(state == UnitStates.instance.move) {
            mMoveCurTime += Time.fixedDeltaTime;

            float t = mMoveEaseFunc(mMoveCurTime, mMoveDelay, 0f, 0f);

            position = Vector2.Lerp(mMoveStartPos, targetPosition, t);

            if(mMoveCurTime >= mMoveDelay)
                state = UnitStates.instance.idle;
        }
    }

    IEnumerator DoAttackCheck() {
        while(true) {
            yield return new WaitForSeconds(attackCheckDelay);

            Collider2D nearestColl = null;
            float nearestCollDistSqr = float.MaxValue;

            var checkPos = position;

            int collCount = Physics2D.OverlapCircleNonAlloc(checkPos, attackCheckRadius, mAttackCheckColls, attackCheckLayerMask);
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
                    mAttackTarget = unit;
                    mAttackTarget.SetMark(true);

                    mRout = null;
                    state = UnitStates.instance.act;
                    break;
                }
            }
        }
    }

    IEnumerator DoAttack() {
        //hold still
        mAttackTarget.state = UnitStates.instance.idle;

        //move towards target
        if(!string.IsNullOrEmpty(takeAttackMove))
            animator.Play(takeAttackMove);

        var startPos = position;
        var endPos = mAttackTarget.position;

        if(startPos.x < endPos.x)
            endPos.x -= moveAttackDistance;
        else
            endPos.x += moveAttackDistance;

        Vector2 dpos = endPos - startPos;        

        curDir = new Vector2(Mathf.Sign(dpos.x), 0f);

        float dist = dpos.magnitude;
        float moveDelay = dist / moveAttackSpeed;

        float curTime = 0f;
        while(curTime < moveDelay) {            
            yield return null;

            curTime += Time.deltaTime;

            float t = mMoveEaseFunc(curTime, moveDelay, 0f, 0f);

            position = Vector2.Lerp(startPos, endPos, t);
        }
        //

        //strike
        curDir = new Vector2(Mathf.Sign(mAttackTarget.position.x - position.x), 0f); //re-orient

        if(!string.IsNullOrEmpty(takeAttackStrike)) {
            animator.Play(takeAttackStrike);
            while(animator.isPlaying)
                yield return null;
        }

        //kill target
        mAttackTarget.state = UnitStates.instance.dead;
        mAttackTarget = null;

        mRout = null;

        state = UnitStates.instance.move;
    }

    private void ClearAttackTarget() {
        if(mAttackTarget) {
            if(!mAttackTarget.isReleased)
                mAttackTarget.SetMark(false);
            mAttackTarget = null;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, attackCheckRadius);
    }
}
