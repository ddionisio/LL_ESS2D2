using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEnemyHopper : Unit {
    [Header("Data")]
    public float hopDelay;
    public Vector2 hopRangeMin;
    public Vector2 hopRangeMax;
    public float hopWaitDelay;
    public Rect hopAttackArea;
    public LayerMask hopAttackLayerMask;
    [M8.TagSelector]
    public string[] hopAttackTagFilters;
    public float flowerReduceScale = 0.1f; //scale of flower's max growth

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeSpawn;
    public string takeHop;
    public string takeHopLand;
    public string takeDeath;

    private Collider2D[] mCheckColliders = new Collider2D[8];

    private float mScreenOffX;

    protected override void StateChanged() {
        base.StateChanged();

        if(state == UnitStates.instance.spawning)
            mRout = StartCoroutine(DoAnimatorToState(animator, takeSpawn, UnitStates.instance.move));
        else if(state == UnitStates.instance.idle) { //for enemies, this means being struck by ally
            isPhysicsActive = false;
        }
        else if(state == UnitStates.instance.move) {
            isPhysicsActive = true;

            mRout = StartCoroutine(DoMove());
        }
        else if(state == UnitStates.instance.despawning || state == UnitStates.instance.dead || state == UnitStates.instance.blowOff) {
            mRout = StartCoroutine(DoAnimatorToRelease(animator, takeDeath));
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        //clamp to ground
        UnitPoint groundPt;
        if(UnitPoint.GetGroundPoint(position, out groundPt)) {
            position = groundPt.position;
            up = groundPt.up;
        }
        else {
            up = Vector2.up;
        }

        if(parms != null) {
            curDir = parms.GetValue<Vector2>(UnitSpawnParams.dir);
        }

        if(curDir.x < 0f)
            mScreenOffX = GameController.instance.levelBounds.rect.xMin;
        else
            mScreenOffX = GameController.instance.levelBounds.rect.xMax;

        state = UnitStates.instance.spawning;
    }

    IEnumerator DoMove() {
        var wait = new WaitForFixedUpdate();
        var waitDelay = new WaitForSeconds(hopWaitDelay);

        while(true) {
            float xDir = Mathf.Sign(curDir.x);

            float xDist = Random.Range(hopRangeMin.x, hopRangeMax.x);
            float height = Random.Range(hopRangeMin.y, hopRangeMax.y);

            Vector2 p0 = position;
            Vector2 p1 = new Vector2(p0.x + xDir * xDist * 0.5f, p0.y + height);
            Vector2 p2 = new Vector2(p0.x + xDir * xDist, p0.y);

            Vector2 destUp = up;

            UnitPoint p2GroundPt;
            if(UnitPoint.GetGroundPoint(p2, out p2GroundPt)) {
                p2 = p2GroundPt.position;
                destUp = p2GroundPt.up;
            }

            up = Vector2.up;

            //do hop
            if(!string.IsNullOrEmpty(takeHop))
                animator.Play(takeHop);

            float curTime = 0f;
            while(curTime < hopDelay) {
                yield return wait;

                curTime += Time.fixedDeltaTime;

                float t = Mathf.Clamp01(curTime / hopDelay);

                position = M8.MathUtil.Bezier(p0, p1, p2, t);
            }

            //check if we are off screen
            if((curDir.x < 0f && position.x < mScreenOffX) || (curDir.x > 0f && position.x > mScreenOffX)) {
                mRout = null;
                Release();
                yield break;
            }
                        
            up = destUp;

            if(!string.IsNullOrEmpty(takeHopLand))
                animator.Play(takeHopLand);

            //attack area
            var hopAttackAreaWorld = hopAttackArea;
            hopAttackAreaWorld.center = position + hopAttackArea.position;

            int collCount = Physics2D.OverlapBoxNonAlloc(hopAttackAreaWorld.center, hopAttackAreaWorld.size, Vector2.SignedAngle(Vector2.up, up), mCheckColliders, hopAttackLayerMask);
            for(int i = 0; i < collCount; i++) {
                var coll = mCheckColliders[i];
                var go = coll.gameObject;

                bool isTagMatched = false;
                for(int j = 0; j < hopAttackTagFilters.Length; j++) {
                    if(go.CompareTag(hopAttackTagFilters[j])) {
                        isTagMatched = true;
                        break;
                    }
                }

                if(!isTagMatched)
                    continue;

                var unit = go.GetComponent<Unit>();
                if(!unit || unit.isReleased || (unit.flags & Flags.PoisonImmune) != Flags.None)
                    continue;

                if(unit is UnitAllyFlower) {
                    //reduce flower growth
                    var flowerUnit = (UnitAllyFlower)unit;

                    float growthAmt = flowerUnit.growthMax * flowerReduceScale;
                    flowerUnit.ApplyGrowth(-growthAmt);
                    if(flowerUnit.growth <= 0f)
                        flowerUnit.Release(); //kill flower
                }
                else //despawn (TODO: death? (for visual purpose))
                    unit.state = UnitStates.instance.despawning;
            }

            yield return waitDelay;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;

        var hopAttackAreaWorld = hopAttackArea;
        hopAttackAreaWorld.center = position + hopAttackArea.position;

        Gizmos.DrawWireCube(hopAttackAreaWorld.center, hopAttackAreaWorld.size);
    }
}
