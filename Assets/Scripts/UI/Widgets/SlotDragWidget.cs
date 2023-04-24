using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

public class SlotDragWidget : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public delegate void ClickCallback(SlotDragWidget item);
    public delegate void EventCallback(SlotDragWidget item, PointerEventData eventData);

    [Header("Display")]
    public Transform dragRoot; //point to use for drag movement
    public float dragRevertDelay = 0.3f;
    public Image image;
    public TMP_Text label;
    public GameObject dragInactiveGO;
    public GameObject correctGO;
    public GameObject incorrectGO;

    public int index { get; set; }
    public bool isDragging { get; private set; }
    public Vector2 originPointDragStart { get; private set; }
    public Vector2 originPoint { get; private set; }
    public bool isClickEnabled { get; set; }
    public bool isDragEnabled { get; set; }

    public event ClickCallback clickCallback;
    public event EventCallback dragCallback;
    public event EventCallback dragEndCallback;

    private DG.Tweening.EaseFunction mRevertEaseFunc;
    
    public void SetOrigin(Vector2 point) {
        originPoint = point;

        //stop drag (animates dragRoot towards origin)
        //ensure drag is reverted
        if(isDragging)
            SetDragging(false);
        else
            RevertDrag();
    }

    public void SetCorrect(bool correct) {
        if(correctGO) correctGO.SetActive(correct);
        if(incorrectGO) incorrectGO.SetActive(!correct);
    }
    
    public void Setup(Sprite sprite, string text) {        
        if(image) image.sprite = sprite;
        if(label) label.text = text;
    }

    public void Init(Vector2 point) {
        transform.position = originPoint = point;

        dragRoot.localPosition = Vector3.zero;

        if(correctGO) correctGO.SetActive(false);
        if(incorrectGO) incorrectGO.SetActive(false);

        if(dragInactiveGO) dragInactiveGO.SetActive(true);

        isClickEnabled = false;
        isDragEnabled = true;
    }

    void Awake() {
        mRevertEaseFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);
    }

    void OnApplicationFocus(bool focus) {
        if(!focus)
            SetDragging(false);
    }

    IEnumerator DoRevertDragRoot() {
        Vector2 start = dragRoot.localPosition;
        Vector2 end = Vector2.zero;

        float curTime = 0f;
        while(curTime < dragRevertDelay) {
            yield return null;

            curTime += Time.deltaTime;
            float t = mRevertEaseFunc(curTime, dragRevertDelay, 0f, 0f);
            dragRoot.localPosition = Vector2.Lerp(start, end, t);
        }
    }

    void RevertDrag() {
        StopAllCoroutines();

        //move self to origin and preserve dragRoot position
        var dragRootPos = dragRoot.position;
        transform.position = originPoint;
        dragRoot.position = dragRootPos;

        //animate drag root towards origin
        StartCoroutine(DoRevertDragRoot());
    }

    void SetDragging(bool dragging) {
        if(isDragging != dragging) {
            isDragging = dragging;
            if(isDragging) {
                StopAllCoroutines();

                if(correctGO) correctGO.SetActive(false);
                if(incorrectGO) incorrectGO.SetActive(false);
            }
            else {
                RevertDrag();
            }

            if(dragInactiveGO) dragInactiveGO.SetActive(!isDragging);

            originPointDragStart = originPoint;

            //set as last children in parent to ensure proper render order
            transform.SetAsLastSibling();
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(!isClickEnabled || isDragging)
            return;

        if(clickCallback != null)
            clickCallback(this);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(!isDragEnabled)
            return;

        SetDragging(true);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!isDragging)
            return;

        dragRoot.position = eventData.position;

        if(dragCallback != null)
            dragCallback(this, eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(!isDragging)
            return;

        SetDragging(false);

        if(dragEndCallback != null)
            dragEndCallback(this, eventData);
    }
}
