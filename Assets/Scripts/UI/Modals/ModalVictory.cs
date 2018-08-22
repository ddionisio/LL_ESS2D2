using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.UIModal.Controller, M8.UIModal.Interface.IPush {
    //TODO: info and stuff (Score, etc.)
    public GameObject proceedGO; //root to allow moving to next level

    public void Proceed() {
        GameData.instance.Progress();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {        

        StartCoroutine(DoWaitForProceed());
    }

    IEnumerator DoWaitForProceed() {
        if(proceedGO) proceedGO.SetActive(false);

        //wait for motherbase to finish its victory thing
        while(GameController.instance.motherbase.state == Motherbase.State.Victory)
            yield return null;

        if(proceedGO) proceedGO.SetActive(true);
    }
}
