using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using SpawnConfig = ImageGameObjectSpawner.SpawnConfig;


[CreateAssetMenu(fileName = "TrackedImagesPackage", menuName = "TDV/Tracked Images Package")]
public class TrackedImageLibraryPackage : ScriptableObject
{
   [SerializeField] XRReferenceImageLibrary library;
   [SerializeField] string displayName = "Tracked Images";
   [SerializeField] Sprite icon;

   [SerializeField] bool autoResize = false;
   [SerializeField, ConditionalField(nameof(autoResize))] string sizeSettingKeyword = "SizeAdjusted";

   [SerializeField] SpawnConfig[] spawnConfigs;

   public XRReferenceImageLibrary Library => library;
   public SpawnConfig[] SpawnConfigs => spawnConfigs;
   public Sprite Icon => icon;
   public string Name => displayName;

   private void OnValidate()
   {
      if (library != null)
      {
         var spawnConfigs = new List<SpawnConfig>();
         var configList = this.spawnConfigs.ToList();

         for (int i = 0; i < library?.count; i++)
         {
            var ctx = library[i];
            spawnConfigs.Add(new SpawnConfig(ctx, configList.Find(x => x.Equals(ctx))));
         }

         this.spawnConfigs = spawnConfigs.ToArray();
      }

      if (autoResize && !sizeSettingKeyword.IsNullOrEmpty())
      {
         foreach (var config in this.spawnConfigs)
         {
            GameObject prefab = config.prefab;
            var size = config.ReferenceImage.specifySize ? config.ReferenceImage.size.MaxComponent() : 1f;
            if (prefab != null) SetSize(prefab.transform, size);
         }

         void SetSize(Transform transform, float size)
         {
            if (transform.name == sizeSettingKeyword)
               transform.localScale = Vector3.one * size;

            foreach (Transform child in transform)
               SetSize(child, size);
         }
      }
   }
}
