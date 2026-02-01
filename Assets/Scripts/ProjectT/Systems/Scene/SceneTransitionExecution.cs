using System.Collections;
using ProjectT.Core;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Player.Controller;
using ProjectT.Systems.Camera;
using ProjectT.Systems.GameMode;
using ProjectT.Systems.Player;
using ProjectT.Systems.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectT.Systems.Scene
{
    public class SceneTransitionExecution : Singleton<SceneTransitionExecution>
    {
        [SerializeField] private ActionLockFlags transitionLockFlags =
            ActionLockFlags.Move | ActionLockFlags.BasicAttack | ActionLockFlags.Dash | ActionLockFlags.Skill;

        private bool _isTransitioning;

        public void Request(SceneTransitionRequest request)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionExecution] Transition request ignored: already in progress.");
                return;
            }

            if (string.IsNullOrWhiteSpace(request.sceneName))
            {
                Debug.LogWarning("[SceneTransitionExecution] sceneName is null or empty.");
                return;
            }

            StartCoroutine(ExecuteTransition(request));
        }

        private IEnumerator ExecuteTransition(SceneTransitionRequest request)
        {
            _isTransitioning = true;

            AcquireTransitionLock();
            UIInteractionSystem.Instance?.SetInteractable(false);

            if (request.useFade)
            {
                var fader = UIFadeSystem.Instance;
                if (fader != null)
                {
                    yield return fader.FadeOut();
                }
            }

            AsyncOperation async = SceneManager.LoadSceneAsync(request.sceneName, LoadSceneMode.Single);
            if (async == null)
            {
                Debug.LogError("[SceneTransitionExecution] LoadSceneAsync failed: null AsyncOperation.");
                ReleaseTransitionLock();
                _isTransitioning = false;
                yield break;
            }

            while (!async.isDone)
            {
                yield return null;
            }

            yield return WaitForSceneReady();

            RebindScene(request);

            if (request.useFade)
            {
                var fader = UIFadeSystem.Instance;
                if (fader != null)
                {
                    yield return fader.FadeIn();
                }
            }

            UIInteractionSystem.Instance?.SetInteractable(true);
            ReleaseTransitionLock();

            if (request.resetTimeScale)
            {
                Time.timeScale = 1f;
            }

            _isTransitioning = false;
        }

        private void AcquireTransitionLock()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.AcquireTransitionLock(transitionLockFlags);
            }
        }

        private void ReleaseTransitionLock()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.ReleaseTransitionLock(transitionLockFlags);
            }
        }

        private IEnumerator WaitForSceneReady()
        {
            SceneReadyEmitter emitter = FindSceneReadyEmitter();
            if (emitter == null)
            {
                yield break;
            }

            bool ready = false;
            void OnReady()
            {
                ready = true;
            }

            emitter.OnReady += OnReady;
            while (!ready)
            {
                yield return null;
            }
            emitter.OnReady -= OnReady;
        }

        private void RebindScene(SceneTransitionRequest request)
        {
            var entry = FindEntryPoint(request.entryId);
            if (entry != null)
            {
                PlayerWarpSystem.Instance?.WarpTo(entry.transform);
            }

            var player = PlayerMovementExecution.Instance != null ? PlayerMovementExecution.Instance.transform : null;
            if (player != null)
            {
                CameraSystem.Instance?.BindFollow(player);
            }

            var confinerProvider = FindConfinerProvider();
            if (confinerProvider != null)
            {
                CameraSystem.Instance?.SetConfiner(confinerProvider.GetConfiner());
            }

            GameModeSystem.Instance?.SetMode(request.targetGameMode);
        }

        private static SceneReadyEmitter FindSceneReadyEmitter()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) return null;

            foreach (var root in scene.GetRootGameObjects())
            {
                var emitter = root.GetComponentInChildren<SceneReadyEmitter>(true);
                if (emitter != null) return emitter;
            }
            return null;
        }

        private static EntryPointMarker FindEntryPoint(string entryId)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) return null;

            EntryPointMarker fallback = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                var markers = root.GetComponentsInChildren<EntryPointMarker>(true);
                foreach (var marker in markers)
                {
                    if (string.IsNullOrWhiteSpace(entryId))
                    {
                        if (fallback == null) fallback = marker;
                        continue;
                    }

                    if (marker.EntryId == entryId)
                    {
                        return marker;
                    }
                }
            }
            return fallback;
        }

        private static CameraConfinerProvider FindConfinerProvider()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) return null;

            foreach (var root in scene.GetRootGameObjects())
            {
                var provider = root.GetComponentInChildren<CameraConfinerProvider>(true);
                if (provider != null) return provider;
            }
            return null;
        }
    }
}
