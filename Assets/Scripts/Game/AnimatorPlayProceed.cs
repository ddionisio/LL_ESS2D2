using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLExt;

public class AnimatorPlayProceed : MonoBehaviour {
    public enum Mode {
        None,
        Begin,
        Progress,        
        Complete
    }

    public M8.Animator.Animate animator;
    public string playTake;
    public Mode mode = Mode.Progress;

    public void Proceed() {
        animator.Play(playTake);
    }

    void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnTakeComplete;
    }

    void Awake() {
        animator.takeCompleteCallback += OnTakeComplete;
    }

    void OnTakeComplete(M8.Animator.Animate animate, M8.Animator.Take take) {
        if(take.name == playTake) {
            switch(mode) {
                case Mode.Begin:
                    GameData.instance.Begin();
                    break;
                case Mode.Progress:
                    GameData.instance.Progress();
                    break;
                case Mode.Complete:
                    LoLManager.instance.Complete();
                    break;
            }
        }
    }
}
