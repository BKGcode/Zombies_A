using UnityEngine;
using UnityEngine.InputSystem;

namespace GallinasFelices.Core
{
    public class InteractionManager : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Layer mask for interactable objects")]
        [SerializeField] private LayerMask interactableLayer = ~0;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference clickAction;

        private void OnEnable()
        {
            if (clickAction != null)
            {
                clickAction.action.performed += OnClick;
                clickAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (clickAction != null)
            {
                clickAction.action.performed -= OnClick;
                clickAction.action.Disable();
            }
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            HandleClick();
        }

        private void HandleClick()
        {
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    PanelLane lane = DetermineLane(interactable);
                    
                    if (PanelManager.Instance != null)
                    {
                        PanelManager.Instance.ShowPanel(interactable, lane);
                    }
                }
            }
        }

        private PanelLane DetermineLane(IInteractable interactable)
        {
            Component component = interactable as Component;
            if (component == null) return PanelLane.Right;

            if (component.GetComponent<Chicken.Chicken>() != null)
            {
                return PanelLane.Left;
            }

            return PanelLane.Right;
        }
    }
}
