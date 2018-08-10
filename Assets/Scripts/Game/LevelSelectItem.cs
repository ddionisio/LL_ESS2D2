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

    public M8.SpriteColorGroup spriteColorGroup;
    public Color spriteColorInactive = new Color(0.5f, 0.5f, 0.5f, 0.75f);

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

        if(spriteColorGroup)
            spriteColorGroup.Init();

        //initialize as inactive
        OnSignalActive(false);

        if(signalActive) signalActive.callback += OnSignalActive;
    }

    void OnSignalActive(bool active) {
        if(mColl) mColl.enabled = active;

        if(activeGO) activeGO.SetActive(active);
        if(inactiveGO) inactiveGO.SetActive(!active);

        if(spriteColorGroup) {
            if(active)
                spriteColorGroup.Revert();
            else
                spriteColorGroup.ApplyColor(spriteColorInactive);
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(signalOnClicked)
            signalOnClicked.Invoke(data);
    }
}
