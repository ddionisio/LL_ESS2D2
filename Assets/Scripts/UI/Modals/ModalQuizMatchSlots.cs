using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalQuizMatchSlots : M8.UIModal.Controller, M8.UIModal.Interface.IPush {
    [System.Serializable]
    public struct SlotData {
        public Transform anchor;

        public SlotDropWidget widget { get; private set; }

        public void Init(Transform root, Vector3 point, SlotDropWidget template, int index) {
            if(!widget) {
                widget = Instantiate(template, root);
            }

            widget.Init(index, point);
        }
    }

    [Header("Data")]
    public SlotDragWidget itemTemplate;
    public SlotDropWidget slotTemplate;

    [Header("Display")]
    public Selectable commitAnswerInteract;
    public GameObject commitActiveGO;
    public GameObject progressActiveGO;

    [Header("Slot")]
    public Transform[] slotPoints;
    public Transform[] itemPoints;
        
    public void CommitAnswers() {

    }

    public void Proceed() {
        GameData.instance.Progress();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {

    }
}
