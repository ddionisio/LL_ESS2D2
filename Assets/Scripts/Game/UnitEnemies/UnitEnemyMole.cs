using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEnemyMole : Unit {
    [Header("Data")]
    public LayerMask groundLayerMask;
    public float moveSpeed;
    public float growthEatMod;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeSpawn;
    public string takeMove;
    public string takeGrab;
    public string takeLeave;

    public override Unit target { get { return mFlowerTarget; } }

    private UnitAllyFlower mFlowerTarget;

    private Vector2 mCurMoveDir;
    private float mDirSign;
    private float mGrowthEatRate;

    protected override void StateChanged() {
        base.StateChanged();

        if(animator && animator.isPlaying)
            animator.Stop();

        bool bodySimulate = false;

        if(prevState == UnitStates.instance.grab) {
            if(animator && !string.IsNullOrEmpty(takeGrab))
                animator.ResetTake(takeGrab);

            //revert flower growing (most likely while grabbing flower)
            if(mFlowerTarget && !mFlowerTarget.isReleased && mFlowerTarget.state == UnitStates.instance.hold)
                mFlowerTarget.state = UnitStates.instance.normal;
        }

        if(state == UnitStates.instance.spawning)
            mRout = StartCoroutine(DoSpawn());
        else if(state == UnitStates.instance.normal) {
            if(animator && !string.IsNullOrEmpty(takeMove))
                animator.Play(takeMove);

            bodySimulate = true;
        }
        else if(state == UnitStates.instance.grab) {
            if(animator && !string.IsNullOrEmpty(takeGrab))
                animator.Play(takeGrab);

            //hold flower growth
            if(mFlowerTarget)
                mFlowerTarget.state = UnitStates.instance.hold;

            mRout = StartCoroutine(DoGrabFlower());

            bodySimulate = true; //allow allies to see this mole while grabbing
        }
        else if(state == UnitStates.instance.leave) {
            mRout = StartCoroutine(DoLeave());
        }

        body.simulated = bodySimulate;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);
        
        //grab nearest flower to seek
        mFlowerTarget = GameController.instance.motherbase.GetNearestFlower(position.x);
        if(!mFlowerTarget) {
            Debug.LogWarning("No flower target, releasing this unit.");
            Release();
            return;
        }

        //determine move dir based on flower target
        mDirSign = Mathf.Sign(mFlowerTarget.position.x - position.x);

        mGrowthEatRate = mFlowerTarget.GetGrowthRate(growthEatMod);

        //apply initial position/dir
        UpdatePosition(position);

        state = UnitStates.instance.spawning;
    }

    protected override void OnDespawned() {
        base.OnDespawned();
    }

    void FixedUpdate() {
        if(state == UnitStates.instance.normal) {
            //move
            var nextPos = position + mCurMoveDir * moveSpeed * Time.fixedDeltaTime;

            //check if we are at destination (slightly passed)
            if(nextPos.x == mFlowerTarget.position.x || (mDirSign < 0f && nextPos.x < mFlowerTarget.position.x) || (mDirSign > 0f && nextPos.x > mFlowerTarget.position.x)) {
                nextPos = mFlowerTarget.position;

                state = UnitStates.instance.grab;
            }

            UpdatePosition(nextPos);
        }
    }

    IEnumerator DoSpawn() {
        if(animator && !string.IsNullOrEmpty(takeSpawn)) {
            animator.Play(takeSpawn);
            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;

        state = UnitStates.instance.normal;
    }

    IEnumerator DoGrabFlower() {
        //eat up the flower's growth until it is 0
        //mGrowthEatRate
        while(mFlowerTarget.growth > 0f) {
            mFlowerTarget.ApplyGrowth(-mGrowthEatRate * Time.deltaTime);
            yield return null;
        }

        mRout = null;

        //kill flower
        if(mFlowerTarget) {
            mFlowerTarget.Release();
            mFlowerTarget = null;
        }

        state = UnitStates.instance.leave;
    }

    IEnumerator DoLeave() {
        if(animator && !string.IsNullOrEmpty(takeLeave)) {
            animator.Play(takeLeave);
            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;

        //we are done
        Release();
    }

    private void UpdatePosition(Vector2 toPos) {
        UnitPoint point;
        if(UnitPoint.GetGroundPoint(toPos, groundLayerMask, out point)) {
            ApplyUnitPoint(point);

            mCurMoveDir = M8.MathUtil.Rotate(Vector2.up, mDirSign * M8.MathUtil.HalfPI);
        }
    }
}
