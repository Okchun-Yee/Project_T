using System;
using System.Collections;
using UnityEngine;
namespace ProjectT.Gameplay.Combat
{
    public class Combo : MonoBehaviour
    {
        [SerializeField] private int comboLength = 3;
        [SerializeField] private float comboDelay = 0.6f;

        private int comboIndex = -1;
        private Coroutine timeoutCoroutine;

        public event Action<int> OnComboAdvanced;
        public event Action OnComboReset;

        public int CurrentIndex => comboIndex;

        public int Advance()
        {
            comboIndex = (comboIndex + 1) % Mathf.Max(1, comboLength);
            OnComboAdvanced?.Invoke(comboIndex);
            RestartTimeout();
            return comboIndex;
        }

        public void Reset()
        {
            comboIndex = -1;
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }
            OnComboReset?.Invoke();
        }

        private void RestartTimeout()
        {
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }
            if (comboDelay > 0f)
                timeoutCoroutine = StartCoroutine(ComboTimeout());
        }

        private IEnumerator ComboTimeout()
        {
            float t = 0f;
            while (t < comboDelay)
            {
                t += Time.deltaTime;
                yield return null;
            }
            Reset();
        }

        public void SetComboLength(int len) => comboLength = Mathf.Max(1, len);
        public void SetComboDelay(float d) => comboDelay = Mathf.Max(0f, d);
    }
}
