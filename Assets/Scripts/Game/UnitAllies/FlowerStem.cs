using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerStem : MonoBehaviour {
    public GameObject leavesGO;

    public float maxGrowth = 1.0f;
    public float topOfsY;

    public float growth {
        get { return transform.localScale.y; }
        set {
            var s = transform.localScale;
            s.y = value;
            transform.localScale = s;
        }
    }

    public Vector2 topWorldPosition {
        get {
            var pos = transform.localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, topOfsY, 0f));
            return pos;
        }
    }

    public void ShowLeaves() {
        if(!leavesGO.activeSelf) {
            leavesGO.SetActive(true);

            //invert leaves scale
            var stemScale = transform.localScale;
            var leavesScale = Vector3.one;

            if(stemScale.x != 0f) leavesScale.x /= stemScale.x;
            if(stemScale.y != 0f) leavesScale.y /= stemScale.y;
            if(stemScale.z != 0f) leavesScale.z /= stemScale.z;

            leavesGO.transform.localScale = stemScale;
        }
    }

    void Awake() {
        leavesGO.SetActive(false);
    }

    void OnDrawGizmos() {
        const float radius = 0.1f;

        Gizmos.color = new Color(0.75f, 0.75f, 0f, 0.4f);
        Gizmos.DrawSphere(topWorldPosition, radius);
    }
}
