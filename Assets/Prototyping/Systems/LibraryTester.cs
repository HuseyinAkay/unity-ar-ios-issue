using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyBox;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARSubsystems;

public class LibraryTester : ReferenceImageObjectManager
{
   [SerializeField] TrackedImageLibraryPackage testLibrary;
   [SerializeField] MeshRenderer quadPrefab;
   [SerializeField] UiCustomPointerEvents pointerEvents;
   [SerializeField] LayerMask focusLayerMask = 1 << 3;

   [SerializeField] uint lineCapacity = 10;
   [SerializeField] bool quadNormalsAlignedToZAxis = true;


   // GenericPropertyJSON:{"name":"changeObjectViewed","type":-1,"children":[{"name":"m_Name","type":3,"val":"Change Object Viewed"},{"name":"m_Type","type":7,"val":"Enum:Pass Through"},{"name":"m_ExpectedControlType","type":3,"val":"Vector2"},{"name":"m_Id","type":3,"val":"f2fe0829-f59b-48f7-8e1c-b5cccb05506b"},{"name":"m_Processors","type":3,"val":""},{"name":"m_Interactions","type":3,"val":""},{"name":"m_SingletonActionBindings","type":-1,"arraySize":5,"arrayType":"InputBinding","children":[{"name":"Array","type":-1,"arraySize":5,"arrayType":"InputBinding","children":[{"name":"size","type":12,"val":5},{"name":"data","type":-1,"children":[{"name":"m_Name","type":3,"val":"2D Vector"},{"name":"m_Id","type":3,"val":"7a3071f6-f06d-448c-9c64-ff876f7410e6"},{"name":"m_Path","type":3,"val":"2DVector"},{"name":"m_Interactions","type":3,"val":""},{"name":"m_Processors","type":3,"val":""},{"name":"m_Groups","type":3,"val":""},{"name":"m_Action","type":3,"val":"Change Object Viewed"},{"name":"m_Flags","type":7,"val":"Enum:Composite"}]},{"name":"data","type":-1,"children":[{"name":"m_Name","type":3,"val":"up"},{"name":"m_Id","type":3,"val":"654d3e12-d2ef-4207-b4f3-20ef30417236"},{"name":"m_Path","type":3,"val":"<Keyboard>/upArrow"},{"name":"m_Interactions","type":3,"val":""},{"name":"m_Processors","type":3,"val":""},{"name":"m_Groups","type":3,"val":""},{"name":"m_Action","type":3,"val":"Change Object Viewed"},{"name":"m_Flags","type":7,"val":"Enum:Part Of Composite"}]},{"name":"data","type":-1,"children":[{"name":"m_Name","type":3,"val":"down"},{"name":"m_Id","type":3,"val":"34102e1e-edb2-4a9a-a99a-feba376db75c"},{"name":"m_Path","type":3,"val":"<Keyboard>/downArrow"},{"name":"m_Interactions","type":3,"val":""},{"name":"m_Processors","type":3,"val":""},{"name":"m_Groups","type":3,"val":""},{"name":"m_Action","type":3,"val":"Change Object Viewed"},{"name":"m_Flags","type":7,"val":"Enum:Part Of Composite"}]},{"name":"data","type":-1,"children":[{"name":"m_Name","type":3,"val":"left"},{"name":"m_Id","type":3,"val":"1bd457ff-97ba-4fd8-8120-e8f64e4386da"},{"name":"m_Path","type":3,"val":"<Keyboard>/leftArrow"},{"name":"m_Interactions","type":3,"val":""},{"name":"m_Processors","type":3,"val":""},{"name":"m_Groups","type":3,"val":""},{"name":"m_Action","type":3,"val":"Change Object Viewed"},{"name":"m_Flags","type":7,"val":"Enum:Part Of Composite"}]},{"name":"data","type":-1,"children":[{"name":"m_Name","type":3,"val":"right"},{"name":"m_Id","type":3,"val":"908e3dc0-c3e0-400a-b735-3a32e708853e"},{"name":"m_Path","type":3,"val":"<Keyboard>/rightArrow"},{"name":"m_Interactions","type":3,"val":""},{"name":"m_Processors","type":3,"val":""},{"name":"m_Groups","type":3,"val":""},{"name":"m_Action","type":3,"val":"Change Object Viewed"},{"name":"m_Flags","type":7,"val":"Enum:Part Of Composite"}]}]}]},{"name":"m_Flags","type":7,"val":"Enum:0"}]}
   [SerializeField] InputAction changeObjectViewed;

