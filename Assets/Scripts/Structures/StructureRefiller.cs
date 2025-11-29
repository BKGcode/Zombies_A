using UnityEngine;

namespace GallinasFelices.Structures
{
    public class StructureRefiller : MonoBehaviour
    {
        [Header("Auto Refill Settings")]
        [SerializeField] private bool autoRefillEnabled = false;
        [SerializeField] private float refillInterval = 30f;
        [SerializeField] private float refillPercentage = 1f;

        [Header("Manual Refill")]
        [SerializeField] private KeyCode manualRefillKey = KeyCode.R;

        private ConsumableStructure structure;
        private float refillTimer;

        private void Awake()
        {
            structure = GetComponent<ConsumableStructure>();
        }

        private void Update()
        {
            HandleManualRefill();

            if (autoRefillEnabled)
            {
                HandleAutoRefill();
            }
        }

        private void HandleManualRefill()
        {
            if (Input.GetKeyDown(manualRefillKey) && structure != null)
            {
                structure.RefillToMax();
                Debug.Log($"{gameObject.name} refilled manually");
            }
        }

        private void HandleAutoRefill()
        {
            refillTimer += Time.deltaTime;

            if (refillTimer >= refillInterval)
            {
                refillTimer = 0f;

                if (structure != null)
                {
                    float amountToRefill = structure.MaxCapacity * refillPercentage;
                    structure.Refill(amountToRefill);
                }
            }
        }
    }
}
