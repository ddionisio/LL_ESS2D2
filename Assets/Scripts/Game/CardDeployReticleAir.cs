using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeployReticleAir : CardDeployReticle {
    public Transform pointerRoot;
    public GameObject pointerInvalidGO;

    public float groundToAirOfs = 2.0f;

    public override bool canDeploy {
        get {
            return mCanDeploy;
        }
    }

    private bool mCanDeploy;

    protected override void DoUpdatePosition(Vector2 pos) {
        pointerRoot.position = pos;
        mTargetPos = pos;

        //check if target position is above ground
        bool canDeploy = false;

        UnitPoint groundPt;
        if(UnitPoint.GetGroundPoint(pos, out groundPt)) {
            canDeploy = pos.y >= groundPt.position.y + groundToAirOfs;
        }

        SetCanDeploy(canDeploy);
    }

    protected override void Awake() {
        mCanDeploy = true;
        if(pointerInvalidGO) pointerInvalidGO.SetActive(false);
    }

    private void SetCanDeploy(bool deploy) {
        if(mCanDeploy != deploy) {
            mCanDeploy = deploy;

            if(pointerInvalidGO) pointerInvalidGO.SetActive(!mCanDeploy);
        }
    }
}