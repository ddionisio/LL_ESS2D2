using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEnemyWeed : Unit {
    public const string growthModId = "weed";

    [Header("Data")]
    public float flowerReduceGrowthMod;
    public float initialGrowth;
    public float growthDelay = 4f; //how long it takes to get to maximum growth

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeEnter;
    public string takeGrow;

    public bool isGrowing { get { return state == UnitStates.instance.act; } }

    private float mGrowthRate;
    private float mCurGrowth;

    private UnitAllyFlower mFlowerTarget;
    private bool mIsFlowerGrowth;

    private float mTakeGrowTotalTime;

    /// <summary>
    /// mod is a scale relative to growth rate, usu. [0, 1]
    /// </summary>
    public float GetGrowthRate(float mod) {
        return mGrowthRate * mod;
    }

    /// <summary>
    /// Give minus delta to shrink, will despawn if mCurGrowth reaches 0
    /// </summary>
    public void ApplyGrowth(float growthDelta) {
        var newGrowth = Mathf.Clamp01(mCurGrowth + growthDelta);
        if(mCurGrowth != newGrowth) {
            mCurGrowth = newGrowth;
            if(mCurGrowth > 0f) {
                if(animator.currentPlayingTakeIndex != -1)
                    animator.Goto(mCurGrowth * mTakeGrowTotalTime);
                else
                    animator.Goto(takeGrow, mCurGrowth * mTakeGrowTotalTime);
            }
            else //die
                state = UnitStates.instance.despawning;
        }
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(prevState == UnitStates.instance.act) {
            animator.ResetTake(takeGrow);

            ApplyFlowerGrowth(false);
        }

        if(state == UnitStates.instance.spawning) {
            mRout = StartCoroutine(DoAnimatorToState(animator, takeEnter, UnitStates.instance.act));
        }
        else if(state == UnitStates.instance.act) {
            ApplyFlowerGrowth(true);

            ApplyGrowth(initialGrowth);

            isPhysicsActive = true;
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        ApplyFlowerGrowth(false);

        mFlowerTarget = null;

        mCurGrowth = 0f;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        animator.ResetTake(takeGrow);

        mFlowerTarget = parms.GetValue<UnitAllyFlower>(UnitSpawnParams.unitTarget);

        state = UnitStates.instance.spawning;
    }

    protected override void Awake() {
        base.Awake();

        mGrowthRate = 1.0f / growthDelay;

        mTakeGrowTotalTime = animator.GetTakeTotalTime(takeGrow);
    }

    void Update() {
        if(state == UnitStates.instance.act) {
            //flower gone, release ourself
            if(!mFlowerTarget || mFlowerTarget.isReleased) {
                ApplyGrowth(0f);
                return;
            }

            float growthDelta = mGrowthRate * Time.deltaTime;
            ApplyGrowth(growthDelta);
        }
    }
    
    private void ApplyFlowerGrowth(bool isGrow) {
        if(mIsFlowerGrowth != isGrow) {
            mIsFlowerGrowth = isGrow;

            if(mFlowerTarget == null || mFlowerTarget.isReleased)
                return;

            if(mIsFlowerGrowth)
                mFlowerTarget.ApplyGrowthMod(growthModId, -flowerReduceGrowthMod);
            else
                mFlowerTarget.RemoveGrowthMod(growthModId);

            mFlowerTarget.SetMark(mIsFlowerGrowth);
        }
    }
}
