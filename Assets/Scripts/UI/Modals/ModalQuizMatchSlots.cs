using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ModalQuizMatchSlots : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IClose {    
    [System.Serializable]
    public class SlotData {
        public ClimateZoneData zone;
        public RectTransform anchor;
        
        public SlotDropWidget widget { get; private set; }

        public void Init(SlotDropWidget template) {
            if(!widget) {
                widget = Instantiate(template, anchor.parent);
                widget.gameObject.SetActive(true);
            }

            widget.Init(anchor.position);
        }
    }

    [System.Serializable]
    public class ItemData {
        public Transform anchor;
        public ClimateData climate;

        public SlotDragWidget widget { get; private set; }

        public void Init(SlotDragWidget template, SlotDragWidget.ClickCallback clickCallback, SlotDragWidget.EventCallback dragCallback, SlotDragWidget.EventCallback dragEndCallback) {
            if(!widget) {
                widget = Instantiate(template, anchor.parent);
                widget.name = climate.name;                
                widget.Setup(climate.image, climate.titleText);
                widget.clickCallback += clickCallback;
                widget.dragCallback += dragCallback;
                widget.dragEndCallback += dragEndCallback;
            }

            widget.gameObject.SetActive(true);
            widget.Init(anchor.position);
        }
    }

    [Header("Data")]
    public SlotDragWidget itemTemplate;
    public SlotDropWidget slotTemplate;
    public string modalInfo;

    [Header("Slot Info")]
    public SlotData[] slots;

    [Header("Item Info")]
    public ItemData[] items;

    [Header("Display")]
    public GameScoreWidget gameScoreWidget;
    public Selectable commitAnswerInteract;
    public bool commitAnswerUseVisibility; //if true, hide commitActiveGO when not all items have been slotted
    public GameObject commitActiveGO;
    public GameObject progressResultGO;
    public GameObject progressActiveGO;    
    
    private int mCorrectCount;

    private M8.GenericParams mInfoParms = new M8.GenericParams();

    private int mCurSlotDragIndex;

    private Transform[] mItemsAnchorCache;
            
    public void CommitAnswers() {
        if(commitActiveGO) commitActiveGO.SetActive(false);
        if(progressActiveGO) progressActiveGO.SetActive(true);
        if(progressResultGO) progressResultGO.SetActive(true);

        int correctCount = 0;

        for(int i = 0; i < slots.Length; i++) {
            var slot = slots[i];
            var item = items[slot.widget.droppedWidget.index];

            bool isCorrect = item.climate.zone == slot.zone;

            slot.widget.droppedWidget.SetCorrect(isCorrect);
            slot.widget.droppedWidget.isClickEnabled = true;
            slot.widget.droppedWidget.isDragEnabled = false;

            if(isCorrect)
                correctCount++;
        }

        //add points
        Debug.Log("Correct Count: " + correctCount);

        int score = correctCount * GameData.instance.scoreMatchPerCorrect;

        LoLManager.instance.curScore += score;

        gameScoreWidget.Play(score);
    }

    void Awake() {
        //initialize data
        mItemsAnchorCache = new Transform[items.Length];
        for(int i = 0; i < items.Length; i++) {
            mItemsAnchorCache[i] = items[i].anchor;
        }

        itemTemplate.gameObject.SetActive(false);
        slotTemplate.gameObject.SetActive(false);
    }

    void M8.UIModal.Interface.IClose.Close() {
        //hide items
        for(int i = 0; i < items.Length; i++) {
            if(items[i].widget)
                items[i].widget.gameObject.SetActive(false);
        }
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        //mix up item positions
        M8.ArrayUtil.Shuffle(mItemsAnchorCache);

        for(int i = 0; i < items.Length; i++)
            items[i].anchor = mItemsAnchorCache[i];
        //

        for(int i = 0; i < slots.Length; i++)
            slots[i].Init(slotTemplate);

        for(int i = 0; i < items.Length; i++) {
            items[i].Init(itemTemplate, OnItemClick, OnItemDrag, OnItemDragEnd);

            items[i].widget.index = i;
            items[i].widget.isClickEnabled = true;
        }

        //apply mode
        if(commitAnswerInteract) commitAnswerInteract.interactable = false;
        if(commitActiveGO) commitActiveGO.SetActive(!commitAnswerUseVisibility);
        if(progressActiveGO) progressActiveGO.SetActive(false);
        if(progressResultGO) progressResultGO.SetActive(false);

        mCurSlotDragIndex = -1;
    }
    
    void OnItemDrag(SlotDragWidget itemWidget, PointerEventData eventData) {
        //determine which potential slot to drop
        int slotIndex = -1;
        for(int i = 0; i < slots.Length; i++) {
            var slot = slots[i];

            var rect = slot.anchor.rect;
            rect.position = slot.anchor.TransformPoint(rect.position);

            if(rect.Contains(eventData.position)) {
                slotIndex = i;
                break;
            }
        }

        if(mCurSlotDragIndex != slotIndex)
            SetCurrentSlotDrag(slotIndex);
    }

    void OnItemDragEnd(SlotDragWidget itemWidget, PointerEventData eventData) {
        if(mCurSlotDragIndex != -1) {
            var slot = slots[mCurSlotDragIndex];

            var prevDroppedWidget = slot.widget.droppedWidget;
            if(itemWidget != prevDroppedWidget) {
                slot.widget.SetDroppedWidget(itemWidget);

                //swap prev dragged widget, otherwise, set it to the origin position of the new dragged widget
                if(prevDroppedWidget) {
                    SlotDropWidget slotWidget = null;
                    for(int i = 0; i < slots.Length; i++) {
                        if(slots[i] != slot && slots[i].widget.droppedWidget == itemWidget) {
                            slotWidget = slots[i].widget;
                            break;
                        }
                    }

                    if(slotWidget)
                        slotWidget.SetDroppedWidget(prevDroppedWidget);
                    else {
                        prevDroppedWidget.SetOrigin(itemWidget.originPointDragStart);
                    }
                }
                else { //clear out dropped widget from previous slot
                    for(int i = 0; i < slots.Length; i++) {
                        if(slots[i] != slot && slots[i].widget.droppedWidget == itemWidget) {
                            slots[i].widget.SetDroppedWidget(null);
                            break;
                        }
                    }
                }
            }

            SetCurrentSlotDrag(-1);
        }

        //check if all slots have dropped item
        int slotFilledCount = 0;
        for(int i = 0; i < slots.Length; i++) {
            if(slots[i].widget.droppedWidget)
                slotFilledCount++;
        }

        bool isAllSlotsFilled = slotFilledCount == slots.Length;

        if(commitAnswerInteract) commitAnswerInteract.interactable = isAllSlotsFilled;
        if(commitAnswerUseVisibility && commitActiveGO) commitActiveGO.SetActive(isAllSlotsFilled);
    }

    void OnItemClick(SlotDragWidget itemWidget) {
        for(int i = 0; i < items.Length; i++) {
            var item = items[i];
            if(item.widget == itemWidget) {
                mInfoParms[ModalInfo.parmImage] = item.climate.image;
                mInfoParms[ModalInfo.parmTitleTextRef] = item.climate.titleTextRef;
                mInfoParms[ModalInfo.parmDescTextRef] = item.climate.descTextRef;

                M8.UIModal.Manager.instance.ModalOpen(modalInfo, mInfoParms);
                return;
            }
        }
    }

    void SetCurrentSlotDrag(int index) {
        if(mCurSlotDragIndex != -1) {
            slots[mCurSlotDragIndex].widget.highlightGO.SetActive(false);
        }

        mCurSlotDragIndex = index;

        if(mCurSlotDragIndex != -1) {
            slots[mCurSlotDragIndex].widget.highlightGO.SetActive(true);
        }
    }
}
