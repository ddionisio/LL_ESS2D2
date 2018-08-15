using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotDropWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler {
    public delegate void DropCallback(SlotDropWidget slot, SlotDragWidget prevItem);

    [Header("Data")]
    [M8.TagSelector]
    public string dragTag;

    [Header("Display")]
    public GameObject highlightGO;

    public int index { get; set; }

    public SlotDragWidget droppedWidget { get; private set; }

    public event DropCallback dropCallback; //SlotDragWidget is previous dragged widget

    public void SetDroppedWidget(SlotDragWidget item) {
        droppedWidget = item;
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

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        bool isValidDragItem = eventData.dragging && eventData.pointerDrag && eventData.pointerDrag.CompareTag(dragTag);
        if(highlightGO) highlightGO.SetActive(isValidDragItem);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(highlightGO) highlightGO.SetActive(false);
    }

    void IDropHandler.OnDrop(PointerEventData eventData) {
        if(highlightGO) highlightGO.SetActive(false);

        if(eventData.pointerDrag && eventData.pointerDrag.CompareTag(dragTag)) {
            var prevDroppedWidget = droppedWidget;

            droppedWidget = eventData.pointerDrag.GetComponent<SlotDragWidget>();
            droppedWidget.SetOrigin(transform.position);

            if(droppedWidget != prevDroppedWidget) {
                if(dropCallback != null)
                    dropCallback(this, prevDroppedWidget);
            }
        }
    }
}
