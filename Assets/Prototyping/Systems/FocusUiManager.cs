using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FocusUiManager : MonoBehaviour
{
   [SerializeField] UiCustomPointerEvents raycastArea;

   [SerializeField] UnityEvent onFocus, onDeFocus;

   public static bool isFocused { get; private set; } = false;

   private void Awake()
   {
      raycastArea.PointerDownPosition.AddListener(OnPointerDown);
      raycastArea.PointerUpPosition.AddListener(OnPointerUp);
   }

   void OnPointerDown(Vector2 pos)
   {
      if (isFocused) return;
      SelectionManager.SelectFocus(pos);
   }

   void OnPointerUp(Vector2 pos)
   {
      if (isFocused) return;
      isFocused = SelectionManager.TryFocus(pos);
      if (isFocused) onFocus.Invoke();
   }

   public void DeFocus()
   {
      if (!isFocused) return;

      isFocused = false;
      onDeFocus.Invoke();
      SelectionManager.StopFocus();
   }
}
