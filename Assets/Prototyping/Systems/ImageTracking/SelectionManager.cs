using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

public class SelectionManager : MonoBehaviour
{
   // [SerializeField] InputAction focusAction = new InputAction(name: "Tap", type: InputActionType.Button, binding: "<Pointer>/press", interactions: "Tap");
   // {"m_Name":"","m_Id":"0d3ce0a5-81d7-4226-aa12-65fd59147799","m_Path":"<Pointer>/press","m_Interactions":"Tap","m_Processors":"","m_Groups":"","m_Action":"Focus","m_Flags":0}
   [SerializeField] bool autoConfigure = true;
   [SerializeField, Range(0f, 5f)] float minActionInterval = 1f;
   [SerializeField, Range(.001f, .2f)] float rayRadius = .01f;
   [SerializeField] LayerMask focusLayerMask = 1 << 3;
   [SerializeField] Camera mainCam;
   [SerializeField] ImageGameObjectSpawner spawner;
   [SerializeField, ReadOnly] ARTrackedImageManager trackedImageManager;

   private void OnValidate()
   {
      if (!autoConfigure) return;
      if (spawner != null) trackedImageManager = spawner.GetComponent<ARTrackedImageManager>();
      else Debug.LogError($"Missing {nameof(spawner)} reference on {name}.");
      if (mainCam != null) Debug.LogError($"Missing {nameof(mainCam)} reference on {name}."); ;
      autoConfigure = mainCam == null || spawner == null || trackedImageManager == null;
   }

   // private void Awake()
   // {
   //    focusAction.performed += (context) => OnFocus(true, context);
   //    focusAction.canceled += (context) => OnFocus(false, context);
   // }

   static ObjectFocuser selectedFocuser;
   static Camera Cam;
   static float RayRadius;
   static LayerMask FocusLayerMask;

   public static void SelectFocus(Vector2 screenPosition)
   {
      Ray ray = Cam.ScreenPointToRay(screenPosition);
      if (Physics.SphereCast(ray, RayRadius, out RaycastHit hit, 2f, FocusLayerMask, QueryTriggerInteraction.Ignore))
      {
         selectedFocuser = hit.collider.GetComponent<ObjectFocuser>();

         Debug.DrawLine(ray.origin, hit.point, Color.green, 5f);
      }
      else
         Debug.DrawRay(ray.origin, ray.direction * 2f, Color.red, 5f);
   }

   public static bool TryFocus(Vector2 screenPosition)
   {
      Ray ray = Cam.ScreenPointToRay(screenPosition);
      if (Physics.SphereCast(ray, RayRadius, out RaycastHit hit, 2f, FocusLayerMask, QueryTriggerInteraction.Ignore))
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

   static FocusViewBackground backgroundObject;

   private static void Focus()
   {
      var backgroundObject = selectedFocuser.Focus(Cam);
      if (backgroundObject != null)
      {
         SelectionManager.backgroundObject = GameObject.Instantiate(backgroundObject, Cam.transform);
      }
   }

   public static void StopFocus()
   {
      selectedFocuser?.DeFocus();

      if (backgroundObject != null)
         GameObject.Destroy(backgroundObject.gameObject, backgroundObject.Hide());
   }


   private void Awake()
   {
      if (Cam == null && RayRadius == default && FocusLayerMask == default)
      {
         Cam = mainCam;
         RayRadius = rayRadius;
         FocusLayerMask = focusLayerMask;
      }
      else if (Cam != mainCam || RayRadius != rayRadius || FocusLayerMask != focusLayerMask)
      {
         Debug.LogError($"the static references are already set there should be only one instance of SelectionManager in the scene simultaneously.", this);
         enabled = false;
      }
   }


   private void OnDestroy()
   {
      if (!enabled) return;
      if (Cam == mainCam && RayRadius == rayRadius && FocusLayerMask == focusLayerMask)
      {
         Cam = null;
         RayRadius = default;
         FocusLayerMask = default;
      }
   }

   float lastActionTime = 0f;
   private void Update()
   {
      if (lastActionTime + minActionInterval < Time.time && ReferenceImageSelectableObject.UpdateSelection(mainCam))
         lastActionTime = Time.time;
   }
}
