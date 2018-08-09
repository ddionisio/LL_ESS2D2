using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEnemyMole : Unit {
    [Header("Data")]
    public float moveSpeed;
    public float flowerEatScale;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeSpawn;
    public string takeGrab;
    public string takeDead;

    private UnitAllyFlower mFlowerTarget;

    private float mDirSign;

    protected override void StateChanged() {
        base.StateChanged();
        
        if(prevState == UnitStates.instance.act) {
            if(animator && !string.IsNullOrEmpty(takeGrab))
                animator.ResetTake(takeGrab);

            ClearFlower();
        }
        else if(prevState == UnitStates.instance.move && state != UnitStates.instance.act) {
            ClearFlower();
        }

        if(state == UnitStates.instance.spawning)
            mRout = StartCoroutine(DoAnimatorToState(animator, takeSpawn, UnitStates.instance.move));
        else if(state == UnitStates.instance.idle) { //for enemies, this means being struck by ally
            isPhysicsActive = false;
        }
        else if(state == UnitStates.instance.move) {
            isPhysicsActive = true;
        }
        else if(state == UnitStates.instance.act) {
            if(animator && !string.IsNullOrEmpty(takeGrab))
                animator.Play(takeGrab);
            
            mRout = StartCoroutine(DoGrabFlower());

            isPhysicsActive = true; //allow allies to see this mole while grabbing
        }
        else if(state == UnitStates.instance.dead) {
            mRout = StartCoroutine(DoAnimatorToRelease(animator, takeDead));
        }
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        //grab nearest flower to seek
        mFlowerTarget = parms.GetValue<UnitAllyFlower>(UnitSpawnParams.unitTarget);
        if(!mFlowerTarget) {
            Debug.LogWarning("No flower target, releasing this unit.");
            Release();
            return;
        }

        mFlowerTarget.SetMark(true);

        //determine move dir based on flower target
        mDirSign = Mathf.Sign(mFlowerTarget.position.x - position.x);
        
        //apply initial position/dir
        UpdatePosition(position);

        state = UnitStates.instance.spawning;
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        ClearFlower();
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
    
    IEnumerator DoGrabFlower() {
        //eat up the flower's growth until it is 0
        //mGrowthEatRate
        while(!mFlowerTarget.isReleased && mFlowerTarget.growth > 0f) {
            float growthDelta = mFlowerTarget.growthMax * flowerEatScale;
            mFlowerTarget.ApplyGrowth(-growthDelta * Time.deltaTime);
            if(mFlowerTarget.growth <= 0f)
                break;

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

    private void ClearFlower() {
        if(mFlowerTarget) {
            if(!mFlowerTarget.isReleased)
                mFlowerTarget.SetMark(false);

            mFlowerTarget = null;
        }
    }
    
    private void UpdatePosition(Vector2 toPos) {
        UnitPoint point;
        if(UnitPoint.GetGroundPoint(toPos, out point)) {
            ApplyUnitPoint(point);

            curDir = M8.MathUtil.Rotate(Vector2.up, mDirSign * M8.MathUtil.HalfPI);
        }
    }
}
