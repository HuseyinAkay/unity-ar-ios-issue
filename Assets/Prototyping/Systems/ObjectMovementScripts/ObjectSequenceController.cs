using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MyBox;
using System.Linq;
using UnityEngine.Events;

public class ObjectSequenceController : MonoBehaviour
{
   [SerializeField] bool completeUnfinishedSequences = true;
   [SerializeField] TransitionSequencer[] showSequencers, hideSequencers;

   private void OnValidate()
   {
      List<TransitionSequencer> allSequencers = new();
      GetComponentsInChildren<TransitionSequencer>(true, allSequencers);
      showSequencers = allSequencers.FindAll(x => x.DoFlagsMatch(TransitionSequencer.e_TransitionType.Show)).ToArray();
      hideSequencers = allSequencers.FindAll(x => x.DoFlagsMatch(TransitionSequencer.e_TransitionType.Hide)).ToArray();
   }

   public void PlayShowSequence()
   {
      DOTween.Kill(this, completeUnfinishedSequences);
      var seq = DOTween.Sequence(this);

      foreach (var sequencer in showSequencers)
      sequencer.OnSequenceEvent(TransitionSequencer.e_TransitionType.Show, seq);
   }

   public float PlayHideSequence()
   {
      DOTween.Kill(this, completeUnfinishedSequences);
      var seq = DOTween.Sequence(this);

      foreach (var sequencer in hideSequencers)
      sequencer.OnSequenceEvent(TransitionSequencer.e_TransitionType.Hide, seq);

      return seq.Duration();
   }
}

namespace TweenSequence.Configuration
{
   [Serializable]
   public abstract class SequenceGenericAmountTweenSetup<T> : SequenceTweenSetup where T : struct
   {
      [SerializeField] protected T amount = default;
      new protected Func<T, float, Tween> tween;

      /// <summary>
      /// Initialize the tween setup for runtime
      /// </summary>
      /// <param name="tweeningFunction">tweening function, should take duration and amount to return a Tween</param>
      public void Init(Func<float, T, Tween> tweeningFunction)
      {
         this.tween = (val, duration) => tweeningFunction(duration, val);
      }
   }

   [Serializable]
   public abstract class SequenceTweenSetup
   {
      [SerializeField] protected Ease ease = Ease.Linear;
      [SerializeField] protected float duration = 1f;
      [SerializeField] protected float initialDelay = 0f;
      [SerializeField] protected List<SequenceCallback> actions = new();
      protected Func<Tween> tween;

      /// <summary>
      /// Initialize the tween setup for runtime
      /// </summary>
      /// <param name="tweeningFunction">tweening function, should take duration to return a Tween</param>
      public void Init(Func<float, Tween> tweeningFunction)
      {
         this.tween = () => tweeningFunction(duration);
      }

      public List<(float, Action)> InsertToSequence(Sequence seq)
      {
         seq.Insert(initialDelay, tween().SetEase(ease));

         return actions.ConvertAll(x => x.GetDelayAndAction());
      }

      public void AddCallback(SequenceCallback callback) => actions.Add(callback);

      public bool RemoveCallback(SequenceCallback callback) => actions.Remove(callback);
   }


   [Serializable]
   public class SequenceCallback
   {
      [SerializeField] protected float initialDelay = 0f;
      [SerializeField] UnityEngine.Events.UnityEvent action = new();

      public SequenceCallback( float initialDelay, UnityEngine.Events.UnityEvent action)
      {
         this.initialDelay = initialDelay;
         this.action = action;
      }

      public SequenceCallback(UnityAction action, float initialDelay = 0f) : this(initialDelay, new()) { this.action.AddListener(action); }

      public (float, Action) GetDelayAndAction() => (initialDelay, () => action.Invoke());
   }
}