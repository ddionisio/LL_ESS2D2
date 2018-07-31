using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAllyGardener : UnitCard {
    [Header("Data")]
    public float moveSpeed;
    public float flowerGrowthMod;
    public float weedReduceGrowthMod;

    [Header("Check Data")]
    public float checkRadius = 1f;
    public float checkDelay = 0.5f;
    public LayerMask checkLayerMask;

    private Collider2D[] mCheckColliders = new Collider2D[8];
    private int mCheckColliderCount;

    public override void MotherbaseSpawnFinish() {
        state = UnitStates.instance.move;
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(state == UnitStates.instance.idle) {
            mRout = StartCoroutine(DoGardening());
        }
        else if(state == UnitStates.instance.move) {
            //determine dir
            var dpos = targetPosition - position;
            curDir = new Vector2(Mathf.Sign(dpos.x), 0f);
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

            UpdatePosition(nextPos);
        }
    }

    IEnumerator DoGardening() {
        while(true) {
            mCheckColliderCount = Physics2D.OverlapCircleNonAlloc(transform.position, checkRadius, mCheckColliders, checkLayerMask);
            for(int i = 0; i < mCheckColliderCount; i++) {
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
                }
                else if(unit is UnitEnemyWeed) { //weed
                    var weed = (UnitEnemyWeed)unit;
                    if(weed.isGrowing) {
                        float growthReduceRate = weed.GetGrowthRate(weedReduceGrowthMod);
                        weed.ApplyGrowth(-growthReduceRate * checkDelay);
                    }
                }
            }

            yield return new WaitForSeconds(checkDelay);
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

    void OnDrawGizmos() {
        Gizmos.color = new Color(0.25f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}
