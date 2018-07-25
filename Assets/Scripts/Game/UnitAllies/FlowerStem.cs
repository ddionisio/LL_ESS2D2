using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerStem : MonoBehaviour {
    public float leafOfsY;
    public float topOfsY;

    public float growth {
        get { return transform.localScale.y; }
        set {
            var s = transform.localScale;
            s.y = value;
            transform.localScale = s;
        }
    }

    public Vector2 leafWorldPosition {
        get {
            var localPos = transform.localPosition;
            localPos.y += leafOfsY;
            var pos = transform.localToWorldMatrix.MultiplyPoint3x4(localPos);
            return pos;
        }
    }

    public Vector2 topWorldPosition {
        get {
            var localPos = transform.localPosition;
            localPos.y += topOfsY;
            var pos = transform.localToWorldMatrix.MultiplyPoint3x4(localPos);
            return pos;
        }
    }

    void OnDrawGizmos() {
        const float radius = 0.1f;

        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.4f);
        Gizmos.DrawSphere(leafWorldPosition, radius);

        Gizmos.color = new Color(0.75f, 0.75f, 0f, 0.4f);
        Gizmos.DrawSphere(topWorldPosition, radius);
    }
}
