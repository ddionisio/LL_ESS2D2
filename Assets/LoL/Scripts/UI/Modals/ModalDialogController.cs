using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    /// <summary>
    /// Use this as a convenience to open dialog in a sequence
    /// </summary>
    public class ModalDialogController : MonoBehaviour {
        public string modal = "dialog";

        public Sprite portrait;
        public bool applyPortrait;

        [M8.Localize]
        public string nameTextRef;
        [M8.Localize]
        public string[] dialogTextRefs;

        public bool isPlaying { get { return mPlayRout != null; } }

        private Coroutine mPlayRout;
        private bool mIsNext;

        public void Play() {
            if(mPlayRout == null)
                mPlayRout = StartCoroutine(DoPlay());
        }

        public void Stop() {
            if(mPlayRout != null) {
                StopCoroutine(mPlayRout);
                mPlayRout = null;
            }

            if(M8.UIModal.Manager.isInstantiated && M8.UIModal.Manager.instance.ModalIsInStack(modal))
                M8.UIModal.Manager.instance.ModalCloseUpTo(modal, true);

            mIsNext = false;
        }

        void OnDisable() {
            Stop();
        }

        IEnumerator DoPlay() {
            for(int i = 0; i < dialogTextRefs.Length; i++) {
                string textRef = dialogTextRefs[i];
                if(string.IsNullOrEmpty(textRef))
                    continue;

                mIsNext = false;

                if(applyPortrait)
                    ModalDialog.OpenApplyPortrait(modal, portrait, nameTextRef, textRef, OnDialogNext);
                else
                    ModalDialog.Open(modal, nameTextRef, textRef, OnDialogNext);

                while(!mIsNext)
                    yield return null;
            }

            if(M8.UIModal.Manager.instance.ModalIsInStack(modal))
                M8.UIModal.Manager.instance.ModalCloseUpTo(modal, true);

            //wait for dialog to close
            while(M8.UIModal.Manager.instance.isBusy || M8.UIModal.Manager.instance.ModalIsInStack(modal))
                yield return null;

            mPlayRout = null;
        }

        void OnDialogNext() {
            mIsNext = true;
        }
    }
}