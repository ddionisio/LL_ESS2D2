using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEnemyFlying : Unit {
    [Header("Data")]
    public int feedCount = 3;
    public float moveSpeed = 1.5f;
    public float moveSpeedLeave = 4f;
    public float moveHeightRangeMin = 0.5f;
    public float moveHeightRangeMax = 1.5f;
    public int moveWaveCount = 4;
    public float flowerReduceScale = 0.1f; //scale of flower's max growth
    public float leaveOfs = 1.5f;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeAct;
    public string takeMoveIn;
    public string takeLeave;

    private UnitAllyFlower mFlowerTarget;

    private Vector2 mStartPosition;
    private Vector2 mMoveTarget;
    private float mCurMoveTime;
    private float mCurMoveDelay;
    private float mCurMoveHeight;
    private float mSinLength;
    private int mCurFeedCount;

    protected override void StateChanged() {
        base.StateChanged();
        
        if(state == UnitStates.instance.idle) { //for enemies, this means being struck by ally
            isPhysicsActive = false;
        }
        if(state == UnitStates.instance.move) {
            mMoveTarget = new Vector2(mFlowerTarget.position.x, mStartPosition.y);

            var dX = mMoveTarget.x - mStartPosition.x;

            curDir = new Vector2(Mathf.Sign(dX), 0f);

            float dist = Mathf.Abs(dX); //(mMoveTarget - mStartPosition).magnitude;
            mCurMoveDelay = dist / moveSpeed;

            mCurMoveHeight = Random.Range(moveHeightRangeMin, moveHeightRangeMax);

            mCurMoveTime = 0f;

            isPhysicsActive = true;
        }
        else if(state == UnitStates.instance.act) {
            mRout = StartCoroutine(DoAct());

            isPhysicsActive = true;
        }
        else if(state == UnitStates.instance.despawning) {
            mRout = StartCoroutine(DoLeave());
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        mFlowerTarget = null;
        mCurFeedCount = 0;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        position = mStartPosition = parms.GetValue<Vector2>(UnitSpawnParams.position);
                
        UpdateFlowerTarget();
        if(!mFlowerTarget) {
            Debug.LogWarning("No flower target, despawning.");
            Release();
            return;
        }

        //move out
        state = UnitStates.instance.move;
    }

    private void UpdateFlowerTarget() {
        //grab random flower
        var flowers = GameController.instance.motherbase.GetFlowersExcept(mFlowerTarget, false);
        if(flowers.Count == 0) {
            mFlowerTarget = null;
        }
        else {
            mFlowerTarget = flowers[Random.Range(0, flowers.Count)];
        }
    }

    protected override void Awake() {
        base.Awake();

        mSinLength = moveWaveCount * Mathf.PI;
    }

    void FixedUpdate() {
        if(state == UnitStates.instance.move) {
            mCurMoveTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(mCurMoveTime / mCurMoveDelay);

            var newPos = new Vector2(Mathf.Lerp(mStartPosition.x, mMoveTarget.x, t), mMoveTarget.y + mCurMoveHeight * Mathf.Sin(t * mSinLength));

            position = newPos;

            if(mCurMoveTime >= mCurMoveDelay) {
                state = UnitStates.instance.act;
            }
        }
    }

    IEnumerator DoAct() {
        yield return null;

        Vector2 startPos = position;

        //grab flower point
        var flowerPoint = mFlowerTarget.topPosition;

        float curDelay = (flowerPoint - startPos).magnitude / moveSpeedLeave;

        if(!string.IsNullOrEmpty(takeMoveIn))
            animator.Play(takeMoveIn);

        //move towards flower point
        float curTime = 0f;
        while(curTime < curDelay) {
            yield return new WaitForFixedUpdate();

            curTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(curTime / curDelay);

            position = Vector2.Lerp(startPos, flowerPoint, t);
        }

        //play animation, remain on point
        if(!string.IsNullOrEmpty(takeAct)) {
            animator.Play(takeAct);
            while(animator.isPlaying)
                yield return null;
        }

        if(mFlowerTarget.isReleased) {
            mFlowerTarget = null;
        }
        else {
            float growthAmt = mFlowerTarget.growthMax * flowerReduceScale;
            mFlowerTarget.ApplyGrowth(-growthAmt);
            if(mFlowerTarget.growth <= 0f) {
                //kill flower
                mFlowerTarget.Release();
                mFlowerTarget = null;
            }
        }

        //update to next flower target
        UpdateFlowerTarget();
        
        if(mFlowerTarget) {
            mCurFeedCount++;
            if(mCurFeedCount < feedCount) {
                //move back up and go to next flower
                if(!string.IsNullOrEmpty(takeLeave))
                    animator.Play(takeLeave);
                                
                curTime = 0f;
                while(curTime < curDelay) {
                    yield return new WaitForFixedUpdate();

                    curTime += Time.fixedDeltaTime;
                    float t = Mathf.Clamp01(curTime / curDelay);

                    position = Vector2.Lerp(flowerPoint, startPos, t);
                }

                mRout = null;

                mStartPosition = position;
                state = UnitStates.instance.move;
            }
            else
                state = UnitStates.instance.despawning;
        }
        else
            state = UnitStates.instance.despawning;
    }

    IEnumerator DoLeave() {
        if(!string.IsNullOrEmpty(takeLeave))
            animator.Play(takeLeave);

        Vector2 startPos = position;
        Vector2 leavePos = new Vector2(startPos.x, GameController.instance.levelBounds.rect.yMax + leaveOfs);

        float dist = Mathf.Abs(leavePos.y - startPos.y);
        float delay = dist / moveSpeedLeave;

        float curTime = 0f;
        while(curTime < delay) {
            yield return null;

            curTime += Time.deltaTime;
            float t = Mathf.Clamp01(curTime / delay);

            position = Vector2.Lerp(startPos, leavePos, t);
        }

        Release();
    }
}