   [SerializeField] Camera cam;

   TrackedImageLibraryPackage _currentLibrary;

   private void Awake()
   {
      if (quadPrefab == null)
      {
         quadPrefab = GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<MeshRenderer>();
         quadPrefab.transform.parent = transform;
         quadPrefab.transform.localPosition = Vector3.zero;
         quadPrefab.transform.localRotation = Quaternion.identity;
         quadPrefab.transform.localScale = Vector3.one;
         quadPrefab.gameObject.SetActive(false);
      }

      changeObjectViewed.performed += ChangeObjectViewed;
      changeObjectViewed.Enable();

      if (pointerEvents != null)
      {
         pointerEvents.OnPointerDown += OnPointerDown;
         pointerEvents.OnPointerUp += OnPointerUp;
      }
   }

   void OnPointerDown(PointerEventData eventData)
   {
      if (!focused)
         SelectFocus(eventData.position);
   }

   void OnPointerUp(PointerEventData eventData)
   {
      if (!focused)
         TryFocus(eventData.position);
      else
         DeFocus();
   }

   void DeFocus()
   {
      selectedFocuser?.DeFocus();

      if (backgroundObject != null)
         GameObject.Destroy(backgroundObject.gameObject, backgroundObject.Hide());

      focused = false;
   }

   void SelectFocus(Vector2 screenPosition)
   {
      Ray ray = cam.ScreenPointToRay(screenPosition);
      if (Physics.SphereCast(ray, .1f, out RaycastHit hit, 2f, focusLayerMask, QueryTriggerInteraction.Ignore))
      {
         selectedFocuser = hit.collider.GetComponent<ObjectFocuser>();

         Debug.DrawLine(ray.origin, hit.point, Color.green, 5f);
      }
      else
         Debug.DrawRay(ray.origin, ray.direction * 2f, Color.red, 5f);
   }

   bool TryFocus(Vector2 screenPosition)
   {
      Ray ray = cam.ScreenPointToRay(screenPosition);
      if (Physics.SphereCast(ray, .1f, out RaycastHit hit, 2f, focusLayerMask, QueryTriggerInteraction.Ignore))
      {
         var focuser = hit.collider.GetComponent<ObjectFocuser>();
         if (focuser != null)
         {
            if (selectedFocuser != null && selectedFocuser == focuser)
            {
               // Focus previously selected object
               Debug.Log($"focusing previously selected object {focuser.name}.", focuser);
               Focus();
               return true;
            }
         }

         Debug.DrawLine(ray.origin, hit.point, Color.red, 5f);
      }
      else
         Debug.DrawRay(ray.origin, ray.direction * 2f, Color.yellow, 5f);

      selectedFocuser = null;
      return false;
   }

   void Focus()
   {
      var backgroundObject = selectedFocuser.Focus(cam);
      if (backgroundObject != null)
      {
         LibraryTester.backgroundObject = GameObject.Instantiate(backgroundObject, cam.transform);
      }
      focused = true;
   }

   static bool focused = false;
   static ObjectFocuser selectedFocuser;
   static FocusViewBackground backgroundObject;


   uint viewIndex = 0;
   [SerializeField, Range(0.1f, 1f)] float camDistance = 0.5f;

   void ChangeObjectViewed(InputAction.CallbackContext ctx)
   {
      var vec = ctx.ReadValue<Vector2>();

      if (vec.sqrMagnitude > 0f)
      {
         int viewIndex = (int)this.viewIndex;

         viewIndex = (Mathf.RoundToInt(vec.y) * (int)lineCapacity) + Mathf.RoundToInt(vec.x) + viewIndex;
         viewIndex = viewIndex < 0 ? liveObjects.Length + viewIndex : viewIndex % liveObjects.Length;

         if (viewIndex - this.viewIndex != 0)
         {
            UpdateCameraView(liveObjects[viewIndex]);
            this.viewIndex = (uint)viewIndex;
         }
      }
   }

   void Update()
   {
      if (_currentLibrary != testLibrary)
         InitLibrary(testLibrary);

      ReferenceImageSelectableObject.UpdateSelection(cam);

      if (cam.transform.parent != null)
         cam.transform.localPosition = cam.transform.localPosition.normalized * camDistance;
      else
         Debug.LogError($"Please set a parent on {cam.name}, it will be used for camera target origin.", cam);
   }

