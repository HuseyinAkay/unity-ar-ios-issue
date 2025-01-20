using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
   Transform cam;
   Quaternion rot => Quaternion.LookRotation((cam.position - transform.position).normalized, Vector3.up);

   private void Start()
   {
      cam = Camera.main.transform;
   }
   void LateUpdate()
   {
      transform.rotation = rot;
   }
}
