using UnityEngine;
using UnityEngine.InputSystem;

namespace GallinasFelices.Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("Pan Settings")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float panBorderThickness = 10f;
        [SerializeField] private Vector2 panLimit = new Vector2(50f, 50f);

        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 30f;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference panAction;
        [SerializeField] private InputActionReference zoomAction;
        [SerializeField] private InputActionReference dragAction;
        [SerializeField] private InputActionReference mousePositionAction;

        [Header("Touch/Drag Settings")]
        [SerializeField] private float dragSpeed = 2f;

        private Vector3 dragOrigin;
        private bool isDragging;
        private UnityEngine.Camera cam;

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            if (cam == null)
            {
                cam = UnityEngine.Camera.main;
            }
        }

        private void OnEnable()
        {
            if (panAction != null)
            {
                panAction.action.Enable();
            }

            if (zoomAction != null)
            {
                zoomAction.action.Enable();
            }

            if (dragAction != null)
            {
                dragAction.action.Enable();
                dragAction.action.performed += OnDragPerformed;
                dragAction.action.canceled += OnDragCanceled;
            }

            if (mousePositionAction != null)
            {
                mousePositionAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (panAction != null)
            {
                panAction.action.Disable();
            }

            if (zoomAction != null)
            {
                zoomAction.action.Disable();
            }

            if (dragAction != null)
            {
                dragAction.action.Disable();
                dragAction.action.performed -= OnDragPerformed;
                dragAction.action.canceled -= OnDragCanceled;
            }

            if (mousePositionAction != null)
            {
                mousePositionAction.action.Disable();
            }
        }

        private void Update()
        {
            HandleKeyboardPan();
            HandleMouseDrag();
            HandleZoom();
        }

        private void HandleKeyboardPan()
        {
            if (panAction == null)
            {
                return;
            }

            Vector2 panInput = panAction.action.ReadValue<Vector2>();
            Vector3 position = transform.position;

            position.x += panInput.x * panSpeed * Time.deltaTime;
            position.z += panInput.y * panSpeed * Time.deltaTime;

            position.x = Mathf.Clamp(position.x, -panLimit.x, panLimit.x);
            position.z = Mathf.Clamp(position.z, -panLimit.y, panLimit.y);

            transform.position = position;
        }

        private void HandleMouseDrag()
        {
            if (isDragging && mousePositionAction != null)
            {
                Vector2 mousePosition = mousePositionAction.action.ReadValue<Vector2>();
                Vector3 worldPosition = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, cam.transform.position.y));

                Vector3 difference = dragOrigin - worldPosition;
                Vector3 newPosition = transform.position + difference;

                newPosition.x = Mathf.Clamp(newPosition.x, -panLimit.x, panLimit.x);
                newPosition.z = Mathf.Clamp(newPosition.z, -panLimit.y, panLimit.y);

                transform.position = newPosition;
            }
        }

        private void HandleZoom()
        {
            if (zoomAction == null)
            {
                return;
            }

            float scrollInput = zoomAction.action.ReadValue<float>();

            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                Vector3 position = transform.position;
                position.y -= scrollInput * zoomSpeed * Time.deltaTime;
                position.y = Mathf.Clamp(position.y, minZoom, maxZoom);
                transform.position = position;
            }
        }

        private void OnDragPerformed(InputAction.CallbackContext context)
        {
            isDragging = true;

            if (mousePositionAction != null && cam != null)
            {
                Vector2 mousePosition = mousePositionAction.action.ReadValue<Vector2>();
                dragOrigin = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, cam.transform.position.y));
            }
        }

        private void OnDragCanceled(InputAction.CallbackContext context)
        {
            isDragging = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(panLimit.x * 2f, 0f, panLimit.y * 2f));
        }
    }
}
