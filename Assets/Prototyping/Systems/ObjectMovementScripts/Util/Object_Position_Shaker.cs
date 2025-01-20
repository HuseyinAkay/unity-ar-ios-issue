using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class Object_Position_Shaker : MonoBehaviour
{
   [SerializeField, ConditionalField("customBasePosition")] private Vector3 basePosition;
   public float Animate_Speed = 8f;
   public Vector3 Animate_Amount = new Vector3(0.05f, 0.05f, 0.05f);

   void Start()
   {
      if (!customBasePosition) basePosition = transform.localPosition;
      if (customCurve) curveScale = Mathf.Abs(curve[0].time - curve[curve.length - 1].time);
   }

   void Update()
   {
      Vector3 offset = Mathf.Sin(Time.time * Animate_Speed) * Animate_Amount;
      transform.localPosition = GetPosition();
   }


   public bool advanced = false, customCurve = false, customBasePosition = false;
   [SerializeField, ConditionalField("customCurve")] AnimationCurve curve = new AnimationCurve(new[] { new Keyframe(0, -1), new Keyframe(0, 1) });
   [SerializeField, ConditionalField("advanced")] Vector3 phaseOffset = Vector3.zero;
   float curveScale;

   Vector3 GetPosition()
   {
      var time = Time.time;
      var phaseOffset = this.phaseOffset * MathF.PI / 4f;
      return (advanced, customCurve) switch
      {
         (true, false) => basePosition + Vector3.Scale(new Vector3(Mathf.Sin((time + phaseOffset.x) * Animate_Speed),
                                                                 Mathf.Sin((time + phaseOffset.y) * Animate_Speed),
                                                                 Mathf.Sin((time + phaseOffset.z) * Animate_Speed)),
                                                     Animate_Amount),
         (true, true) => basePosition + Vector3.Scale(new Vector3(curve.Evaluate(((Mathf.Sin((time + phaseOffset.x) * Animate_Speed) + 1) / 2) * curveScale),
                                                                curve.Evaluate(((Mathf.Sin((time + phaseOffset.y) * Animate_Speed) + 1) / 2) * curveScale),
                                                                curve.Evaluate(((Mathf.Sin((time + phaseOffset.z) * Animate_Speed) + 1) / 2) * curveScale)),
                                                    Animate_Amount),
         (false, true) => basePosition + (Animate_Amount * curve.Evaluate(((Mathf.Sin(time * Animate_Speed) + 1) / 2) * curveScale)),
         _ => basePosition + (Animate_Amount * Mathf.Sin(time * Animate_Speed))
      };
   }
}
