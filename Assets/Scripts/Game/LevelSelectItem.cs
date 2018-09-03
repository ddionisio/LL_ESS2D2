using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelSelectItem : MonoBehaviour, IPointerClickHandler {
    [Header("Data")]
    public LevelLocationData data;

    [Header("Display")]
    public GameObject activeGO;
    public GameObject inactiveGO;
    
    [Header("Signals")]
    public SignalBoolean signalActive;
    public SignalLevelLocationData signalOnClicked;

    private Collider2D mColl;

    void OnDisable() {
        if(signalActive) signalActive.callback -= OnSignalActive;
    }

    void OnEnable() {
        if(!mColl)
            mColl = GetComponent<Collider2D>();
        
        //initialize as inactive
        OnSignalActive(false);

        if(signalActive) signalActive.callback += OnSignalActive;
    }

    void OnSignalActive(bool active) {
        if(mColl) mColl.enabled = active;

        if(activeGO) activeGO.SetActive(active);
        if(inactiveGO) inactiveGO.SetActive(!active);        
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(signalOnClicked)
            signalOnClicked.Invoke(data);
    }
}
