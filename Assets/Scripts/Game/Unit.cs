using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : M8.EntityBase {
    [System.Flags]
    public enum Flags {
        None = 0x0,
        PoisonImmune = 0x1,
        PhysicalImmune = 0x2,
        WindImmune = 0x4
    }

    [Header("Data")]
    [M8.EnumMask]
    public Flags flags;

    [Header("Display")]
    public GameObject displayRootGO;
    public GameObject spawnRootGO;

    public Rigidbody2D body { get; private set; }
    public Collider2D coll { get; private set; }

    public bool isMarked { get { return mMarkCounter > 0; } }

    public Vector2 curDir {
        get { return mCurDir; }
        protected set {
            if(mCurDir != value) {
                mCurDir = value;

                if(dirChangedCallback != null)
                    dirChangedCallback(mCurDir);
            }
        }
    }

    public Vector2 position {
        get {
            return body && body.simulated ? body.position : (Vector2)transform.position;
        }

        set {
            if(body && body.simulated)
                body.position = value;
            else
                transform.position = value;
        }
    }

    public Vector2 up {
        get {
            return body && body.simulated ? M8.MathUtil.RotateAngle(Vector2.up, body.rotation) : (Vector2)transform.up;
        }

        set {
            if(body && body.simulated) {
                float dirSign = Mathf.Sign(value.x);
                body.rotation = dirSign * Vector2.Angle(Vector2.up, value);
            }
            else
                transform.up = value;
        }
    }

    public bool isDespawnOnCycleEnd {
        get { return mIsDespawnOnCycleEnd; }
        set {
            if(mIsDespawnOnCycleEnd != value) {
                mIsDespawnOnCycleEnd = value;
                if(mIsDespawnOnCycleEnd) {
                    GameController.instance.weatherCycle.cycleEndCallback += OnWeatherCycleEnd;
                }
                else {
                    if(GameController.isInstantiated && GameController.instance.weatherCycle)
                        GameController.instance.weatherCycle.cycleEndCallback -= OnWeatherCycleEnd;
                }
            }
        }
    }

    public bool isPhysicsActive {
        get { return mIsPhysicsActive; }
        set {
            if(mIsPhysicsActive != value) {
                mIsPhysicsActive = value;
                ApplyPhysicsActive();
            }
        }
    }

    public event System.Action<Vector2> dirChangedCallback;
    
    protected Coroutine mRout;

    private int mMarkCounter;
    private Vector2 mCurDir;
    private bool mIsDespawnOnCycleEnd;
    private bool mIsPhysicsActive;

    /// <summary>
    /// Increase/decrease mark counter, make sure to called with marked=false at some point
    /// </summary>
    public void SetMark(bool marked) {
        if(marked)
            mMarkCounter++;
        else
            mMarkCounter--;
    }

    public void ApplyUnitPoint(UnitPoint point) {
        position = point.position;
        up = point.up;
    }

    public virtual void SetDisplayActive(bool active) {
        if(displayRootGO)
            displayRootGO.SetActive(active);
    }

    /// <summary>
    /// Called during Motherbase after spawn has completed for this unit
    /// </summary>
    public virtual void MotherbaseSpawnFinish() {

    }

    protected void StopRoutine() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    protected override void StateChanged() {
        StopRoutine();

        if(prevState == UnitStates.instance.spawning) {
            if(spawnRootGO)
                spawnRootGO.SetActive(false);

            SetDisplayActive(true);
        }

        //spawn states are set via Motherbase or EnemyController
        if(state == UnitStates.instance.spawning) {
            if(spawnRootGO)
                spawnRootGO.SetActive(true);

            isPhysicsActive = false;
        }
        else if(state == UnitStates.instance.despawning) {
            isPhysicsActive = false;
        }
        else if(state == UnitStates.instance.dead) {
            isPhysicsActive = false;
            isDespawnOnCycleEnd = false;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        SetDisplayActive(false);

        if(spawnRootGO)
            spawnRootGO.SetActive(false);

        isPhysicsActive = false;

        mMarkCounter = 0;

        isDespawnOnCycleEnd = false;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //set position if available
        if(parms != null) {
            if(parms.ContainsKey(UnitSpawnParams.position))
                position = parms.GetValue<Vector2>(UnitSpawnParams.position);

            isDespawnOnCycleEnd = parms.GetValue<bool>(UnitSpawnParams.despawnOnCycleEnd);
        }
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        body = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();

        mIsPhysicsActive = false;
        ApplyPhysicsActive();

        //initialize data/variables
        SetDisplayActive(false);

        if(spawnRootGO)
            spawnRootGO.SetActive(false);
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    protected IEnumerator DoAnimatorToState(M8.Animator.Animate animator, string take, M8.EntityState toState) {
        if(!string.IsNullOrEmpty(take)) {
            animator.Play(take);
            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;
        state = toState;
    }

    protected IEnumerator DoAnimatorToRelease(M8.Animator.Animate animator, string take) {
        if(!string.IsNullOrEmpty(take)) {
            animator.Play(take);
            while(animator.isPlaying)
                yield return null;
        }

        mRout = null;
        Release();
    }

    private void ApplyPhysicsActive() {
        if(body) {
            body.simulated = mIsPhysicsActive;

            if(!mIsPhysicsActive) { //reset some values
                body.rotation = 0f;
            }
        }
        else if(coll)
            coll.enabled = mIsPhysicsActive;
    }

    void OnWeatherCycleEnd() {
        isDespawnOnCycleEnd = false;
        state = UnitStates.instance.despawning;
    }
}
