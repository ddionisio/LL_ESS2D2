using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectActiveFlag : MonoBehaviour {
    public GameObject rootGO;
    public string key;
    public bool defaultActive;
    public bool isActiveFlag;

    [Header("Signals")]
    public SignalGameFlag signalGameFlagChanged;

    void OnEnable() {
        UpdateState();
    }

    void OnDestroy() {
        if(signalGameFlagChanged) signalGameFlagChanged.callback -= OnSignalFlagChanged;
    }

    void Awake() {        
        if(rootGO) rootGO.SetActive(defaultActive);

        if(signalGameFlagChanged) signalGameFlagChanged.callback += OnSignalFlagChanged;
    }

    void UpdateState() {
        int flag = GameData.instance.GetFlag(key);

        if(rootGO) rootGO.SetActive(flag == 1 ? isActiveFlag : !isActiveFlag);
    }

    void OnSignalFlagChanged(string key, int flag) {
        if(key != this.key)
            return;

        if(rootGO) rootGO.SetActive(flag == 1 ? isActiveFlag : !isActiveFlag);
    }
}
