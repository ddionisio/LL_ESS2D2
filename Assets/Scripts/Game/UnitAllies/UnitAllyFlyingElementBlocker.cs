using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAllyFlyingElementBlocker : UnitCard {
    [Header("Data")]
    public float moveSpeed;

    private DG.Tweening.EaseFunction mMoveEaseFunc;

    private Vector2 mMoveStartPos;
    private float mMoveCurTime;
    private float mMoveDelay;

    public override void MotherbaseSpawnFinish() {
        state = UnitStates.instance.move;

        //add callback to weather end
        GameController.instance.weatherCycle.weatherEndCallback += OnWeatherEnd;
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(state == UnitStates.instance.move) {
            mMoveStartPos = position;

            var dpos = targetPosition - mMoveStartPos;

            float dist = dpos.magnitude;

            mMoveCurTime = 0f;
            mMoveDelay = dist / moveSpeed;

            curDir = new Vector2(Mathf.Sign(targetPosition.x - GameController.instance.motherbase.transform.position.x), 0f);
        }
    }

    protected override void OnDespawned() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle)
            GameController.instance.weatherCycle.weatherEndCallback -= OnWeatherEnd;

        base.OnDespawned();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        base.OnSpawned(parms);
    }

    protected override void Awake() {
        base.Awake();

        mMoveEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.InOutSine);
    }

    void FixedUpdate() {
        if(state == UnitStates.instance.move) {
            mMoveCurTime += Time.fixedDeltaTime;

            float t = mMoveEaseFunc(mMoveCurTime, mMoveDelay, 0f, 0f);

            position = Vector2.Lerp(mMoveStartPos, targetPosition, t);

            if(mMoveCurTime >= mMoveDelay)
                state = UnitStates.instance.idle;
        }
    }

    void OnWeatherEnd() {
        //despawn
        state = UnitStates.instance.despawning;
    }
}
