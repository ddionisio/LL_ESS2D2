using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardDeployReticle : MonoBehaviour {
    [Header("Display")]
    public SpriteRenderer iconSprite;
    public ReticleIndicator reticle;

    /// <summary>
    /// target destination
    /// </summary>
    public Vector2 targetPosition { get { return mTargetPos; } }

    public virtual bool canDeploy { get { return true; } }

    protected Vector2 mTargetPos; //set this during DoUpdatePosition

    private bool mIsReticleActive;

    public virtual void Init(CardData card) {
        if(iconSprite) iconSprite.sprite = card.icon;

        mIsReticleActive = reticle && card.indicatorRadius > 0f;
        if(mIsReticleActive) {
            reticle.radius = card.indicatorRadius;
            reticle.color = card.indicatorColor;
        }
    }
    
    public void Show() {
        gameObject.SetActive(true);

        if(reticle) reticle.gameObject.SetActive(mIsReticleActive);
    }

    public void Hide() {
        gameObject.SetActive(false);

        if(reticle) reticle.gameObject.SetActive(false);
    }

    public void UpdatePosition(Vector2 uiPoint) {
        //convert to world space
        var gameCam = M8.Camera2D.main;

        var worldPoint = gameCam.unityCamera.ScreenToWorldPoint(uiPoint);

        DoUpdatePosition(worldPoint);
    }

    protected virtual void Awake() {        
        gameObject.SetActive(false);
    }

    /// <summary>
    /// pos in world space
    /// </summary>
    protected virtual void DoUpdatePosition(Vector2 pos) {

    }
}
