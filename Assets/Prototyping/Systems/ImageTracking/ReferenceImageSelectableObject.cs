using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using DG.Tweening;
using System.Linq;
using UnityEngine.XR.ARSubsystems;

public class ReferenceImageSelectableObject : MonoBehaviour
{
   [SerializeField] Vector3 centerOffset = Vector3.zero;
   [SerializeField] bool startSelected = false;
   [SerializeField] ObjectSequenceController sequenceController;
   [SerializeField, Range(.4f, 2f)] float maxCameraRange = 1f;
   static List<ReferenceImageSelectableObject> liveObjects = new();
   static ReferenceImageSelectableObject selectedObject, focusedObject;

   const float selectionChangeResistance = .2f;
   public static bool UpdateSelection(Camera cam)
   {
      var liveObjData = ReferenceImageObjectManager.GetLiveObjectData();
      bool IsTrackingStatusValid(GameObject obj)
      {
         if (obj == null) return false;
         var ctx = liveObjData.Find(x => x.IsMatching(obj));
         return ctx != null && ctx.trackingState == TrackingState.Tracking;
      }

      if (focusedObject != null) return false;

      var result = false;
      var bestScore = float.MinValue;
      var bestObject = (ReferenceImageSelectableObject)null;

      // Debug.Log($"{liveObjects.Count} live selectable objects.");
      foreach (var item in liveObjects)
      {
         bool isSelected = selectedObject == item;
         float scoreOffset = isSelected ? selectionChangeResistance : -selectionChangeResistance;
         if (item.TryGetCameraPriorityScore(cam, out var score) && IsTrackingStatusValid(item.gameObject) && (score + scoreOffset) > bestScore)
         {
            bestScore = score;
            bestObject = item;
         }
         else if (isSelected)
         {
            selectedObject = null;
            item.DeSelect();
            result = true;
         }
      }
      if (bestObject != null && bestObject != selectedObject)
      {
         result = true;
         if (selectedObject != null) selectedObject.DeSelect();
         selectedObject = bestObject;
         selectedObject.Select();
      }
      return result;
   }

   Transform originalParent;
   public Quaternion originalRotation { get; private set; }

   public void Focus(Camera cam, float cameraDistance)
   {
      if (selectedObject != null && selectedObject != this)
      {
         selectedObject.DeSelect();
         this.Select();
      }
      selectedObject = this;
      focusedObject = this;

      transform.SetParent(cam.transform, true);

      var seq = DOTween.Sequence(transform);

      seq.Insert(0f, transform.DOLocalMove((Vector3.forward * cameraDistance) - centerOffset, .5f).SetEase(Ease.OutSine));
      seq.Insert(0f, transform.DOLocalRotateQuaternion(Quaternion.LookRotation(Vector3.up, Vector3.back) * originalRotation, .5f).SetEase(Ease.OutSine));
   }

   public void DeFocus()
   {
      if (focusedObject == this && selectedObject == this)
      {
         focusedObject = null;

         if (originalParent == null) DeSelect(true);
         else
         {
            transform.SetParent(originalParent, true);

            var seq = DOTween.Sequence(transform);

            seq.Insert(0f, transform.DOLocalMove(Vector3.zero, .5f).SetEase(Ease.OutSine));
            seq.Insert(0f, transform.DOLocalRotateQuaternion(originalRotation, .5f).SetEase(Ease.OutSine));
         }
      }
   }

   void Awake()
   {
      liveObjects.Add(this);
      gameObject.SetActive(startSelected);

      originalParent = transform.parent;
      originalRotation = transform.localRotation;

      if (startSelected)
      {
         selectedObject?.DeSelect();
         selectedObject = this;
         Select();
      }

      // Debug.Log($"a new selectable object has been spawned.", this);
   }

   void OnDestroy()
   {
      liveObjects.Remove(this);
      if (selectedObject == this) selectedObject = null;
   }

   bool TryGetCameraPriorityScore(Camera cam, out float score)
   {
      var pos = GetOriginWorld();
      var vec = pos - cam.transform.position;
      var dist = vec.magnitude;
      var dot = Mathf.Clamp01(Vector3.Dot(cam.transform.forward, vec));
      var distRatio = Mathf.InverseLerp(0f, maxCameraRange, dist);
      score = (2 * dot * dot) - ((distRatio * distRatio) + dist);
      return cam.IsWorldPointInViewport(pos); // && distRatio < 1f;
   }

   void Select()
   {
      gameObject.SetActive(true);
      sequenceController?.PlayShowSequence();
   }

   float deselectDelay => sequenceController == null ? 0f : sequenceController.PlayHideSequence();
   void DeSelect(bool destroy = false)
   {
      Debug.Log($"{(destroy ? "destroy" : "deselect")}ing object {name}.", this);
      var delay = deselectDelay;
      if (delay > 0f)
      {
         if (destroy) Destroy(gameObject, delay);
         else Invoke(nameof(DisableObject), delay);
      }
      else
      {
         if (destroy) Destroy(gameObject);
         else DisableObject();
      }
   }

   void DisableObject() => gameObject.SetActive(false);

   private void OnDrawGizmos()
   {
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(GetOriginWorld(), .02f);
   }


   public Vector3 GetOriginWorld() => transform.TransformPoint(centerOffset);

   public Vector3 GetOriginLocal(bool worldScale = false) => worldScale ? Vector3.Scale(transform.lossyScale, centerOffset) : centerOffset;
}