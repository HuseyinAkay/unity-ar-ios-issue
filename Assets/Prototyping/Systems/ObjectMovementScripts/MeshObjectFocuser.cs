using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class MeshObjectFocuser : ObjectFocuser
{
   [SerializeField, ReadOnly] MeshCollider meshCollider;
   [SerializeField] bool autoConfigureSize = true;

   protected override void OnValidate()
   {
      base.OnValidate();
      if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();
      if (meshCollider == null) Debug.LogError($"Missing {nameof(MeshCollider)} component.", this);
      else if (autoConfigureSize && selectionManager != null)
      {
         var offset = selectionManager.GetOriginLocal(false);
         Vector3 ConvertVector(Vector3 val)
         {
            var result = transform.TransformPoint(val);
            result = selectionManager.transform.InverseTransformPoint(result);
            return result-offset;
         }

         size = ConvertSize(FindLocalBounds(meshCollider.sharedMesh, ConvertVector));
         autoConfigureSize = size != default && size.x > 0 && size.y > 0;
      }
   }

   static Vector3 FindLocalBounds(Mesh mesh, Transform parent)
   {
      Vector3 result = default;
      for (int i = 0; i < mesh.vertices.Length; i++)
      {
         var vert = mesh.vertices[i];
         result.x = Mathf.Max(result.x, MathF.Abs(vert.x));
         result.y = Mathf.Max(result.y, MathF.Abs(vert.y));
         result.z = Mathf.Max(result.z, MathF.Abs(vert.z));
      }
      return parent.TransformVector(result * 2f);
   }

   static Vector3 FindLocalBounds(Mesh mesh, Func<Vector3, Vector3> conversionMethod)
   {
      Vector3 result = default;
      for (int i = 0; i < mesh.vertices.Length; i++)
      {
         var vert = conversionMethod(mesh.vertices[i]);
         result.x = Mathf.Max(result.x, MathF.Abs(vert.x));
         result.y = Mathf.Max(result.y, MathF.Abs(vert.y));
         result.z = Mathf.Max(result.z, MathF.Abs(vert.z));
      }
      return result * 2f;
   }


   static Vector2 ConvertSize(Vector3 bounds, Transform parent) => ConvertSize(parent.InverseTransformVector(bounds));
   static Vector2 ConvertSize(Vector3 bounds) => new Vector2(bounds.x, bounds.z);

   // [SerializeField] Quaternion test = default;
   // protected override void OnDrawGizmos()
   // {

   //    Vector2 GetRotationSizeMultiplier()
   //    {
   //       var rot = test;
   //       var corner1 = rot * (Vector3.right + Vector3.forward);
   //       var corner2 = rot * (Vector3.right + Vector3.back);
   //       var vertical = Mathf.Max(Vector3.Project(corner1, Vector3.forward).magnitude, Vector3.Project(corner2, Vector3.forward).magnitude);
   //       var horizontal = Mathf.Max(Vector3.Project(corner1, Vector3.right).magnitude, Vector3.Project(corner2, Vector3.right).magnitude);
   //       return new Vector2(horizontal, vertical);
   //    }

   //    Gizmos.color = Color.red;
   //    var scale = GetRotationSizeMultiplier();
   //    Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * scale.y);
   //    Gizmos.DrawLine(transform.position, transform.position + Vector3.right * scale.x);
   // }
}

public abstract class ObjectFocuser : MonoBehaviour
{
   [SerializeField, ReadOnly] protected ReferenceImageSelectableObject selectionManager;
   [SerializeField] protected FocusViewBackground backgroundObject;
   [SerializeField] protected bool fitIncludingRotation = true;
   [SerializeField] protected Vector2 size = Vector2.one * .1f;
   [SerializeField] protected float padding = .01f;

   public virtual float GetCameraDistance(Camera cam)
   {
      var fovVertical = cam.fieldOfView / 2f;

      var sizeVec = fitIncludingRotation ? Vector2.Scale(size, GetRotationSizeMultiplier()) : size;

      float v = 1f / Mathf.Tan(fovVertical * Mathf.Deg2Rad);
      var vertical = ((sizeVec.y / 2f) + padding) * v;
      var horizontal = ((sizeVec.x / 2f) + padding) * (v / cam.aspect);

      var result = Mathf.Max(vertical, horizontal);

      return result;
   }

   Vector2 GetRotationSizeMultiplier()
   {
      var rot = selectionManager.originalRotation;
      var corner1 = rot * (Vector3.right + Vector3.forward);
      var corner2 = rot * (Vector3.right + Vector3.back);
      var vertical = Mathf.Max(Vector3.Project(corner1, Vector3.forward).magnitude, Vector3.Project(corner2, Vector3.forward).magnitude);
      var horizontal = Mathf.Max(Vector3.Project(corner1, Vector3.right).magnitude, Vector3.Project(corner2, Vector3.right).magnitude);
      return new Vector2(horizontal, vertical);
   }

   protected virtual void OnDrawGizmos()
   {
      if (selectionManager == null) return;
      Gizmos.color = Color.red;
      Gizmos.matrix = Matrix4x4.TRS(selectionManager.transform.position, selectionManager.transform.rotation, Vector3.one);
      var origin = selectionManager.GetOriginLocal(true);
      Gizmos.DrawWireCube(origin, new(size.x, .002f, size.y));
      if (padding == 0) return;
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireCube(origin, new(size.x + padding, .002f, size.y + padding));
   }

   protected virtual void OnValidate()
   {
      if (selectionManager == null) selectionManager = GetComponentInParent<ReferenceImageSelectableObject>();
   }

   public virtual FocusViewBackground Focus(Camera cam)
   {
      if (selectionManager == null)
         throw new Exception($"Missing {nameof(selectionManager)} reference on {name}");
      selectionManager.Focus(cam, GetCameraDistance(cam));
      return backgroundObject;
   }

   public virtual void DeFocus()
   {
      if (selectionManager == null)
         throw new Exception($"Missing {nameof(selectionManager)} reference on {name}");
      selectionManager.DeFocus();
   }
}
