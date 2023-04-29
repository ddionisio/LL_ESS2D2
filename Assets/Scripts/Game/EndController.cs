using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLExt;

public class EndController : GameModeController<EndController> {
    [Header("Display")]
    public GameScoreWidget flowerScoreWidget;
    public GameScoreWidget gameScoreWidget;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    public string takePlay;
    public float playDelay = 1f;

    [Header("Audio")]
    public string musicPath;

    protected override IEnumerator Start() {
        float _l = Time.time;

        animator.ResetTake(takePlay);

        if(!string.IsNullOrEmpty(musicPath) && LoLManager.instance.lastSoundBackgroundPath != musicPath)
            LoLManager.instance.PlaySound(musicPath, true, false);

        yield return base.Start();

        yield return new WaitForSeconds(playDelay);

        animator.Play(takePlay);

        if(flowerScoreWidget) flowerScoreWidget.Play(GameData.instance.gameScore);
        if(gameScoreWidget) gameScoreWidget.Play(LoLManager.instance.curScore);

        while(animator.isPlaying)
            yield return null;

        yield return new WaitForSeconds(8f);

        LoLManager.instance.Complete();

        Debug.Log((Time.time - _l).ToString());
    }
}
