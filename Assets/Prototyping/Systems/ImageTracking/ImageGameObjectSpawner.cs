using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using MyBox;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageGameObjectSpawner : ReferenceImageObjectManager
{
   [SerializeField] XRReferenceImageLibrary imageLibrary;
   [SerializeField] SpawnConfig[] spawnConfigs;
   [SerializeField] bool autoConfigure = true;
   // [SerializeField] e_LibrarySelectionMethod librarySelectionMethod = e_LibrarySelectionMethod.swapLibraryReference;

   // bool swapLibraryReference => librarySelectionMethod is e_LibrarySelectionMethod.swapLibraryReference;
   // enum e_LibrarySelectionMethod
   // {
   //    swapLibraryReference,
   //    changeLibraryContents
   // }

   [Serializable]
   public class SpawnConfig : IEquatable<XRReferenceImage>
   {
      [SerializeField, HideInInspector] string name;
      // [SerializeField, ReadOnly] UnityEngine.Object referenceImage;
      [SerializeField, HideInInspector] XRReferenceImage _referenceImage;
      public XRReferenceImage ReferenceImage
      {
         get => _referenceImage; private set
         {
            name = value.name;
            // referenceImage = value.texture;
            _referenceImage = value;
         }
      }

      public GameObject prefab;

      public bool Equals(XRReferenceImage comparison)
      {
#if UNITY_EDITOR
         return ReferenceImage == comparison;
#else
         return ReferenceImage.textureGuid == comparison.textureGuid;
#endif
      }

      public SpawnConfig(XRReferenceImage referenceImage, GameObject prefab = null)
      {
         this.ReferenceImage = referenceImage;
         this.prefab = prefab;
      }

      public SpawnConfig(XRReferenceImage referenceImage, SpawnConfig copyFrom) : this(referenceImage, copyFrom?.prefab) { }
   }


   private void OnValidate()
   {
      if (!autoConfigure) return;

      var trackedImageManager = GetComponent<ARTrackedImageManager>();
      if (trackedImageManager == null)
         Debug.LogError($"Missing {nameof(ARTrackedImageManager)} component on {gameObject.name}.", this);
      else
      {
         if (imageLibrary != null)
         {
            if (trackedImageManager.referenceLibrary != imageLibrary as IReferenceImageLibrary)
            {
               Debug.LogError($"image libraries do not match on {gameObject.name}.", this);
               return;
            }
         }
         else if (trackedImageManager != null && trackedImageManager.referenceLibrary is XRReferenceImageLibrary)
         {
            imageLibrary = trackedImageManager.referenceLibrary as XRReferenceImageLibrary;
         }

         var convertedList = new List<SpawnConfig>();
         var configList = spawnConfigs?.ToList();
         for (int i = 0; i < imageLibrary?.count; i++)
         {
            var ctx = imageLibrary[i];
            convertedList.Add(new SpawnConfig(ctx, configList.Find(x => x.Equals(ctx))));
         }

         spawnConfigs = convertedList.ToArray();
      }
   }

   // TODO Make a parent class for static things or at least for sandbox testing the rest of the systems


   public void SetLibrary(SpawnConfig[] spawnConfigs, XRReferenceImageLibrary imageLibrary)
   {
      if (imageLibrary == null || spawnConfigs.IsNullOrEmpty())
      {
         Debug.LogError($"library or spawnConfigs is null.", this);
         return;
      }

      // if (!swapLibraryReference && trackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
      // {
      //    // try
      //    // {
      //    //    mutableLibrary.se;
      //    // }
      //    // catch (InvalidOperationException e)
      //    // {
      //    //    Debug.LogError($"Clear threw exception: {e.Message}", this);
      //    // }
      //    try
      //    {
      //       foreach (var image in imageLibrary)
      //       {
      //          // Note: You do not need to do anything with the returned JobHandle, but it can be
      //          // useful if you want to know when the image has been added to the library since it may
      //          // take several frames.
      //          // image.jobState = 
      //          mutableLibrary.ScheduleAddImageWithValidationJob(image.texture, image.name, image.width);
      //       }
      //    }
      //    catch (InvalidOperationException e)
      //    {
      //       Debug.LogError($"ScheduleAddImageJob threw exception: {e.Message}", this);
      //    }
      // }
      // else
      // {
      //    if (!swapLibraryReference) Debug.LogError($"The reference image library is not mutable.");

      trackedImageManager.referenceLibrary = imageLibrary;

      // }


      foreach (var element in spawnedPrefabs)
         Destroy(element.liveObject);

      spawnedPrefabs.Clear();

      OnDisable();
      this.spawnConfigs = spawnConfigs;
      this.imageLibrary = imageLibrary;
      enabled = true;
      trackedImageManager.enabled = true;

      if (!gameObject.activeInHierarchy) Debug.LogError($"game object is not active in hierarchy.", this);

      OnEnable();
   }

   private ARTrackedImageManager trackedImageManager;

   private void Awake()
   {
      trackedImageManager = GetComponent<ARTrackedImageManager>();
      trackedImageManager.enabled = trackedImageManager.referenceLibrary != null;
   }

   private void OnEnable()
   {
      trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
   }

   private void OnDisable()
   {
      trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
   }

   // private void Update()
   // {
   //    if (trackedImageManager == null || spawnConfigs == null) return;

   //    var currentList = trackedImageManager.trackables.GetEnumerator().SingleToEnumerable();
   //    var removed = new List<TrackableId>(spawnedPrefabs.Keys.ToList());
   // }

   private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
   {
      // Debug.Log($"{spawnedPrefabs.Count} spawned, TrackedImagesChanged: {eventArgs.added.Count} added, {eventArgs.updated.Count} updated, {eventArgs.removed.Count} removed.");

      var liveList = new List<ARTrackedImage>(eventArgs.added);
      liveList.AddRange(eventArgs.updated);

      // Debug.Log($"{spawnedPrefabs.Count} spawned, TrackedImagesChanges: {liveList.Count} added + updated, {eventArgs.removed.Count} removed.");
      foreach (var newItem in liveList)
      {
         var id = newItem.trackableId;
         var ctx = spawnedPrefabs.Find(x => x.IsMatching(id));
         if (ctx == null && TryGetPrefab(newItem, out var prefab))
         {
            var spawned = Instantiate(prefab, newItem.transform);
            ctx = new(id, spawned);
            spawnedPrefabs.Add(ctx);
         }

         ctx?.SetTrackingState(newItem.trackingState);
      }

      foreach (var removedItems in eventArgs.removed)
      {
         var id = removedItems.Key;
         var element = spawnedPrefabs.Find(x => x.IsMatching(id));
         if (element != null)
         {
            element.SetTrackingState(TrackingState.None);
            Destroy(element.liveObject);
            spawnedPrefabs.Remove(element);
         }
      }
      liveData = new(spawnedPrefabs);
   }
   [SerializeField] List<TrackableObjectData> liveData;

#if UNITY_EDITOR && BYPASS_REFERENCE_MATCHING
   [SerializeField] int selectedConfigIndex = 0;
#endif
   private bool TryGetPrefab(ARTrackedImage trackedImage, out GameObject prefab)
   {
#if UNITY_EDITOR && BYPASS_REFERENCE_MATCHING
      var index = selectedConfigIndex % spawnConfigs.Length;
      var result = spawnConfigs[index < 0 ? index + spawnConfigs.Length : index];
#else
      var result = spawnConfigs.ToList().Find(x => x.Equals(trackedImage.referenceImage));
#endif
      prefab = result?.prefab;
      if (result == null) Debug.LogError($"No match found for {trackedImage.referenceImage.name}.", this);
      return result != null;
   }

}


public abstract class ReferenceImageObjectManager : MonoBehaviour
{

   protected static List<TrackableObjectData> spawnedPrefabs = new();

   public static List<TrackableObjectData> GetLiveObjectData() => new(spawnedPrefabs);

   [Serializable]
   public class TrackableObjectData
   {
      public event Action<TrackingState> onTrackingStateChanged;
      public TrackableId trackableId; // { get; private set; }
      public GameObject liveObject; // { get; private set; }
      public TrackingState trackingState = TrackingState.None; // { get; private set; };

      public void SetTrackingState(TrackingState trackingState)
      {
         if (this.trackingState == trackingState) return;
         this.trackingState = trackingState;
         onTrackingStateChanged?.Invoke(trackingState);
      }

      public TrackableObjectData(TrackableId trackableId, GameObject liveObject) => (this.trackableId, this.liveObject) = (trackableId, liveObject);
      public bool IsMatching(TrackableId comparisonValue) => trackableId == comparisonValue;
      public bool IsMatching(GameObject comparisonValue) => liveObject == comparisonValue;
   }
}