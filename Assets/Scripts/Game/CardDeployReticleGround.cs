using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeployReticleGround : CardDeployReticle {
    [Header("Ground Display")]
    public Transform groundTargetRoot;
    public Transform pointerRoot;
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
        UnitPoint.GetGroundPoint(pos, out groundPt);

        groundTargetRoot.position = groundPt.position;

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
        float groundToPointerScaleY = groundPt.position.y - pos.y;
        if(groundToPointerScaleY != 0f) {
            groundToPointerSprite.gameObject.SetActive(true);

            var t = groundToPointerSprite.transform;
            t.position = pos;

            var s = t.localScale;
            s.y = groundToPointerScaleY * mGroundToPointerUnitHeightRatio;

            t.localScale = s;
        }
        else {
            groundToPointerSprite.gameObject.SetActive(false);
        }
    }
}
