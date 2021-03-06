﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handle despawn to release
/// </summary>
public class UnitDespawningRelease : MonoBehaviour {
    public enum BlowOffMode {
        BlowOff,
        Despawn,
        Death
    }

    [Header("Flags")]
    public bool isDespawnEnable = true;
    public bool isDeathEnable = true;
    public bool isBlowOffEnable = true;

    [Header("Blow Off")]
    public BlowOffMode blowOffMode = BlowOffMode.BlowOff;
    public float blowOffEndOfs = 2.0f;
    public float blowOffDelay = 1f;
    public float blowOffHeight = 3.0f;
    
    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takeDespawn;
    public string takeDeath;
    public string takeBlowOff;
    
    private Unit mUnit;

    void Awake() {
        mUnit = GetComponent<Unit>();
        mUnit.setStateCallback += OnEntityStateChanged;

        if(animator)
            animator.takeCompleteCallback += OnAnimatorTakeComplete;
    }

    void OnEntityStateChanged(M8.EntityBase ent) {
        if(ent.state == UnitStates.instance.despawning) {
            if(isDespawnEnable)
                Despawn(takeDespawn);
        }
        else if(ent.state == UnitStates.instance.dead) {
            if(isDeathEnable)
                Despawn(takeDeath);
        }
        else if(ent.state == UnitStates.instance.blowOff) {
            if(isBlowOffEnable) {
                switch(blowOffMode) {
                    case BlowOffMode.BlowOff:
                        StartCoroutine(DoBlowOff());
                        break;
                    case BlowOffMode.Despawn:
                        Despawn(takeDespawn);
                        break;
                    case BlowOffMode.Death:
                        Despawn(takeDeath);
                        break;
                }
            }
        }
    }

    void Despawn(string takeName) {
        if(animator && !string.IsNullOrEmpty(takeName))
            animator.Play(takeName);
        else
            mUnit.Release();
    }

    void OnAnimatorTakeComplete(M8.Animator.Animate anim, M8.Animator.Take take) {
        if(take.name == takeDespawn || take.name == takeDeath) {
            mUnit.Release();
        }
    }

    IEnumerator DoBlowOff() {
        if(animator && !string.IsNullOrEmpty(takeBlowOff))
            animator.Play(takeBlowOff);

        var trans = transform;

        //set waypoints
        Vector2 p0 = trans.position;

        var levelRect = GameController.instance.levelBounds.rect;

        float endX;
        if(Mathf.Sign(mUnit.curDir.x) < 0.0f) {
            endX = levelRect.xMin - blowOffEndOfs;
        }
        else {
            endX = levelRect.xMax + blowOffEndOfs;
        }

        Vector2 p1 = new Vector2(Mathf.Lerp(p0.x, endX, 0.5f), p0.y + blowOffHeight);
        Vector2 p2 = new Vector2(endX, p0.y);

        float curTime = 0f;
        while(curTime < blowOffDelay) {
            yield return null;

            curTime += Time.deltaTime;
            float t = Mathf.Clamp01(curTime / blowOffDelay);
            trans.position = M8.MathUtil.Bezier(p0, p1, p2, t);
        }

        mUnit.Release();
    }
}
