using UnityEngine;
using TMPro;

namespace GallinasFelices.UI
{
    public class StructureStatusUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Structures.ConsumableStructure structure;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private UnityEngine.UI.Slider capacitySlider;

        [Header("Settings")]
        [SerializeField] private bool showPercentage = true;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);

        private Canvas canvas;
        private UnityEngine.Camera mainCamera;

        private void Awake()
        {
            mainCamera = UnityEngine.Camera.main;
            canvas = GetComponentInChildren<Canvas>();

            if (canvas != null)
            {
                canvas.worldCamera = mainCamera;
            }

            if (structure == null)
            {
                structure = GetComponentInParent<Structures.ConsumableStructure>();
            }
        }

        private void Start()
        {
            if (structure != null)
            {
                structure.OnCapacityChanged.AddListener(UpdateDisplay);
                UpdateDisplay(structure.FillPercentage);
            }
        }

        private void LateUpdate()
        {
            if (canvas != null && mainCamera != null)
            {
                canvas.transform.position = transform.position + offset;
                canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - mainCamera.transform.position);
            }
        }

        private void UpdateDisplay(float fillPercentage)
        {
            if (capacitySlider != null)
            {
                capacitySlider.value = fillPercentage;
            }

            if (statusText != null && showPercentage)
            {
                statusText.text = $"{Mathf.RoundToInt(fillPercentage * 100f)}%";

                if (structure.IsEmpty)
                {
                    statusText.color = Color.red;
                }
                else if (fillPercentage < 0.3f)
                {
                    statusText.color = Color.yellow;
                }
                else
                {
                    statusText.color = Color.green;
                }
            }
        }

        private void OnDestroy()
        {
            if (structure != null)
            {
                structure.OnCapacityChanged.RemoveListener(UpdateDisplay);
            }
        }
    }
}
