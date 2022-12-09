using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSummary : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    [System.Serializable]
    public class Data {
        public M8.SceneAssetPath scene; //which scene to match
        public GameObject rootGO;
        public M8.Animator.Animate animator;
        public string take;
    }

    [Header("Display")]
    public Selectable animateReplay;

    [Header("Data")]
    public Data[] data;

    private int mCurDataIndex;

    public void PlayAnimate() {
        if(mCurDataIndex != -1) {
            var curData = data[mCurDataIndex];

            if(curData.animator && !string.IsNullOrEmpty(curData.take))
                StartCoroutine(DoPlayAnimate());
        }
    }

    public void Proceed() {
        Close();

        GameData.instance.Progress();
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {
            PlayAnimate();
        }
    }

    void Awake() {
        for(int i = 0; i < data.Length; i++) {
            if(data[i].rootGO)
                data[i].rootGO.SetActive(false);
        }
    }

    void M8.UIModal.Interface.IPop.Pop() {
        if(mCurDataIndex != -1) {
            var curData = data[mCurDataIndex];

            if(curData.rootGO)
                curData.rootGO.SetActive(false);

            mCurDataIndex = -1;
        }
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        mCurDataIndex = -1;
        for(int i = 0; i < data.Length; i++) {
            if(data[i].scene == M8.SceneManager.instance.curScene) {
                mCurDataIndex = i;
                break;
            }
        }

        if(mCurDataIndex != -1) {
            var curData = data[mCurDataIndex];

            if(curData.rootGO)
                curData.rootGO.SetActive(true);

            if(animateReplay) {
                if(curData.animator && !string.IsNullOrEmpty(curData.take)) {
                    animateReplay.gameObject.SetActive(true);
                    animateReplay.interactable = false;
                }
                else
                    animateReplay.gameObject.SetActive(false);
            }
        }
    }
        
    IEnumerator DoPlayAnimate() {
        if(animateReplay) animateReplay.interactable = false;

        var curData = data[mCurDataIndex];

        curData.animator.Play(curData.take);
        while(curData.animator.isPlaying)
            yield return null;

        if(animateReplay) animateReplay.interactable = true;
    }
}
