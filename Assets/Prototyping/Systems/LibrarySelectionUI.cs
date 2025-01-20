using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LibrarySelectionUI : MonoBehaviour
{
   [SerializeField] TMP_Dropdown dropdown;

   List<TrackedImageLibraryPackage> packages;
   LibraryManager libraryManager;

   private void Awake()
   {
      dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
   }

   public void Init(LibraryManager libraryManager, List<TrackedImageLibraryPackage> packages, int selectedIndex)
   {
      this.packages = packages;
      dropdown.options = packages.ConvertAll(x => new TMP_Dropdown.OptionData(x.Name, x.Icon, default));
      dropdown.value = selectedIndex;

      this.libraryManager = libraryManager;
   }

   public void OnDropdownValueChanged(int index)
   {
      if (libraryManager != null)
         libraryManager.SelectLibrary(packages[index]);
   }
}
