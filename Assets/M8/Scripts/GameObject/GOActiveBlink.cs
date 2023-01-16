using UnityEngine;
using System.Collections;

namespace M8 {
    [AddComponentMenu("M8/Game Object/Active Blink")]
    public class GOActiveBlink : MonoBehaviour {

        public GameObject target;
        public float delay;
        public bool isRealtime;

        private bool mDefaultActive;
        private YieldInstruction mWait;

        void OnEnable() {
            mDefaultActive = target.activeSelf;
            if(isRealtime)
                StartCoroutine(DoBlinkRealtime());
            else
                StartCoroutine(DoBlink());
        }

        void OnDisable() {
            StopAllCoroutines();
            target.SetActive(mDefaultActive);
        }

        void Blink() {
            target.SetActive(!target.activeSelf);
        }

        IEnumerator DoBlink() {
            var wait = new WaitForSeconds(delay);

            while(true) {
                yield return wait;
                Blink();
            }
        }

        IEnumerator DoBlinkRealtime() {
            var wait = new WaitForSecondsRealtime(delay);

            while(true) {
                yield return wait;
                Blink();
            }
        }
    }
}