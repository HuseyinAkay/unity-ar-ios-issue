using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class UiCustomPointerEvents : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
   [SerializeField] public UnityEvent<Vector2> PointerEnterPosition, PointerMoveDelta, PointerExitPosition, PointerDownPosition, PointerUpPosition;

   public event Action<PointerEventData> OnPointerEnter, OnPointerMove, OnPointerExit, OnPointerDown, OnPointerUp;

   void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
   {
      PointerDownPosition.Invoke(eventData.position);
      OnPointerDown?.Invoke(eventData);
   }

   void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
   {
      PointerEnterPosition.Invoke(eventData.position);
      OnPointerEnter?.Invoke(eventData);
   }

   void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
   {
      PointerExitPosition.Invoke(eventData.position);
      OnPointerExit?.Invoke(eventData);
   }

   void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
   {
      PointerUpPosition.Invoke(eventData.position);
      OnPointerUp?.Invoke(eventData);
   }

   void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
   {
      PointerMoveDelta.Invoke(eventData.delta);
      OnPointerMove?.Invoke(eventData);
   }
}
