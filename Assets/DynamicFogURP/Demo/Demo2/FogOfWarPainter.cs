using UnityEngine;
using UnityEngine.InputSystem;
using DynamicFogAndMist2;

namespace DynamicFogAndMist2_Demos
{

    public class FogOfWarPainter : MonoBehaviour
    {

        [Header("Fog Settings")]
        [Tooltip("Radius of fog clearing area")]
        public float clearRadius = 5f;
        public float clearDuration = 0f;
        public float restoreDelay = 10f;
        public float restoreDuration = 2f;
        [Range(0, 1)]
        public float borderSmoothness = 0.2f;

        DynamicFog fog;
        Camera mainCamera;

        void Start()
        {
            fog = GetComponent<DynamicFog>();
            mainCamera = Camera.main;
        }

        void Update()
        {
            if (Mouse.current == null || mainCamera == null)
            {
                return;
            }

            if (Mouse.current.leftButton.isPressed)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                
                if (Physics.Raycast(ray, out RaycastHit terrainHit))
                {
                    fog.SetFogOfWarAlpha(terrainHit.point, clearRadius, 0, true, clearDuration, borderSmoothness, restoreDelay, restoreDuration);
                }
            }
        }

        public void RestoreFog()
        {
            fog.ResetFogOfWar();
        }
    }

}