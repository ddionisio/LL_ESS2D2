using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorPlayProceed : MonoBehaviour {
    public M8.Animator.Animate animator;
    public string playTake;

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
        if(take.name == playTake)
            GameData.instance.Progress();
    }
}
