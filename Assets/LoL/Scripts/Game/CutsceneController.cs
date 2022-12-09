using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    public class CutsceneController : MonoBehaviour {
        [System.Serializable]
        public struct Data {
            public string name;
            public GameObject root;
            public M8.Animator.Animate animator;
            public string takeEnter;
            public string takeExit;
        }

        [Header("Music")]
        public string playMusicPath; //if not empty, play this music
        public bool startMusicPlaylist = true; //make sure playMusicPath is empty

        [Header("Modals")]
        public string modalDialog; //use for opening dialog via OpenDialog, set to null to use generic dialog.

        [Header("Animation")]
        public M8.Animator.Animate animator;
        public string takeStart; //prep background
        public string takeInteractEnter; //'next' button enter
        public string takeInteractExit; //'next' button exit

        [Header("Pages")]
        public Data[] pages;

        public event System.Action endCallback;

        public bool playOnStart = true;

        private int mCurPageInd;
        private bool mIsDialogOpen;
        private bool mIsInteractActive;

        public void OpenDialog(string nameTextRef, string dialogTextRef) {
            if(!string.IsNullOrEmpty(modalDialog))
                ModalDialog.Open(modalDialog, nameTextRef, dialogTextRef, NextPage);
            else
                ModalDialog.Open(nameTextRef, dialogTextRef, NextPage);

            mIsDialogOpen = true;
        }

        public void OpenDialogPortrait(Sprite portrait, string nameTextRef, string dialogTextRef) {
            if(!string.IsNullOrEmpty(modalDialog))
                ModalDialog.OpenApplyPortrait(modalDialog, portrait, nameTextRef, dialogTextRef, NextPage);
            else
                ModalDialog.OpenApplyPortrait(portrait, nameTextRef, dialogTextRef, NextPage);

            mIsDialogOpen = true;
        }

        public void CloseDialog() {
            if(!string.IsNullOrEmpty(modalDialog)) {
                M8.UIModal.Manager.instance.ModalCloseUpTo(modalDialog, true);
            }
            else
                M8.UIModal.Manager.instance.ModalCloseUpTo(ModalDialog.modalNameGeneric, true);

            mIsDialogOpen = false;
        }

        public void Play() {
            StopAllCoroutines();

            HideAllPages();

            mIsDialogOpen = false;
            mIsInteractActive = false;

            StartCoroutine(DoPlay());
        }

        public void NextPage() {
            StartCoroutine(DoGoNextPage());
        }

        void Awake() {
            //ensure UIRoot exists
            if(!UIRoot.isInstantiated)
                UIRoot.Reinstantiate();

            HideAllPages();
        }

        IEnumerator Start() {
            ResetAnimates();

            if(playOnStart) {
                while(M8.SceneManager.instance.isLoading)
                    yield return null;

                StartCoroutine(DoPlay());
            }
        }

        void HideAllPages() {
            for(int i = 0; i < pages.Length; i++) {
                if(pages[i].root)
                    pages[i].root.SetActive(false);
            }
        }

        // Use this for initialization
        IEnumerator DoPlay() {
            if(animator && !string.IsNullOrEmpty(takeStart)) {
                animator.Play(takeStart);
                while(animator.isPlaying)
                    yield return null;
            }

            //music
            if(!string.IsNullOrEmpty(playMusicPath)) {
                LoLMusicPlaylist.instance.Stop(); //ensure playlist is stopped

                LoLManager.instance.PlaySound(playMusicPath, true, true);
            }
            else if(startMusicPlaylist)
                LoLMusicPlaylist.instance.Play();

            //start up the first page
            mCurPageInd = 0;
            ShowCurrentPage();
        }

        void ResetAnimates() {
            if(animator) {
                if(!string.IsNullOrEmpty(takeStart))
                    animator.ResetTake(takeStart);
                if(!string.IsNullOrEmpty(takeInteractEnter))
                    animator.ResetTake(takeInteractEnter);
            }
        }

        void ShowCurrentPage() {
            StartCoroutine(DoShowCurrentPage());
        }

        IEnumerator DoShowCurrentPage() {
            if(mCurPageInd < pages.Length) {
                var page = pages[mCurPageInd];

                if(page.root)
                    page.root.SetActive(true);

                if(page.animator && !string.IsNullOrEmpty(page.takeEnter)) {
                    page.animator.Play(page.takeEnter);
                    while(page.animator.isPlaying)
                        yield return null;
                }
            }

            if(!mIsDialogOpen) {
                if(animator && !string.IsNullOrEmpty(takeInteractEnter))
                    animator.Play(takeInteractEnter);

                mIsInteractActive = true;
            }
        }

        IEnumerator DoGoNextPage() {
            if(mIsInteractActive) {
                if(animator && !string.IsNullOrEmpty(takeInteractExit))
                    animator.Play(takeInteractExit);

                mIsInteractActive = false;
            }

            bool isLastPage = pages.Length == 0 || mCurPageInd == pages.Length - 1;

            if(mCurPageInd < pages.Length) {
                var page = pages[mCurPageInd];

                if(page.animator && !string.IsNullOrEmpty(page.takeExit)) {
                    page.animator.Play(page.takeExit);
                    while(page.animator.isPlaying)
                        yield return null;
                }

                //only deactivate if it's the last page or the next page has a different root
                if(isLastPage || page.root != pages[mCurPageInd + 1].root)
                    page.root.SetActive(false);
            }

            if(!isLastPage) {
                mCurPageInd++;
                ShowCurrentPage();
            }
            else {
                if(endCallback != null)
                    endCallback();
            }
        }
    }
}