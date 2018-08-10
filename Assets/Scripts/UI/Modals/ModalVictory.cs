using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.UIModal.Controller, M8.UIModal.Interface.IPush {
    //TODO: info and stuff (Score, etc.)

    public void Proceed() {
        GameData.instance.Progress();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {

    }
}
