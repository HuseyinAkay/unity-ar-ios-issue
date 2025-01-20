using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MyBox;
using TweenSequence.Configuration;
using UnityEngine;


[RequireComponent(typeof(Renderer))]
public class RendererFadeSequencer : TransitionSequencer
{
   [SerializeField, ReadOnly] Renderer rendr;
   [SerializeField, ConditionalField(nameof(overrideDefaultAlpha)), Range(0f, 1f)] float defaultAlpha = 1f;

   [SerializeField, HideInInspector] bool overrideDefaultAlpha = false;
   [SerializeField, HideInInspector] bool showEnabled, hideEnabled;
   [SerializeField, ConditionalField(nameof(showEnabled))] SequenceAlphaTweenSetup showTransition;
   [SerializeField, ConditionalField(nameof(hideEnabled))] SequenceAlphaTweenSetup hideTransition;

   [Serializable]
   public class SequenceAlphaTweenSetup : SequenceTweenSetup
   {
      public void Init(Renderer rendr, float startingAlpha, float finishAlpha)
      {
         if (rendr == null)
         {
            Debug.LogError($"Missing renderer reference");
            return;
         }
         this.tween = () =>
         {
            rendr.material.color = rendr.material.color.WithAlphaSetTo(startingAlpha);
            return rendr.material.DOFade(finishAlpha, duration);
         };
      }
   }

   protected override Action<Sequence> GetAction(e_TransitionType transition) => (transition, gameObject.activeInHierarchy) switch
   {
      (_, false) => null,
      (e_TransitionType.Show, true) => (seq) => InitiateSequence(seq, showTransition),
      (e_TransitionType.Hide, true) => (seq) => InitiateSequence(seq, hideTransition),
      _ => null
   };

   void InitiateSequence(Sequence seq, SequenceAlphaTweenSetup setup)
   {
      if (sequenceCallbacksRoutine != null)
      {
         StopCoroutine(sequenceCallbacksRoutine);
         foreach (var (_, callback) in sequenceCallbacks) callback?.Invoke();
      }

      sequenceCallbacks = setup.InsertToSequence(seq).OrderBy(x => x.Item1).ToList();

      StartCoroutine(SequenceCallbacks());
   }

   List<(float, Action)> sequenceCallbacks;

   IEnumerator SequenceCallbacks()
   {
      var duration = 0.0f;
      while (sequenceCallbacks.Count > 0)
      {
         float wait = sequenceCallbacks[0].Item1 - duration;
         if (wait > 0) yield return new WaitForSeconds(wait);
         sequenceCallbacks[0].Item2?.Invoke();
         sequenceCallbacks.RemoveAt(0);
      }
   }

   Coroutine sequenceCallbacksRoutine;

   private void Awake()
   {
      if (rendr == null) rendr = GetComponent<Renderer>();
      if (rendr == null) Debug.LogError($"Missing renderer on this object.", this);
      else
      {
         var defaultAlpha = overrideDefaultAlpha && this.defaultAlpha != 1f ? this.defaultAlpha : 1f;
         if (showEnabled) showTransition.Init(rendr, 0f, defaultAlpha);
         if (hideEnabled)
         {
            hideTransition.Init(rendr, defaultAlpha, 0f);
            hideTransition.AddCallback(new SequenceCallback(() => Debug.Log($"hiding {name}.", this)));
         }
      }
   }

   private void OnValidate()
   {
      rendr = GetComponent<Renderer>();
      defaultAlpha = rendr == null || overrideDefaultAlpha ? defaultAlpha : rendr.sharedMaterial.color.a;
      showEnabled = DoFlagsMatch(e_TransitionType.Show);
      hideEnabled = DoFlagsMatch(e_TransitionType.Hide);
   }
}

public abstract class TransitionSequencer : MonoBehaviour
{
   [SerializeField] protected e_TransitionSelection doTransitionOn = e_TransitionSelection.Show | e_TransitionSelection.Hide;

   [Flags]
   protected enum e_TransitionSelection
   {
      Show = 1,
      Hide = 2
   }

   public enum e_TransitionType
   {
      Show,
      Hide
   }

   public bool DoFlagsMatch(e_TransitionType transitionType) => (transitionType) switch
   {
      e_TransitionType.Show => doTransitionOn.HasFlag(e_TransitionSelection.Show),
      e_TransitionType.Hide => doTransitionOn.HasFlag(e_TransitionSelection.Hide),
      _ => false
   };


   public virtual void OnSequenceEvent(e_TransitionType transitionType, Sequence seq)
   {
      if (!DoFlagsMatch(transitionType)) return;

      var action = GetAction(transitionType);
      action?.Invoke(seq);
   }

   protected abstract Action<Sequence> GetAction(e_TransitionType transitionType);
}