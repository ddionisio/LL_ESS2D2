using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotDropWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler {
    [Header("Data")]
    [M8.TagSelector]
    public string dragTag;

    [Header("Display")]
    public GameObject highlightGO;

    public int index { get; private set; }

    public SlotDragWidget droppedWidget { get; private set; }

    public event System.Action<SlotDropWidget, SlotDragWidget> dropCallback; //SlotDragWidget is previous dragged widget

    public void Init(int index, Vector3 point) {
        this.index = index;

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
            var prevDragWidget = droppedWidget;

            droppedWidget = eventData.pointerDrag.GetComponent<SlotDragWidget>();
            droppedWidget.SetOrigin(transform.position);

            if(dropCallback != null)
                dropCallback(this, prevDragWidget);
        }
    }
}
