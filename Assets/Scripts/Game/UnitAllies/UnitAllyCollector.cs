using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAllyCollector : UnitCard {
    [Header("Data")]
    public float moveSpeed = 4f;
    public float moveReturnSpeed = 2f; //once we collect
    public float collectCheckRadius;
    public LayerMask collectCheckLayerMask;
    public float collectCheckDelay = 0.3f;

    [Header("Collect Display")]
    public Transform collectRoot; //attach collect here

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeMove;
    public string takeMoveReturn;

    private Collider2D[] mCheckColliders = new Collider2D[4];
    private int mCheckColliderCount;

    private UnitAllyFlower mTargetFlowerReturn;

    private UnitCollect mCollect;
    private Transform mCollectPrevParent;

    public override void MotherbaseSpawnFinish() {
        const float offScreenOfs = 1f;

        //set target position to be at end of screen
        curDir = new Vector2(Mathf.Sign(position.x - GameController.instance.motherbase.transform.position.x), position.y);
        if(curDir.x < 0f)
            targetPosition = new Vector2(GameController.instance.levelBounds.rect.xMin - offScreenOfs, position.y);
        else
            targetPosition = new Vector2(GameController.instance.levelBounds.rect.xMax + offScreenOfs, position.y);

        state = UnitStates.instance.move;
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(prevState == UnitStates.instance.move) {
            DetachCollect();
        }

        if(state == UnitStates.instance.move) {
            if(mTargetFlowerReturn)
                animator.Play(takeMoveReturn);
            else {
                mRout = StartCoroutine(DoCollectCheck());
                animator.Play(takeMove);
            }
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        if(mCollect) { //fail-safe, there shouldn't be a collect at this point
            mCollect.Release();
            mCollect = null;
        }

        mCollectPrevParent = null;

        mTargetFlowerReturn = null;
    }

    void FixedUpdate() {
        if(state == UnitStates.instance.move) {
            float curMoveSpeed = mTargetFlowerReturn ? moveReturnSpeed : moveSpeed;

            //move
            var nextPos = position + curDir * curMoveSpeed * Time.fixedDeltaTime;

            //check if we are at destination (slightly passed)
            float dirSign = Mathf.Sign(curDir.x);
            if(nextPos.x == targetPosition.x || (dirSign < 0f && nextPos.x < targetPosition.x) || (dirSign > 0f && nextPos.x > targetPosition.x)) {
                nextPos = targetPosition;

                //check if we are replenishing a flower
                if(mTargetFlowerReturn) {
                    if(!mTargetFlowerReturn.isReleased && mCollect) {
                        float growthDelta = mTargetFlowerReturn.growthMax * mCollect.flowerGrowthScale;
                        mTargetFlowerReturn.ApplyGrowth(growthDelta);
                    }

                    mTargetFlowerReturn = null;

                    //release collect
                    if(mCollect) {
                        mCollect.Release();
                        mCollect = null;
                    }

                    state = UnitStates.instance.despawning;
                }
                else
                    Release();
            }

            UpdatePosition(nextPos);
        }
    }

    IEnumerator DoCollectCheck() {
        var wait = new WaitForSeconds(collectCheckDelay);

        while(true) {
            var checkColliderCount = Physics2D.OverlapCircleNonAlloc(transform.position, collectCheckRadius, mCheckColliders, collectCheckLayerMask);
            for(int i = 0; i < checkColliderCount; i++) {
                var coll = mCheckColliders[i];

                //check tag
                if(!mCardItem.card.IsTargetValid(coll.gameObject))
                    continue;

                //check if collectible
                var unitCollect = coll.GetComponent<UnitCollect>();
                if(!unitCollect)
                    continue;

                //attach collect
                mCollect = unitCollect;
                mCollectPrevParent = mCollect.transform.parent;

                mCollect.transform.SetParent(collectRoot);
                mCollect.transform.localPosition = Vector3.zero;

                mCollect.state = UnitStates.instance.idle;

                //move to flower with lowest growth
                mTargetFlowerReturn = GameController.instance.motherbase.GetFlowerLowestGrowth();
                if(mTargetFlowerReturn) {
                    targetPosition = mTargetFlowerReturn.position;

                    curDir = new Vector2(Mathf.Sign(targetPosition.x - position.x), 0f);
                }

                animator.Play(takeMoveReturn);

                mRout = null;
                yield break;
            }

            yield return wait;
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

    private void DetachCollect() {
        if(mCollect) {
            if(!mCollect.isReleased) {
                mCollect.transform.SetParent(mCollectPrevParent, true);
                mCollect.transform.position = position;
                mCollect.state = UnitStates.instance.act;
            }
            mCollect = null;
        }

        mCollectPrevParent = null;
        mTargetFlowerReturn = null;
    }

    void OnDrawGizmos() {
        Gizmos.color = new Color(0.5f, 1f, 0.0f);
        Gizmos.DrawWireSphere(transform.position, collectCheckRadius);
    }
}
