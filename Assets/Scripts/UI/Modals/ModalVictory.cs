using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalVictory : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    //TODO: info and stuff (Score, etc.)
    public GameObject proceedGO; //root to allow moving to next level

    public Text scoreLabel;

    public string modalNext;

    public void Proceed() {
        if(!string.IsNullOrEmpty(modalNext)) {
            Close();
            M8.UIModal.Manager.instance.ModalOpen(modalNext);
        }
        else
            GameData.instance.Progress();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {

        if(proceedGO) proceedGO.SetActive(false);

        if(scoreLabel) scoreLabel.text = Mathf.RoundToInt(GameController.instance.motherbase.flowerTotalGrowth).ToString();

        StartCoroutine(DoWaitForProceed());
    }

    void M8.UIModal.Interface.IPop.Pop() {
        if(proceedGO) proceedGO.SetActive(false);
    }

    IEnumerator DoWaitForProceed() {
        if(proceedGO) proceedGO.SetActive(false);

        //wait for motherbase to finish its victory thing
        while(GameController.instance.motherbase.state == Motherbase.State.Victory)
            yield return null;

        if(proceedGO) proceedGO.SetActive(true);
    }
}
