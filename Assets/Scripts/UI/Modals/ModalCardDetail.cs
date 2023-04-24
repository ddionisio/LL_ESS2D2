﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LoLExt;

using TMPro;

public class ModalCardDetail : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    public const string parmCardRef = "c";

    [Header("Display")]
    public TMP_Text titleLabel;
    public TMP_Text descLabel;
    public Image cardImage;
    public Image cardIcon;
    public Image cardIllustration;
    
    private bool mIsPaused = false;

    private CardData mCard;

    void M8.UIModal.Interface.IPop.Pop() {
        SetPause(false);
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        mCard = parms.GetValue<CardData>(parmCardRef);
        
        if(titleLabel) titleLabel.text = mCard.title;
        if(descLabel) descLabel.text = mCard.description;
        if(cardImage) cardImage.sprite = mCard.image;

        if(cardIcon) {
            cardIcon.sprite = mCard.icon;
            cardIcon.SetNativeSize();
        }

        if(cardIllustration) {
            cardIllustration.sprite = mCard.illustration;
            cardIllustration.SetNativeSize();
        }

        SetPause(true);
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {
            //play speeches
            LoLManager.instance.SpeakTextQueue(mCard.titleRef, mCard.name, 0);
            LoLManager.instance.SpeakTextQueue(mCard.descriptionRef, mCard.name, 1);
        }
    }

    void OnDestroy() {
        SetPause(false);
    }

    private void SetPause(bool pause) {
        if(mIsPaused != pause) {
            mIsPaused = pause;
            if(M8.SceneManager.isInstantiated) {
                if(mIsPaused)
                    M8.SceneManager.instance.Pause();
                else
                    M8.SceneManager.instance.Resume();
            }
        }
    }
}
