using ProjectT.Contracts;
using ProjectT.Core;
using ProjectT.Gameplay.Player.Input;
using UnityEngine;


namespace ProjectT.Systems.UI
{
    public class InteractionManager : Singleton<InteractionManager>
    {
        private IInteractable interactableInRange = null;   // 상호작용 가능한 오브젝트
        public GameObject interactionIcon;                  // 상호작용 아이콘 UI
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
            interactionIcon.SetActive(false);
        }
        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInteractInput += HandleInteractInput;
            }
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInteractInput -= HandleInteractInput;
            }
        }
        private void HandleInteractInput()
        {
            interactableInRange?.Interact();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
            {
                interactableInRange = interactable;
                interactionIcon.SetActive(true);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
            {
                interactableInRange = null;
                interactionIcon.SetActive(false);
            }
        }
    }
}
