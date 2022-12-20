using System.Collections;
using UnityEngine;

namespace Game.Extension
{
    public static class CoroutineExtension
    {
        public static Coroutine WaitOneFrame(this MonoBehaviour monoBehaviour, System.Action action)
        {
            return monoBehaviour.StartCoroutine(DelayOneFrame(action));
        }
        private static IEnumerator DelayOneFrame(System.Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        public static Coroutine WaitToDo(this MonoBehaviour monoBehaviour, System.Action action, float waitDuration)
        {
            if (waitDuration <= 0)
            {
                action?.Invoke();
                return null;
            }
            else
            {
                return monoBehaviour.StartCoroutine(DelayToDo(action, waitDuration));
            }
        }

        private static IEnumerator DelayToDo(System.Action action, float waitDuration)
        {
            yield return new WaitForSeconds(waitDuration);
            action?.Invoke();
        }

        public static Coroutine WaitToDoUntil(this MonoBehaviour monoBehaviour, System.Action action, System.Func<bool> predicate)
        {
            if (predicate())
            {
                action?.Invoke();
                return null;
            }
            else
            {
                return monoBehaviour.StartCoroutine(DelayToDoUntil(action, predicate));
            }
        }

        private static IEnumerator DelayToDoUntil(System.Action action, System.Func<bool> predicate)
        {
            yield return new WaitUntil(predicate);
            action?.Invoke();
        }

        public static Coroutine QuickStopTime(this MonoBehaviour monoBehaviour, float durationRealtime)
        {
            if (durationRealtime > 0f) return monoBehaviour.StartCoroutine(DoQuickStopTime(durationRealtime));
            else return null;
        }

        private static IEnumerator DoQuickStopTime(float durationRealtime)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(durationRealtime);
            Time.timeScale = 1f;
        }
    }
}