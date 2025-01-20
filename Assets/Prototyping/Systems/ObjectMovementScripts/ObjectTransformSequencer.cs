using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MyBox;
using TweenSequence.Configuration;
using UnityEngine;

public class ObjectTransformSequencer : TransitionSequencer
{
   [SerializeField] bool reverseSequences = false, doMoveTransition = true, doRotateTransition = false, doScaleTransition = true;

   [SerializeField, ConditionalField(nameof(doMoveTransition))] BidirectionalTransformTween moveTransition = new BidirectionalTransformTween(Vector3.up);
   [SerializeField, ConditionalField(nameof(doRotateTransition))] BidirectionalTransformTween rotateTransition = new BidirectionalTransformTween(Vector3.one);
   [SerializeField, ConditionalField(nameof(doScaleTransition))] BidirectionalTransformTween scaleTransition = new BidirectionalTransformTween(Vector3.one);

   [Serializable]
   protected class BidirectionalTransformTween : SequenceGenericAmountTweenSetup<Vector3>
   {
      [SerializeField] protected List<SequenceCallback> reverseActions = new();
      [SerializeField, ConditionalField(nameof(customReverseTiming))] float reverseDuration = 1f, reverseDelay = 0f;
      [SerializeField] bool customReverseTiming = false, invertDirection = false, rollbackEachSequenceToStartValue = false;
      Action<Vector3> valueSetter;
      Vector3 defaultValue;

      public BidirectionalTransformTween(Vector3 amount)
      {
         this.amount = amount;
      }

      public void Init(Func<float, Vector3, Tween> tweeningFunction, Action<Vector3> valueSetter, Vector3 defaultValue)
      {
         this.defaultValue = defaultValue;
         this.valueSetter = valueSetter;
         base.Init(tweeningFunction);
      }

      public List<(float, Action)> InsertToSequence(Sequence sequence, bool reverse)
      {
         reverse = invertDirection ? !reverse : reverse;
         var endValue = reverse ? defaultValue + amount : defaultValue;

         if (rollbackEachSequenceToStartValue && valueSetter != null)
            valueSetter(reverse ? defaultValue : defaultValue + amount);

         var initialDelay = reverse && customReverseTiming ? reverseDelay : this.initialDelay;
         var duration = reverse && customReverseTiming ? reverseDuration : this.duration;

         sequence.Insert(initialDelay, tween(endValue, duration).SetEase(ease));

         return (reverse ? reverseActions : actions).ConvertAll(x => x.GetDelayAndAction());
      }
   }

   private void Awake()
   {
      if (!doMoveTransition && !doRotateTransition && !doScaleTransition)
      {
         Debug.LogWarning($"no transitions sequence configured", this);
         return;
      }

      if (doMoveTransition)
         moveTransition.Init((duration, endValue) => transform.DOLocalMove(endValue, duration), (val) => transform.localPosition = val, transform.localPosition);
      if (doRotateTransition)
         rotateTransition.Init((duration, endValue) => transform.DOLocalRotate(endValue, duration), (val) => transform.localRotation = Quaternion.Euler(val), transform.localRotation.eulerAngles);
      if (doScaleTransition)
         scaleTransition.Init((duration, endValue) => transform.DOScale(endValue, duration), (val) => transform.localScale = val, transform.localScale);
   }

   protected override Action<Sequence> GetAction(e_TransitionType transitionType)
   {
      if (transitionType is not e_TransitionType.Show and not e_TransitionType.Hide) return null;

      var reverse = transitionType == e_TransitionType.Hide == !reverseSequences;
      return (sequence) =>
      {
         var sequenceCallbacks = new List<(float, Action)>();
         if (doMoveTransition) sequenceCallbacks.AddRange(moveTransition.InsertToSequence(sequence, reverse));
         if (doRotateTransition) sequenceCallbacks.AddRange(rotateTransition.InsertToSequence(sequence, reverse));
         if (doScaleTransition) sequenceCallbacks.AddRange(scaleTransition.InsertToSequence(sequence, reverse));

         InitiateSequenceCallbacks(sequenceCallbacks);
      };
   }


   void InitiateSequenceCallbacks(List<(float, Action)> sequenceCallbacks)
   {
      if (sequenceCallbacksRoutine != null)
      {
         StopCoroutine(sequenceCallbacksRoutine);
         foreach (var (_, callback) in sequenceCallbacks) callback?.Invoke();
      }

      if (sequenceCallbacks.IsNullOrEmpty()) return;

      this.sequenceCallbacks = sequenceCallbacks.OrderBy(x => x.Item1).ToList();

      sequenceCallbacksRoutine = StartCoroutine(SequenceCallbacks());
   }

   List<(float, Action)> sequenceCallbacks;

   IEnumerator SequenceCallbacks()
   {
      var duration = 0.0f;
      while (sequenceCallbacks.Count > 0)
      {
         (float, Action) ctx = sequenceCallbacks[0];
         float wait = ctx.Item1 - duration;
         if (wait > 0) yield return new WaitForSeconds(wait);
         ctx.Item2?.Invoke();
         sequenceCallbacks.RemoveAt(0);

         if (sequenceCallbacks.Count <= 0) break;
      }
   }

   Coroutine sequenceCallbacksRoutine;
}
