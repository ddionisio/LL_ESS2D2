using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEnemyMole : Unit {
    [Header("Data")]
    public float moveSpeed;
    public float growthEatMod;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeSpawn;
    public string takeGrab;
    public string takeDead;

    private UnitAllyFlower mFlowerTarget;

    private float mDirSign;
    private float mGrowthEatRate;

    public override void Hit(Unit striker) {
        state = UnitStates.instance.dead;
    }

    protected override void StateChanged() {
        base.StateChanged();

        bool bodySimulate = false;

        if(prevState == UnitStates.instance.act) {
            if(animator && !string.IsNullOrEmpty(takeGrab))
                animator.ResetTake(takeGrab);

            //revert flower growing (most likely while grabbing flower)
            if(mFlowerTarget && !mFlowerTarget.isReleased && mFlowerTarget.state == UnitStates.instance.idle) {
                mFlowerTarget.state = UnitStates.instance.grow;
                mFlowerTarget.SetMark(false);

                mFlowerTarget = null;
            }
        }

        if(state == UnitStates.instance.spawning)
            mRout = StartCoroutine(DoSpawn());
        else if(state == UnitStates.instance.idle) {
            bodySimulate = true;
        }
        else if(state == UnitStates.instance.move) {
            bodySimulate = true;
        }
        else if(state == UnitStates.instance.act) {
            if(animator && !string.IsNullOrEmpty(takeGrab))
                animator.Play(takeGrab);

            //hold flower growth
            if(mFlowerTarget)
                mFlowerTarget.state = UnitStates.instance.idle;

            mRout = StartCoroutine(DoGrabFlower());

            bodySimulate = true; //allow allies to see this mole while grabbing
        }
        else if(state == UnitStates.instance.dead) {
            isDespawnOnCycleEnd = false;
            mRout = StartCoroutine(DoDeath());
        }

        body.simulated = bodySimulate;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);
        
        //grab nearest flower to seek
        mFlowerTarget = GameController.instance.motherbase.GetNearestFlower(position.x, true);
        if(!mFlowerTarget) {
            Debug.LogWarning("No flower target, releasing this unit.");
            Release();
            return;
        }

        mFlowerTarget.SetMark(true);

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
        if(state == UnitStates.instance.move) {
            //move
            var nextPos = position + curDir * moveSpeed * Time.fixedDeltaTime;

            //check if we are at destination (slightly passed)
            if(nextPos.x == mFlowerTarget.position.x || (mDirSign < 0f && nextPos.x < mFlowerTarget.position.x) || (mDirSign > 0f && nextPos.x > mFlowerTarget.position.x)) {
                nextPos = mFlowerTarget.position;

                state = UnitStates.instance.act;
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

        state = UnitStates.instance.move;
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

        state = UnitStates.instance.despawning;
    }

    IEnumerator DoDeath() {
        if(animator && !string.IsNullOrEmpty(takeDead)) {
            animator.Play(takeDead);
            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;

        Release();
    }
    
    private void UpdatePosition(Vector2 toPos) {
        UnitPoint point;
        if(UnitPoint.GetGroundPoint(toPos, out point)) {
            ApplyUnitPoint(point);

            curDir = M8.MathUtil.Rotate(Vector2.up, mDirSign * M8.MathUtil.HalfPI);
        }
    }
}
