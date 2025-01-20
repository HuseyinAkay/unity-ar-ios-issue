using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Rotator : MonoBehaviour {
    public Vector3 Rotation;

    private void Update() {
        float dt = Time.deltaTime;
        transform.localEulerAngles += Rotation*dt;
    }

}
