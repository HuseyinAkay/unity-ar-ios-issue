using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomizableAnimationCallbacks : MonoBehaviour
{
   [SerializeField] UnityEvent event1, event2, event3, event4, event5;
   public event Action OnEvent1, OnEvent2, OnEvent3, OnEvent4, OnEvent5;

   void Event1Fire()
   {
      OnEvent1?.Invoke();
      event1.Invoke();
   }

   void Event2Fire()
   {
      OnEvent2?.Invoke();
      event2.Invoke();
   }

   void Event3Fire()
   {
      OnEvent3?.Invoke();
      event3.Invoke();
   }

   void Event4Fire()
   {
      OnEvent4?.Invoke();
      event4.Invoke();
   }

   void Event5Fire()
   {
      OnEvent5?.Invoke();
      event5.Invoke();
   }
}