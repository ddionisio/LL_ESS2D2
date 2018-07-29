using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : M8.EntityBase {
    [Header("Display")]
    public GameObject displayRootGO;
    public GameObject spawnRootGO;

    public Rigidbody2D body { get; private set; }

    public bool isMarked { get { return mMarkCounter > 0; } }

    public virtual Unit target { get { return null; }  }

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
    
    protected Coroutine mRout;

    private int mMarkCounter;

    /// <summary>
    /// Increase/decrease mark counter, make sure to called with marked=false at some point
    /// </summary>
    public void SetMark(bool marked) {
        if(marked)
            mMarkCounter++;
        else
            mMarkCounter--;
    }

    public bool GetGroundPoint(out UnitPoint point) {
        return UnitPoint.GetGroundPoint(position, out point);
    }

    public void ApplyUnitPoint(UnitPoint point) {
        if(body && body.simulated) {
            body.position = point.position;

            float dirSign = Mathf.Sign(point.up.x);

            body.rotation = dirSign * Vector2.Angle(Vector2.up, point.up);
        }
        else {
            transform.position = point.position;
            transform.up = point.up;
        }
    }

    public virtual void SetDisplayActive(bool active) {
        if(displayRootGO)
            displayRootGO.SetActive(active);
    }

    protected void StopRoutine() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    protected override void StateChanged() {
        StopRoutine();

        //spawn states are set via Motherbase or EnemyController
        if(state == UnitStates.instance.spawning) {
            if(spawnRootGO)
                spawnRootGO.SetActive(true);
        }
        else if(state == UnitStates.instance.normal) {
            if(spawnRootGO)
                spawnRootGO.SetActive(false);

            SetDisplayActive(true);
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        SetDisplayActive(false);

        if(spawnRootGO)
            spawnRootGO.SetActive(false);

        if(body) {
            body.rotation = 0f;
            body.simulated = false;
        }

        mMarkCounter = 0;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //set position if available
        if(parms != null && parms.ContainsKey(UnitSpawnParams.position))
            position = parms.GetValue<Vector2>(UnitSpawnParams.position);
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        body = GetComponent<Rigidbody2D>();
        if(body)
            body.simulated = false;

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
}
