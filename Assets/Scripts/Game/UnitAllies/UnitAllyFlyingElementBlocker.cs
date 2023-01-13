using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAllyFlyingElementBlocker : UnitCard {
    [Header("Data")]
    public float moveSpeed;
    public float targetOffscreenOfs = 1.5f;
    public string[] targetTemplates;

    protected M8.PoolController targetPool {
        get {
            if(mTargetPool == null)
                mTargetPool = M8.PoolController.GetPool(WeatherCycleSpawner.poolGroup);
            return mTargetPool;
        }
    }

    private DG.Tweening.EaseFunction mMoveEaseFunc;

    private Vector2 mMoveStartPos;
    private float mMoveCurTime;
    private float mMoveDelay;

    private Coroutine mTargetCheckRout;
    private M8.PoolController mTargetPool;

    public override void MotherbaseSpawnFinish() {
        state = UnitStates.instance.move;

        mTargetCheckRout = StartCoroutine(DoFindTarget());

        //add callback to weather end
        GameController.instance.weatherCycle.weatherEndCallback += OnWeatherEnd;
    }

    protected override void StateChanged() {
        base.StateChanged();

        if(state == UnitStates.instance.move) {
            ApplyMoveToTargetPos();
        }
    }

    protected override void OnDespawned() {
        if(GameController.isInstantiated && GameController.instance.weatherCycle)
            GameController.instance.weatherCycle.weatherEndCallback -= OnWeatherEnd;

        if(mTargetCheckRout != null) {
            StopCoroutine(mTargetCheckRout);
            mTargetCheckRout = null;
        }

        mTargetPool = null;

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

    IEnumerator DoFindTarget() {
        var wait = new WaitForSeconds(0.5f);

        var pool = targetPool;

        bool isTargetFound = false;

        while(true) {

            for(int i = 0; !isTargetFound && i < targetTemplates.Length; i++) {
                var actives = pool.GetActiveList(targetTemplates[i]);
                for(int j = 0; j < actives.Count; j++) {
                    var ent = actives[j];
                    if(ent) {
                        var enemyElem = ent.GetComponent<UnitEnemyElement>();
                        if(enemyElem) {
                            MoveToTarget(enemyElem);
                            isTargetFound = true;
                            break;
                        }
                    }
                }
            }

            if(isTargetFound)
                break;

            yield return wait;
        }

        mTargetCheckRout = null;
    }

    void OnWeatherEnd() {
        //despawn
        state = UnitStates.instance.despawning;
    }

    private void ApplyMoveToTargetPos() {
        mMoveStartPos = position;

        var dpos = targetPosition - mMoveStartPos;

        float dist = dpos.magnitude;

        mMoveCurTime = 0f;
        mMoveDelay = dist / moveSpeed;

        curDir = new Vector2(Mathf.Sign(targetPosition.x - GameController.instance.motherbase.transform.position.x), 0f);
    }

    private void MoveToTarget(Unit target) {
        float destX;

        var dposX = target.position.x - position.x;
        if(dposX < 0f)
            destX = GameController.instance.levelBounds.rect.xMin + targetOffscreenOfs;
        else
            destX = GameController.instance.levelBounds.rect.xMax - targetOffscreenOfs;

        targetPosition = new Vector2(destX, targetPosition.y);

        if(state == UnitStates.instance.move)
            ApplyMoveToTargetPos();
        else
            state = UnitStates.instance.move;
    }
}
