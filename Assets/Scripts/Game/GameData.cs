using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLExt;

/// <summary>
/// Make sure to create this in Resources with name: gameData
/// </summary>
[CreateAssetMenu(fileName = "gameData", menuName = "Game/Game Data", order = 0)]
public class GameData : M8.SingletonScriptableObject<GameData> {
    public const int progressPerLevel = 2; //includes: level select, play

    [System.Serializable]
    public class LevelData {
        public ClimateData climateMatch;
        public M8.SceneAssetPath scene;
    }

    [Header("Scenes")]
    //public M8.SceneAssetPath introScene;
    public M8.SceneAssetPath endScene;
    public M8.SceneAssetPath levelSelectScene; //only really one level to select
    public M8.SceneAssetPath levelCompleteScene; //after level is completed, then go back to level select

    public M8.SceneAssetPath postGameScene; //one last scene before ending

    [Header("Levels")]
    public LevelData[] levels;

    [Header("Global Level Settings")]
    public float levelCycleStartDelay = 1.0f;
    public float levelCycleEndDelay = 1.0f;
    public LayerMask groundLayerMask;

    [Header("Data")]
    public int scoreMatchPerCorrect = 500;
    public int scoreIncorrectPenalty = 200;

    [Header("Signals")]
    public SignalGameFlag signalGameFlagChanged;

    public bool isGameStarted { get; private set; } //true: we got through start normally, false: debug
    public int curLevelIndex { get; private set; }
    public LevelData curLevelData { get { return levels[curLevelIndex]; } }

    public int gameScore { get; set; }

    private Dictionary<string, int> mFlags;

#if UNITY_EDITOR
    public void OverrideLevelIndex(int index) {
        curLevelIndex = index;
    }
#endif

    public void SetFlag(string key, int flag) {
        if(mFlags.ContainsKey(key)) {
            if(mFlags[key] != flag) {
                mFlags[key] = flag;

                if(signalGameFlagChanged) signalGameFlagChanged.Invoke(key, flag);
            }
        }
        else {
            mFlags.Add(key, flag);

            if(signalGameFlagChanged) signalGameFlagChanged.Invoke(key, flag);
        }
    }

    public int GetFlag(string key) {
        int flag = 0;
        mFlags.TryGetValue(key, out flag);
        return flag;
    }

    /// <summary>
    /// Called in start scene
    /// </summary>
    public void Begin() {
        isGameStarted = true;

        if(LoLManager.instance.curProgress == 0)
            levelSelectScene.Load();
        //introScene.Load();
        else {
            LoLMusicPlaylist.instance.Play();
            Current();
        }
    }

    /// <summary>
    /// Update level index based on current progress, and load scene
    /// </summary>
    public void Current() {
        int progress = LoLManager.instance.curProgress;

        if(progress == LoLManager.instance.progressMax)
            endScene.Load();
        else if(progress < levels.Length * progressPerLevel) {
            UpdateLevelIndexFromProgress(progress);

            if(curLevelIndex < levels.Length) {
                int sceneIndex = progress % progressPerLevel;
                switch(sceneIndex) {
                    case 0:
                        levelSelectScene.Load();
                        break;
                    case 1:
                        levels[curLevelIndex].scene.Load();
                        break;
                    default:
                        M8.SceneManager.instance.Reload();
                        break;
                }
            }   
        }
        else {
            //do the last bit before end
            postGameScene.Load();
        }
    }

    /// <summary>
    /// Update progress, go to next level-scene
    /// </summary>
    public void Progress() {
        var curScene = M8.SceneManager.instance.curScene;

        if(isGameStarted) {
            //we are in intro, proceed
            //if(curScene.name == introScene.name) {
                //Current();
            //}
            //ending if we are already at max
            if(LoLManager.instance.curProgress == LoLManager.instance.progressMax)
                endScene.Load();
            //proceed to next progress
            else {
                LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);
                Current();
            }
        }
        else { //debug
            /*if(curScene.name == introScene.name) {
                //play first level intro
                curLevelIndex = 0;

                levelSelectScene.Load();
            }*/
            if(curScene.name == levelSelectScene.name) {
                //play level
                levels[curLevelIndex].scene.Load();
            }
            else if(curScene.name == levelCompleteScene.name) {
                //go to next level intro
                if(curLevelIndex < levels.Length - 1) {
                    curLevelIndex++;
                    levelCompleteScene.Load();
                }
                else
                    endScene.Load(); //completed
            }
            else if(curScene.name == postGameScene.name) {
                endScene.Load(); //completed
            }
            else {
                //check levels and load level ending
                int levelFoundInd = -1;

                for(int i = 0; i < levels.Length; i++) {
                    if(curScene.name == levels[i].scene.name) {
                        levelFoundInd = i;
                        break;
                    }
                }

                if(levelFoundInd != -1) {
                    curLevelIndex = levelFoundInd;
                    if(levelCompleteScene.isValid)
                        levelCompleteScene.Load();
                    else {
                        if(curLevelIndex < levels.Length - 1) {
                            curLevelIndex++;                            
                        }

                        //level select will deal with ending
                        levelSelectScene.Load();
                    }
                }
                else
                    M8.SceneManager.instance.Reload(); //not found, just reload current
            }
        }
    }

    protected override void OnInstanceInit() {
        //compute max progress
        if(LoLManager.isInstantiated) {
            LoLManager.instance.progressMax = levels.Length * progressPerLevel + 2;
        }
        else
            curLevelIndex = DebugControl.instance.levelIndex;

        isGameStarted = false;
        curLevelIndex = 0;
        gameScore = 0;

        mFlags = new Dictionary<string, int>();
    }

    private void UpdateLevelIndexFromProgress(int progress) {
        curLevelIndex = Mathf.Clamp(progress / progressPerLevel, 0, levels.Length);
    }
}