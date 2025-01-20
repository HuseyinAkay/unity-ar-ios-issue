using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Object_Shake : MonoBehaviour
{
   private List<Tween> list; public bool completeTweens = false, snapping = false, fadeOut = true;
   public float duration = 1f, randomness = 90f, strength = 1f; public int vibrato = 10;
   private void Awake()
   {
      DOTween.Init();
   }
   public void Shake(float stengthMultiplyer = 1f, float vibratoMultiplyer = 1f, float durationMultiplyer = 1f, bool killRunningTweens = false)
   {
      DOTween.PlayingTweens(list);
      if (list == null || list.Count <= 0 || killRunningTweens)
      {
         if (list != null && list.Count >= 1) list.ForEach(x => x.Kill(completeTweens));
         Sequence sequence = DOTween.Sequence();
         sequence.Append(transform.DOShakePosition(duration * durationMultiplyer, strength * stengthMultiplyer, Mathf.RoundToInt((float)vibrato * vibratoMultiplyer), randomness, snapping, fadeOut));
      }
   }
   public void Shake(float duration = 1f, float randomness = 90f, float strength = 1f, int vibrato = 10, bool killRunningTweens = false, bool completeTweens = false, bool snapping = false, bool fadeOut = true)
   {
      DOTween.PlayingTweens(list);
      if (list == null || list.Count <= 0 || killRunningTweens)
      {
         if (list.Count >= 1) list.ForEach(x => x.Kill(completeTweens));
         Sequence sequence = DOTween.Sequence();
         sequence.Append(transform.DOShakePosition(duration, strength, vibrato, randomness, snapping, fadeOut));
      }
   }

}
