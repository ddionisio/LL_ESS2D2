using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardDeployReticle : MonoBehaviour {
    [Header("Display")]
    public SpriteRenderer iconSprite;

    /// <summary>
    /// target destination
    /// </summary>
    public Vector2 targetPosition { get { return mTargetPos; } }

    protected Vector2 mTargetPos;

    public virtual void Init(CardData card) {
        if(iconSprite) iconSprite.sprite = card.icon;
    }
    
    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
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
