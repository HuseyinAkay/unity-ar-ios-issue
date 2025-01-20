using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Follower : MonoBehaviour {

    public Transform Target;
    public float Follow_Time = 0.2f;
    private Vector3 Follow_Pos_D = Vector3.zero;

    public void Set_Target(Transform target) {
        Target = target;
    }

    private void LateUpdate() {
        if(Target != null) {
            Vector3 target_pos = Vector3.SmoothDamp(transform.position, Target.transform.position, ref Follow_Pos_D, Follow_Time);
            target_pos.y = 0f;
            transform.position = target_pos;
        }
    }

}
