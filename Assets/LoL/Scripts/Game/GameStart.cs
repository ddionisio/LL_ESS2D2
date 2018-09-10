using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStart : MonoBehaviour {
    public static bool isStarted = false;

    public GameObject loadingGO;
    public GameObject readyGO;

    public GameObject titleGO;
    public Text titleText;
    [M8.Localize]
    public string titleStringRef;

    public string musicPath;

    void Awake() {
        if(loadingGO) loadingGO.SetActive(true);
        if(readyGO) readyGO.SetActive(false);
        if(titleGO) titleGO.SetActive(false);
    }

    IEnumerator Start () {
        //hide other stuff

        yield return null;

        //wait for scene to load
        while(M8.SceneManager.instance.isLoading)
            yield return null;
        
        //wait for LoL to load/initialize
        while(!LoLManager.instance.isReady)
            yield return null;
        
        //start title
        if(titleText) titleText.text = LoLLocalize.Get(titleStringRef);
        if(titleGO) titleGO.SetActive(true);

        //show other stuff

        if(loadingGO) loadingGO.SetActive(false);
        if(readyGO) readyGO.SetActive(true);

        //play music
        if(!string.IsNullOrEmpty(musicPath))
            LoLManager.instance.PlaySound(musicPath, true, true);

        isStarted = true;
    }
}
