using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class LibraryManager : MonoBehaviour
{
   [SerializeField] LibrarySelectionUI librarySelectionUI;
   [SerializeField] TrackingInfoDebug trackingInfoDebug;
   [SerializeField] ImageGameObjectSpawner spawner;
   [SerializeField] TrackedImageLibraryPackage defaultSelectedLibrary;
   [SerializeField] List<TrackedImageLibraryPackage> packages = new();

   private void Awake()
   {
      defaultSelectedLibrary = defaultSelectedLibrary == null ? packages[0] : defaultSelectedLibrary;
      librarySelectionUI.Init(this, packages, packages.IndexOf(defaultSelectedLibrary));

      Invoke(nameof(SelectDefaultLibrary), 0.1f);
   }

   void SelectDefaultLibrary() => SelectLibrary(defaultSelectedLibrary);


   static TrackedImageLibraryPackage selectedLibrary;
   public void SelectLibrary(TrackedImageLibraryPackage libraryPackage)
   {
      if (selectedLibrary == libraryPackage)
         return;

      selectedLibrary = libraryPackage;

      spawner.SetLibrary(libraryPackage.SpawnConfigs, libraryPackage.Library);
   }
}
