using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerStem : MonoBehaviour {
    [Header("Display")]
    public SpriteRenderer stemSpriteRenderer;

    [Header("Leaf")]
    public float leafAngleMin;
    public float leafAngleMax;
    public float leafGrowDelay;

    [Header("Data")]
    public float maxGrowth = 1.0f;
    
    private GameObject[] mLeafGOs;
    private bool mShowLastLeaf;

    public float growth {
        get { return stemSpriteRenderer.size.y; }
        set {
            var newVal = Mathf.Clamp(value, 0f, maxGrowth);

            var stemSize = stemSpriteRenderer.size;
            if(stemSize.y != newVal) {
                stemSize.y = value;
                stemSpriteRenderer.size = stemSize;

                float curY = stemSpriteRenderer.transform.localPosition.y + newVal;

                for(int i = 0; i < mLeafGOs.Length; i++) {
                    var leafGO = mLeafGOs[i];
                    var curActive = leafGO.activeSelf;
                    var newActive = curY >= mLeafGOs[i].transform.localPosition.y && (i < mLeafGOs.Length - 1 || mShowLastLeaf);
                    if(curActive != newActive) {
                        mLeafGOs[i].SetActive(newActive);
                        if(newActive)
                            StartCoroutine(DoLeafGrow(leafGO.transform));
                    }
                }
            }
        }
    }

    public Vector2 topLocalPosition {
        get {
            if(!stemSpriteRenderer)
                return transform.position;

            var pos = stemSpriteRenderer.transform.localPosition;
            pos.y += stemSpriteRenderer.size.y;
            return pos;
        }
    }

    public Vector2 topLocalMaxPosition {
        get {
            if(!stemSpriteRenderer)
                return transform.position;

            var pos = stemSpriteRenderer.transform.localPosition;
            pos.y += maxGrowth;
            return pos;
        }
    }

    public Vector2 topWorldPosition {
        get {
            var pos = transform.localToWorldMatrix.MultiplyPoint3x4(topLocalPosition);
            return pos;
        }
    }

    public void Init(Sprite leafSprite, int leafCount, bool leafFlipStart, bool showLastLeaf) {
        var stemSpriteUnitHeight = stemSpriteRenderer.sprite.rect.height / stemSpriteRenderer.sprite.pixelsPerUnit;

        //generate leaves
        float leafYInc = stemSpriteUnitHeight / leafCount;

        mLeafGOs = new GameObject[leafCount];
        mShowLastLeaf = showLastLeaf;

        bool flipY = leafFlipStart;

        float curLeafY = stemSpriteRenderer.transform.localPosition.y + leafYInc;

        for(int i = 0; i < leafCount; i++) {
            var newGO = new GameObject("leaf"+i, typeof(SpriteRenderer));
            var t = newGO.transform;
            t.SetParent(transform);
            t.localPosition = new Vector3(0f, curLeafY, 0f);

            float rotZ = Random.Range(leafAngleMin, leafAngleMax);
            t.localEulerAngles = new Vector3(0f, 0f, flipY ? 180f - rotZ : rotZ);

            var spriteRender = newGO.GetComponent<SpriteRenderer>();
            spriteRender.sprite = leafSprite;
            spriteRender.flipY = flipY;
            spriteRender.sortingLayerID = stemSpriteRenderer.sortingLayerID;
            spriteRender.sortingOrder = stemSpriteRenderer.sortingOrder + 1;

            newGO.SetActive(false);

            mLeafGOs[i] = newGO;

            curLeafY += leafYInc;
            flipY = !flipY;
        }
        //

        var stemSpriteSize = stemSpriteRenderer.size;
        stemSpriteSize.y = 0f;
        stemSpriteRenderer.size = stemSpriteSize;
    }

    IEnumerator DoLeafGrow(Transform leafTrans) {
        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutElastic);

        var scale = new Vector3(0f, 0f, 1f);

        leafTrans.localScale = scale;

        float curTime = 0f;
        while(curTime < leafGrowDelay) {
            yield return null;

            curTime += Time.deltaTime;

            var t = easeFunc(curTime, leafGrowDelay, 1f, 1f);
            scale.x = scale.y = t;
            leafTrans.localScale = scale;
        }
    }

    void OnDrawGizmos() {
        const float radius = 0.1f;

        Gizmos.color = new Color(0.75f, 0.75f, 0f, 0.4f);
        Gizmos.DrawSphere(topWorldPosition, radius);
    }
}
