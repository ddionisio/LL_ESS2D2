using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEnemyFungus : Unit {
    [Header("Spawn Data")]
    public float spawnOfsX = 0.1f;
    public float spawnRandOfsX = 0.2f; //slight deviation from starting position

    [Header("Gas Data")]
    public Rect gasArea;
    public float gasStartDelay = 2f;
    public int gasCheckCount = 5;
    public float gasCheckDelay = 0.333f;
    public LayerMask gasLayerMask; //flowers and allies
    [M8.TagSelector]
    public string[] gasTagFilters;
    public GameObject gasActiveGO;
    public bool gasPlayAnimPerCheck; //play takeGas for each check

    [Header("Stats")]
    public float flowerReduceScale = 0.1f; //scale of flower's max growth

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeSpawn;
    public string takeIdle;
    public string takeAct;
    public string takeGas; //during gas active (loop)

    private Collider2D[] mCollCache = new Collider2D[8];
    private float mGasCheckRot;

    protected override void StateChanged() {
        base.StateChanged();

        if(gasActiveGO) gasActiveGO.SetActive(false);

        if(state == UnitStates.instance.spawning) {
            mRout = StartCoroutine(DoAnimatorToState(animator, takeSpawn, UnitStates.instance.act));
        }
        else if(state == UnitStates.instance.idle) { //just play idle
            if(!string.IsNullOrEmpty(takeIdle))
                animator.Play(takeIdle);

            isPhysicsActive = false;
        }
        else if(state == UnitStates.instance.act) {
            mRout = StartCoroutine(DoAct());

            isPhysicsActive = true;
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        if(gasActiveGO) gasActiveGO.SetActive(false);
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        //offset position slightly
        var pos = position;

        float randOfsX = Random.Range(-spawnRandOfsX, spawnRandOfsX);
        pos.x += randOfsX + Mathf.Sign(randOfsX)*spawnOfsX;

        //apply up value based on ground tilt
        UnitPoint groundPos;
        if(UnitPoint.GetGroundPoint(pos, out groundPos)) {
            position = groundPos.position;
            up = groundPos.up;
            mGasCheckRot = Vector2.SignedAngle(Vector2.up, groundPos.up);
        }
        else {
            position = pos;
            up = Vector2.up;
            mGasCheckRot = 0f;
        }

        state = UnitStates.instance.spawning;
    }

    protected override void Awake() {
        base.Awake();

        if(gasActiveGO) gasActiveGO.SetActive(false);
    }
        
    IEnumerator DoAct() {
        var waitStart = new WaitForSeconds(gasStartDelay);
        var waitCheck = new WaitForSeconds(gasCheckDelay);

        var gasAreaWorld = gasArea;
        gasAreaWorld.center = position + gasArea.position;

        while(true) {
            if(!string.IsNullOrEmpty(takeIdle))
                animator.Play(takeIdle);
            
            //wait a bit
            yield return waitStart;

            //play act
            if(!string.IsNullOrEmpty(takeAct)) {
                animator.Play(takeAct);
                while(animator.isPlaying)
                    yield return null;
            }

            //gas things
            if(!string.IsNullOrEmpty(takeGas)) {
                animator.Play(takeGas);
            }
            else if(!string.IsNullOrEmpty(takeIdle))
                animator.Play(takeIdle);

            if(gasActiveGO) gasActiveGO.SetActive(true);
                        
            for(int i = 0; i < gasCheckCount; i++) {
                int collCount = Physics2D.OverlapBoxNonAlloc(gasAreaWorld.center, gasAreaWorld.size, mGasCheckRot, mCollCache, gasLayerMask);
                for(int collInd = 0; collInd < collCount; collInd++) {
                    var coll = mCollCache[collInd];
                    var go = coll.gameObject;

                    //check tag
                    bool isTagMatched = false;
                    for(int tagInd = 0; tagInd < gasTagFilters.Length; tagInd++) {
                        if(go.CompareTag(gasTagFilters[tagInd])) {
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
                    else {
                        unit.state = UnitStates.instance.dead;
                    }
                }


                yield return waitCheck;

                if(gasPlayAnimPerCheck) {
                    if(!string.IsNullOrEmpty(takeGas))
                        animator.Play(takeGas);
                }
            }

            if(gasActiveGO) gasActiveGO.SetActive(false);
            //
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;

        var gasAreaWorld = gasArea;
        gasAreaWorld.center = position + gasArea.position;

        Gizmos.DrawWireCube(gasAreaWorld.center, gasAreaWorld.size);
    }
}
