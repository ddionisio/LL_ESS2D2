using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAllyFlyingAttacker : UnitCard {
    [Header("Data")]
    public float moveSpeed;
    public float moveAttackSpeed;
    public float moveAttackDistance; //distance from left or right of target
    
    public LayerMask attackCheckLayerMask;
    public float attackCheckDelay = 0.333f;

    public Transform attackRotateRoot;
    public float attackRotateDelay = 0.3f;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeAttackStart;
    public string takeAttackMove;
    public string takeAttackStrike;
    
    private Unit mAttackTarget;
    private DG.Tweening.EaseFunction mMoveEaseFunc;

    private Vector2 mMoveStartPos;
    private float mMoveCurTime;
    private float mMoveDelay;

    private float mIdleEndX;

    private Collider2D[] mAttackCheckColls = new Collider2D[4];

    public override void MotherbaseSpawnFinish() {
        var motherbase = GameController.instance.motherbase;
        curDir = new Vector2(Mathf.Sign(position.x - motherbase.transform.position.x), 0f);

        state = UnitStates.instance.move;
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(prevState == UnitStates.instance.idle || prevState == UnitStates.instance.move) {
            ShowReticleIndicator(false);
        }
        else if(prevState == UnitStates.instance.act) {
            //in case we change state mid-attack, reset attackRotate
            if(attackRotateRoot)
                attackRotateRoot.localRotation = Quaternion.identity;
        }
                
        if(state == UnitStates.instance.move) {
            ShowReticleIndicator(true);

            mMoveStartPos = position;

            var dpos = targetPosition - mMoveStartPos;

            float dist = dpos.magnitude;

            mMoveCurTime = 0f;
            mMoveDelay = dist / moveSpeed;

            mRout = StartCoroutine(DoAttackCheck());
        }
        else if(state == UnitStates.instance.idle) {
            ShowReticleIndicator(true);

            //setup wander
            var motherbase = GameController.instance.motherbase;
            if(curDir.x < 0f)
                mIdleEndX = motherbase.transform.position.x + motherbase.spawnAreaLeft.xMin;
            else
                mIdleEndX = motherbase.transform.position.x + motherbase.spawnAreaRight.xMax;

            mMoveCurTime = 0f;
            mMoveDelay = Mathf.Abs(position.x - mIdleEndX) / moveSpeed;

            mMoveStartPos = position;

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

        if(attackRotateRoot)
            attackRotateRoot.localRotation = Quaternion.identity;
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
        else if(state == UnitStates.instance.idle) {
            var endPos = new Vector2(mIdleEndX, targetPosition.y);

            mMoveCurTime += Time.fixedDeltaTime;

            float t = mMoveEaseFunc(mMoveCurTime, mMoveDelay, 0f, 0f);

            position = Vector2.Lerp(mMoveStartPos, endPos, t);

            targetPosition = position;

            //bounce to other dir
            if(mMoveCurTime >= mMoveDelay) {
                curDir = new Vector2(-curDir.x, curDir.y);

                var motherbase = GameController.instance.motherbase;
                if(curDir.x < 0f)
                    mIdleEndX = motherbase.transform.position.x + motherbase.spawnAreaLeft.xMin;
                else
                    mIdleEndX = motherbase.transform.position.x + motherbase.spawnAreaRight.xMax;

                mMoveCurTime = 0f;
                mMoveDelay = Mathf.Abs(position.x - mIdleEndX) / moveSpeed;

                mMoveStartPos = position;
            }
        }
    }

    IEnumerator DoAttackCheck() {
        var checkWait = new WaitForSeconds(attackCheckDelay);
        while(true) {
            yield return checkWait;

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

        float curTime = 0f;

        var easeFuncRot = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);

        var startPos = position;
        var endPos = mAttackTarget.position;
        Vector2 dpos = mAttackTarget.position - position;        
        float dist = dpos.magnitude;

        Vector2 dir;
        if(dist > 0f)
            dir = dpos / dist;
        else
            dir = Vector2.up;

        if(attackRotateRoot) {
            //rotate towards target
            float startRotate = attackRotateRoot.localEulerAngles.z;
            float endRotate = Vector2.SignedAngle(attackRotateRoot.up, dir);

            while(curTime < attackRotateDelay) {
                yield return null;

                curTime += Time.deltaTime;

                float t = easeFuncRot(curTime, attackRotateDelay, 0f, 0f);

                attackRotateRoot.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startRotate, endRotate, t));
            }
        }
                        
        //attack start
        if(!string.IsNullOrEmpty(takeAttackStart)) {
            animator.Play(takeAttackStart);
            while(animator.isPlaying)
                yield return null;
        }

        //move towards target
        if(!string.IsNullOrEmpty(takeAttackMove))
            animator.Play(takeAttackMove);

        if(moveAttackDistance > 0f) {
            dist = Mathf.Max(0f, dist - moveAttackDistance);
            endPos = startPos + dir * dist;
        }

        //curDir = new Vector2(Mathf.Sign(dpos.x), 0f);
                
        float moveDelay = dist / moveAttackSpeed;

        curTime = 0f;
        while(curTime < moveDelay) {            
            yield return null;

            curTime += Time.deltaTime;

            float t = mMoveEaseFunc(curTime, moveDelay, 0f, 0f);

            position = Vector2.Lerp(startPos, endPos, t);
        }
        //

        //strike
        //curDir = new Vector2(Mathf.Sign(mAttackTarget.position.x - position.x), 0f); //re-orient

        if(!string.IsNullOrEmpty(takeAttackStrike)) {
            animator.Play(takeAttackStrike);
            while(animator.isPlaying)
                yield return null;
        }
        //

        //kill target
        mAttackTarget.state = UnitStates.instance.dead;
        mAttackTarget = null;

        //rotate back to normal
        if(attackRotateRoot) {
            //rotate towards target
            float startRotate = attackRotateRoot.localEulerAngles.z;
            float endRotate = Vector2.SignedAngle(attackRotateRoot.up, Vector2.up);

            curTime = 0f;
            while(curTime < attackRotateDelay) {
                yield return null;

                curTime += Time.deltaTime;

                float t = easeFuncRot(curTime, attackRotateDelay, 0f, 0f);

                attackRotateRoot.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startRotate, endRotate, t));
            }
        }
        //

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
        if(mCardItem != null) {
            Gizmos.color = mCardItem.card.indicatorColor;
            Gizmos.DrawWireSphere(position, mCardItem.card.indicatorRadius);
        }
    }
}
