using UnityEngine;
using TMPro;
using MyBox;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TrackingInfoDebug : MonoBehaviour
{
   [SerializeField, ReadOnly] private TextMeshProUGUI text;
   [SerializeField] ARTrackedImageManager trackedImageManager;

   private void OnValidate()
   {
      if (text == null) text = GetComponent<TextMeshProUGUI>();
   }

   public void SetManager(ARTrackedImageManager trackedImageManager) => this.trackedImageManager = trackedImageManager;

   private void Update()
   {
      text.text = trackedImageManager == null ? $"TrackedImageManager is null" : $"Tracking {trackedImageManager.trackables.count} images";
   }
}
