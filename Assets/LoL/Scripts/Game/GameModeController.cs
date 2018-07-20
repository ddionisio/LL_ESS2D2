using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameModeController<T> : M8.SingletonBehaviour<T> where T : MonoBehaviour {
    [Header("Mode Info")]
    public GameMode mode;
    public GameModeSignal signalModeChanged;

    protected override void OnInstanceInit() {
        //ensure UIRoot exists
        if(!UIRoot.isInstantiated)
            UIRoot.Reinstantiate();

        if(signalModeChanged)
            signalModeChanged.Invoke(mode);
    }

    protected override void OnInstanceDeinit() {
        //invalidate mode
        if(signalModeChanged)
            signalModeChanged.Invoke(null);
    }
}
