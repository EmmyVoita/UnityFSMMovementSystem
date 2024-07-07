using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerData
{
    [System.Serializable]
    public class SimpleTimer
    {
        public bool flag = false;
        private Coroutine timerCoroutine;
        private float startTime = 0f; // Added this line

        public float ElapsedTime { get { return Time.time - startTime; } } // Added this line

        public void SetFlag(bool value, float duration)
        {
            // If a timer is already running, stop it
            if (timerCoroutine != null)
            {
                CoroutineStarter.Instance.StopCoroutine(timerCoroutine);
            }

            startTime = Time.time; // Reset the elapsed time
            timerCoroutine = CoroutineStarter.Instance.StartCoroutine(TimerCoroutine(value, duration));
        }

        public void StopTimer()
        {
            if (timerCoroutine != null)
            {
                CoroutineStarter.Instance.StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }

        private IEnumerator TimerCoroutine(bool value, float duration)
        {
            flag = value;
            float startTime = Time.time; // Store the start time

            yield return new WaitForSeconds(duration);

            flag = !value;
            //elapsedTime = Time.time - startTime; // Calculate the elapsed time
        }
    }
}
