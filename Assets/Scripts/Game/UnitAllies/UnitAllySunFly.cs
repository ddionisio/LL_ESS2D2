using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAllySunFly : UnitCard {
    [Header("Data")]
    public float moveSpeed;
    public LayerMask checkLayerMask;
    [M8.TagSelector]
    public float checkDelay = 0.3f;
    public float flowerGrowthMod; //scale of growth rate, growDelta = growRate*flowerGrowthMod*checkDelay
    public float weedReduceGrowthMod;

    [Header("Display")]
    public GameObject glowGO;

    private DG.Tweening.EaseFunction mMoveEaseFunc;

    private Vector2 mMoveStartPos;
    private float mMoveCurTime;
    private float mMoveDelay;

    private float mCurTime;

    private Collider2D[] mCheckColliders = new Collider2D[8];

    public override void MotherbaseSpawnFinish() {        
        state = UnitStates.instance.move;
    }

    protected override void StateChanged() {
        base.StateChanged();

        bool showGlow = false;

        if(state == UnitStates.instance.idle) {
            showGlow = true;
        }
        if(state == UnitStates.instance.move) {
            mMoveStartPos = position;

            var dpos = targetPosition - mMoveStartPos;

            float dist = dpos.magnitude;

            mMoveCurTime = 0f;
            mMoveDelay = dist / moveSpeed;

            curDir = new Vector2(Mathf.Sign(targetPosition.x - GameController.instance.motherbase.transform.position.x), 0f);

            showGlow = true;
        }

        if(glowGO) glowGO.SetActive(showGlow);
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        if(glowGO) glowGO.SetActive(false);
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        mCurTime = 0f;
    }

    protected override void Awake() {
        base.Awake();

        if(glowGO) glowGO.SetActive(false);

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

        if(state == UnitStates.instance.idle || state == UnitStates.instance.move) {
            mCurTime += Time.fixedDeltaTime;
            if(mCurTime >= checkDelay) {
                int collCount = Physics2D.OverlapCircleNonAlloc(position, mCardItem.card.indicatorRadius, mCheckColliders, checkLayerMask);
                for(int i = 0; i < collCount; i++) {
                    var coll = mCheckColliders[i];
                    if(!mCardItem.card.IsTargetValid(coll.gameObject))
                        continue;

                    var unit = coll.GetComponent<Unit>();
                    if(!unit)
                        continue;

                    if(unit is UnitAllyFlower) {
                        var flowerUnit = (UnitAllyFlower)unit;
                        if(!flowerUnit)
                            continue;

                        //can't grow flower if infected with weed
                        if(flowerUnit.ContainsGrowthMod(UnitEnemyWeed.growthModId))
                            continue;

                        float growthDelta = flowerUnit.GetGrowthRate(flowerGrowthMod) * mCurTime;

                        flowerUnit.ApplyGrowth(growthDelta);
                    }
                    else if(unit is UnitEnemyWeed) {
                        //reduce weed growth
                        var weed = (UnitEnemyWeed)unit;
                        if(weed.isGrowing) {
                            float growthReduceRate = weed.GetGrowthRate(weedReduceGrowthMod);
                            weed.ApplyGrowth(-growthReduceRate * checkDelay);
                        }
                    }
                }

                mCurTime = 0f;
            }
        }
    }

    void OnDrawGizmos() {
        if(mCardItem != null) {
            Gizmos.color = mCardItem.card.indicatorColor;
            Gizmos.DrawWireSphere(position, mCardItem.card.indicatorRadius);
        }
    }
}
