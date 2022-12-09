using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotDropWidget : MonoBehaviour {
    public delegate void DropCallback(SlotDropWidget slot, SlotDragWidget prevItem);

    [Header("Data")]
    [M8.TagSelector]
    public string dragTag;

    [Header("Display")]
    public GameObject highlightGO;
    
    public SlotDragWidget droppedWidget { get; private set; }
    
    public void SetDroppedWidget(SlotDragWidget item) {
        droppedWidget = item;
        if(droppedWidget)
            droppedWidget.SetOrigin(transform.position);
    }

    public void Init(Vector2 point) {
        if(highlightGO) highlightGO.SetActive(false);

        transform.position = point;

        droppedWidget = null;
    }

    void OnDisable() {
        if(highlightGO) highlightGO.SetActive(false);

        droppedWidget = null;
    }
}
