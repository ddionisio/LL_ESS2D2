using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleIndicator : MonoBehaviour {
    public SpriteRenderer[] particles;
    public float particleAngleSeparator = 15f;

    public Transform rotateRoot;
    public float rotateAngleSpeed = 720f;
    
    public float radius {
        get { return mRadius; }
        set {
            if(mRadius != value) {
                mRadius = value;
                ApplyCurrentRadius();
            }
        }
    }

    public Color color {
        get { return mColor; }
        set {
            if(mColor != value) {
                mColor = value;
                ApplyCurrentColor();
            }
        }
    }

    private float mRadius = 0f;
    private Color mColor = Color.white;
    private Color[] mParticleDefaultColors;

    void OnEnable() {
        ApplyCurrentRadius();

        rotateRoot.localEulerAngles = Vector3.zero;
    }
    
    void Update() {
        var rot = rotateRoot.localEulerAngles;
        rot.z += rotateAngleSpeed * Time.deltaTime;
        rotateRoot.localEulerAngles = rot;
	}

    void InitData() {
        mParticleDefaultColors = new Color[particles.Length];
        for(int i = 0; i < particles.Length; i++)
            mParticleDefaultColors[i] = particles[i].color;
    }

    void ApplyCurrentRadius() {
        Vector2 up = Vector2.up;
        float rotSign = Mathf.Sign(rotateAngleSpeed);

        for(int i = 0; i < particles.Length; i++) {
            particles[i].transform.localPosition = up * radius;

            up = M8.MathUtil.RotateAngle(up, rotSign * particleAngleSeparator);
        }
    }

    void ApplyCurrentColor() {
        if(mParticleDefaultColors == null)
            InitData();

        for(int i = 0; i < particles.Length; i++)
            particles[i].color = mParticleDefaultColors[i] * mColor;
    }

    private void OnDrawGizmos() {
        if(mRadius > 0f) {
            Gizmos.color = new Color(mColor.r, mColor.g, mColor.b);
            Gizmos.DrawWireSphere(transform.position, mRadius);
        }
    }
}
