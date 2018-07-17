using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeController : MonoBehaviour {
    [Header("Mode Info")]
    public GameMode mode;
    public GameModeSignal signalModeChanged;

    protected virtual void Awake() {
        if(signalModeChanged)
            signalModeChanged.Invoke(mode);
    }
}
