using UnityEngine;

namespace GallinasFelices.VFX
{
    public class SimpleBillboard : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool freezeXAxis = false;
        [SerializeField] private bool freezeYAxis = false;
        [SerializeField] private bool freezeZAxis = false;

        private UnityEngine.Camera mainCamera;

        private void Start()
        {
            mainCamera = UnityEngine.Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null) return;
            }

            Vector3 lookDirection = mainCamera.transform.position - transform.position;
            
            if (freezeXAxis) lookDirection.x = 0f;
            if (freezeYAxis) lookDirection.y = 0f;
            if (freezeZAxis) lookDirection.z = 0f;
            
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
}
