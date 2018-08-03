using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEnemyElement : Unit {
    [Header("Data")]
    public float checkLength; //width relative to up vector
    public LayerMask checkBlockerLayerMask;
    public float checkBlockerDelay;
    public LayerMask checkAffectLayerMask;
    public float checkAffectDelay;
    public float despawnDelay;

    [Header("Display")]
    public ParticleSystem particle;

    private float mCheckBlockerCurTime;
    private float mCheckAffectCurTime;
    private float mCheckDistance;

    private Collider2D[] mCheckColls = new Collider2D[16];
    private M8.CacheList<UnitAllyFlower> mCheckFlowers = new M8.CacheList<UnitAllyFlower>(16);

    protected override void StateChanged() {
        base.StateChanged();

        if(prevState == UnitStates.instance.act) {
            if(particle) {
                var fxmod = particle.main;
                fxmod.loop = false;
            }
        }

        if(state == UnitStates.instance.act) {
            if(particle) {
                var fxmod = particle.main;
                fxmod.loop = true;

                particle.Play();
            }

            mCheckBlockerCurTime = 0f;
            mCheckAffectCurTime = 0f;

            mCheckDistance = GameController.instance.levelBounds.rect.width * 2.0f;
        }
        else if(state == UnitStates.instance.despawning) {
            mRout = StartCoroutine(DoDespawn());
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();

        mCheckFlowers.Clear();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);

        if(parms != null) {
            if(parms.ContainsKey(UnitSpawnParams.dir))
                curDir = parms.GetValue<Vector2>(UnitSpawnParams.dir);
            else
                curDir = Vector2.up;
        }

        up = curDir;

        state = UnitStates.instance.act;
    }

    void Update() {
        if(state == UnitStates.instance.act) {
            float dt = Time.deltaTime;

            //adjust check length by blocker
            mCheckBlockerCurTime += dt;
            if(mCheckBlockerCurTime >= checkBlockerDelay) {
                mCheckBlockerCurTime = 0f;

                float checkDist = GameController.instance.levelBounds.rect.width * 2.0f;

                var hit = Physics2D.BoxCast(position, new Vector2(checkLength, 1f), Vector2.SignedAngle(Vector2.up, curDir), curDir, checkDist, checkBlockerLayerMask);
                if(hit.collider)
                    mCheckDistance = hit.distance;
                else
                    mCheckDistance = checkDist;

                //update particle dimension
                if(particle) {

                }
            }

            //check for entities to affect
            mCheckAffectCurTime += dt;
            if(mCheckAffectCurTime >= checkAffectDelay) {
                mCheckAffectCurTime = 0f;

                mCheckFlowers.Clear();

                var checkCenter = position + curDir * (mCheckDistance * 0.5f);
                var collCount = Physics2D.OverlapBoxNonAlloc(checkCenter, new Vector2(checkLength, mCheckDistance), Vector2.SignedAngle(Vector2.up, curDir), mCheckColls, checkAffectLayerMask);
                for(int i = 0; i < collCount; i++) {
                    var unit = mCheckColls[i].GetComponent<Unit>();
                    if(!unit || (unit.flags & Flags.ElementImmune) != Flags.None)
                        continue;

                    //special treatment to flowers
                    if(unit is UnitAllyFlower)
                        mCheckFlowers.Add((UnitAllyFlower)unit);
                    else {
                        unit.curDir = new Vector2(Mathf.Sign(curDir.x), 0f);
                        unit.state = UnitStates.instance.blowOff;
                    }
                }

                //get tallest flower
                UnitAllyFlower targetFlower = null;

                for(int i = 0; i < mCheckFlowers.Count; i++) {
                    var flower = mCheckFlowers[i];
                    if(targetFlower == null || flower.growth > targetFlower.growth)
                        targetFlower = flower;
                }

                if(targetFlower) {
                    targetFlower.curDir = new Vector2(Mathf.Sign(curDir.x), 0f);
                    targetFlower.state = UnitStates.instance.blowOff;
                }
                //
            }
        }
    }
    
    IEnumerator DoDespawn() {
        yield return new WaitForSeconds(despawnDelay);

        mRout = null;
        Release();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(
            transform.TransformPoint(new Vector3(-checkLength * 0.5f, 0f, 0f)),
            transform.TransformPoint(new Vector3(checkLength * 0.5f, 0f, 0f)));
    }
}
