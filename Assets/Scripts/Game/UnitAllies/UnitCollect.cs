using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCollect : Unit {
    [Header("Data")]
    public float flowerGrowthScale = 0.1f; //percentage of maxgrowth to be added to flower
    public float despawnDelay = 8f;
    public float despawnWarningDelay = 3f; //ensure this is less than despawnDelay
    public float spawnFallSpeed = 5f;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeSpawn;
    public string takeIdle;
    public string takeDespawning; //close to despawn

    protected override void StateChanged() {
        base.StateChanged();

        if(state == UnitStates.instance.spawning) {
            mRout = StartCoroutine(DoSpawn());
        }
        else if(state == UnitStates.instance.idle) {
            isPhysicsActive = false;

            if(!string.IsNullOrEmpty(takeIdle))
                animator.Play(takeIdle);
        }
        else if(state == UnitStates.instance.act) {
            isPhysicsActive = true;

            mRout = StartCoroutine(DoAct());
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        state = UnitStates.instance.spawning;
    }

    IEnumerator DoSpawn() {
        if(!string.IsNullOrEmpty(takeSpawn))
            animator.Play(takeSpawn);

        //fall to ground
        if(spawnFallSpeed > 0f) {
            var startPos = position;

            UnitPoint groundPt;
            if(UnitPoint.GetGroundPoint(startPos, out groundPt)) {
                float groundPosY = groundPt.position.y;
                if(startPos.y > groundPosY) {
                    float curTime = 0f;
                    float delay = (startPos.y - groundPosY) / spawnFallSpeed;
                    while(curTime < delay) {
                        yield return null;

                        curTime += Time.deltaTime;

                        float t = Mathf.Clamp01(curTime / delay);

                        position = new Vector2(startPos.x, Mathf.Lerp(startPos.y, groundPosY, t));
                    }
                }
            }
        }

        while(animator.isPlaying)
            yield return null;

        mRout = null;
        state = UnitStates.instance.act;
    }

    IEnumerator DoAct() {
        if(!string.IsNullOrEmpty(takeIdle))
            animator.Play(takeIdle);

        yield return new WaitForSeconds(despawnDelay - despawnWarningDelay);

        if(!string.IsNullOrEmpty(takeDespawning))
            animator.Play(takeDespawning);

        yield return new WaitForSeconds(despawnWarningDelay);

        mRout = null;
        state = UnitStates.instance.despawning;
    }
}
