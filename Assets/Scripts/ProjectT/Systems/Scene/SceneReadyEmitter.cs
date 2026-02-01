using System;
using System.Collections;
using UnityEngine;

namespace ProjectT.Systems.Scene
{
    public class SceneReadyEmitter : MonoBehaviour
    {
        public event Action OnReady;
        private bool _hasEmitted;

        private void Start()
        {
            StartCoroutine(EmitReadyNextFrame());
        }

        private IEnumerator EmitReadyNextFrame()
        {
            // 한 프레임 대기하여 씬 오브젝트 활성화 보장
            yield return null;
            if (_hasEmitted) yield break;
            _hasEmitted = true;
            OnReady?.Invoke();
        }
    }
}
