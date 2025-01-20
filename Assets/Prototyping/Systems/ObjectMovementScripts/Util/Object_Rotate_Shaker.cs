using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Rotate_Shaker : MonoBehaviour {

    private Vector3 Base_Rotation;
    public float Animate_Speed = 8f;
    public bool unscaledTime = false;
    public Vector3 Animate_Amount = new Vector3(0.05f, 0.05f, 0.05f);

    void Start() {
        Base_Rotation = transform.localEulerAngles;
    }

    void Update() {
        Vector3 offset = Mathf.Sin((unscaledTime ? Time.unscaledTime : Time.time) * Animate_Speed) * Animate_Amount;
        transform.localEulerAngles = Base_Rotation + offset;
    }
}
