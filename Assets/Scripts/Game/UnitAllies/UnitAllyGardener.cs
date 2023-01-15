using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAllyGardener : UnitCard {
    [Header("Data")]
    public float moveSpeed;
    public float flowerGrowthMod;
    public float weedReduceGrowthMod;

    [Header("Check Data")]
    public float checkDelay = 0.5f;
    public LayerMask checkLayerMask;
    public string targetTemplateCheck = "weed";

    [Header("Display")]
    public GameObject idleGOActive;

    protected M8.PoolController targetPool {
        get {
            if(mTargetPool == null)
                mTargetPool = M8.PoolController.GetPool(WeatherCycleSpawner.poolGroup);
            return mTargetPool;
        }
    }

    private Collider2D[] mCheckColliders = new Collider2D[8];

    private Unit mTargetUnit;
    private Coroutine mTargetCheckRout;
    private M8.PoolController mTargetPool;

    public override void MotherbaseSpawnFinish() {
        StartTargetCheck();
        
        state = UnitStates.instance.move;

        //reorient target position to nearest flower
        SetTargetPositionToNearestFlower();
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(prevState == UnitStates.instance.idle) {
            if(idleGOActive) idleGOActive.SetActive(false);
        }

        if(state == UnitStates.instance.idle) {
            mRout = StartCoroutine(DoGardening());

            if(idleGOActive) idleGOActive.SetActive(true);
        }
        else if(state == UnitStates.instance.move) {
            if(mRout != null) {
                StopCoroutine(mRout);
                mRout = null;
            }

            //determine dir
            var dpos = targetPosition - position;
            curDir = new Vector2(Mathf.Sign(dpos.x), 0f);
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        StopTargetCheck();

        mTargetPool = null;

        if(idleGOActive) idleGOActive.SetActive(false);
    }

    protected override void Awake() {
        base.Awake();

        if(idleGOActive) idleGOActive.SetActive(false);
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

    IEnumerator DoTargetCheck() {
        var wait = new WaitForSeconds(checkDelay);

        if(!targetPool) {
            mTargetCheckRout = null;
            yield break;
        }

        var activeList = targetPool.GetActiveList(targetTemplateCheck);

        while(!mTargetUnit) {
            if(activeList.Count > 0) {
                Unit newTarget = null;
                float newTargetDistX = float.MaxValue;
                Vector2 newTargetPos = position;

                for(int i = 0; i < activeList.Count; i++) {
                    var ent = activeList[i];
                    if(ent) {
                        Vector2 entPos = ent.transform.position;
                        var distX = Mathf.Abs(entPos.x - position.x);
                        if(distX < newTargetDistX) {
                            var entUnit = ent.GetComponent<Unit>();
                            if(entUnit && !entUnit.isMarked) {
                                newTarget = entUnit;
                                newTargetDistX = distX;
                                newTargetPos = entPos;
                            }
                        }
                    }
                }

                //move to new target
                if(newTarget) {
                    mTargetUnit = newTarget;
                    mTargetUnit.SetMark(true);

                    targetPosition = newTargetPos;

                    if(state == UnitStates.instance.move) {
                        var dpos = targetPosition - position;
                        curDir = new Vector2(Mathf.Sign(dpos.x), 0f);
                    }
                    else
                        state = UnitStates.instance.move;
                    break;
                }
            }
            
            yield return wait;
        }

        mTargetCheckRout = null;
    }

    IEnumerator DoGardening() {
        var wait = new WaitForSeconds(checkDelay);

        while(true) {
            int flowerCount = 0;

            var checkColliderCount = Physics2D.OverlapCircleNonAlloc(transform.position, mCardItem.card.indicatorRadius, mCheckColliders, checkLayerMask);
            for(int i = 0; i < checkColliderCount; i++) {
                var coll = mCheckColliders[i];

                //check tag
                if(!mCardItem.card.IsTargetValid(coll.gameObject))
                    continue;

                //check if unit
                var unit = coll.GetComponent<Unit>();
                if(!unit)
                    continue;

                //check if flower
                if(unit is UnitAllyFlower) {
                    var flower = (UnitAllyFlower)unit;
                    
                    if(flower.isBlossomed) //can't grow blossomed
                        continue;

                    float growRate = flower.GetGrowthRate(flowerGrowthMod);

                    flower.ApplyGrowth(growRate * checkDelay);

                    if(flower.growth < flower.growthMax)
                        flowerCount++;
                }
                else if(unit is UnitEnemyWeed) { //weed
                    var weed = (UnitEnemyWeed)unit;
                    if(weed.isGrowing) {
                        float growthReduceRate = weed.GetGrowthRate(weedReduceGrowthMod);
                        weed.ApplyGrowth(-growthReduceRate * checkDelay);
                    }

                    if(mTargetUnit != weed) {
                        ClearTarget();
                        mTargetUnit = weed;
                        mTargetUnit.SetMark(true);
                    }
                }
            }

            //find new flower to blossom
            if(flowerCount == 0)
                SetTargetPositionToNearestFlower();

            if(!mTargetUnit || mTargetUnit.isReleased)
                StartTargetCheck();

            yield return wait;
        }
    }

    private void ClearTarget() {
        if(mTargetUnit) {
            mTargetUnit.SetMark(false);
            mTargetUnit = null;
        }
    }

    private void StartTargetCheck() {
        ClearTarget();

        if(mTargetCheckRout == null)
            mTargetCheckRout = StartCoroutine(DoTargetCheck());
    }

    private void StopTargetCheck() {
        ClearTarget();

        if(mTargetCheckRout != null) {
            StopCoroutine(mTargetCheckRout);
            mTargetCheckRout = null;
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

    private void SetTargetPositionToNearestFlower() {
        if(mTargetUnit && mTargetUnit.isMarked)
            return;

        if(!GameController.isInstantiated)
            return;

        var motherbase = GameController.instance.motherbase;
        if(!motherbase)
            return;

        var flower = motherbase.GetFlowerGrowingNear(position.x);
        if(flower) {
            targetPosition = flower.position;

            if(state == UnitStates.instance.move) {
                var dpos = targetPosition - position;
                curDir = new Vector2(Mathf.Sign(dpos.x), 0f);
            }
            else
                state = UnitStates.instance.move;
        }
    }

    void OnDrawGizmos() {
        if(mCardItem != null) {
            Gizmos.color = new Color(0.5f, 1f, 0.0f);
            Gizmos.DrawWireSphere(transform.position, mCardItem.card.indicatorRadius);
        }
    }
}
