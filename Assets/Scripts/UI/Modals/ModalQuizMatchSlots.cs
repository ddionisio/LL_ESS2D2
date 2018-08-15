using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalQuizMatchSlots : M8.UIModal.Controller, M8.UIModal.Interface.IPush {    
    [System.Serializable]
    public class SlotData {
        public Transform anchor;

        public int index { get; set; }

        public SlotDropWidget widget { get; private set; }

        public void Init(SlotDropWidget template, SlotDropWidget.DropCallback dropCallback) {
            if(!widget) {
                widget = Instantiate(template, anchor);
                widget.gameObject.SetActive(true);
                widget.index = index;
                widget.dropCallback += dropCallback;
            }

            widget.Init(anchor.position);
        }
    }

    [System.Serializable]
    public class ItemData {        
        public Transform anchor;
        public Sprite imageSprite;
        [M8.Localize]
        public string nameRef;
        [M8.Localize]
        public string descRef;

        public int index { get; set; }

        public SlotDragWidget widget { get; private set; }

        public void Init(SlotDragWidget template, SlotDragWidget.ClickCallback clickCallback) {
            if(!widget) {
                widget = Instantiate(template, anchor);
                widget.gameObject.SetActive(true);
                widget.index = index;
                widget.Setup(imageSprite, M8.Localize.Get(nameRef));
                widget.clickCallback += clickCallback;
            }

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
    public Selectable commitAnswerInteract;
    public GameObject commitActiveGO;
    public GameObject progressActiveGO;
    
    private int mCorrectCount;

    private M8.GenericParams mInfoParms = new M8.GenericParams();
            
    public void CommitAnswers() {
        if(commitActiveGO) commitActiveGO.SetActive(false);
        if(progressActiveGO) progressActiveGO.SetActive(true);

        for(int i = 0; i < items.Length; i++) {
            var item = items[i];
            item.widget.isClickEnabled = true;
            item.widget.isDragEnabled = false;
        }
    }

    public void Proceed() {
        GameData.instance.Progress();
    }

    void Awake() {
        //initialize data
        for(int i = 0; i < slots.Length; i++)
            slots[i].index = i;

        for(int i = 0; i < items.Length; i++)
            items[i].index = i;

        itemTemplate.gameObject.SetActive(false);
        slotTemplate.gameObject.SetActive(false);
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        //mix up items
        M8.ArrayUtil.Shuffle(items);

        for(int i = 0; i < slots.Length; i++)
            slots[i].Init(slotTemplate, OnSlotDrop);

        for(int i = 0; i < items.Length; i++)
            items[i].Init(itemTemplate, OnItemClick);

        //apply mode
        if(commitAnswerInteract) commitAnswerInteract.interactable = false;
        if(commitActiveGO) commitActiveGO.SetActive(true);
        if(progressActiveGO) progressActiveGO.SetActive(false);
    }

    void OnSlotDrop(SlotDropWidget slot, SlotDragWidget prevItem) {
        if(prevItem) {
            //swap, look for the slot that has the item from this 'slot'
            for(int i = 0; i < slots.Length; i++) {
                if(slots[i].widget.droppedWidget == slot.droppedWidget) {
                    slots[i].widget.SetDroppedWidget(prevItem);
                    break;
                }
            }
        }
                
        if(commitAnswerInteract) {
            //check if all slots have dropped item
            int slotFilledCount = 0;
            for(int i = 0; i < slots.Length; i++) {
                if(slots[i].widget.droppedWidget)
                    slotFilledCount++;
            }

            commitAnswerInteract.interactable = slotFilledCount == slots.Length;
        }
    }

    void OnItemClick(SlotDragWidget itemWidget) {
        for(int i = 0; i < items.Length; i++) {
            var item = items[i];
            if(item.widget == itemWidget) {
                mInfoParms[ModalInfo.parmImage] = item.imageSprite;
                mInfoParms[ModalInfo.parmTitleTextRef] = item.nameRef;
                mInfoParms[ModalInfo.parmDescTextRef] = item.descRef;

                M8.UIModal.Manager.instance.ModalOpen(modalInfo, mInfoParms);
                return;
            }
        }
    }
}
