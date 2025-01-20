using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationFinishCallback : MonoBehaviour
{
   [SerializeField] UnityEvent onFinish;
   void AnimationEnd() => onFinish.Invoke();
}