   void UpdateCameraView(MeshRenderer obj)
   {
      if (cam.transform.parent != null && cam.transform.parent != transform)
      {
         DOTween.Kill(this, false);
         var seq = DOTween.Sequence(this);
         seq.Insert(0f, cam.transform.parent.DOMove(obj.transform.position, .5f));
         seq.Insert(0f, cam.transform.parent.DORotateQuaternion(obj.transform.rotation, .5f));
      }
      else
         Debug.LogError($"Please set a parent on {cam.name}, it will be used for camera target origin.", cam);
   }

   MeshRenderer[] liveObjects;

   void InitLibrary(TrackedImageLibraryPackage libraryPackage)
   {
      var liveObjects = new List<MeshRenderer>();
      var spawnedPrefabs = new List<TrackableObjectData>();

      var referenceLibrary = libraryPackage.Library;
      var targetCount = referenceLibrary.count;
      var configs = libraryPackage.SpawnConfigs;
      var maxSize = Vector2.zero;
      var liveCount = this.liveObjects.IsNullOrEmpty() ? 0 : this.liveObjects.Length;

      for (int i = 0; i < targetCount; i++)
      {
         var ctx = referenceLibrary[i];

         var obj = NewReferenceObject(ctx, (liveCount > i ? this.liveObjects[i] : null));

         GameObject prefab = null;

         if (obj != null && configs[i].prefab != null)
            prefab = Instantiate(configs[i].prefab, obj.transform.parent);
         else
            Debug.LogWarning($"Something went wrong with object creation for {ctx.name}");

         if (ctx.specifySize) maxSize = Vector2.Max(maxSize, ctx.size);

         liveObjects.Add(obj);
         var data = new TrackableObjectData(default, prefab);
         data.SetTrackingState(TrackingState.Tracking);
         spawnedPrefabs.Add(data);
      }

      for (int i = 0; i < liveObjects.Count; i++)
      {
         var xIndex = i % (int)lineCapacity;
         var yIndex = i / (int)lineCapacity;
         var max = maxSize.MaxComponent();

         var obj = liveObjects[i].transform;
         XRReferenceImage reference = referenceLibrary[i];

         obj.localScale = reference.specifySize ? reference.size.WithZ(max) : Vector3.one * max;
         obj.parent.position = Vector3.right * (xIndex * maxSize.x) + (quadNormalsAlignedToZAxis ? Vector3.up : Vector3.forward) * (yIndex * maxSize.y);
      }

      for (int i = targetCount; i < liveCount; i++)
         Destroy(this.liveObjects[i].transform.parent.gameObject);

      ReferenceImageObjectManager.spawnedPrefabs = spawnedPrefabs;
      this.liveObjects = liveObjects.ToArray();
      _currentLibrary = testLibrary;

      // StartCoroutine(AssignReferenceImages());
   }


   MeshRenderer NewReferenceObject(XRReferenceImage ctx, MeshRenderer husk = null)
   {
      if (husk == null)
      {
         var objectParent = new GameObject();
         objectParent.transform.SetParent(transform, true);
         husk = Instantiate(quadPrefab, objectParent.transform.position, quadPrefab.transform.rotation, objectParent.transform);
      }

      if (ctx.texture == null)
         Debug.LogError($"Missing texture for {ctx.name}", husk);

      string name = ctx.name;
      husk.transform.parent.name = name;
      husk.name = ctx.name;
      husk.gameObject.SetActive(true);

      var mat = Material.Instantiate(husk.material);
      mat.mainTexture = ctx.texture;
      husk.material = mat;
      // husk.material.SetTexture("_BaseMap", ctx.texture);

      return husk;
   }

   // IEnumerator AssignReferenceImages()
   // {
   //    yield return new WaitForSeconds(.1f);
   //    yield return new WaitForEndOfFrame();
   //    var library = testLibrary.Library;
   //    for (int i = 0; i < liveObjects.Length; i++)
   //    {
   //       var obj = liveObjects[i];
   //       // obj.material.mainTexture = library[i].texture;
   //       var mat = new Material() obj.material;.SetTexture("_BaseMap",library[i].texture);
   //    }
   // }
}
