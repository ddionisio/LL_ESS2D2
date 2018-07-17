using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for persistent objects (mostly used for HUD)
/// </summary>
public class GameModeGOActiveSwitch : MonoBehaviour {
    [System.Serializable]
    public class StateData {
        public GameMode mode;
        public GameObject rootGO;

        public void Show() {
            if(rootGO) rootGO.SetActive(true);
        }

        public void Hide() {
            if(rootGO) rootGO.SetActive(false);
        }
    }

    public StateData[] states;

    [Header("Signals")]
    public GameModeSignal signalGameStateChanged; //invoked when state is changed

    private int mCurState = -1;

    void OnDestroy() {
        if(M8.SceneManager.isInstantiated)
            M8.SceneManager.instance.sceneChangeCallback -= OnSceneStateChange;

        signalGameStateChanged.callback -= OnGameModeChanged;
    }

    void Awake() {
        for(int i = 0; i < states.Length; i++)
            states[i].Hide();

        M8.SceneManager.instance.sceneChangeCallback += OnSceneStateChange;

        signalGameStateChanged.callback += OnGameModeChanged;
    }

    void OnGameModeChanged(GameMode mode) {
        int toStateInd = -1;
        for(int i = 0; i < states.Length; i++) {
            if(states[i].mode == mode) {
                toStateInd = i;
                break;
            }
        }

        if(mCurState != toStateInd) {
            //hide previous
            if(mCurState != -1)
                states[mCurState].Hide();

            mCurState = toStateInd;

            //show new
            if(mCurState != -1)
                states[mCurState].Show();
        }
    }

    void OnSceneStateChange(string nextScene) {
        //hide current
        if(mCurState != -1)
            states[mCurState].Hide();

        mCurState = -1;
    }
}
