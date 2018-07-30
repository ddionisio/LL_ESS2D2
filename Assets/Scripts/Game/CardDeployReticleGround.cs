using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeployReticleGround : CardDeployReticle {
    [Header("Ground Display")]    
    public Transform pointerRoot;
    public SpriteRenderer groundTargetSprite;
    public SpriteRenderer groundToPointerSprite; //line display between ground and pointer, ensure anchor is bottom

    private Vector2 mIconOfsDefault;
    private float mGroundToPointerUnitHeightRatio;
    
    protected override void Awake() {
        base.Awake();

        mIconOfsDefault = iconSprite.transform.localPosition;

        mGroundToPointerUnitHeightRatio = groundToPointerSprite.sprite.pixelsPerUnit / groundToPointerSprite.sprite.rect.height;
    }

    protected override void DoUpdatePosition(Vector2 pos) {
        //set ground position
        UnitPoint groundPt;

        if(UnitPoint.GetGroundPoint(pos, out groundPt))
            mTargetPos = groundPt.position;
        else
            mTargetPos = pos;

        groundTargetSprite.transform.position = groundPt.position;

        //set pointer position
        pointerRoot.position = pos;

        //determine icon position (if above ground, set relative to ground; pointer otherwise)
        var iconTrans = iconSprite.transform;

        if(groundPt.position.y >= pos.y) {
            iconTrans.position = groundPt.position + mIconOfsDefault;
        }
        else {
            iconTrans.position = pos + mIconOfsDefault;
        }

        //set line display
        float groundToPointerScaleY = pos.y - groundPt.position.y;
        if(groundToPointerScaleY > 0f) {
            groundToPointerSprite.gameObject.SetActive(true);

            var t = groundToPointerSprite.transform;

            float groundTargetHeightUnit = groundTargetSprite.sprite.rect.height / groundTargetSprite.sprite.pixelsPerUnit;
            float ofsY = groundTargetHeightUnit - (groundTargetHeightUnit * groundTargetSprite.sprite.pivot.y);

            ofsY *= groundTargetSprite.transform.localScale.y;

            var groundToPointerStart = groundTargetSprite.transform.position;
            groundToPointerStart.y += ofsY;

            t.position = groundToPointerStart;

            var s = t.localScale;
            s.y = (groundToPointerScaleY - ofsY) * mGroundToPointerUnitHeightRatio;

            t.localScale = s;
        }
        else {
            groundToPointerSprite.gameObject.SetActive(false);
        }
    }
}
