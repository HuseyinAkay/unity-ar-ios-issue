using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MyBox;
using UnityEngine;

public class FocusViewBackground : MonoBehaviour
{
   [SerializeField] Vector2 size = Vector2.one;
   [SerializeField] float margin = .01f;
   [SerializeField] bool allowSqueeze = false;
   [SerializeField] Renderer[] renderers;
   [SerializeField] ObjectSequenceController sequenceController;
   [SerializeField] Animator animator;
   [SerializeField, ConditionalField(nameof(animator))] string showStateName = "Show", hideStateName = "Hide";
   [SerializeField, ConditionalField(nameof(animator))] float hideAnimationDuration = 1f;

   private void OnValidate()
   {
      sequenceController = GetComponentInChildren<ObjectSequenceController>();
      animator = GetComponentInChildren<Animator>();
      if (sequenceController == null && animator == null)
      {
         renderers = GetComponentsInChildren<Renderer>();
         
         foreach (var vector in renderers.ToList().ConvertAll(x => transform.InverseTransformVector(x.bounds.extents)))
         {
            size.x = Mathf.Max(size.x, vector.x);
            size.y = Mathf.Max(size.y, vector.y);
         }
      }
      if (sequenceController == null && animator == null && renderers.IsNullOrEmpty())
         Debug.LogError($"Configuration error there are no renderers or any other form of transition Controller.", this);
   }

   private void Awake()
   {
      var cam = GetComponentInParent<Camera>();
      if (cam != null)
      {
         var fwd = cam.transform.forward;
         var dist = cam.farClipPlane * .95f;
         transform.position = cam.transform.position + fwd * dist;
         transform.rotation = Quaternion.LookRotation(fwd, cam.transform.up);

         var fittedSize = GetCameraFittedSize(cam, dist).ToVector3();
         fittedSize = allowSqueeze ? fittedSize : Vector3.one * Mathf.Max(fittedSize.x, fittedSize.y);
         // fittedSize = transform.parent.InverseTransformVector(fittedSize).SetZ(1f);
         transform.localScale = fittedSize.SetZ(1f);
      }

      Show();
   }

   Vector2 GetCameraFittedSize(Camera cam, float dist)
   {
      var fovVertical = (cam.fieldOfView / 2f) * Mathf.Deg2Rad;

      var Tan = Mathf.Tan(fovVertical);

      var vertical = ((size.y / 2f) - margin) / (Tan * dist);
      var horizontal = ((size.x / 2f) - margin) / (Tan * dist * cam.aspect);

      return new Vector2(1f / horizontal, 1f / vertical);
   }

   private void OnDrawGizmos()
   {
      Gizmos.color = Color.red;
      Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
      Gizmos.DrawWireCube(Vector3.zero, new(size.x, size.y, .01f));
      if (margin == 0) return;
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireCube(Vector3.zero, new(size.x - margin, size.y - margin, .01f));
   }


   // [SerializeField] Vector3 testVec = Vector3.one;
   // [SerializeField, Range(30f, 120f)] float testFov = 60f;
   // [SerializeField] float aspect = 2f;

   // private void OnDrawGizmos()
   // {
   //    float dist = testVec.z;
   //    Gizmos.DrawFrustum(transform.position, testFov, dist * 1.1f, dist * .1f, aspect);
   //    Gizmos.color = Color.red;
   //    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

   //    var fittedSize = GetCameraFittedSize(testFov, aspect, dist, testVec.ToVector2());
   //    fittedSize = allowSqueeze ? fittedSize : Vector2.one * Mathf.Max(fittedSize.x, fittedSize.y);
   //    fittedSize = Vector2.Scale(fittedSize, testVec.ToVector2());

   //    Gizmos.DrawWireCube(Vector3.forward * dist, new(fittedSize.x, fittedSize.y, .01f));
   //    if (margin == 0) return;
   //    Gizmos.color = Color.yellow;
   //    Gizmos.DrawWireCube(Vector3.forward * dist, new(fittedSize.x - margin, fittedSize.y - margin, .01f));
   // }


   public void Show()
   {
      if (sequenceController == null && animator == null && renderers.IsNullOrEmpty())
      {
         Debug.LogError($"Configuration error there are no renderers or any other form of transition Controller.", this);
         return;
      }

      if (sequenceController != null) sequenceController.PlayShowSequence();
      if (animator != null) animator.Play(showStateName);

      if (renderers.IsNullOrEmpty()) return;

      DOTween.Kill(this, true);
      var seq = DOTween.Sequence(this);

      foreach (var renderer in renderers)
      {
         renderer.material.color = renderer.material.color.WithAlphaSetTo(0f);
         seq.Insert(0f, renderer.material.DOFade(1f, .5f).SetEase(Ease.OutSine));
      }
   }

   public float Hide()
   {
      if (sequenceController == null && animator == null && renderers.IsNullOrEmpty())
      {
         Debug.LogError($"Configuration error there are no renderers or any other form of transition Controller.", this);
         return 0f;
      }

      float duration = sequenceController == null ? 0f : sequenceController.PlayHideSequence();
      duration = animator == null ? duration : Mathf.Max(duration, hideAnimationDuration);
      if (animator != null) animator.Play(hideStateName);

      if (renderers.IsNullOrEmpty()) return duration;

      DOTween.Kill(this, true);
      var seq = DOTween.Sequence(this);
      var transitionDuration = .5f;

      foreach (var renderer in renderers)
      {
         renderer.material.color = renderer.material.color.WithAlphaSetTo(1f);
         seq.Insert(0f, renderer.material.DOFade(0f, transitionDuration).SetEase(Ease.OutSine));
      }

      return Mathf.Max(duration, transitionDuration);
   }
}
